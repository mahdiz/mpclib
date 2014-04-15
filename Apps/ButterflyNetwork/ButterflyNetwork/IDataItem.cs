using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unm.DistributedSystem.ButterflyNetwork
{
	public interface IDataItem<T> : ITransmittable
	{
		string Title { get; }
		int GetHashCode();
		T Content { get; }
	}
}
