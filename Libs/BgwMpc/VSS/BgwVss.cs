using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MpcLib.Common.FiniteField;
using MpcLib.DistributedSystem.SecretSharing;

namespace MpcLib.DistributedSystem.Mpc.Bgw.Vss
{
	public class BgwVss
	{
		/// <summary>
		/// Creates a random polynomial f(x,y) and then to create from it for
		/// the i-th player two polynomials : fi(x) = f(x,w^i) and gi(y) = f(w^i,y).
		/// </summary>
		public static IList<SecretPolynomials> ShareByzantineCase(Zp secret,
			int numPlayers, int polynomDeg)
		{
			if (numPlayers <= 4 * polynomDeg)
				throw new System.ArgumentException("Cannot use Byzantine algoritm -- numberOfPlayers <= 4*polynomDeg - " + "use regular computation instead");

			// Creating the Random Polynomial - f(x , y)
			// Note : there are (t+1)^2 coefficiet for the polynomial including the free coefficient (the secret)
			// first  row  coef are of  (x^0,x^1,x^2,...,x^t)y^0, second  row  coef are (x^0, x1,...,x^t)y^1 and so forth...
			var randomMatrix_f_xy = ZpMatrix.GetRandomMatrix(polynomDeg + 1, polynomDeg + 1, secret.Prime);
			randomMatrix_f_xy.SetMatrixCell(0, 0, secret);
			var polynomialShares = new List<SecretPolynomials>();

			for (int i = 0; i < numPlayers; i++)
			{
				var pSecret = new SecretPolynomials();
				pSecret.Fi_xPolynomial = GenerateF_i_xPolynomial(randomMatrix_f_xy, secret, i);
				pSecret.Gi_yPolynomial = GenerateG_i_yPolynomial(randomMatrix_f_xy, secret, i);
				polynomialShares.Add(pSecret);
			}
			return polynomialShares;
		}

		private static IList<Zp> GenerateF_i_xPolynomial(ZpMatrix f_x_y,
			Zp secret, int playerNum)
		{
			int w = NumTheoryUtils.GetFieldMinimumPrimitive(secret.Prime);
			int w_i = NumTheoryUtils.ModPow(w, playerNum, secret.Prime);

			var y_values = new int[f_x_y.ColCount];
			for (int i = 0; i < f_x_y.ColCount; i++)
			{
				y_values[i] = NumTheoryUtils.ModPow(w_i, i, secret.Prime);
			}
			return f_x_y.MulMatrixByScalarsVector(y_values).SumMatrixRows();
		}

		private static IList<Zp> GenerateG_i_yPolynomial(ZpMatrix f_x_y,
			Zp secret, int playerNum)
		{
			int w = NumTheoryUtils.GetFieldMinimumPrimitive(secret.Prime);
			int w_i = NumTheoryUtils.ModPow(w, playerNum, secret.Prime);

			var x_values = new Zp[f_x_y.RowCount];
			for (int i = 0; i < f_x_y.RowCount; i++)
			{
				x_values[i] = new Zp(secret.Prime, NumTheoryUtils.ModPow(w_i, i, secret.Prime));
			}

			var tempArr = f_x_y.Times(new ZpMatrix(x_values, VectorType.Column)).ZpVector;
			return tempArr;
		}
	}
}