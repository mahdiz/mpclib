namespace MpcLib.ByzantineAgreement
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