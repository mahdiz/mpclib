using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unm.DistributedSystem.ButterflyNetwork
{
	public enum NodeConnectionDirection
	{
		Up,
		Down
	}

	public abstract class NodeConnection : ITransmittable
	{
		public NodeConnectionDirection Direction { get; private set; }	// TODO: Can we remove this discriminator and just use "obj is DownNodeConnection" instead. Doesn't this decrease performance?
		public int FromSupernode { get; private set; }
		public int ToSupernode { get; private set; }

		public NodeConnection(NodeConnectionDirection direction, int fromSupernode, int toSupernode)
		{
			Direction = direction;
			FromSupernode = fromSupernode;
			ToSupernode = toSupernode;
		}

		public virtual int GetSize()
		{
			return 2 * sizeof(int) + sizeof(NodeConnectionDirection);
		}

		public abstract object Clone();

		public override string ToString()
		{
			return "FromSupernode=" + FromSupernode + ", ToSupernode=" + ToSupernode + ", Direction=" + Direction;
		}
	}

	public class DownNodeConnection : NodeConnection
	{
		public List<int> NodeIds { get; private set; }

		public DownNodeConnection(int fromSupernode, int toSupernode, List<int> nodeIds)
			: base(NodeConnectionDirection.Down, fromSupernode, toSupernode)
		{
			NodeIds = new List<int>(nodeIds.Count);
			NodeIds.AddRange(nodeIds);
		}

		public override int GetSize()
		{
			return base.GetSize() + (sizeof(int) * NodeIds.Count);
		}

		public override object Clone()
		{
			var listCopy = new List<int>();
			listCopy.AddRange(NodeIds);
			return new DownNodeConnection(FromSupernode, ToSupernode, listCopy);
		}
	}

	public class UpNodeConnection : NodeConnection
	{
		public int NodeId { get; private set; }

		public UpNodeConnection(int fromSupernode, int toSupernode, int nodeId)
			: base(NodeConnectionDirection.Up, fromSupernode, toSupernode)
		{
			NodeId = nodeId;
		}

		public override int GetSize()
		{
			return base.GetSize() + sizeof(int);
		}

		public override object Clone()
		{
			return new UpNodeConnection(FromSupernode, ToSupernode, NodeId);
		}
	}
}
