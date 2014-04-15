using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MpcLib.DistributedSystem.Mpc.Dkms
{
	/// <summary>
	/// Implements a fast directed-acyclic graph of gates. The circuit object contains all
	/// the information a player needs to know about the circuit except the members (players) of the gates.
	/// </summary>
	public class Circuit
	{
		public readonly ReadOnlyCollection<Gate> InputGates;
		private readonly IDictionary<int, Gate> gatesMap;

		public Circuit(ReadOnlyCollection<Gate> inputGates, IEnumerable<Gate> otherGates)
		{
			InputGates = inputGates;

			gatesMap = new Dictionary<int, Gate>();
			foreach (var g in inputGates)
				gatesMap.Add(g.Id, g);

			foreach (var g in otherGates)
				gatesMap.Add(g.Id, g);
		}

		/// <summary>
		/// O(1) operation.
		/// </summary>
		public Gate FindGate(int gateId)
		{
			return gatesMap[gateId];
		}
	}
}