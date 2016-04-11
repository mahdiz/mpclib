using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.Common
{
	public enum ProtocolIds
	{
		NotSet,			// To show invalid id
		BGW,			// Ben-Or, Goldwasser, Wigderson 1988
		eVSS,			// eVSS of Kate, Zaverucha, Goldberg 2010
		DKMS,			// Dani, King, Movahedi, Saia 2013
		MaSKZ,			// Zamani, Saia, Movahedi, Khoury 2013
		ZMS,			// Zamani, Movahedi, Saia 2014 with eVSS
		ZMSQ,			// Zamani, Movahedi, Saia 2014 MPC in quorums
		ZMS_DL,			// Zamani, Movahedi, Saia 2014 with DL commitments
		Rabin,			// Michael Rabin's BA
		AE,				// Almost Everywhere BA
		Cuckoo,			// Commensal cuckoo
		StaticSampler,	// Static sampler quorum building
        QuorumShareRenewal,
        AllToAllRandGen,
	}
}
