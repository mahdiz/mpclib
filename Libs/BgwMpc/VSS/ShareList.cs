using System.Collections.Generic;

namespace MpcLib.MpcProtocols.Bgw.Vss
{
	public abstract class ShareList<T> : BgwShare where T : BgwShare
	{
		public const int BITS_FOR_LENGTH_OF_LIST = 5;
		protected internal IList<T> shares;

		public abstract T NewInstrance { get; }

		public ShareList(BgwShareType type, IList<T> shares)
			: base(type)
		{
			this.shares = shares;
		}

		public ShareList(BgwShareType type)
			: base(type)
		{
		}

		public virtual IList<T> List
		{
			get
			{
				return shares;
			}
			set
			{
				this.shares = value;
			}
		}

		//    public override byte[] writeToByteArray()
		//    {
		//        Debug.Assert(sendableList != null);
		//        var bs = new BitStream();
		//        writeToBitStreamNoHeader(bs);
		//        bs.close();
		//        return bs.ByteArray;
		//    }

		//    public override void writeToBitStreamNoHeader(BitStream bs)
		//    {
		//        bs.writeMessageType(MessageType);
		//        bs.writeInt(sendableList.Count, BITS_FOR_LENGTH_OF_LIST);
		//        foreach (T senable in sendableList)
		//        {
		//            bs.writeBoolean(senable != null);
		//            if (senable != null)
		//                senable.writeToBitStreamNoHeader(bs);
		//        }
		//    }

		//    public override void loadFromByteArrayNoHeader(BitStream bs, int prime)
		//    {
		//var	sendableList = new List<T>();
		//        int length = bs.readInt(BITS_FOR_LENGTH_OF_LIST);
		//        for (int i = 0; i < length; i++)
		//        {
		//            if (bs.readBoolean())
		//            {
		//                var sendable = NewInstrance;
		//                sendable.loadFromByteArrayNoHeader(bs, prime);
		//                sendableList.Add(sendable);
		//            }
		//            else sendableList.Add(null);
		//        }
		//    }

		//    public override bool Equals(object obj)
		//    {
		//        if (!(obj is SendableList<T>))
		//            return false;

		//        var bundle = (SendableList<T>)obj;
		//        if (sendableList.Count != bundle.sendableList.Count)
		//            return false;

		//        for (int i = 0; i < sendableList.Count; i++)
		//        {
		//            if ((sendableList[i] == null) && (bundle.sendableList[i] == null))
		//                continue;

		//            if (sendableList[i] == null && sendableList[i] != null || sendableList[i] != null && sendableList[i] == null)
		//                return false;

		//            if (!(sendableList[i].Equals(bundle.sendableList[i])))
		//                return false;
		//        }
		//        return true;
		//    }
	}
}