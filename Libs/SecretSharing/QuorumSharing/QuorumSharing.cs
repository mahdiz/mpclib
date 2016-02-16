using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpcLib.Common.FiniteField;

namespace MpcLib.SecretSharing
{
    public class QuorumSharing
    {
        /// <summary>
        /// Creates new shares that should be distributed to each of the parties in the new quorum
        /// </summary>
        public static IList<Zp> CreateReshares(Zp secret, int newQuorumSize, int polyDeg)
        {
            IList<Zp> coeffs;
            return ShamirSharing.Share(secret, newQuorumSize, polyDeg, out coeffs);
        }
        
        public static IList<Zp> CreateReshares(Zp secret, int newQuorumSize, int polyDeg, out IList<Zp> coeffs)
        {
            return ShamirSharing.Share(secret, newQuorumSize, polyDeg, out coeffs);
        }

        /// <summary>
        /// Each party in the new quorum needs to call this with the shares received from the old quorum to calculate its share
        /// </summary>
        public static Zp CombineReshares(IList<Zp> reshares, int newQuorumSize, int prime)
        {
            int oldQuorumSize = reshares.Count;
            if (oldQuorumSize != newQuorumSize)
                throw new System.ArgumentException("Do not support case where quorums are of different sizes");

            // Compute the first row of the inverse Vandermonde matrix
            var vandermonde = ZpMatrix.GetVandermondeMatrix(oldQuorumSize, newQuorumSize, prime);
            var vandermondeInv = vandermonde.Inverse.GetMatrixColumn(0);
            
            var S = new Zp(prime);
            for (var i = 0; i < newQuorumSize; i++)
                S += vandermondeInv[i] * reshares[i];

            return S;
        }
    }
}
