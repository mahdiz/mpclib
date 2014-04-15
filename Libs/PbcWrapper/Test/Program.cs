using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pbc;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Starting...\n");

			var pbc = new PbcWrapper();
			pbc.Init();

			Console.WriteLine("Exiting...");
		}
	}
}
