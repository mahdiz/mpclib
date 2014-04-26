using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using MpcLib.Common;
using MpcLib.Common.FiniteField;
using MpcLib.Common.FiniteField.Circuits;
using MpcLib.DistributedSystem;
using MpcLib.MpcProtocols.Bgw.Vss;
using MpcLib.SecretSharing;

namespace MpcLib.MpcProtocols.Bgw
{
	public delegate void FinishHandler(StateKey stateKey);

	/// <summary>
	/// Implements the MPC protocol of BenOr-Goldwasser-Wigderson (1988) for non-Byzantine case.
	/// </summary>
	public class BgwProtocol : AsyncMpc<Zp>
	{
		#region Fields

		public int PolynomialDeg;
		protected readonly Circuit Circuit;
		protected readonly int Prime;
		public event FinishHandler MpcFinish;
		public override ProtocolIds Id { get { return ProtocolIds.BGW; } }

		#endregion Fields

		public BgwProtocol(Circuit circuit, ReadOnlyCollection<int> pIds,
			AsyncParty e, Zp pInput, SendHandler send, StateKey stateKey)
			: base(e, pIds, pInput, send, stateKey)
		{
			Debug.Assert(circuit.InputCount == pIds.Count);
			Circuit = circuit;
			Prime = pInput.Prime;

			// to get the maximum polynomial degree, we should ask if there is a MUL in the circuit
			// Mahdi: This is probably valid only when we assume a reliable broadcast channel.
			//PolynomialDeg = circuit.MultipleContained ? (NumEntities - 1) / 2 : NumEntities - 1;

			// Mahdi: Changed to the following since n/3 - 1 of players can be dishonest.
			// degree = n - t, where t is the number of dishonest players
			PolynomialDeg = (int)Math.Floor(2 * NumParties / 3.0);
		}

		public BgwProtocol(AsyncParty p, Circuit circuit, ReadOnlyCollection<int> playerIds,
			Zp playerInput, StateKey stateKey)
			: this(circuit, playerIds, p, playerInput, p.Send, stateKey)
		{
		}

		#region Methods

		public override void Run()
		{
			// secret-share my input among all parties
			var sharesValues = ShamirSharing.Share(Input, NumParties, PolynomialDeg);
			var shareMsgs = new List<ShareMsg<Zp>>();
			foreach (var shareValue in sharesValues)
				shareMsgs.Add(new ShareMsg<Zp>(new Share<Zp>(shareValue), Stage.InputReceive));

			Send(shareMsgs);

			OnReceive((int)Stage.InputReceive,
				delegate(List<Msg> shares)
				{
					int k = 1;	// TODO: temp only - needed to be index of gate
					foreach (var gate in Circuit.Gates)
					{
						RunGateComputation(gate, GetZps(shares.Cast<ShareMsg<Zp>>()), k + ".");
						k++;
					}
					var resultList = new SortedDictionary<int, Zp>();
					FilterPlayers(PartyIds);		// remove unwanted players if necessary

					// share the result with all players
					SendToAll(new ShareMsg<Zp>(new Share<Zp>(Circuit.Output), Stage.ResultReceive));

					OnReceive((int)Stage.ResultReceive,
						delegate(List<Msg> resMsgs)
						{
							Result = GetRecombinedResult(GetZps(resMsgs.OrderBy(s => s.SenderId).Cast<ShareMsg<Zp>>()), Input.Prime);
							if (MpcFinish != null)
								MpcFinish(StateKey);
						});
				});
		}

		#region Virtual Methods

		//public override void Receive(Message msg)
		//{
		//	// TODO: [CRITICAL] - THE PLAYER MUST WAIT ONLY A CERTAIN AMOUNT OF TIME FOR A MESSAGE TO BE RECEIVED, NOT FOREVER!
		//	MsgDictionary<ShareMessage> shares = null;

		//	switch (msg.StageKey)
		//	{
		//		case Stage.InputReceive:
		//			var shareMsg = msg as ShareMessage;
		//			shares = MsgCollector.Collect(shareMsg.SenderId, shareMsg, NumEntities);

