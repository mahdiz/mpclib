namespace MpcLib.DistributedSystem.QuorumSystem
{
	internal interface IQsMessage
	{
	}

	internal class QsMessage<T> : Msg, IQsMessage
	{
		public T Data { get; set; }

		public QsMessage()
		{
		}

		public override int StageKey
		{
			get { return -1; }	// dummy
		}

		//public override int GetSize()
		//{
		//    return base.Size;
		//}
	}
}