namespace MpcLib.DistributedSystem.Mpc
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
		CommitmentsBroadcast
	}
}