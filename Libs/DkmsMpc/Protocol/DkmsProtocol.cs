using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using MpcLib.Common.StochasticUtils;
using MpcLib.SecretSharing;
using MpcLib.DistributedSystem;
using MpcLib.Common;
using MpcLib.Common.FiniteField;
using MpcLib.MpcProtocols.Bgw;

namespace MpcLib.MpcProtocols.Dkms
{
	using SessionCollector = SessionCollector<MpcSession>;	// alias

	/// <summary>
	/// Implements the MPC protocol of Dani, King, Movahedi, and Saia (DKMS'12).
	/// </summary>
	public class DkmsProtocol : AsyncProtocol		// TODO: Shouldn't this inherit MpcProtocol?
	{
		#region Fields and Properties

		public readonly Zp Input;
		public readonly int Prime;
		public readonly Circuit Circuit;
		public readonly int NumSlots;
		public override ProtocolIds Id { get { return ProtocolIds.DKMS; } }

		public IDictionary<int, int[]> QuorumsMap { set { quorumsMap = value; } }
		private IDictionary<int, int[]> quorumsMap;
		protected SessionCollector MpcSessions = new SessionCollector();

		/// <summary>
		/// Computation result.
		/// </summary>
		public Zp Result { get; protected set; }

		#endregion Fields and Properties

		public DkmsProtocol(Circuit circuit, AsyncParty p, ReadOnlyCollection<int> playerIds,
			Zp playerInput, int numSlots, StateKey stateKey)
			: base(p, playerIds, stateKey)
		{
			Circuit = circuit;
			Input = playerInput;
			Prime = playerInput.Prime;
			NumSlots = numSlots;
		}

		#region Methods

		public override void Run()
		{
			Debug.Assert(quorumsMap != null);
			Debug.Assert(Circuit.InputGates.Count == NumParties * NumSlots);

			// TODO: input must be encrypted with a random number and the random must be secret shared.
			// share my input in a random input gate (random slot) and share zero in others
			var randomSlot = StaticRandom.Next(0, NumSlots);

			for (int s = 0; s < NumSlots; s++)
			{
				// TODO: IMPORTANT: EntityId's are assumed to be continous integers with option base 0. Not other parts of the code have this assumption.
				var gate = Circuit.InputGates[Party.Id * NumSlots + s];
				var quorum = quorumsMap[gate.QuorumIndex];

				// in the byzantine case, we have to secret share the input among 'quorum' members but
				// here in the HBC case, we send the masked input to only one member of 'quorum' and randoms to other players.
				// These randoms form a global random, which is added to the input to mask it.
				Zp toSend;
				if (s == randomSlot)
					toSend = Input;
				else
					toSend = new Zp(Prime);		// send a zero

				int mask = 0;
				var minPlayer = quorum.Min();
				var stateKey = new DkmsKey(Stage.Input, gate.Id);

				foreach (var player in quorum)
				{
					if (player != minPlayer)
					{
						var rand = StaticRandom.Next(0, Prime);
						mask += rand;
						Send(Party.Id, player, new InputMsg(new Zp(Prime, rand), stateKey));
					}
				}
				Send(Party.Id, minPlayer, new InputMsg(Input + mask, stateKey));
			}
		}

		protected void ShareSecret(Zp secret, IList<int> players, DkmsKey key)
		{
			var shares = ShamirSharing.Share(secret, players.Count, players.Count - 1);

			var shareMsgs = new List<ShareMsg<Zp>>();
			foreach (var share in shares)
				shareMsgs.Add(new ShareMsg<Zp>(new Share<Zp>(share), key));

			Send(players, shareMsgs);
		}

		//public override void Receive(Message msg)
		//{
		//	Gate anchor, myGate;
		//	var ssmpcMsg = msg as ScalableMpcMessage;
		//	Debug.Assert(ssmpcMsg != null);

		//	switch (ssmpcMsg.StateKey.Stage)
		//	{
		//		case Stage.Input:
		//			var inputMsg = ssmpcMsg as InputMessage;

		//			// start a heavy-weight smpc with parent and sibling gates
		//			myGate = Circuit.FindGate(ssmpcMsg.StateKey.GateId);
		//			RunChildMpc(myGate.OutNodes[0], myGate, inputMsg.Data);
		//			break;

		//		case Stage.Mpc:
		//			MpcProtocol smpc;
		//			var mpcMsg = ssmpcMsg as MpcMessage;
		//			if (MpcSessions.ContainsKey(smpcMsg.StateKey))
		//				mpc = MpcSessions[mpcMsg.StateKey].Mpc;
		//			else
		//			{
		//				// I must be in the anchor gate
		//				Debug.Assert(mpcMsg.ToGateId == mpcMsg.AnchorId, "Synchronization exception: Why don't I have an session for this MPC?");

		//				// my child gate is asking me to participate in an MPC, so create an MPC protocol instance and join
		//				anchor = Circuit.FindGate(mpcMsg.AnchorId);
		//				mpc = RunAnchorMpc(anchor);
		//			}
		//			mpc.Receive(mpcMsg.InnerMessage);
		//			break;
		//	}
		//}

