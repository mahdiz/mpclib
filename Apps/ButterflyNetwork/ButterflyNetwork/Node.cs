using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Unm.Common.StochasticUtils;

namespace Unm.DistributedSystem.ButterflyNetwork
{
	public class Node<T> : Entity
	{
		public HashSet<IDataItem<T>> DataItems { get; protected set; }
		public List<DownNodeConnection> TopConnections { get; private set; }
		public List<NodeConnection> InternalConnections { get; private set; }

		public delegate void SearchStartedHandler(Node<T> node);
		public delegate void SearchFinishedHandler(Node<T> node);
		public event SearchFinishedHandler SearchStarted;
		public event SearchFinishedHandler SearchFinished;

		protected int netHeight, netWidth, B;
		private Dictionary<int, List<IDataItem<T>>> ReceivedData = new Dictionary<int, List<IDataItem<T>>>();

		public Node()
		{
		}

		public override void Receive(Message msg)
		{
			base.Receive(msg);

			var bnMsg = msg as BnMessage;
			if (bnMsg == null)
				throw new Exception("Invalid message type.");

			switch (bnMsg.Type)
			{
				case BnMessageType.Setup:
					var setupMsg = bnMsg as SetupMessage;
					if (setupMsg == null)
						throw new Exception("Invalid message type.");

					Initialize(setupMsg);
					break;

				case BnMessageType.Store:
					// TODO: Check duplicate data items
					DataItems.Add(bnMsg.Data as IDataItem<T>);
					if (DataItems == null)
						throw new Exception("Invalid message payload.");
					break;

				case BnMessageType.Query:
					var query = bnMsg.Data as IDataItem<T>;
					if (query == null)
						throw new Exception("Invalid message payload.");

					Search(query);
					break;

				case BnMessageType.Data:
					var dataMsg = bnMsg as DataMessage;
					if (dataMsg == null)
						throw new Exception("Invalid message type.");

					if (dataMsg.Direction == DataMessageDir.Down)
					{
						var ddMsg = (DataDownMessage)dataMsg;
						if (ddMsg.NextSupernode >= netWidth * (netHeight - 1))
						{
							if (ddMsg.NextSupernode != ddMsg.DestinationSupernode)
								throw new Exception("Routing error: unexpected message received at bottom.");

							// reached a bottom supernode; send my data item up along the 
							// same path as the query was sent downwards if the titles match.
							query = dataMsg.Data as IDataItem<T>;
							if (query == null)
								throw new Exception("Invalid query message received in bottom node " + Id);

							var dataItem = DataItems.FirstOrDefault(di => di.Title == query.Title);
							if (dataItem == null)
								throw new Exception("Data item not found in node " + Id);
							else
							{
								var upMsg = new DataMessage(DataMessageDir.Up,
									dataMsg.BackPath, dataItem);
								ForwardDataUp(upMsg);
							}
						}
						else ForwardDataDown(ddMsg);
					}
					else
					{
						var backConn = dataMsg.BackPath.Pop();
						if (dataMsg.BackPath.Count == 0)
						{
							if (SearchFinished != null)
								SearchFinished(this);

							Console.WriteLine("Search result received in node " +
								Id + " from node " + dataMsg.SenderId + ". Result title is '" +
								((IDataItem<System.Drawing.Image>)dataMsg.Data).Title + "'");
						}
						else
						{
							if (!ReceivedData.ContainsKey(backConn.FromSupernode))
							{
								ReceivedData[backConn.FromSupernode] = new List<IDataItem<T>>();
								ForwardDataUp(dataMsg);
							}
							ReceivedData[backConn.FromSupernode].Add((IDataItem<T>)dataMsg.Data);
						}
					}
					break;

				default:
					throw new NotSupportedException();
			}
		}

