using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unm.Common.StochasticUtils;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Unm.DistributedSystem.ButterflyNetwork
{
	public class Dealer : Entity
	{
		private Supernode[,] supernodes;
		private double beta;
		private int n, B, T, netWidth, netHeight;
		private Dictionary<int, SetupMessageData> nodeConnections;
		private readonly RandomUtils randUtils;

		public bool IsInitialized { get; private set; }
		public delegate void InitializationHandler(Supernode[,] e);
		public event InitializationHandler InitFinished;

		public Dealer()
		{
			randUtils = new RandomUtils();
		}

		public Dealer(CryptoRandom randGen)
		{
			randUtils = new RandomUtils(randGen);
		}

		/// <summary>
		/// Builds the butterfly network.
		/// </summary>
		/// <param name="n">Total number of nodes.</param>
		/// <param name="C">Every node chooses at random C top supernodes, C bottom supernodes, and Clog(n) middle supernodes to which each the node belongs.</param>
		/// <param name="D">The degree of the random expander graph constructed between two supernodes.</param>
		/// <param name="alpha">An expander graph maybe constructed between two supernodes if both are of size at least alpha*C*ln(n) and no more than beta*C*ln(n).</param>
		/// <param name="beta">An expander graph maybe constructed between two supernodes if both are of size at least alpha*C*ln(n) and no more than beta*C*ln(n).</param>
		internal void Initialize(IEnumerable<int> nodeIds, int n, int B, int C, int D, int T, double alpha, double beta)
		{
			IsInitialized = false;
			this.n = n;
			this.B = B;
			this.T = T;
			this.beta = beta;
			var netWidthTemp = n / Math.Log(n, 2);

			// round this to the nearest greater power of two
			netWidth = (int)Math.Pow(2.0, Math.Ceiling(Math.Log(netWidthTemp, 2)));
			netHeight = (int)Math.Log(netWidth, 2) + 1;

			supernodes = new Supernode[netHeight, netWidth];
			for (int i = 0; i < netHeight; i++)
			{
				for (int j = 0; j < netWidth; j++)
				{
					supernodes[i, j] = new Supernode(i * netWidth + j, randUtils);
					supernodes[i, j].AddNodeConnection += 
						new Supernode.AddNodeConnectionHandler(CollectNodeConnections);
				}
			}

			// construct the network
			nodeConnections = new Dictionary<int, SetupMessageData>();
			RegisterNodes(nodeIds, supernodes, C);
			MakeConnections(supernodes, n, C, D, alpha, beta);

			// each node should also point to T top supernodes
			foreach (var nodeId in nodeIds)
			{
				var topSupernodeIds = randUtils.GetRandomPerm(0, netWidth, T);
				foreach (var topSupernodeId in topSupernodeIds)
				{
					var topSupernode = supernodes[0, topSupernodeId];
					// -1 means the node is directly connected to a supernode 
					// without belonging to any supernode
					CollectNodeConnections(
						new KeyValuePair<int, NodeConnection>(nodeId, 
							new DownNodeConnection(-1, topSupernodeId, topSupernode.NodeIds)));

					foreach (var topNodeId in topSupernode.NodeIds)
					{
						// -1 means topNode is connected to an individual node (nodeId) that 
						// doesn't belong to any supernode
						CollectNodeConnections(
							new KeyValuePair<int, NodeConnection>(topNodeId,
								new UpNodeConnection(topSupernode.Id, -1, nodeId)));
					}
				}
			}

			//// send the network setup information to nodes
			//foreach (var nodeId in nodeIds)
			//{
			//	var msg = new SetupMessage(netHeight, netWidth, B, nodeConnections[nodeId]);
			//	Send(nodeId, msg, 100);
			//}

			IsInitialized = true;
			if (InitFinished != null)
				InitFinished(supernodes);
		}

		public void StoreDataItems<T>(List<T> dataItems) where T : ITransmittable
		{
			if (!IsInitialized)
				throw new Exception("The dealer has not been initialized yet.");

			var supernodesLists = HashBottomSuperNodes(dataItems, B);
			for (int i = 0; i < supernodesLists.Count; i++)
			{
				var dataItem = dataItems[i];
				foreach (var supernode in supernodesLists[i])
				{
					foreach (var nodeId in supernode.NodeIds)
					{
						var msg = new BnMessage(BnMessageType.Store, dataItem);
						Send(nodeId, msg, 100);
					}
				}
			}
		}

		public void Search<T>(T query, int nodeId) where T : ITransmittable
		{
			var queryMsg = new BnMessage(BnMessageType.Query, query);
			Send(nodeId, queryMsg, 100);
		}

		private List<Supernode[]> HashBottomSuperNodes<T>(List<T> dataItems, int B)
		{
			var outputList = new List<Supernode[]>();
			var lastRowIndex = netHeight - 1;

			foreach (var dataItem in dataItems)
			{
				var hashSuperNodes = new Supernode[B];
				var randomHash = new Random(dataItem.GetHashCode());
				for (int i = 0; i < B; i++)
				{
					hashSuperNodes[i] = supernodes[lastRowIndex, randomHash.Next(0, netWidth)];
				}
				outputList.Add(hashSuperNodes);
			}
			return outputList;
		}

		private void CollectNodeConnections(KeyValuePair<int, NodeConnection> e)
		{
			if (nodeConnections.ContainsKey(e.Key))
				nodeConnections[e.Key].Add(e.Value);
			else
				nodeConnections.Add(e.Key, new SetupMessageData { e.Value });
		}

		/// <summary>
		/// For each node, chooses at random C top supernodes, C bottom supernodes, and Clog(n) 
		/// middle supernodes to which each the node belongs.
		/// </summary>
		private void RegisterNodes(IEnumerable<int> nodeIds, Supernode[,] supernodes, int C)
		{
			var clogn = (int) (C * Math.Log(n, 2));

			foreach (var nodeId in nodeIds)
			{
				var topColIndexes = randUtils.GetRandomPerm(0, netWidth, C);
				var botColIndexes = randUtils.GetRandomPerm(0, netWidth, C);

				for (int i = 0; i < C; i++)
				{
					supernodes[0, topColIndexes[i]].AddNode(nodeId);
					supernodes[netHeight - 1, botColIndexes[i]].AddNode(nodeId);
				}

				var randIndexes = randUtils.GetRandomPerm(netWidth, netWidth * (netHeight - 1), clogn);
				for (int i = 0; i < clogn; i++)
					supernodes[(randIndexes[i] / netWidth),
						       (randIndexes[i] % netWidth)].AddNode(nodeId);
			}
		}

		private void MakeConnections(Supernode[,] superNodes, int n, int C, int D, double alpha, double beta)
		{
			if (Math.Log(netWidth, 2) != Math.Truncate(Math.Log(netWidth, 2)))
				throw new Exception("Number of rows must be a power of two!");

			var minLimit = (int)Math.Round(alpha * C * Math.Log(n, Math.E));
			var maxLimit = (int)Math.Round(beta * C * Math.Log(n, Math.E));

			var firstRow = new Supernode[netWidth];
			for (int i = 0; i < netWidth; i++)
				firstRow[i] = superNodes[0, i];

			MakeConnections(superNodes, firstRow, 0, 1, false, minLimit, maxLimit, D);
		}

		private void MakeConnections(
			Supernode[,] superNodes,
			Supernode[] nodes,
			int columnOffset,
			int nextRow,
			bool toFirstHalf,
			int minLimit,
			int maxLimit,
			int D)
		{
			if (nextRow >= netHeight)
				return;

			var halfLen = nodes.Length / 2;
			var nextRowFirstHalf = new Supernode[halfLen];
			var nextRowSecondHalf = new Supernode[halfLen];

			for (int i = 0; i < halfLen; i++)
			{
				nextRowFirstHalf[i] = superNodes[nextRow, columnOffset + i];
				nextRowSecondHalf[i] = superNodes[nextRow, halfLen + columnOffset + i];
			}

			for (int i = 0; i < halfLen; i++)
			{
				if (CheckSuperNodeSize(nodes[i], nextRowSecondHalf[i], minLimit, maxLimit))
					nodes[i].ConnectRight(nextRowSecondHalf[i], D);

				if (CheckSuperNodeSize(nodes[halfLen + i], nextRowFirstHalf[i], minLimit, maxLimit))
					nodes[halfLen + i].ConnectLeft(nextRowFirstHalf[i], D);
			}

			for (int i = 0; i < nodes.Length; i++)
			{
				if (CheckSuperNodeSize(nodes[i], superNodes[nextRow, columnOffset + i], minLimit, maxLimit))
				{
					if (nodes[i].LowerLeft == null)
						nodes[i].ConnectLeft(superNodes[nextRow, columnOffset + i], D);
					else
						nodes[i].ConnectRight(superNodes[nextRow, columnOffset + i], D);
				}
			}

			MakeConnections(superNodes, nextRowFirstHalf, columnOffset,
				nextRow + 1, false, minLimit, maxLimit, D);

			MakeConnections(superNodes, nextRowSecondHalf, columnOffset + halfLen,
				nextRow + 1, true, minLimit, maxLimit, D);
		}

		private bool CheckSuperNodeSize(Supernode node1, Supernode node2, int min, int max)
		{
			return (node1.Size >= min && node1.Size <= max &&
					node2.Size >= min && node2.Size <= max);
		}

		public override void Run()
		{
			throw new NotImplementedException();
		}
	}
}
