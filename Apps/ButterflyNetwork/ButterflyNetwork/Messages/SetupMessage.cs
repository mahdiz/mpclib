using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Unm.DistributedSystem.ButterflyNetwork
{
	public class SetupMessageData : List<NodeConnection>, ITransmittable
	{
		public SetupMessageData()
		{
		}

		public int GetSize()
		{
			if (Count == 0)
				return 0;
			else
				return this.Sum(n => n.GetSize());
		}

		public object Clone()
		{
			var copy = new SetupMessageData();
			foreach (var nc in this)
				copy.Add((NodeConnection)nc.Clone());

			return copy;
		}
	}

	public class SetupMessage : BnMessage
	{
		public List<NodeConnection> NodeConnections { get; private set; }
		public int NetworkHeight { get; private set; }
		public int NetworkWidth { get; private set; }
		public int B { get; private set; }

		public SetupMessage(int netHeight, int netWidth, int B, List<NodeConnection> connections)
			: base(BnMessageType.Setup, null)
		{
			NetworkHeight = netHeight;
			NetworkWidth = netWidth;
			this.B = B;
			NodeConnections = connections;
		}

		//public override int GetSize()
		//{
		//	return base.GetSize() + 2 * sizeof(int);
		//}

		public override object Clone()
		{
			return new SetupMessage(NetworkHeight, NetworkWidth, B, NodeConnections);
		}
	}
}
