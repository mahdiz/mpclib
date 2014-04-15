using MpcLib.DistributedSystem.Mpc;
using MpcLib.DistributedSystem.Mpc.Bgw;

namespace MpcLib.DistributedSystem.Mpc.Dkms
{
	public struct GatePlayer
	{
		public int PlayerId;
		public int GateId;
	}

	public class MpcSession
	{
		public readonly BgwProtocol Mpc;

		public MpcSession(BgwProtocol mpc)
		{
			Mpc = mpc;
		}
	}
}