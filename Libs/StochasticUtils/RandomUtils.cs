using System;
using System.Collections.Generic;
using System.Linq;

namespace MpcLib.Common.StochasticUtils
{
	public class RandomUtils
	{
		private SafeRandom rand;

		public RandomUtils()
		{
			rand = new SafeRandom();
		}

		public RandomUtils(int seed)
		{
			rand = new SafeRandom(seed);
		}

		public RandomUtils(SafeRandom randGen)
		{
			rand = randGen;
		}

		/// <summary>
		/// Picks n elements from inputList uniformly at random.
		/// </summary>
		public List<T> PickRandomElements<T>(List<T> inputList, int n)
		{
			var length = inputList.Count();
			if (n < 0 || length < n)
				throw new InvalidOperationException();

			var indexList = GetRandomPerm(0, length, n);
			var outputList = new List<T>();
			foreach (var index in indexList)
				outputList.Add(inputList[index]);

			return outputList;
		}

		/// <summary>
		/// Returns n permutations of integers between min and max chosen uniformly at random.
		/// </summary>
		public List<int> GetRandomPerm(int min, int max, int n)
		{
			if (min < 0 || max < 0 || max < min || max - min < n)
				throw new InvalidOperationException();

			var list = new List<int>();
			for (int i = min; i < max; i++)
				list.Add(i);

			var array = list.ToArray();
			Shuffle<int>(array);
			return array.Take(n).ToList();
		}

		/// <summary>
		/// Fischer-Yates shuffle (an unbiased shuffle method).
		/// </summary>
		public void Shuffle<T>(T[] array)
		{
			for (int i = array.Length; i > 0; i--)
			{
				int j = rand.Next(0, i);
				T tmp = array[j];
				array[j] = array[i - 1];
				array[i - 1] = tmp;
			}
		}
	}
}