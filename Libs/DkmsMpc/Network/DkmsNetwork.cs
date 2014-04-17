using MpcLib.Common.FiniteField;
using MpcLib.DistributedSystem.QuorumSystem;
using MpcLib.SecretSharing;
using MpcLib.Simulation.Des;

namespace MpcLib.MpcProtocols.Dkms
{
	/// <summary>
	/// Represents a distributed network for DKMS'12 protocol.
	/// </summary>
	public class DkmsNetwork : QuorumSystem<DkmsParty>
	{
		public bool IsInitialized { get; protected set; }

		public Circuit Circuit { get; protected set; }

		public DkmsNetwork(Simulator s, int seed)
			: base(s, seed)
		{
		}

		public void Init(int numPlayers, int numQuorums, int numSlots, int quorumSize, QuorumBuildingMethod qbMethod, Circuit circuit, Zp[] inputs, int prime)
		{
			// create and init players
			var players = AddNewEntities(numPlayers);
			for (int i = 0; i < numPlayers; i++)
				players[i].Init(circuit, inputs[i], numSlots, EntityIds, numQuorums, quorumSize, qbMethod);

			IsInitialized = true;
		}
	}
}