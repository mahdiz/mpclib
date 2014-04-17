namespace MpcLib.MpcProtocols.Dkms
{
	/// <summary>
	/// Scalabe SMPC protocol stages.
	/// </summary>
	public enum Stage
	{
		Input,
		Mpc,
		OutputConstruction,
		OutputPropagation
	}

	public enum GateType
	{
		Input,		// input gate
		Output,		// output gate
		Internal	// computation gate
	}

	public enum AdversaryModel
	{
		HonestButCurious,
		Byzantine
	}
}