		//			////////////// TODO: STOPPED HERE ON 09/21/2012
		//			////////////// MAKE SURE JOSH'S BINARY HEAP STORES EVENTS IN EXACTLY THE SAME ORDER THEY ARE INSERTED.
		//			////////////// THIS IS BECAUSE WE DO A SYNCHRONOUS ASSUMPTION HERE THAT ALL MESSAGES (SHARES) OF EACH OF
		//			////////////// THE THREE QUORUMS HAVE ARRIVED TOGETHER IN THE SAME ORDER THEY WERE SENT. OTHERWISE,
		//			////////////// THE CIRCUIT WILL BE COMPUTED ON INCORRECT INPUTS.

		//			if (shares != null)
		//			{
		//				int k = 1;	// TODO: temp only - needed to be index of gate
		//				foreach (var gate in Circuit.Gates)
		//				{
		//					RunGateComputation(gate, GetZps(shares), k + ".");
		//					k++;
		//				}

		//				var resultList = new SortedDictionary<int, Zp>();
		//				FilterPlayers(EntityIds);		// remove unwanted players if necessary

		//				// share the result with all players
		//				SendToAll(new ShareMessage(new ShareObject(Circuit.Output), Stage.ResultReceive));
		//			}
		//			break;

		//		case Stage.ResultReceive:
		//			shareMsg = smpcMsg as ShareMessage;
		//			shares = MsgCollector.Collect(shareMsg.SenderId, shareMsg, NumEntities);
		//			if (shares != null)
		//			{
		//				Result = GetRecombinedResult(GetZps(shares.OrderBy(s => s.SenderId)), Input.Prime);
		//				if (MpcFinish != null)
		//					MpcFinish(StateKey);
		//			}
		//			break;

		//		case Stage.RandomizationReceive:
		//			shareMsg = smpcMsg as ShareMessage;
		//			shares = MsgCollector.Collect(shareMsg.SenderId, shareMsg, NumEntities);

		//			if (shares != null)
		//			{
		//				var vanderFirstRow =
		//					ZpMatrix.GetSymmetricVanderMondeMatrix(NumEntities, Prime)
		//						.Transpose.Inverse.GetMatrixRow(0);

		//				// Calculate the value of the polynomial H(x) at i = H(i) as defined at GRR
		//				var tempSecret = new Zp(Prime, 0);
		//				for (int i = 0; i < NumEntities; i++)
		//					tempSecret.Add((shares[i].Share as ShareObject).SharedSecret.Mul(vanderFirstRow[i]));
		//			}
		//			break;
		//	}
		//}

		/// <summary>
		/// Inputs are all the inputs to the circuit. Gate should know to choose the inputs it needs.
		/// </summary>
		public void RunGateComputation(Gate gate, IList<Zp> inputs, string gatePrefix)
		{
			var values = new List<Zp>();
			foreach (var wire in gate.InputWires)
			{
				if (wire.IsInput)
				{
					if (inputs.Count <= wire.InputIndex)
						throw new Exception("Input " + wire.InputIndex + " is expected - not found in the list given");
					values.Add(inputs[wire.InputIndex]);
				}
				else
				{
					Debug.Assert(wire.SourceGate != null && wire.SourceGate.IsOutputReady);
					values.Add(wire.ConstValue != null ? wire.ConstValue : wire.SourceGate.OutputValue);
				}
			}
			var result = new Zp(values[0]);
			values.RemoveAt(0);

			foreach (var value in values)
			{
				Zp currValue = value;
				if (gate.Operation == Operation.Div)
				{
					throw new NotImplementedException();

					// TODO: FOR DIVISION IN SMPC: COMMENTED BY MAHDI: TEMPORARILY
					//MpcProtocol divProtocol;
					//if (protocol is MPCProtocolMultStepCheaterPlayer)
					//{
					//    divProtocol = new MPCProtocolMultStepCheaterPlayer((MPCProtocolMultStepCheaterPlayer) protocol, Circuit.getDivisionExponentialCircuit(p));
					//}
					//else if (protocol is ByzantineMpcProtocol)
					//{
					//    divProtocol = new ByzantineMpcProtocol((ByzantineMpcProtocol)protocol, Circuit.getDivisionExponentialCircuit(p));
					//}
					//else
					//{
					//    divProtocol = new MpcProtocol(protocol, Circuit.getDivisionExponentialCircuit(p));
					//}
					//currValue = new Zp(p, divProtocol.Calculate(value, true, gatePrefix)[0].Value);
				}
				result.Calculate(currValue, gate.Operation == Operation.Div ? Operation.Mul : gate.Operation);
			}
			if (gate.IsDegreeReductionNeeded)
				RunReductionRandomization(result);
			else
				gate.OutputValue = result;		// computation of any linear gate can be done with no further communication at all.
		}