		private void Initialize(SetupMessage msg)
		{
			B = msg.B;
			netHeight = msg.NetworkHeight;
			netWidth = msg.NetworkWidth;
			DataItems = new HashSet<IDataItem<T>>();
			var connections = msg.NodeConnections as List<NodeConnection>;

			if (connections == null)
				throw new Exception("Invalid payload type!");

			TopConnections = new List<DownNodeConnection>();
			InternalConnections = new List<NodeConnection>();

			foreach (var connection in connections)
			{
				if (connection.FromSupernode < 0)
				{
					var downConnection = connection as DownNodeConnection;
					if (downConnection == null)
						throw new Exception("Invalid node top connection.");
					TopConnections.Add(downConnection);
				}
				else InternalConnections.Add(connection);
			}
		}

		public void Search(IDataItem<T> query)
		{
			if (SearchStarted != null)
				SearchStarted(this);

			var bottomSupernodeIndexes = HashBottomSupernodes(query, B);

			foreach (var topConnection in TopConnections)
			{
				foreach (var topNodeId in topConnection.NodeIds)
				{
					foreach (var bottomSupernodeIndex in bottomSupernodeIndexes)
					{
						// follow the path from topSuperNode to bottomSuperNode
						var msg = new DataDownMessage(
							bottomSupernodeIndex, 
							topConnection.ToSupernode, 
							query);

						msg.BackPath.Push(new BackConnection() { NodeId = Id, FromSupernode = topConnection.ToSupernode });
						Send(topNodeId, msg, 100);
					}
				}
			}
		}

		private int[] HashBottomSupernodes(IDataItem<T> dataItem, int B)
		{
			var indexArray = new int[B];
			var randomHash = new Random(dataItem.GetHashCode());
			for (int i = 0; i < B; i++)
			{
				// TODO: What about duplicate bottom supernodes?
				var hashIndex = randomHash.Next(0, netWidth);
				indexArray[i] = (netHeight - 1) * netWidth + hashIndex;
			}
			return indexArray;
		}

		private void ForwardDataDown(DataDownMessage msg)
		{
			// forward the message down based on the destination supernode id
			// find the appropriate direction (left or right?) based on ddMsg.DestinationSupernode
			// find the corresponding supernode id
			// forward the message to all connected nodes in the supernode

			var nextHops = (from NodeConnection nc in InternalConnections
							where nc.FromSupernode == msg.NextSupernode &&
								  nc.Direction == NodeConnectionDirection.Down
							select nc).ToArray();

			if (nextHops.Length != 2)
				throw new Exception("Invalid node connection information: node " + Id);

			NodeConnection leftConnection, rightConnection;
			if (nextHops[0].ToSupernode < nextHops[1].ToSupernode)
			{
				leftConnection = nextHops[0];
				rightConnection = nextHops[1];
			}
			else
			{
				leftConnection = nextHops[1];
				rightConnection = nextHops[0];
			}

			NodeConnection nextHop;
			var dir = (msg.DestinationSupernode % netWidth) >> (netHeight - (msg.NextSupernode / netWidth) - 2);
			if (dir % 2 == 0)
				nextHop = leftConnection;
			else
				nextHop = rightConnection;

			var nextHopDown = nextHop as DownNodeConnection;
			if (nextHopDown == null)
				throw new Exception("Invalid node connection information: node " + Id);

			msg.NextSupernode = nextHopDown.ToSupernode;
			msg.BackPath.Push(new BackConnection() { NodeId = Id, FromSupernode = nextHopDown.ToSupernode });

			foreach (var nextHopNodeId in nextHopDown.NodeIds)
			{
				Send(nextHopNodeId, msg, 100);
			}
		}

		private void ForwardDataUp(DataMessage msg)
		{
			var backConnection = msg.BackPath.Peek();
			Send(backConnection.NodeId, msg, 100);
		}

		public int GetSize()
		{
			int s = 0;
			foreach (var dataItem in DataItems)
				s += dataItem.GetSize();

			foreach (var c in TopConnections)
				s += c.GetSize();

			foreach (var c in InternalConnections)
				s += c.GetSize();

			s += 3 * sizeof(int);
			// what about received data?

			return s;
		}

		public override void Run()
		{
			throw new NotImplementedException();
		}
	}
}
