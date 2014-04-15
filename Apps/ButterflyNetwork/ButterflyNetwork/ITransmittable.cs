using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unm.DistributedSystem.ButterflyNetwork
{
	public interface ITransmittable : ICloneable
	{
		int GetSize();
	}
}
