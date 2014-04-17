namespace MpcLib.MpcProtocols.Bgw
{
	///// <summary>
	///// State key used for the Byzantine SMPC protocol.
	///// </summary>
	//public class ByzantineMsgKey
	//{
	//	public readonly Stage Stage;
	//	public readonly int PlayerToVerify;

	//	/// <summary>
	//	/// Indicates whether the sender had received a bad polynomial. This is not a part of the key.
	//	/// </summary>
	//	public readonly bool ReceivedGoodPoly;

	//	public ByzantineMsgKey(Stage s, int playerToVerify, bool receivedGoodPolynomial)
	//	{
	//		Stage = s;
	//		PlayerToVerify = playerToVerify;
	//		ReceivedGoodPoly = receivedGoodPolynomial;
	//	}

	//	public override bool Equals(object obj)
	//	{
	//		var key = (ByzantineMsgKey)obj;
	//		return key.Stage == Stage && key.PlayerToVerify == PlayerToVerify;
	//	}

	//	public override int GetHashCode()
	//	{
	//		return ((int)Stage) ^ PlayerToVerify;		// XOR
	//	}

	//	public override string ToString()
	//	{
	//		return "(Stage=" + Stage.ToString() + ", PlayerToVerify=" + PlayerToVerify + ")";
	//	}
	//}
}