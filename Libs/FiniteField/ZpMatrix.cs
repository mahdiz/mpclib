using System;
using System.Collections.Generic;
using MpcLib.Common.StochasticUtils;

namespace MpcLib.Common.FiniteField
{
	public class ZpMatrix
	{
		public int Prime { get; private set; }

		public int RowCount { get; private set; }

		public int ColCount { get; private set; }

		public int[][] Data
		{
			get
			{
				return data;
			}
		}

		private int[][] data;

		/// <summary>
		/// Returns the inverse matrix of the invoking matrix.
		/// </summary>
		public ZpMatrix Inverse
		{
			get
			{
				var piv = new int[RowCount];
				var fieldInv = NumTheoryUtils.GetFieldInverse(Prime);
				var lu = GetLUDecomposition(piv, fieldInv);
				return lu.SolveInv(ZpMatrix.GetIdentityMatrix(RowCount, Prime), piv, fieldInv);
			}
		}

		// TODO: Mahdi: Incorrect code! Works only if the matrix is triangular.
		// Should choose a fast algorithm to find determinant.
		private bool Nonsingular
		{
			get
			{
				for (int i = 0; i < RowCount; i++)
				{
					if (data[i][i] == 0)
						return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Creates M-by-N matrix of zero initialized elements.
		/// </summary>
		public ZpMatrix(int rowNum, int colNum, int prime)
		{
			RowCount = rowNum;
			ColCount = colNum;
			Prime = prime;
			data = initMatrix<int>(rowNum, colNum);
		}

		/// <summary>
		/// Creates matrix based on 2d array of integers.
		/// </summary>
		public ZpMatrix(int[][] data, int prime)
		{
			RowCount = data.Length;
			ColCount = data[0].Length;
			Prime = prime;
			this.data = initMatrix<int>(RowCount, ColCount);

			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColCount; j++)
					this.data[i][j] = data[i][j];
			}
		}

		/// <summary>
		/// Creates a  vector matrix from Zp array.
		/// </summary>
		public ZpMatrix(Zp[] vector, VectorType vec_type)
		{
			Prime = vector[0].Prime;
			if (vec_type.Equals(VectorType.Row))
			{
				RowCount = 1;
				ColCount = vector.Length;
				data = initMatrix<int>(RowCount, ColCount);
				for (int j = 0; j < ColCount; j++)
					data[0][j] = vector[j].Value;
			}
			else	// VectorType.COLOMN_VECTOR
			{
				RowCount = vector.Length;
				ColCount = 1;
				data = initMatrix<int>(RowCount, ColCount);
				for (int i = 0; i < RowCount; i++)
					data[i][0] = vector[i].Value;
			}
		}

		private ZpMatrix(ZpMatrix A)
			: this(A.data, A.Prime)
		{
		}

		private T[][] initMatrix<T>(int r, int c)
		{
			var array = new T[r][];
			for (int i = 0; i < r; i++)
				array[i] = new T[c];
			return array;
		}

		public Zp[] ZpVector
		{
			get
			{
				Zp[] vector = null;
				if (RowCount == 1)
				{
					vector = new Zp[ColCount];
					for (int j = 0; j < ColCount; j++)
						vector[j] = new Zp(Prime, data[0][j]);
				}
				else if (ColCount == 1)
				{
					vector = new Zp[RowCount];
					for (int i = 0; i < RowCount; i++)
						vector[i] = new Zp(Prime, data[i][0]);
				}
				return vector;
			}
		}

		public IList<Zp> GetMatrixRow(int rowNumber)
		{
			if (RowCount <= rowNumber)
				throw new ArgumentException("Illegal  matrix  row number.");

			var wantedRow = new List<Zp>();
			for (int j = 0; j < ColCount; j++)
				wantedRow.Add(new Zp(Prime, data[rowNumber][j]));

			return wantedRow;
		}

		/* Create and return the transpose of the invoking matrix */

		public ZpMatrix Transpose
		{
			get
			{
				var A = new ZpMatrix(ColCount, RowCount, Prime);
				for (int i = 0; i < RowCount; i++)
				{
					for (int j = 0; j < ColCount; j++)
						A.data[j][i] = data[i][j];
				}
				return A;
			}
		}

		/* return C = A + B */

		public ZpMatrix Plus(ZpMatrix B)
		{
			var A = this;
			if ((B.RowCount != A.RowCount) || (B.ColCount != A.ColCount))
				throw new ArgumentException("Illegal  matrix  dimensions.");

			if (A.Prime != B.Prime)
				throw new ArgumentException("Trying to add Matrix  from different fields.");

			var C = new ZpMatrix(RowCount, ColCount, A.Prime);
			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColCount; j++)
					C.data[i][j] = Modulo(A.data[i][j] + B.data[i][j]);
			}
			return C;
		}

