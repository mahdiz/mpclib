using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MpcLib.DistributedSystem
{
	public abstract class StateKey
	{
		public abstract override bool Equals(object obj);
		public abstract override int GetHashCode();
	}
}
