using System;
using System.Collections.Generic;
using System.Linq;
using Unm.Common.StochasticUtils;


namespace Unm.DistributedSystem.ButterflyNetwork
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T">The generic type of data items.</typeparam>
	public class ButterflyNetwork<T> : DistributedSystem<Node<T>>
    {
		//private Dealer dealer;
		protected CryptoRandom rand;

		public delegate void InitializationHandler(Supernode[,] supernodes);
		public delegate void SearchStartedHandler(double simTime, Node<T> nodeId);
		public delegate void SearchFinishedHandler(double simTime, Node<T> nodeId);
		public event InitializationHandler InitializationFinished;
		public event SearchFinishedHandler SearchStarted;
		public event SearchFinishedHandler SearchFinished;

		public int Seed
		{
			get
			{
				return rand.Seed;
			}
		}
		public int SentSize { get; private set; }

		public ButterflyNetwork()
		{
			rand = new CryptoRandom();
		}

		public ButterflyNetwork(int seed)
		{
			rand = new CryptoRandom(seed);
		}

		public void InitializeNetwork(int n, int B, int C, int D, int T, double alpha, double beta)
		{
			System.Console.WriteLine("Butterfly network initializing with seed " + Seed);
			Supernode[,] supernodes = null;

			// create n nodes
			for (int i = 0; i < n; i++)
			{
				var entity = AddNewEntity();
				entity.SearchStarted += new Node<T>.SearchFinishedHandler(NodeSearchStarted);
				entity.SearchFinished += new Node<T>.SearchFinishedHandler(NodeSearchFinished);
			}

			// create a dealer entity and initialize the network
			dealer = CreateEntity(new Dealer(rand));
			dealer.InitFinished += delegate(Supernode[,] e)
			{
				supernodes = e;
			};
			dealer.Initialize(EntityIds, n, B, C, D, T, alpha, beta);

			if (InitializationFinished != null)
			{
				InitializationFinished(supernodes);
			}
		}

		private void NodeSearchFinished(Node<T> nodeId)
		{
			if (SearchFinished != null)
				SearchFinished(SimulationTime, nodeId);
		}

		private void NodeSearchStarted(Node<T> nodeId)
		{
			if (SearchStarted != null)
				SearchStarted(SimulationTime, nodeId);
		}

		public void StoreDataItems<S>(List<S> dataItems) where S : IDataItem<T>
		{
			if (dealer == null || !dealer.IsInitialized)
				throw new Exception("The dealer has not been initialized yet.");

			if (dataItems == null)
				throw new ArgumentNullException("dataItems");

			dealer.StoreDataItems(dataItems);
		}

		public void Search(IDataItem<T> query, int nodeId)
		{
			dealer.Search(query, nodeId);
		}

		//protected override void EntityMessageSent(Message msg)
		//{
		//	SentSize += msg.GetSize();
		//}

/*		public void HashData(List<IDataItem<D>> dataItems)
		{
			if (dataItems.Count != EntityCount)
				throw new InvalidOperationException();

			// hash data items to B random bottom supernodes and store each data item
 			// in all component nodes of all the supernodes to which it has been hashed.
			List<int[]> indexArrays;
			HashBottomSuperNodes(dataItems, B, true, out indexArrays);

			// delete bottom supernodes, which have more than beta*B*ln(n) data items.
			// the corresponding reference to such supernodes in the supernode matrix are nullified.
			var badSuperNodes = GetBottomSuperNodes().Where(
				s => s.DataItemsCount() > beta * B * Math.Log(EntityCount, Math.E));

			DeleteFromBottomSuperNodes(badSuperNodes);
		}

		public IDataItem<D> Search(IDataItem<D> query)
		{
			// TODO: WHY THE QUERY IS A DATAITEM? MAYBE BETTER TO BE A STRING.
			var networkHeight = SuperNodes.GetLength(0);
			var networkWidth = SuperNodes.GetLength(1);

			// take the hash of the data item
			List<int[]> hashIndexArrays;
			HashBottomSuperNodes(new List<IDataItem<D>>() { query }, B, false, out hashIndexArrays);
			var bottomSuperNodeIndexes = hashIndexArrays[0];

			// the node should point to T top supernodes
			var topSuperNodeIndexes = RandomUtils.GetRandomPermutations(0, networkWidth, T);

			var success = false;
			foreach (var topSuperNodeIndex in topSuperNodeIndexes)
			{
				var topSuperNode = SuperNodes[0, topSuperNodeIndex];

				foreach (var bottomSuperNodeIndex in bottomSuperNodeIndexes)
				{
					// follow the path from topSuperNode to bottomSuperNode
					var bottomSuperNode = SuperNodes[networkHeight - 1, bottomSuperNodeIndex];
					topSuperNode.Send(query, bottomSuperNode, networkWidth, TransmitDirection.Down);
				}
			}
			return null;
		}

		private List<Supernode[]> HashBottomSuperNodes(
			IEnumerable<IDataItem<D>> dataItems, int B, bool storeDataItems, out List<int[]> indexArrays)
		{
			// TODO: IMPLEMENT WITH A NICER SET OF ARGUMENTS!

			indexArrays = new List<int[]>();
			var outputList = new List<Supernode[]>();
			var bottomSuperNodes = GetBottomSuperNodes();
			var count = bottomSuperNodes.Count();

			foreach (var dataItem in dataItems)
			{
				var indexArray = new int[B];
				var hashSuperNodes = new Supernode[B];
				var randomHash = new Random(dataItem.GetHashCode());
				for (int i = 0; i < B; i++)
				{
					var hashIndex = randomHash.Next(0, count);
					indexArray[i] = hashIndex;
					hashSuperNodes[i] = bottomSuperNodes[hashIndex];
					if (storeDataItems) hashSuperNodes[i].StoreDataItem(dataItem.Content);
				}
				outputList.Add(hashSuperNodes);
				indexArrays.Add(indexArray);
			}
			return outputList;
		}

		public Supernode[] GetTopSuperNodes()
		{
			var colsCount = SuperNodes.GetLength(1);
			var topNodes = new Supernode[colsCount];
			for (int i = 0; i < colsCount; i++)
				topNodes[i] = SuperNodes[0, i];

			return topNodes;
		}

		public Supernode[] GetBottomSuperNodes()
		{
			var lastRowIndex = SuperNodes.GetLength(0) - 1;
			var colsCount = SuperNodes.GetLength(1);
			var bottomNodes = new Supernode[colsCount];
			for (int i = 0; i < colsCount; i++)
				bottomNodes[i] = SuperNodes[lastRowIndex, i];

			return bottomNodes;
		}

		private void DeleteFromBottomSuperNodes(IEnumerable<Supernode> nodes)
		{
			var lastRowIndex = SuperNodes.GetLength(0) - 1;
			var colsCount = SuperNodes.GetLength(1);
			var bottomNodes = new Supernode[colsCount];
			for (int i = 0; i < colsCount; i++)
			{
				if (nodes.Contains(SuperNodes[lastRowIndex, i]))
					SuperNodes[lastRowIndex, i] = null;
			}
		}

		//private void RegisterQuorums(Supernode[] quorums)
		//{
		//    int degree = (int)Math.Ceiling((double)Nodes.Length / quorums.Length);

		//    foreach (var q in quorums)
		//    {
		//        for (int i = 0; i < degree; i++)
		//        {
		//            var randIndex = randGen.Next(0, Nodes.Length);
		//            Nodes[randIndex].SetQuorum(q, true);
		//        }
		//    }
		//}	*/
	}
}
