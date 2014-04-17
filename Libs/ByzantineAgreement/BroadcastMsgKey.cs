using MpcLib.DistributedSystem;

namespace MpcLib.ByzantineAgreement
{
	public class BroadcastMsgKey : StateKey
	{
		public readonly BroadcastStage Stage;

		public BroadcastMsgKey(BroadcastStage s)
		{
			Stage = s;
		}

		public override bool Equals(object obj)
		{
			return ((BroadcastMsgKey)obj).Stage == Stage;
		}

		public override int GetHashCode()
		{
			return (int)Stage;
		}

		public override string ToString()
		{
			return Stage.ToString();
		}
	}
}