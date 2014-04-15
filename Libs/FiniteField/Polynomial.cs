using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MpcLib.Common.FiniteField
{
	public class Polynomial
	{
		public int CoefficinesFieldSize
		{
			get
			{
				if (Coefficients.Count == 0)
					throw new Exception("Polynom is empty");
				return Coefficients[0].Prime;
			}
		}

		public IList<Zp> Coefficients { get; private set; }

		public int Degree
		{
			get
			{
				return Coefficients.Count - 1;
			}
		}

		public Polynomial(IList<Zp> coeffs)
		{
			init(new List<Zp>(coeffs));
		}

		public Polynomial(Zp[] coeffs)
		{
			init(new List<Zp>(coeffs));
		}

		public Polynomial(int[] coeffs, int coeffsFieldSize)
		{
			var TempList = new List<Zp>();
			for (int i = 0; i < coeffs.Length; i++)
			{
				TempList.Add(new Zp(coeffsFieldSize, coeffs[i]));
			}
			init(TempList);
		}

		private void init(List<Zp> coeffs)
		{
			var Coefficients = new List<Zp>();
			for (int i = coeffs.Count - 1; i >= 0; i--)
			{
				if (coeffs[i].Value != 0 || Coefficients.Count > 0)
				{
					Coefficients.Insert(0, coeffs[i]);
				}
			}
		}

		/// <param name="SamplePoint"> - the desired sampling point. </param>
		/// <returns>  the result of the polynom when replacing variable ("x") with the sampling point </returns>
		public virtual Zp Sample(Zp SamplePoint)
		{
			if (Coefficients.Count == 0)
			{
				return null;
			}

			/* The initialized sum is 0 */
			var Sum = new Zp(CoefficinesFieldSize, 0);

			for (int i = 0; i < Coefficients.Count; i++)
			{
				/* replace each "Ai*x^i" with "Ai*SamplePoint^i" */
				var Xi = new Zp(CoefficinesFieldSize,
					NumTheoryUtils.ModPow(SamplePoint.Value, i, CoefficinesFieldSize));

				var Ai = new Zp(CoefficinesFieldSize, Coefficients[i].Value);
				var AiXi = Xi.Mul(Ai);

				/* Sum all these values(A0+A1X^1+...AnX^n) */
				Sum = Sum.Add(AiXi);
			}
			return Sum;
		}

		public virtual Polynomial divideWithRemainder(Polynomial p)
		{
			if (Coefficients.Count == 0 || p.Coefficients.Count == 0)
				return null; //null

			var answer = new Polynomial[2];
			int prime = Coefficients[0].Prime;
			int m = Degree;
			int n = p.Degree;

			if (m < n)
				return null;

			var quotient = new Zp[m - n + 1];
			var coeffs = new Zp[m + 1];

			for (int k = 0; k <= m; k++)
				coeffs[k] = new Zp(Coefficients[k]);

			var norm = p.Coefficients[n].MultipInverse;
			for (int k = m - n; k >= 0; k--)
			{
				quotient[k] = new Zp(prime, coeffs[n + k].Value * norm.Value);
				for (int j = n + k - 1; j >= k; j--)
					coeffs[j] = new Zp(prime,
						coeffs[j].Value - quotient[k].Value * p.Coefficients[j - k].Value);
			}

			var remainder = new Zp[n];
			for (int k = 0; k < n; k++)
				remainder[k] = new Zp(coeffs[k]);

			answer[0] = new Polynomial(quotient);
			answer[1] = new Polynomial(remainder);
			foreach (Zp zp in answer[1].Coefficients)
			{
				if (zp.Value != 0)
					return null;
			}
			return answer[0];
		}

		/// <summary>
		///  TODO: Performance improvement: This is a naive soultion, find a better solution from the web </summary>
		/// <returns> the roots of the polynom (comparing the polynom to 0). </returns>
		public virtual List<Zp> GetRoots()
		{
			var SolutionsList = new List<Zp>();

			/* BF - go over all the items in the field. and check if they solve the poly */
			for (int i = 0; i < CoefficinesFieldSize; i++)
			{
				var CurrentSamplePoint = new Zp(CoefficinesFieldSize, i);
				if (this.Sample(CurrentSamplePoint).Value == 0)
					SolutionsList.Add(CurrentSamplePoint);
			}
			return SolutionsList;
		}

		public override string ToString()
		{
			var str = new StringBuilder();
			for (int i = 0; i < Degree; i++)
			{
				str.Append(Coefficients[i]);
				if (i < Degree - 1)
					str.Append(", ");
			}
			return str.ToString();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Polynomial))
				return false;

			var p = (Polynomial)obj;
			if (Degree != p.Degree)
				return false;

			for (int i = 0; i <= Degree; i++)
			{
				if (!(Coefficients[i].Equals(p.Coefficients[i])))
					return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		public static Polynomial hanfetzPolynom(int degree, int prime)
		{
			var coeffs = new List<Zp>();
			for (int i = 0; i <= degree; i++)
			{
				int num = (int)((new Random(1)).NextDouble() * 100);
				coeffs.Add(new Zp(prime, num));
			}
			return new Polynomial(coeffs);
		}

		public virtual Polynomial multiply(Polynomial p)
		{
			Debug.Assert(Degree > p.Degree && Coefficients.Count > 0);

			int prime = Coefficients[0].Prime;
			var coeffs = new SortedDictionary<int, Zp>();

			for (int deg1 = 0; deg1 <= Degree; deg1++)
			{
				for (int deg2 = 0; deg2 <= p.Degree; deg2++)
				{
					int deg = deg1 + deg2;
					var curr = coeffs[deg];
					var newValue = new Zp(prime, Coefficients[deg1].Value * p.Coefficients[deg2].Value);

					if (curr == null)
						curr = newValue;
					else
						curr = new Zp(prime, curr.Value + newValue.Value);
					coeffs[deg] = curr;
				}
			}
			return new Polynomial(new List<Zp>(coeffs.Values));
		}
	}
}