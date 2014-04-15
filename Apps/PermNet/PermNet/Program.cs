using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermNet
{
	public class Gate<T>
	{
		public T A;
		public T B;

		public void Swap()
		{
			var t = A;
			A = B;
			B = t;
		}
	}

	public enum PermNetType
	{
		Benes,
		Waksman
	}

	public class Network<T>
	{
		private int N, depth;		// N: num of inputs, D: depth
		private Gate<T>[][] gates;
		private PermNetType netType;
		private Random rand;

		/// <param name="n">Number of inputs.</param>
		/// <param name="seed">RNG seed.</param>
		public Network(int n, PermNetType type, int seed)
		{
			Debug.Assert((n & (n - 1)) == 0, "n must be a power of two!");
			rand = new Random(seed);
			netType = type;

			N = n;
			depth = (int)(2 * Math.Log(n, 2) - 1);		// see Waksman '68
			gates = new Gate<T>[n / 2][];
			for (int i = 0; i < n / 2; i++)
				gates[i] = new Gate<T>[depth];

			for (int i = 0; i < n / 2; i++)
			{
				for (int j = 0; j < depth; j++)
					gates[i][j] = new Gate<T>();
			}
		}

		public bool IsNonSwapGate(int n, int d, int i, int j)
		{
			return (i == 0 && j > (d - Math.Log(n, 2)) - 1) ||
				(i == n / 2 && j > (d - Math.Log(n, 2) - 1) && j != d - 1);
		}

		public T[] Run(T[] inputs, int? bits)
		{
			Debug.Assert(inputs.GetLength(0) == N, "Number of inputs must be " + N + ".");

			// set the inputs
			for (int i = 0; i < N / 2; i++)
			{
				gates[i][0].A = inputs[2 * i];
				gates[i][0].B = inputs[2 * i + 1];
			}

			// find network connections
			var edges = GetEdges(N);

			// run the network
			for (int j = 0; j < depth; j++)
			{
				for (int i = 0; i < N / 2; i++)
				{
					if (netType == PermNetType.Benes || !IsNonSwapGate(N / 2, depth, i, j))
					{
						if (bits == null)
						{
							if (rand.Next(0, 2) == 1)
								gates[i][j].Swap();
						}
						else
						{
							if (bits % 2 == 1)
								gates[i][j].Swap();
							bits = bits >> 1;
						}
					}
				}

				if (j < depth - 1)
				{
					// set inputs of next layer
					for (int i = 0; i < N / 2; i++)
					{
						var AtoWire = edges[2 * i][j];
						var BtoWire = edges[2 * i + 1][j];
						var AtoGate = (int)Math.Floor(AtoWire / 2.0);
						var BtoGate = (int)Math.Floor(BtoWire / 2.0);

						if (AtoWire % 2 == 0)
							gates[AtoGate][j + 1].A = gates[i][j].A;
						else
							gates[AtoGate][j + 1].B = gates[i][j].A;

						if (BtoWire % 2 == 0)
							gates[BtoGate][j + 1].A = gates[i][j].B;
						else
							gates[BtoGate][j + 1].B = gates[i][j].B;
					}
				}
			}

			var outputs = new T[N];
			for (int i = 0; i < N / 2; i++)
			{
				outputs[2 * i] = gates[i][depth - 1].A;
				outputs[2 * i + 1] = gates[i][depth - 1].B;
			}
			return outputs;
		}

		// finds connections of the Bene network recursively.
		private static int[][] GetEdges(int n)
		{
			var depth = (int)(2 * Math.Log(n, 2) - 1);		// see Waksman '68

			// initialize a matrix of pairs with size (n, depth - 1)
			var edges = new int[n][];
			for (int i = 0; i < n; i++)
				edges[i] = new int[depth - 1];

			if (n > 4)
			{
				var subEdges = GetEdges(n / 2);
				var subDepth = (int)(2 * Math.Log(n / 2, 2) - 1);		// see Waksman68

				for (int i = 0; i < n / 2; i++)
				{
					for (int j = 0; j < subDepth - 1; j++)
					{
						edges[i][j + 1] = subEdges[i][j];					// P_A in Waksman68
						edges[i + n / 2][j + 1] = subEdges[i][j] + n / 2;	// P_B in Waksman68
					}
				}
			}

			// first layer edges
			for (int i = 0; i < n; i++)
			{
				if (i % 2 == 0)		// i is even
					edges[i][0] = i / 2;						// an edge from i-th input gate to (i/2)-th output gate
				else
					edges[i][0] = (i - 1) / 2 + n / 2;		// an edge from i-th input gate to ((i-1)/2 + n/2)-th output gate
			}

			// last layer edges (mirrors of the first layer edges)
			for (int i = 0; i < n; i++)
			{
				if (i < n / 2)		// i is even
					edges[i][depth - 2] = 2 * i;				// an edge from i-th input gate to (2i)-th output gate
				else
					edges[i][depth - 2] = 2 * i - n + 1;		// an edge from i-th input gate to (2i - n + 1)-th output gate
			}
			return edges;
		}
	}

	internal class Program
	{
		private static void Main(string[] args)
		{
			int n = 4;
			char[] inputs = new char[n];
			for (int i = 0; i < n; i++)
				inputs[i] = Convert.ToChar(i + 65);

			var numGates = n * (int)Math.Log(n, 2) - n + 1;

			for (int i = 0; i < Math.Pow(2, numGates); i++)
			{
				var net = new Network<char>(n, PermNetType.Benes, 1);
				var perm = net.Run(inputs, i);

				var str = "";
				foreach (var p in perm)
					str += p + " ";

				Console.WriteLine(i + "\t" + str);
			}
			Console.WriteLine("Count = " + Math.Pow(2, numGates));
		}
	}
}