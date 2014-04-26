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
	/// Defines an abstract synchronous MPC protocol.
	/// </summary>
	/// <typeparam name="T">Type of input/output.</typeparam>
	public abstract class SyncMpc<T> : SyncProtocol, IMpcProtocol<T>
	{
		public T Input { get; protected set; }
		public T Result { get; protected set; }

		public SyncMpc(SyncParty p, IList<int> pIds, T pInput)
			: base(p, pIds)
		{
			Input = pInput;
		}
	}

	/// <summary>
	/// Defines an abstract asynchronous MPC protocol.
	/// </summary>
	/// <typeparam name="T">Type of input/output.</typeparam>
	public abstract class AsyncMpc<T> : AsyncProtocol, IMpcProtocol<T> 
	{
		public T Input { get; protected set; }
		public T Result { get; protected set; }

		public AsyncMpc(AsyncParty e, IList<int> pIds, T pInput, SendHandler send, StateKey stateKey)
			: base(e, pIds, send, stateKey)
		{
			Input = pInput;
		}

		public AsyncMpc(AsyncParty p, IList<int> pIds, T pInput, StateKey stateKey)
			: base(p, pIds, stateKey)
		{
			Input = pInput;
		}
	}
}