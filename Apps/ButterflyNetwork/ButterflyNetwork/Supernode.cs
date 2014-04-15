using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unm.Common.StochasticUtils;
using System.Runtime.Serialization;

namespace Unm.DistributedSystem.ButterflyNetwork
{
    public class Supernode
    {
		public int Id { get; private set; }
		public Supernode LowerLeft { get; private set; }
		public Supernode LowerRight { get; private set; }
		public int Size { get { return NodeIds.Count; } }
		public Supernode[] LowerNodes
		{
			get
			{
				if (LowerLeft == null)
				{
					if (LowerRight == null)
						return new Supernode[0];
					else
						return new Supernode[] { LowerRight };
				}
				else
				{
					if (LowerRight == null)
						return new Supernode[] { LowerLeft };
					else
						return new Supernode[] { LowerLeft, LowerRight };
				}
			}
		}

		public List<int> NodeIds { get; private set; }
		public delegate void AddNodeConnectionHandler(KeyValuePair<int, NodeConnection> e);
		public event AddNodeConnectionHandler AddNodeConnection;
		private RandomUtils randUtils;

		public Supernode(int id, RandomUtils randUtils)
		{
			Id = id;
			NodeIds = new List<int>();
			this.randUtils = randUtils;
		}

		public void AddNode(int id)
		{
			if (NodeIds.Contains(id))
				throw new Exception("Node is already added.");
			NodeIds.Add(id);
		}

		/// <param name="D">The degree of the constant-degree expander graph.</param>
		public void ConnectLeft(Supernode toNode, int D)
		{
			LowerLeft = toNode;
			MakeExpanderGraph(toNode, D);
		}

		/// <summary>
		/// Connects the supernode to another supernode and constructs a random 
		/// constant-degree expander graph between the two sets of nodes.
		/// </summary>
		/// <param name="toNode"></param>
		/// <param name="D">The degree of the constant-degree expander graph.</param>
		public void ConnectRight(Supernode toNode, int D)
		{
			LowerRight = toNode;
			MakeExpanderGraph(toNode, D);
		}

		private void MakeExpanderGraph(Supernode toSupernode, int D)
		{
			// construct the expander graph
			foreach (var nodeId in NodeIds)
			{
				var toNodeIds = new List<int>();
				foreach (var toNodeId in randUtils.PickRandomElements(toSupernode.NodeIds, D))
					toNodeIds.Add(toNodeId);

				if (AddNodeConnection != null)
				{
					AddNodeConnection(new KeyValuePair<int, NodeConnection>(nodeId,
							new DownNodeConnection(this.Id, toSupernode.Id, toNodeIds)));

					foreach (var toNodeId in toNodeIds)
					{
						AddNodeConnection(new KeyValuePair<int, NodeConnection>(toNodeId,
								new UpNodeConnection(toSupernode.Id, this.Id, nodeId)));
					}
				}
			}
		}
	}
}
