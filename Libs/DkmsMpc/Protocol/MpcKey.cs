namespace MpcLib.DistributedSystem.Mpc.Dkms
{
	public class MpcKey : DkmsKey
	{
		public readonly int AnchorId;

		public MpcKey(int gateId, int anchorId)
			: base(Stage.Mpc, gateId)
		{
			AnchorId = anchorId;
		}

		public override bool Equals(object obj)
		{
			var cObj = obj as MpcKey;
			return cObj.Stage == Stage && cObj.GateId == GateId && cObj.AnchorId == AnchorId;
		}

		public override int GetHashCode()
		{
			return (int)Stage ^ GateId ^ AnchorId;
		}

		public override string ToString()
		{
			return base.ToString() + ", Anchor=" + AnchorId;
		}
	}
}