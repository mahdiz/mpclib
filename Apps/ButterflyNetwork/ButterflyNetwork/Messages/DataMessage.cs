using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unm.DistributedSystem.ButterflyNetwork
{
	public enum DataMessageDir
	{
		Up,
		Down
	}

	public class BackConnection
	{
		public int NodeId { get; set; }
		public int FromSupernode { get; set; }
	}

	public class DataMessage : BnMessage
	{
		public DataMessageDir Direction { get; set; }
		public Stack<BackConnection> BackPath { get; set; }

		//public override int GetSize()
		//{
		//	return base.GetSize() + BackPath.Count() * sizeof(int) + sizeof(DataMessageDir);
		//}

		public override object Clone()
		{
			return new DataMessage(Direction, BackPath, Data) { SenderId = this.SenderId };
		}

		public DataMessage(DataMessageDir dir, ITransmittable data)
			: base(BnMessageType.Data, data)
		{
			Direction = dir;
			BackPath = new Stack<BackConnection>();
		}

		public DataMessage(DataMessageDir dir, IEnumerable<BackConnection> backPath, ITransmittable data)
			: base(BnMessageType.Data, data)
		{
			Direction = dir;
			BackPath = new Stack<BackConnection>(backPath.Reverse());	// TODO: Temporary decision!
		}
	}

	public class DataDownMessage : DataMessage
	{
		public int DestinationSupernode { get; private set; }
		public int NextSupernode { get; set; }

		public DataDownMessage(int destSupernode, int nextSupernode, ITransmittable payload)
			: base(DataMessageDir.Down, payload)
		{
			DestinationSupernode = destSupernode;
			NextSupernode = nextSupernode;
		}

		public DataDownMessage(int destSupernode, int nextSupernode, IEnumerable<BackConnection> backPath, ITransmittable payload)
			: base(DataMessageDir.Down, backPath, payload)
		{
			DestinationSupernode = destSupernode;
			NextSupernode = nextSupernode;
		}

		public override object Clone()
		{
			return new DataDownMessage(DestinationSupernode, 
				NextSupernode, BackPath, Data) { SenderId = this.SenderId };
		}

		public override int GetSize()
		{
			return base.GetSize() + 2 * sizeof(int);
		}

		public override string ToString()
		{
			return base.ToString() + ", NextSupernode=" + NextSupernode +
				", DestinationSupernode=" + DestinationSupernode;
		}
	}
}
