using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MpcLib.Common.FiniteField;
using MpcLib.DistributedSystem;
using MpcLib.SecretSharing;

namespace MpcLib.MpcProtocols
{
	/// <summary>
	/// Defines an abstract MPC protocol.
	/// </summary>
	/// <typeparam name="T">Type of input/output.</typeparam>
	public abstract class MpcProtocol<T> : Protocol, IMpcProtocol<T> 
	{
		public T Input { get; protected set; }

		public T Result { get; protected set; }

		public MpcProtocol(Entity e, ReadOnlyCollection<int> pIds, T pInput, StateKey stateKey)
			: base(e, pIds, stateKey)
		{
			Input = pInput;
		}

		public MpcProtocol(Entity e, ReadOnlyCollection<int> pIds, T pInput, 
			SendHandler send, StateKey stateKey)
			: base(e, pIds, send, stateKey)
		{
			Input = pInput;
		}
	}
}