		/* return C = A * B     : matrix    multiplication*/

		public ZpMatrix Times(ZpMatrix B)
		{
			var A = this;
			if (A.ColCount != B.RowCount)
				throw new ArgumentException("Illegal matrix dimensions.");

			if (A.Prime != B.Prime)
				throw new ArgumentException("Matrix  from different fields.");

			// create initialized matrix (zero value to all elements)
			var C = new ZpMatrix(A.RowCount, B.ColCount, A.Prime);
			for (int i = 0; i < C.RowCount; i++)
			{
				for (int j = 0; j < C.ColCount; j++)
					for (int k = 0; k < A.ColCount; k++)
						C.data[i][j] = Modulo(C.data[i][j] + A.data[i][k] * B.data[k][j]);
			}
			return C;
		}

		/* does A = B ? */

		public bool Eq(ZpMatrix B)
		{
			var A = this;
			if ((B.RowCount != A.RowCount) || (B.ColCount != A.ColCount) || (A.Prime != B.Prime))
				return false;

			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColCount; j++)
				{
					if (A.data[i][j] != B.data[i][j])
						return false;
				}
			}
			return true;
		}

		public Zp[] Solve(Zp[] vector)
		{
			var vecMatrix = new ZpMatrix(vector, VectorType.Column);
			return Inverse.Times(vecMatrix).ZpVector;
		}

		/// <summary>
		/// Returns x = (A^-1) b, assuming A is square and has full rank (not singular).
		/// </summary>
		public ZpMatrix Solve(ZpMatrix vec)
		{
			var revA = Inverse;
			return revA.Times(vec);
		}

		/* r  -   Array of row indices,  j0 -   Initial column index,  j1 -   Final column index
			return     A(r(:),j0:j1)  */

		private ZpMatrix GetSubMatrix(int[] r, int j0, int j1)
		{
			var X = new ZpMatrix(r.Length, j1 - j0 + 1, Prime);
			var B = X.data;

			for (int i = 0; i < r.Length; i++)
			{
				for (int j = j0; j <= j1; j++)
					B[i][j - j0] = data[r[i]][j];
			}
			return X;
		}

		/* swap rows i and j in the matrix*/

		private void SwapRows(int i, int j)
		{
			var temp = data[i];
			data[i] = data[j];
			data[j] = temp;
		}

		/* calculate i mod prime */

		private int Modulo(int i)
		{
			return Zp.Modulo(i, Prime);
		}

		private ZpMatrix SolveInv(ZpMatrix B, int[] piv, int[] fieldInv)
		{
			if (B.RowCount != RowCount)
				throw new ArgumentException("Matrix row dimensions must agree.");

			if (!Nonsingular)
				throw new ArgumentException("Matrix is singular.");

			// Copy right hand side with pivoting
			int nx = B.ColCount;
			var Xmat = B.GetSubMatrix(piv, 0, nx - 1);
			var X = Xmat.data;

			// Solve L*Y = B(piv,:)
			for (int k = 0; k < RowCount; k++)
			{
				for (int i = k + 1; i < RowCount; i++)
				{
					for (int j = 0; j < nx; j++)
						X[i][j] = Modulo(X[i][j] - X[k][j] * data[i][k]);
				}
			}

			// Solve U*X = Y;
			for (int k = RowCount - 1; k >= 0; k--)
			{
				for (int j = 0; j < nx; j++)
					X[k][j] = Modulo(X[k][j] * fieldInv[data[k][k]]);

				for (int i = 0; i < k; i++)
				{
					for (int j = 0; j < nx; j++)
						X[i][j] = Modulo(X[i][j] - X[k][j] * data[i][k]);
				}
			}
			return Xmat;
		}

		private ZpMatrix GetLUDecomposition(int[] pivot, int[] fieldInv)
		{
			// Use a "left-looking", dot-product, Crout/Doolittle algorithm.
			var LU = new ZpMatrix(this);
			int[][] LUArr = LU.data;

			int[] piv = pivot;
			for (int i = 0; i < RowCount; i++)
				piv[i] = i;

			int pivsign = 1;
			int[] LUrowi;
			var LUcolj = new int[RowCount];

			// Outer loop.
			for (int j = 0; j < RowCount; j++)
			{
				// Make a copy of the j-th column to localize references.
				for (int i = 0; i < RowCount; i++)
					LUcolj[i] = LUArr[i][j];

				// Apply previous transformations.
				for (int i = 0; i < RowCount; i++)
				{
					LUrowi = LUArr[i];

					// Most of the time is spent in the following dot product.
					int kmax = Math.Min(i, j);
					int s = 0;
					for (int k = 0; k < kmax; k++)
						s = Modulo(s + LUrowi[k] * LUcolj[k]);

					LUrowi[j] = LUcolj[i] = Modulo(LUcolj[i] - s);
				}

				// Find pivot and exchange if necessary.
				int p = j;
				for (int i = j + 1; i < RowCount; i++)
				{
					if ((LUcolj[i]) > (LUcolj[p]))
						p = i;
				}
				if (p != j)
				{
					for (int k = 0; k < RowCount; k++)
					{
						int t = LUArr[p][k];
						LUArr[p][k] = LUArr[j][k];
						LUArr[j][k] = t;
					}
					int r = piv[p];
					piv[p] = piv[j];
					piv[j] = r;
					pivsign = -pivsign;
				}

				// Compute multipliers.
				if (j < RowCount & LUArr[j][j] != 0)
				{
					for (int i = j + 1; i < RowCount; i++)
						LUArr[i][j] = Modulo(LUArr[i][j] * fieldInv[Modulo(LUArr[j][j])]);
				}
			}
			return LU;
		}

		/// <summary>
		/// Creates and return a random rowNum-by-colNum  matrix with values between '0' and 'prime-1'.
		/// </summary>
		public static ZpMatrix GetRandomMatrix(int rowNum, int colNum, int prime)
		{
			var A = new ZpMatrix(rowNum, colNum, prime);
			for (int i = 0; i < rowNum; i++)
			{
				for (int j = 0; j < colNum; j++)
					A.data[i][j] = Zp.Modulo((int)(StaticRandom.NextDouble() * (prime)), prime);
			}
			return A;
		}

		/// <summary>
		/// Create and return the N-by-N identity matrix.
		/// </summary>
		public static ZpMatrix GetIdentityMatrix(int matrixSize, int prime)
		{
			var I = new ZpMatrix(matrixSize, matrixSize, prime);
			for (int i = 0; i < matrixSize; i++)
				I.data[i][i] = 1;

			return I;
		}

		/* Create and return N-by-N  matrix that its first "trucToSize" elements in
		  its diagonal is "1" and the rest of the matrix is "0"*/

		public static ZpMatrix GetTruncationMatrix(int matrixSize, int truncToSize, int prime)
		{
			var I = new ZpMatrix(matrixSize, matrixSize, prime);
			for (int i = 0; i < truncToSize; i++)
				I.data[i][i] = 1;

			return I;
		}

		public static ZpMatrix GetVandermondeMatrix(int rowNum, int colNum, int prime)
		{
			var A = new ZpMatrix(rowNum, colNum, prime);

			for (int j = 0; j < colNum; j++)
				A.data[0][j] = 1;

			if (rowNum == 1)
				return A;

			for (int j = 0; j < colNum; j++)
				A.data[1][j] = j + 1;

			for (int j = 0; j < colNum; j++)
			{
				for (int i = 2; i < rowNum; i++)
					A.data[i][j] = Zp.Modulo(A.data[i - 1][j] * A.data[1][j], prime);
			}
			return A;
		}

		public static ZpMatrix GetVandermondeMatrix(int rowNum, IList<Zp> values, int prime)
		{
			int colNum = values.Count;
			var A = new ZpMatrix(rowNum, colNum, prime);

			for (int j = 0; j < colNum; j++)
				A.data[0][j] = 1;

			if (rowNum == 1)
				return A;

			for (int j = 0; j < colNum; j++)
			{
				for (int i = 1; i < rowNum; i++)
					A.data[i][j] = Zp.Modulo(A.data[i - 1][j] * values[j].Value, prime);
			}
			return A;
		}

		public static ZpMatrix GetSymmetricVanderMondeMatrix(int matrixSize, int prime)
		{
			return GetVandermondeMatrix(matrixSize, matrixSize, prime);
		}

		/// <summary>
		/// Returns a Vandermonde matrix in the field (each element is modulu prime).
		/// </summary>
		public static ZpMatrix GetShamirRecombineMatrix(int matrixSize, int prime)
		{
			var A = new ZpMatrix(matrixSize, matrixSize, prime);
			if (matrixSize == 1)
			{
				A.data[0][0] = 1;
				return A;
			}

			for (int i = 0; i < matrixSize; i++)
				A.data[i][0] = 1;

			for (int i = 0; i < matrixSize; i++)
				A.data[i][1] = i + 1;

			for (int i = 0; i < matrixSize; i++)
			{
				for (int j = 2; j < matrixSize; j++)
					A.data[i][j] = Zp.Modulo(A.data[i][j - 1] * A.data[i][1], prime);
			}
			return A;
		}

		public static ZpMatrix GetPrimitiveVandermondeMatrix(int rowNum, int colNum, int prime)
		{
			int primitive = NumTheoryUtils.GetFieldMinimumPrimitive(prime);
			if (primitive == 0)
				throw new ArgumentException("Cannot create a primitive Vandermonde matrix from a non-prime number. ");

			var A = new ZpMatrix(rowNum, colNum, prime);

			for (int j = 0; j < colNum; j++)
				A.data[0][j] = 1;

			if (rowNum == 1)
				return A;

			/*  This variable represents  primitive^j  for the j-th player*/
			int primitive_j = 1;
			for (int j = 0; j < colNum; j++)
			{
				A.data[1][j] = primitive_j;
				primitive_j = Zp.Modulo(primitive_j * primitive, prime);
			}

			for (int j = 0; j < colNum; j++)
			{
				for (int i = 2; i < rowNum; i++)
					A.data[i][j] = Zp.Modulo(A.data[i - 1][j] * A.data[1][j], prime);
			}

			return A;
		}

		public static ZpMatrix GetSymmetricPrimitiveVandermondeMatrix(int matrixSize, int prime)
		{
			return GetPrimitiveVandermondeMatrix(matrixSize, matrixSize, prime);
		}

		// Change the name !!!!
		public ZpMatrix RemoveRowsFromMatrix(bool[] toRemove)
		{
			if (RowCount != toRemove.Length)
				throw new ArgumentException("Illegal row number.");

			int numOfRowsToRemove = 0;
			for (int i = 0; i < toRemove.Length; i++)
			{
				numOfRowsToRemove += toRemove[i] ? 1 : 0;
			}

			var dataCopy = initMatrix<int>(RowCount - numOfRowsToRemove, ColCount - numOfRowsToRemove);
			int rowIndex = 0;

			for (int i = 0; i < RowCount; i++)
			{
				if (toRemove[i])
					continue;

				for (int j = 0; j < ColCount - numOfRowsToRemove; j++)
					dataCopy[rowIndex][j] = data[i][j];

				rowIndex++;
			}
			return new ZpMatrix(dataCopy, Prime);
		}

		public ZpMatrix RemoveRowFromMatrix(int index)
		{
			if ((index < 0) || (index > RowCount - 1))
			{
				throw new ArgumentException("Illegal row number.");
			}

			int[][] dataCopy = initMatrix<int>(RowCount - 1, ColCount);
			for (int i = 0; i < RowCount - 1; i++)
			{
				if (i == index)
					continue;

				for (int j = 0; j < ColCount - 1; j++)
					dataCopy[i][j] = data[i][j];
			}
			return new ZpMatrix(dataCopy, Prime);
		}

		public ZpMatrix RemoveColFromMatrix(int index)
		{
			if ((index < 0) || (index > ColCount - 1))
				throw new ArgumentException("Illegal col number.");

			int[][] dataCopy = initMatrix<int>(RowCount, ColCount - 1);
			for (int i = 0; i < ColCount - 1; i++)
			{
				if (i == index)
				{
					continue;
				}
				for (int j = 0; j < RowCount - 1; j++)
				{
					dataCopy[j][i] = data[j][i];
				}
			}
			return new ZpMatrix(dataCopy, Prime);
		}

		public int Determinant()
		{
			if ((RowCount == 1) && (ColCount == 1))
			{
				return data[0][0];
			}

			var det = new Zp(Prime, 0);

			for (int i = 0; i < ColCount; i++)
			{
				int SubDet = RemoveRowFromMatrix(0).RemoveColFromMatrix(i).Determinant();
				var SubDetAi = new Zp(Prime, SubDet * data[0][i]);
				if (i % 2 == 0)
				{
					det.Add(SubDetAi);
				}
				else
				{
					det.Sub(SubDetAi);
				}
			}
			return det.Value;
		}

		private void MulRowByscalar(int rowNumber, int scalar)
		{
			if (RowCount <= rowNumber)
			{
				throw new ArgumentException("Illegal matrix row number.");
			}

			for (int j = 0; j < ColCount; j++)
			{
				data[rowNumber][j] = Modulo(data[rowNumber][j] * scalar);
			}
		}

		/// <summary>
		/// Multiplies each row by different scalar from the scalars vector.
		/// </summary>
		public ZpMatrix MulMatrixByScalarsVector(int[] scalarsVector)
		{
			if (RowCount != scalarsVector.Length)
			{
				throw new ArgumentException("incompatible vector length and matrix row number.");
			}

			var B = new ZpMatrix(this);
			for (int i = 0; i < RowCount; i++)
			{
				B.MulRowByscalar(i, scalarsVector[i]);
			}
			return B;
		}

		public Zp[] SumMatrixRows()
		{
			var sum = new int[ColCount];

			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColCount; j++)
				{
					sum[j] += data[i][j];
				}
			}

			var zpSum = new Zp[ColCount];
			for (int j = 0; j < ColCount; j++)
			{
				zpSum[j] = new Zp(Prime, sum[j]);
			}

			return zpSum;
		}

		public void SetMatrixCell(int rowNumber, int colNumber, Zp value)
		{
			if ((RowCount <= rowNumber) || (ColCount <= colNumber))
			{
				throw new ArgumentException("Illegal matrix cell.");
			}
			data[rowNumber][colNumber] = value.Value;
		}

		public static ZpMatrix GetConcatenationMatrix(ZpMatrix A, ZpMatrix B)
		{
			if (A.RowCount != B.RowCount)
			{
				throw new ArgumentException("Illegal matrix dimensions - cannot perform concatenation.");
			}

			if (A.Prime != B.Prime)
			{
				throw new ArgumentException("Trying to concatenate Matrix  from different fields.");
			}

			var C = new ZpMatrix(A.RowCount, A.ColCount + B.ColCount, A.Prime);

			// Copy A
			for (int i = 0; i < A.RowCount; i++)
			{
				for (int j = 0; j < A.ColCount; j++)
				{
					C.data[i][j] = A.data[i][j];
				}
			}

			// Copy B
			for (int i = 0; i < A.RowCount; i++)
			{
				for (int j = A.ColCount; j < A.ColCount + B.ColCount; j++)
				{
					C.data[i][j] = B.data[i][j - A.ColCount];
				}
			}
			return C;
		}

		public int Gauss()
		{
			int[] invArr = NumTheoryUtils.GetFieldInverse(Prime);

			// Gaussian elimination with partial pivoting
			int i, j;
			i = j = 0;
			while ((i < RowCount) && (j < ColCount - 1))
			{
				// find pivot row and swap
				int max = i;
				for (int k = i + 1; k < RowCount; k++)
				{
					if (data[k][j] > data[max][j])
					{
						max = k;
					}
				}

				if (data[max][j] != 0)
				{
					SwapRows(i, max);
					int toMul = invArr[data[i][j]];
					for (int k = 0; k < ColCount; k++)
					{
						data[i][k] = Modulo(data[i][k] * toMul);
					}

					for (int u = i + 1; u < RowCount; u++)
					{
						int m = Modulo(data[u][j]);
						for (int v = 0; v < ColCount; v++)
						{
							data[u][v] = Modulo(data[u][v] - data[i][v] * m);
						}
						data[u][j] = 0;
					}
					i++;
				}
				j++;
			}

			// Get number of lines differrent from zero
			int num = 0;

			for (int k = 0; k < RowCount; k++)
			{
				for (int v = 0; v < ColCount; v++)
				{
					if (data[k][v] != 0)
					{
						num++;
						break;
					}
				}
			}
			return num;
		}

		/// <summary>
		/// Print matrix to standard output.
		/// </summary>
		public void Print()
		{
			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColCount; j++)
				{
					Console.Write(data[i][j] + "   ");
				}
				Console.WriteLine();
			}
			Console.WriteLine();
		}
	}
}