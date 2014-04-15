using MpcLib.Common.FiniteField;
using MpcLib.DistributedSystem.SecretSharing;

namespace MpcLib.DistributedSystem.Mpc.Dkms
{
	public class InputMsg : DkmsMsg
	{
		public readonly Zp Data;

		public InputMsg(Zp data, DkmsKey key)
			: base(key)
		{
			Data = data;
		}

		public override int StageKey
		{
			get { return (int)Stage.Input; }
		}
	}
}