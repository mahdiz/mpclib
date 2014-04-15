namespace MpcLib.DistributedSystem.ByzantineAgreement
{
	/// <summary>
	/// Rabin's algorithm for Byzantine agreement stage.
	/// </summary>
	public enum BroadcastStage
	{
		DealerDataSend,
		DealerLotteryShare,
		DataSend,
	}
}