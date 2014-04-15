using System.Diagnostics;
using MpcLib.Common.BasicDataStructures.Dag;
using MpcLib.Common.FiniteField;

namespace MpcLib.DistributedSystem.Mpc.Dkms
{
	public class Gate : Node<Gate>
	{
		/// <summary>
		/// Unique gate ID.
		/// </summary>
		public readonly int Id;

		public readonly GateType Type;

		/// <summary>
		/// The ID of the quorum that the gate corresponds to.
		/// </summary>
		public readonly int QuorumIndex;

		/// <summary>
		/// Gate's internal circuit, which is the heavy-weight SMPC circuit.
		/// To perform heavy-weight SMPC between gates of a subtree in the circuit,
		/// the heavy-weight circuit is the one stored in the anchor (root) gate.
		/// This field remains 'null' for input gates.
		/// </summary>
		public readonly Common.FiniteField.Circuits.Circuit MpcCircuit;

		private static int idGen;

		public Gate(GateType type, Zp quorumIndex, Common.FiniteField.Circuits.Circuit mpcCircuit)
		{
			Debug.Assert(type == GateType.Input || mpcCircuit != null, "Only input gates do not have MPC circuit.");
			Id = idGen++;
			Type = type;
			QuorumIndex = quorumIndex.Value;
			MpcCircuit = mpcCircuit;
		}

		public Gate(Zp quorumIndex)
			: this(GateType.Input, quorumIndex, null)
		{
		}

		public override int GetHashCode()
		{
			return Id;
		}

		public override bool Equals(object obj)
		{
			var gate = obj as Gate;
			Debug.Assert(gate != null);
			return gate.Id == Id;
		}

		public override string ToString()
		{
			var inputs = "{";
			foreach (var inNode in InNodes)
				inputs += inNode.Id + ",";
			inputs = (inputs.Length > 1 ? inputs.Remove(inputs.Length - 1) : inputs) + "}";

			var outputs = "{";
			foreach (var outNode in OutNodes)
				outputs += outNode.Id + ",";
			outputs = (outputs.Length > 1 ? outputs.Remove(outputs.Length - 1) : outputs) + "}";

			return Type + " gate" + ", id=" + Id.ToString() + ", InGates=" + inputs + ", OutGates=" + outputs;
		}

		///// <summary>
		///// Given the input arguments, computes the output of the gate.
		///// </summary>
		//public abstract T Compute(params T[] args);

		///// <summary>
		///// Unmasks the input through the heavy-weight SMPC.
		///// </summary>
		//protected T Unmask(T a)
		//{
		//    throw new NotImplementedException();
		//}

		///// <summary>
		///// Remasks the input through the heavy-weight SMPC.
		///// </summary>
		//protected T Remask(T b)
		//{
		//    throw new NotImplementedException();
		//}

		///// <summary>
		///// Compares two numbers through the heavy-weight SMPC.
		///// </summary>
		///// <returns>0 if a=b, 1 if a>b, -1 if a<b </returns>
		//protected int Compare(T a, T b)
		//{
		//    throw new NotImplementedException();
		//}

		///// <summary>
		///// Add two numbers through the heavy-weight SMPC.
		///// </summary>
		///// <returns></returns>
		//protected T Add(T a, T b)
		//{
		//    throw new NotImplementedException();
		//}

		///// <summary>
		///// Multiplies two numbers through the heavy-weight SMPC.
		///// </summary>
		///// <returns></returns>
		//protected T Multiply(T a, T b)
		//{
		//    throw new NotImplementedException();
		//}
	}
}