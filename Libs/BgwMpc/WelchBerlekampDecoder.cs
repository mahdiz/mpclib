using System.Collections.Generic;
using MpcLib.Common.FiniteField;
using MpcLib.SecretSharing;

namespace MpcLib.MpcProtocols.Bgw
{
	public class WelchBerlekampDecoder
	{
		public static IList<Zp> decode(IList<Zp> XVlaues, IList<Zp> YVlaues, int e, int polynomDeg, int prime)
		{
			var pPolynomial = interpolatePolynomial(XVlaues, YVlaues, e, polynomDeg, prime);
			if (pPolynomial != null)
			{
				var fixedCodWord = new List<Zp>();
				for (int i = 0; i < XVlaues.Count; i++)
					fixedCodWord.Add(pPolynomial.Sample(XVlaues[i]));

				return fixedCodWord;
			}
			return null;	 // decoding failed - we were unable to retrieve the original code word
		}

		private static Polynomial interpolatePolynomial(IList<Zp> XVlaues, IList<Zp> YVlaues, int e, int polynomDeg, int prime)
		{
			int n = XVlaues.Count;
			if ((e < 0) || (n < 2 * e)) // cannot fix e errors if e <0 or  n < 2e.
				return null;

			var A = getWelchBerlekampMatrix(XVlaues, YVlaues, n, e, prime); // the matrix to hold the linear system we'll solve
			var b = getWelchBerlekampConstraintVector(XVlaues, YVlaues, n, e, prime);

			// coefficients of N and E as one vector
			var NE = LinearSolve(A, new ZpMatrix(b, VectorType.Column), prime);

			if (NE != null)
			{
				var N = new Zp[n - e];
				var E = new Zp[e + 1];
				for (int i = 0; i < n - e; i++)
					N[i] = new Zp(NE[i]);

				for (int i = n - e; i < n; i++)
					E[i - (n - e)] = new Zp(NE[i]);

				// Constraint coeef - E has degree exactly e and is monic (shoudn't be zero)
				E[e] = new Zp(prime, 1);
				return (new Polynomial(new List<Zp>(N))).divideWithRemainder(new Polynomial(new List<Zp>(E)));
			}
			return null;
		}

		private static ZpMatrix getWelchBerlekampMatrix(IList<Zp> XVlaues, IList<Zp> YVlaues, int n, int e, int prime)
		{
			var NVanderMonde = ZpMatrix.GetVandermondeMatrix(n - e, XVlaues, prime).Transpose;
			var EVanderMonde = ZpMatrix.GetVandermondeMatrix(e, XVlaues, prime).Transpose;

			int[] scalarVector = new int[YVlaues.Count];
			int i = 0;

			foreach (Zp zp in YVlaues)
				scalarVector[i++] = -zp.Value;

			EVanderMonde = EVanderMonde.MulMatrixByScalarsVector(scalarVector);
			return ZpMatrix.GetConcatenationMatrix(NVanderMonde, EVanderMonde);
		}

		private static Zp[] getWelchBerlekampConstraintVector(IList<Zp> XVlaues, IList<Zp> YVlaues, int n, int e, int prime)
		{
			var bVector = new Zp[n];
			for (int i = 0; i < n; i++)
				bVector[i] = new Zp(prime, NumTheoryUtils.ModPow(XVlaues[i].Value, e, prime) * YVlaues[i].Value);

			return bVector;
		}

		/*
		* Finds a solution to a system of linear equations represtented by an
		* n-by-n+1 matrix A: namely, denoting by B the left n-by-n submatrix of A
		* and by C the last column of A, finds a column vector x such that Bx=C.
		* If more than one solution exists, chooses one arbitrarily by setting some
		* values to 0.  If no solutions exists, returns false.  Otherwise, places
		* a solution into the first argument and returns true.
		*
		* Note : matrix A changes (gets converted to row echelon form).
		*/

		private static Zp[] LinearSolve(ZpMatrix A, ZpMatrix B, int prime)
		{
			var invArray = NumTheoryUtils.GetFieldInverse(prime);
			var C = ZpMatrix.GetConcatenationMatrix(A, B);		// augmented matrix
			int n = C.RowCount;
			int[] solution = new int[n];
			int temp;

			int firstDeterminedValue = n;

			// we will be determining values of the solution
			// from n-1 down to 0.  At any given time,
			// values from firstDeterminedValue to n-1 have been
			// found. Initializing to n means
			// no values have been found yet.
			// To put it another way, the variabe firstDeterminedValue
			// stores the position of first nonzero entry in the row just examined
			// (except at initialization)

			int rank = C.Gauss();

			int[][] cContent = C.Data;

			// can start at rank-1, because below that are all zeroes
			for (int row = rank - 1; row >= 0; row--)
			{
				// remove all the known variables from the equation
				temp = cContent[row][n];
				int col;
				for (col = n - 1; col >= firstDeterminedValue; col--)
					temp = Zp.Modulo(temp - (cContent[row][col] * solution[col]), prime);

				// now we need to find the first nonzero coefficient in this row
				// if it exists before firstDeterminedValue
				// because the matrix is in row echelon form, the first nonzero
				// coefficient cannot be before the diagonal
				for (col = row; col < firstDeterminedValue; col++)
				{
					if (cContent[row][col] != 0)
						break;
				}

				if (col < firstDeterminedValue) // this means we found a nonzero coefficient
				{
					// we can determine the variables in position from col to firstDeterminedValue
					// if this loop executes even once, then the system is undertermined
					// we arbitrarily set the undetermined variables to 0, because it make math easier
					for (int j = col + 1; j < firstDeterminedValue; j++)
						solution[j] = 0;

					// Now determine the variable at the nonzero coefficient
					//div(solution[col], temp, A.getContent()[row][col]);
					solution[col] = temp * invArray[Zp.Modulo(cContent[row][col], prime)];
					firstDeterminedValue = col;
				}
				else
				{
					// this means there are no nonzero coefficients before firstDeterminedValue.
					// Because we skip all the zero rows at the bottom, the matrix is in
					// row echelon form, and firstDeterminedValue is equal to the
					// position of first nonzero entry in row+1 (unless it is equal to n),
					// this means we are at a row with all zeroes except in column n
					// The system has no solution.
					return null;
				}
			}

			// set the remaining undetermined values, if any, to 0
			for (int col = 0; col < firstDeterminedValue; col++)
				solution[col] = 0;

			var ResultVec = new Zp[n];
			for (int i = 0; i < n; i++)
				ResultVec[i] = new Zp(prime, solution[i]);

			return ResultVec;
		}
	}
}