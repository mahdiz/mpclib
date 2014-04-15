using System;
using System.Collections.Generic;
using System.Numerics;
using MpcLib.Common.StochasticUtils;

namespace MpcLib.Common.FiniteField
{
	public class BigZpMatrix
	{
		public BigInteger Prime { get; private set; }

		public int RowCount { get; private set; }

		public int ColCount { get; private set; }

		public BigInteger[][] Data
		{
			get
			{
				return data;
			}
		}

		private BigInteger[][] data;

		///// <summary>
		///// Returns the inverse matrix of the invoking matrix.
		///// </summary>
		//public BigZpMatrix Inverse
		//{
		//	get
		//	{
		//		var piv = new BigInteger[RowCount];
		//		var lu = GetLUDecomposition(piv, fieldInv);
		//		return lu.SolveInv(BigZpMatrix.GetIdentityMatrix(RowCount, Prime), piv, fieldInv);
		//	}
		//}

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
		public BigZpMatrix(int rowNum, int colNum, BigInteger prime)
		{
			RowCount = rowNum;
			ColCount = colNum;
			Prime = prime;
			data = initMatrix<BigInteger>(rowNum, colNum);
		}

		/// <summary>
		/// Creates matrix based on 2d array of integers.
		/// </summary>
		public BigZpMatrix(BigInteger[][] data, BigInteger prime)
		{
			RowCount = data.Length;
			ColCount = data[0].Length;
			Prime = prime;
			this.data = initMatrix<BigInteger>(RowCount, ColCount);

			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColCount; j++)
					this.data[i][j] = data[i][j];
			}
		}

		/// <summary>
		/// Creates a  vector matrix from BigZp array.
		/// </summary>
		public BigZpMatrix(BigZp[] vector, VectorType vec_type)
		{
			Prime = vector[0].Prime;
			if (vec_type.Equals(VectorType.Row))
			{
				RowCount = 1;
				ColCount = vector.Length;
				data = initMatrix<BigInteger>(RowCount, ColCount);
				for (int j = 0; j < ColCount; j++)
					data[0][j] = vector[j].Value;
			}
			else	// VectorType.COLOMN_VECTOR
			{
				RowCount = vector.Length;
				ColCount = 1;
				data = initMatrix<BigInteger>(RowCount, ColCount);
				for (int i = 0; i < RowCount; i++)
					data[i][0] = vector[i].Value;
			}
		}

		private BigZpMatrix(BigZpMatrix A)
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

		public BigZp[] ZpVector
		{
			get
			{
				BigZp[] vector = null;
				if (RowCount == 1)
				{
					vector = new BigZp[ColCount];
					for (int j = 0; j < ColCount; j++)
						vector[j] = new BigZp(Prime, data[0][j]);
				}
				else if (ColCount == 1)
				{
					vector = new BigZp[RowCount];
					for (int i = 0; i < RowCount; i++)
						vector[i] = new BigZp(Prime, data[i][0]);
				}
				return vector;
			}
		}

		public IList<BigZp> GetMatrixRow(int rowNumber)
		{
			if (RowCount <= rowNumber)
				throw new ArgumentException("Illegal  matrix  row number.");

			var wantedRow = new List<BigZp>();
			for (int j = 0; j < ColCount; j++)
				wantedRow.Add(new BigZp(Prime, data[rowNumber][j]));

			return wantedRow;
		}

		/* Create and return the transpose of the invoking matrix */
		public BigZpMatrix Transpose
		{
			get
			{
				var A = new BigZpMatrix(ColCount, RowCount, Prime);
				for (int i = 0; i < RowCount; i++)
				{
					for (int j = 0; j < ColCount; j++)
						A.data[j][i] = data[i][j];
				}
				return A;
			}
		}

		/* return C = A + B */
		public BigZpMatrix Plus(BigZpMatrix B)
		{
			var A = this;
			if ((B.RowCount != A.RowCount) || (B.ColCount != A.ColCount))
				throw new ArgumentException("Illegal  matrix  dimensions.");

			if (A.Prime != B.Prime)
				throw new ArgumentException("Trying to add Matrix  from different fields.");

			var C = new BigZpMatrix(RowCount, ColCount, A.Prime);
			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColCount; j++)
					C.data[i][j] = Modulo(A.data[i][j] + B.data[i][j]);
			}
			return C;
		}

		/* return C = A * B     : matrix    multiplication*/
		public BigZpMatrix Times(BigZpMatrix B)
		{
			var A = this;
			if (A.ColCount != B.RowCount)
				throw new ArgumentException("Illegal matrix dimensions.");

			if (A.Prime != B.Prime)
				throw new ArgumentException("Matrix  from different fields.");

			// create initialized matrix (zero value to all elements)
			var C = new BigZpMatrix(A.RowCount, B.ColCount, A.Prime);
			for (int i = 0; i < C.RowCount; i++)
			{
				for (int j = 0; j < C.ColCount; j++)
					for (int k = 0; k < A.ColCount; k++)
						C.data[i][j] = Modulo(C.data[i][j] + A.data[i][k] * B.data[k][j]);
			}
			return C;
		}

		/* calculate i mod prime */
		private BigInteger Modulo(BigInteger i)
		{
			return BigZp.Modulo(i, Prime);
		}

		/// <summary>
		/// Creates and return a random rowNum-by-colNum  matrix with values between '0' and 'prime-1'.
		/// </summary>
		public static BigZpMatrix GetRandomMatrix(int rowNum, int colNum, BigInteger prime)
		{
			var A = new BigZpMatrix(rowNum, colNum, prime);
			for (int i = 0; i < rowNum; i++)
			{
				for (int j = 0; j < colNum; j++)
					A.data[i][j] = BigZp.Modulo(StaticRandom.Next(prime), prime);
			}
			return A;
		}

		/// <summary>
		/// Create and return the N-by-N identity matrix.
		/// </summary>
		public static BigZpMatrix GetIdentityMatrix(int matrixSize, BigInteger prime)
		{
			var I = new BigZpMatrix(matrixSize, matrixSize, prime);
			for (int i = 0; i < matrixSize; i++)
				I.data[i][i] = 1;

			return I;
		}

		/* Create and return N-by-N  matrix that its first "trucToSize" elements in
		  its diagonal is "1" and the rest of the matrix is "0"*/

		public static BigZpMatrix GetTruncationMatrix(int matrixSize, int truncToSize, BigInteger prime)
		{
			var I = new BigZpMatrix(matrixSize, matrixSize, prime);
			for (int i = 0; i < truncToSize; i++)
				I.data[i][i] = 1;

			return I;
		}

		public static BigZpMatrix GetVandermondeMatrix(int rowNum, int colNum, BigInteger prime)
		{
			var A = new BigZpMatrix(rowNum, colNum, prime);

			for (int j = 0; j < colNum; j++)
				A.data[0][j] = 1;

			if (rowNum == 1)
				return A;

			for (int j = 0; j < colNum; j++)
				A.data[1][j] = j + 1;

			for (int j = 0; j < colNum; j++)
			{
				for (int i = 2; i < rowNum; i++)
					A.data[i][j] = BigZp.Modulo(A.data[i - 1][j] * A.data[1][j], prime);
			}
			return A;
		}

		public static BigZpMatrix GetVandermondeMatrix(int rowNum, IList<BigZp> values, BigInteger prime)
		{
			int colNum = values.Count;
			var A = new BigZpMatrix(rowNum, colNum, prime);

			for (int j = 0; j < colNum; j++)
				A.data[0][j] = 1;

			if (rowNum == 1)
				return A;

			for (int j = 0; j < colNum; j++)
			{
				for (int i = 1; i < rowNum; i++)
					A.data[i][j] = BigZp.Modulo(A.data[i - 1][j] * values[j].Value, prime);
			}
			return A;
		}

		public static BigZpMatrix GetSymmetricVanderMondeMatrix(int matrixSize, BigInteger prime)
		{
			return GetVandermondeMatrix(matrixSize, matrixSize, prime);
		}

		/// <summary>
		/// Returns a Vandermonde matrix in the field (each element is modulu prime).
		/// </summary>
		public static BigZpMatrix GetShamirRecombineMatrix(int matrixSize, BigInteger prime)
		{
			var A = new BigZpMatrix(matrixSize, matrixSize, prime);
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
					A.data[i][j] = BigZp.Modulo(A.data[i][j - 1] * A.data[i][1], prime);
			}
			return A;
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
		public BigZpMatrix MulMatrixByScalarsVector(int[] scalarsVector)
		{
			if (RowCount != scalarsVector.Length)
			{
				throw new ArgumentException("incompatible vector length and matrix row number.");
			}

			var B = new BigZpMatrix(this);
			for (int i = 0; i < RowCount; i++)
			{
				B.MulRowByscalar(i, scalarsVector[i]);
			}
			return B;
		}

		public BigZp[] SumMatrixRows()
		{
			var sum = new BigInteger[ColCount];

			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColCount; j++)
				{
					sum[j] += data[i][j];
				}
			}

			var zpSum = new BigZp[ColCount];
			for (int j = 0; j < ColCount; j++)
			{
				zpSum[j] = new BigZp(Prime, sum[j]);
			}

			return zpSum;
		}

		public void SetMatrixCell(int rowNumber, int colNumber, BigZp value)
		{
			if ((RowCount <= rowNumber) || (ColCount <= colNumber))
			{
				throw new ArgumentException("Illegal matrix cell.");
			}
			data[rowNumber][colNumber] = value.Value;
		}

		public static BigZpMatrix GetConcatenationMatrix(BigZpMatrix A, BigZpMatrix B)
		{
			if (A.RowCount != B.RowCount)
			{
				throw new ArgumentException("Illegal matrix dimensions - cannot perform concatenation.");
			}

			if (A.Prime != B.Prime)
			{
				throw new ArgumentException("Trying to concatenate Matrix  from different fields.");
			}

			var C = new BigZpMatrix(A.RowCount, A.ColCount + B.ColCount, A.Prime);

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