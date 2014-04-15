using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MpcLib.Common.FiniteField;

namespace MpcLib.DistributedSystem.Mpc
{
	public interface IMpcProtocol<T>
	{
		T Input { get; }
		T Result { get; }
	}
}
