using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using MpcLib.Common;
using MpcLib.Common.FiniteField;
using MpcLib.Common.FiniteField.Circuits;
using MpcLib.DistributedSystem;
using MpcLib.SecretSharing;
using MpcLib.SecretSharing.eVSS;

namespace MpcLib.MpcProtocols.Crypto
{
	/// <summary>
	/// Implements the MPC protocol of Zamani-Movahedi-Saia 2014 with eVSS.
	/// </summary>
	public class CryptoMpc : MpcProtocol<BigZp>
	{
		#region Fields

		protected readonly BigCircuit Circuit;

		/// <summary>
		/// MPC field modulus. Party's input is an element of this field.
		/// </summary>
		protected readonly BigInteger Prime;
		protected readonly int ShamirPolyDegree;
		protected IList<BigZp> Shares;
		protected int Seed;
		public override ProtocolIds Id { get { return ProtocolIds.ZMS; } }

		#endregion Fields

		public CryptoMpc(Entity e, BigCircuit circuit, ReadOnlyCollection<int> pIds,
			BigZp input, StateKey stateKey, int seed)
			: base(e, pIds, input, stateKey)
		{
			Debug.Assert(circuit.InputCount == pIds.Count);
			Circuit = circuit;
			Prime = input.Prime;
			Seed = seed;

			// k: min number of shares required to reconstruct the secret
			// t: max number of bad players
			// d: degree of polynomial in Shamir's scheme
			// we have d=k-1 because with k points we can interpolate 
			// a polynomial of degree at most k-1.
			// we also have k>t to prevent bad parties from sharing a fake secret.
			// hence we have d=t. In sync model, d=t=n/3.
			ShamirPolyDegree = (int)Math.Ceiling(NumParties / 3.0);
		}

		#region Methods
		
		public override void Run()
		{
			var evss = new eVSS(Input, Entity, EntityIds, ShamirPolyDegree, StateKey);

			evss.Setup(Seed);
			Console.WriteLine("Party " + Entity.Id + " eVSS setup finished.");
			evss.Run();
		}

		protected void Compute()
		{
			// evaluate the circuit gate by gate
			foreach (var gate in Circuit.Gates)
				ComputeGate(gate, Shares);

			// publish the circuit output
			Broadcast(new ShareMsg<BigZp>(new Share<BigZp>(Circuit.Output), Stage.ResultReceive));

			OnReceive((int)Stage.ResultReceive,
				delegate(List<ShareMsg<BigZp>> resultMsgs)
				{
					var zpList = new List<BigZp>();
					foreach (var resultMsg in resultMsgs.OrderBy(s => s.SenderId))
						zpList.Add(resultMsg.Share.Value);

					Result = BigShamirSharing.Recombine(zpList, ShamirPolyDegree, Prime);
				});
		}

		/// <summary>
		/// Inputs are all the inputs to the circuit. Gate should know to choose the inputs it needs.
		/// </summary>
		private void ComputeGate(BigGate gate, IList<BigZp> shares)
		{
			var values = new List<BigZp>();
			foreach (var wire in gate.InputWires)
			{
				if (wire.IsInput)
				{
					if (shares.Count <= wire.InputIndex)
						throw new Exception("Input " + wire.InputIndex + " is expected - not found in the list given");
					values.Add(shares[wire.InputIndex]);
				}
				else
				{
					Debug.Assert(wire.SourceGate != null && wire.SourceGate.IsOutputReady);
					values.Add(wire.ConstValue != null ? wire.ConstValue : wire.SourceGate.OutputValue);
				}
			}
			var result = new BigZp(values[0]);
			values.RemoveAt(0);

			foreach (var value in values)
			{
				var currValue = value;
				if (gate.Operation == Operation.Div)
					throw new NotImplementedException();
				
				result.Calculate(currValue, gate.Operation == Operation.Div ? Operation.Mul : gate.Operation);
			}
			gate.OutputValue = result;
		}

		#endregion Methods
	}
}