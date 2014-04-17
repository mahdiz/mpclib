using MpcLib.DistributedSystem;
namespace MpcLib.MpcProtocols.Dkms
{
	public class DkmsKey : StateKey
	{
		public readonly Stage Stage;
		public readonly int GateId;

		public DkmsKey(Stage s, int gateId)
		{
			Stage = s;
			GateId = gateId;
		}

		public override bool Equals(object obj)
		{
			var cObj = obj as DkmsKey;
			return cObj.Stage == Stage && cObj.GateId == GateId;
		}

		public override int GetHashCode()
		{
			return (int)Stage ^ GateId;
		}

		public override string ToString()
		{
			return "Stage=" + Stage + ", Gate=" + GateId;
		}
	}
}