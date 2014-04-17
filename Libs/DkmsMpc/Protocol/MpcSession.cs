using MpcLib.MpcProtocols;
using MpcLib.MpcProtocols.Bgw;

namespace MpcLib.MpcProtocols.Dkms
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