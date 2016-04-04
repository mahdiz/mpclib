using MpcLib.DistributedSystem;

namespace MpcLib.ByzantineAgreement
{
	public class BroadcastMessage : Msg
	{
		public readonly int k;
		public readonly BroadcastStage Stage;

		public BroadcastMessage(BroadcastStage stage, int k)
		{
			Stage = stage;
			this.k = k;
		}
	}

	public class BroadcastMessage<T> : BroadcastMessage
	{
		public readonly T Content;

		public BroadcastMessage(BroadcastStage stage, T content, int k)
			: base(stage, k)
		{
			Content = content;
		}

		public override bool Equals(object obj)
		{
			return Content.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Content.GetHashCode();
		}
	}
}