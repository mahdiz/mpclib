using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unm.DistributedSystem.ButterflyNetwork
{
	public enum BnMessageType
	{
		Setup,
		Query,
		Store,
		Data
	}

	public class BnMessage : Message
	{
		public ITransmittable Data { get; set; }
		public BnMessageType Type { get; private set; }

		public BnMessage(BnMessageType type, ITransmittable data)
		{
			Type = type;
			Data = data;
		}

		public override string ToString()
		{
			return base.ToString() + ", Type=" + Type;
		}

		//public override int GetSize()
		//{
		//	return base.Size + sizeof(BnMessageType);
		//}

		public override object Clone()
		{
			return new BnMessage(Type, Data) { SenderId = this.SenderId };
		}
	}
}
