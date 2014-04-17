namespace MpcLib.MpcProtocols
{
	/// <summary>
	/// MPC protocol stage.
	/// </summary>
	public enum Stage
	{
		InputSend,
		InputReceive,
		VerificationReceive,
		ResultReceive,
		RandomizationReceive,
		SecureBroadcast,
		CommitBroadcast
	}
}