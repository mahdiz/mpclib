using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MpcLib.Common.FiniteField;

namespace MpcLib.DistributedSystem.SecretSharing
{
	public class eVSS : Protocol
	{
		public eVSS(Entity e, ReadOnlyCollection<int> pIds, StateKey stateKey)
			: base(e, pIds, stateKey)
		{
		}

		protected IList<Zp> Share(Zp secret, int numPlayers, int polyDeg)
		{
			throw new NotImplementedException();
		}
		 
		public override void Run()
		{
			throw new NotImplementedException();
		}
	}
}