		/// <summary>
		/// Starts a heavy-weight SMPC instance for an SMPC child gate player.
		/// </summary>
		protected virtual BgwProtocol RunChildMpc(Gate anchor, Gate myGate, Zp myInput)
		{
			Debug.Assert(quorumsMap.ContainsKey(anchor.QuorumIndex));
			Debug.Assert(quorumsMap.ContainsKey(myGate.QuorumIndex));

			// find associated quorums
			var childGates = anchor.InNodes;
			var myGateId = myGate.Id;

			var virtualIds = new List<int>(quorumsMap[anchor.QuorumIndex].Select(p => (anchor.Id << 16) + p));		// TODO: IMPORTANT: THE ASSUMPTION HERE LIMITS ENTITY/GATE IDs TO 32768. TO INCREASE THIS LIMIT EITHER USE UINT/ULONG IDs OR CHANGE THIS CODE.
			foreach (var gate in childGates)
			{
				foreach (var playerId in quorumsMap[gate.QuorumIndex])
					virtualIds.Add((gate.Id << 16) + playerId);
			}
			var myVirtualId = (myGateId << 16) + Party.Id;

			// run the protocol and keep it in a the session state
			var key = new MpcKey(myGateId, anchor.Id);

			// Mahdi (3/25/14: Do we really need virtual ids?)
			throw new NotImplementedException();

			//var mpc = new BgwProtocol(anchor.MpcCircuit, virtualIds.AsReadOnly(), myVirtualId, myInput, OnMpcSend, key);

			//mpc.MpcFinish += new FinishHandler(OnMpcFinish);
			//MpcSessions.Collect(key, new MpcSession(mpc));
			//mpc.Run();
			//return mpc;
		}

		/// <summary>
		/// Starts a heavy-weight SMPC instance for an anchor gate player.
		/// </summary>
		protected virtual BgwProtocol RunAnchorMpc(Gate anchor)
		{
			// NOTE: since in the HBC case all players have the same output, just one of them (the guy with min id) SMPCs his output to improve performance.
			Debug.Assert(quorumsMap.ContainsKey(anchor.QuorumIndex));

			// find associated quorums
			var anchorChildren = anchor.InNodes;
			var myQuorum = quorumsMap[anchor.QuorumIndex];

			var virtualIds = new List<int>(myQuorum.Select(p => (anchor.Id << 16) + p));		// TODO: IMPORTANT: THE ASSUMPTION HERE LIMITS ENTITY/GATE IDs TO 32768. TO INCREASE THIS LIMIT EITHER USE UINT/ULONG IDs OR CHANGE THIS CODE.
			foreach (var childGate in anchorChildren)
			{
				foreach (var playerId in quorumsMap[childGate.QuorumIndex])
					virtualIds.Add((childGate.Id << 16) + playerId);
			}
			var myVirtualId = (anchor.Id << 16) + Party.Id;

			// if my id is the minimum in my quorum, then I should just SMPC with a zero, otherwise just SMPC with a random number (r_g).
			// save this random number in the session because it will be my input in next level's SMPC.
			BgwProtocol mpc;
			if (Party.Id == myQuorum.Min())
			{
				// run the protocol and keep it in a the session state
				var key = new MpcKey(anchor.Id, anchor.Id);

				// Mahdi (3/25/14: Do we really need virtual ids?)
				throw new NotImplementedException();

				//mpc = new BgwProtocol(anchor.MpcCircuit, virtualIds.AsReadOnly(), myVirtualId, new Zp(Prime), OnMpcSend, key);
				//mpc.MpcFinish += new FinishHandler(OnMpcFinish);
				//MpcSessions.Collect(key, new MpcSession(mpc));
				//mpc.Run();
			}
			else
			{
				// pick a number uniformly at random (r_g)
				// this random along with other players' randoms forms a global random in the quorum.
				var randomShare = new Zp(Prime, StaticRandom.Next(0, Prime));

				// run an SMPC protocol and keep it in a session state
				var key = new MpcKey(anchor.Id, anchor.Id);

				// Mahdi (3/25/14: Do we really need virtual ids?)
				throw new NotImplementedException();

				//mpc = new BgwProtocol(anchor.MpcCircuit, virtualIds.AsReadOnly(), myVirtualId, randomShare, OnMpcSend, key);
				//mpc.MpcFinish += new FinishHandler(OnMpcFinish);
				//MpcSessions.Collect(key, new MpcSession(mpc));
				//mpc.Run();
			}
			return mpc;
		}

		/// <summary>
		/// This method is invoked once a heavy-weight SMPC is finished.
		/// </summary>
		/// <param name="stateKey"></param>
		protected void OnMpcFinish(StateKey stateKey)
		{
			// forward either the result or r_g up if I am in the anchor otherwise do nothing
			var mpcKey = (MpcKey)stateKey;
			if (mpcKey.GateId == mpcKey.AnchorId)
			{
				var myGate = Circuit.FindGate(mpcKey.GateId);
				var myQuorum = quorumsMap[myGate.QuorumIndex];

				Debug.Assert(MpcSessions.ContainsKey(stateKey));
				var mpc = MpcSessions[stateKey].Mpc;

				if (myGate.OutNodes.Count == 0)

					// we are at the top gate! forward the result down to all players!
					throw new NotImplementedException();

				// forward the result up if I am the min player otherwise forward my share of r_g up
				if (Party.Id == myQuorum.Min())
					RunChildMpc(myGate.OutNodes[0], myGate, mpc.Result);
				else

					// note that my previous SMPC input is my share of r_g
					RunChildMpc(myGate.OutNodes[0], myGate, mpc.Input);
			}
		}

		protected void OnMpcSend(int fromId, int toId, Msg msg/*, StateKey stateKey*/)
		{
			// wrap the smpc message in a scalable smpc message
			//var key = stateKey as MpcKey;			// TODO: Can we make SendHandler generic in order to avoid these costy castings?
			// MAHDI 11/4/2013: Extract the key from msg.

			var origFromId = fromId & 65535;
			var fromGateId = fromId >> 16;

			var origToId = toId & 65535;
			var toGateId = toId >> 16;

			msg.SenderId = fromId;		// put virtual id in the embedded message
			//var wrappedMsg = new MpcMsg(toGateId, key.AnchorId, msg);
			//Send(origFromId, origToId, wrappedMsg);
		}

		#endregion Methods
	}
}