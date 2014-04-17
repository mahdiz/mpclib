using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MpcLib.Common.FiniteField;

namespace MpcLib.MpcProtocols
{
	public interface IMpcProtocol<T>
	{
		T Input { get; }
		T Result { get; }
	}
}
