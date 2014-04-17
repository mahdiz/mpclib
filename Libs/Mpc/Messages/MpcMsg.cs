using System;
using MpcLib.DistributedSystem;

namespace MpcLib.MpcProtocols
{
	public class MpcMsg : Msg
	{
		public readonly Stage Stage;

		public MpcMsg(Stage stage)
		{
			Stage = stage;
		}

		public override string ToString()
		{
			return base.ToString() + ", Stage=" + Stage;
		}

		public override int StageKey
		{
			get { return (int)Stage; }
		}

		public override int Size
		{
			get
			{
				return base.Size + sizeof(int);
			}
		}
	}
}