		/// <summary>
		/// Removes unwanted players if necessary.
		/// </summary>
		protected virtual void FilterPlayers(IList<int> players)
		{
		}

		/// <summary>
		/// Implementation according to GRR.
		/// </summary>
		public virtual void RunReductionRandomization(Zp oldSecret)
		{
			// randomize the coeficients
			// generate a t degree polynomial, hi(x), 
			// with a free coef that equals 'ab' and create share for users from it.
			var sharesValues = ShamirSharing.Share(oldSecret, NumParties, PolynomialDeg);
			var shareMsgs = new List<ShareMsg<Zp>>();
			foreach (var shareValue in sharesValues)
				shareMsgs.Add(new ShareMsg<Zp>(new Share<Zp>(shareValue), Stage.RandomizationReceive));

			// send to the j-th user hi(j) and receive from every other k player hk(i)
			Send(shareMsgs);

			OnReceive((int)Stage.RandomizationReceive,
				delegate(List<Msg> shares)
				{
					var vanderFirstRow =
						ZpMatrix.GetSymmetricVanderMondeMatrix(NumParties, Prime)
							.Transpose.Inverse.GetMatrixRow(0);

					// Calculate the value of the polynomial H(x) at i = H(i) as defined at GRR
					var tempSecret = new Zp(Prime, 0);
					for (int i = 0; i < NumParties; i++)
						tempSecret.Add(((shares[i] as ShareMsg<Zp>).Share as Share<Zp>).Value.Mul(vanderFirstRow[i]));
				});
		}

		// TODO: Mahdi: Should this method be virtual?
		protected virtual Zp GetRecombinedResult(IList<Zp> recvList, int prime)
		{
			return ShamirSharing.Recombine(recvList, PolynomialDeg, prime);
		}

		#endregion Virtual Methods

		#region Utility Methods

		private IList<Zp> GetZps(IEnumerable<ShareMsg<Zp>> shareMsgs)
		{
			var zPs = new List<Zp>();
			foreach (var shareMsg in shareMsgs)
				zPs.Add(shareMsg != null ? (shareMsg.Share as Share<Zp>).Value : null);
			return zPs;
		}

		protected IList<ShareMsg<Zp>> GetShareMessages(IList<Zp> zPs, Stage stage)
		{
			var shareMsgs = new List<ShareMsg<Zp>>();
			foreach (var zp in zPs)
				shareMsgs.Add(new ShareMsg<Zp>(new Share<Zp>(zp), stage));

			return shareMsgs;
		}

		protected IList<ShareMsg<Zp>> GetShareMessages<T>(IList<T> shares, Stage stage)
			where T : BgwShare
		{
			var shareMsgs = new List<ShareMsg<Zp>>();
			foreach (var share in shares)
				shareMsgs.Add(new ShareMsg<Zp>(share, stage));

			return shareMsgs;
		}

		#endregion Utility Methods

		#region Send Methods

		protected void Send<T>(IDictionary<int, T> msgList)
			where T : MpcMsg
		{
			foreach (var msg in msgList)
				Send(Party.Id, msg.Key, msg.Value);
		}

		protected void Send<T>(IList<T> msgList)
			where T : MpcMsg
		{
			for (int i = 0; i < msgList.Count; i++)
				Send(Party.Id, PartyIds[i], msgList[i]);
		}

		protected void Send<T>(IList<T> msgList, HashSet<int> exceptPlayers)
			where T : MpcMsg
		{
			for (int i = 0; i < msgList.Count; i++)
			{
				if (!exceptPlayers.Contains(PartyIds[i]))
					Send(Party.Id, PartyIds[i], msgList[i]);
			}
		}

		protected void SendToAll<T>(T msg)
			where T : MpcMsg
		{
			var sendDic = new Dictionary<int, T>();
			foreach (var pId in PartyIds)
				sendDic[pId] = msg;

			Send(sendDic as IDictionary<int, T>);
		}

		protected void Send<T>(T msg, HashSet<int> exceptPlayers)
			where T : MpcMsg
		{
			var sendDic = new Dictionary<int, T>();
			foreach (var pId in PartyIds)
			{
				if (!exceptPlayers.Contains(pId))
					sendDic[pId] = msg;
			}
			Send(sendDic as IDictionary<int, T>);
		}

		#endregion Send Methods

		#endregion Methods
	}
}