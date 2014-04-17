using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.MpcProtocols.Bgw
{
	public enum BgwShareType
	{
		SIMPLE_ZP = 0,
		ZP_LISTS = 1,
		ZPS_BUNDLE = 2,
		MESSAGE = 3,
		MULT_STEP_BCASE = 4,
		MULT_STEP_BCASE_BUNDLE = 5,
		MULT_STEP_VERIFY_POLY = 6,
		TO_SERVER_OBJECT = 7,
		FROM_SERVER_OBJECT = 8
	}
}
