using MpcLib.DistributedSystem;
namespace MpcLib.MpcProtocols.Dkms
{
	public abstract class DkmsMsg : Msg
	{
		public readonly DkmsKey StateKey;

		public DkmsMsg(DkmsKey key)
		{
			StateKey = key;
		}

		public override string ToString()
		{
			return base.ToString() + ", StateKey=(" + StateKey + ")";
		}

		public override int StageKey
		{
			get { return (int)StateKey.Stage; }
		}
	}
}