

package mpc.protocols;

import mpc.finite_field_math.Polynom;
import mpc.finite_field_math.Shamir;
import mpc.finite_field_math.Zp;
import mpc.finite_field_math.ZpMatrix;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;


public class WelchBerlekampDecoder {

        public static List<Zp> decode(List<Zp> XVlaues, List<Zp> YVlaues, int e, int polynomDeg, int prime){
                
                Polynom pPolynomial = interpolatePolynomial(XVlaues, YVlaues, e, polynomDeg, prime);
                if (pPolynomial != null)
                {
                          List<Zp> fixedCodWord = new ArrayList<Zp>();
                          for (int i = 0; i < XVlaues.size(); i ++){
                                fixedCodWord.add(pPolynomial.Sample(XVlaues.get(i)));
                          }
                          return fixedCodWord;
                }
                return null;  // decoding failed - we were unable to retrieve the original code word
        }


        private static Polynom interpolatePolynomial(List<Zp> XVlaues, List<Zp> YVlaues, int e,int polynomDeg, int prime) {

                  int n = XVlaues.size();
                  if ((e < 0) ||  (n < 2*e))   // cannot fix e errors if e <0 or  n < 2e.
                  {
                           return null;
                  }
                  if (e == 0)                 // special case e=0: then no errors are allowed,
                  {   
                        // The result is just an interpolation of X, Y. 
                      List<Zp> truncXValues = new ArrayList<Zp>();
                      List<Zp> truncYValues = new ArrayList<Zp>();
                      for (int j = 0;j < polynomDeg + 1; j++){
                            truncXValues.add(j, XVlaues.get(j));
                            truncYValues.add(j, YVlaues.get(j));
                      }
                       ZpMatrix rMatrix = ZpMatrix.getVandermondeMatrix(polynomDeg + 1, truncXValues, prime).getTransposeMatrix();
                       Zp[] yVec = new Zp[polynomDeg + 1];
                       Zp[] resultVec = rMatrix.solve(truncYValues.toArray(yVec));
                       return new Polynom(resultVec);
                  }

                // Matrix structure
               /*  We know that  N has degree at most  n-e-1,
                    E has degree exactly e and is monic( highest degree coeff is 1 ),
                    and N(X[i])=Y[i]E(X[i]) for all i.  Thus, we need to solve
                    the following matrix equation to find the coefficients of N and E:

                    [1 x0 x0^2 ... x0^(n-e-1) -y0 -y0x0 -y0x0^2 ... -y0x0^(e-1)]         [N0      ]                                             [y0x0^e]
                    [1 x1 x1^2 ... x1^(n-e-1) -y1 -y1x1 -y1x1^2 ... -y1x1^(e-1)]         [N1      ]                                             [y1x1^e]
                    ...                                                                                                                            ...                                                       ...
                    ...                                                                                                                           [N(n-e-1)]                                        ...
                    ...                                                                                                                           [E0      ]                    =                       ...
                    ...                                                                                                                           [E1      ]                                              ...
                    ...                                                                                                                             ...                                                      ...
                    [1 xm xm^2 ... xm^(n-e-1) -ym -ymxm -ymxm^2 ... -ymxm^(e-1)] [E(e-1)  ]                                          [ymxm^e]

                    where  m = n - 1
                  */
                  ZpMatrix A = getWelchBerlekampMatrix(XVlaues, YVlaues, n, e, prime);         // the matrix to hold the linear system we'll solve
                  Zp[] b = getWelchBerlekampConstraintVector(XVlaues, YVlaues, n, e, prime);

                  // coefficients of N and E as one vector
                   Zp[] NE = LinearSolve(A, new ZpMatrix(b, ZpMatrix.VectorType.COLOMN_VECTOR), prime);
                   if (NE != null)
                   {
                          Zp[] N = new Zp[n - e];
                          Zp[] E = new Zp[e + 1];
                          for (int i = 0; i < n - e ; i++){
                                N[i] = new Zp(NE[i]);
                          }
                          for (int i = n - e; i < n; i++){
                                E[i - (n - e)] = new Zp(NE[i]);
                          }
                          // Constraint coeef - E has degree exactly e and is monic (shoudn't be zero)
                          E[e] = new Zp(prime, 1);

                          return  new Polynom(Arrays.asList(N)).divideWithRemainder(new Polynom(Arrays.asList(E)));
                   }
                   return null;
        }

        
        private static ZpMatrix getWelchBerlekampMatrix(List<Zp> XVlaues, List<Zp> YVlaues, int n, int e, int prime){
            ZpMatrix NVanderMonde = ZpMatrix.getVandermondeMatrix(n - e, XVlaues, prime).getTransposeMatrix();
            ZpMatrix EVanderMonde = ZpMatrix.getVandermondeMatrix(e, XVlaues, prime).getTransposeMatrix();

            int[] scalarVector = new int[YVlaues.size()];
            int i = 0;
            for (Zp zp: YVlaues){
                scalarVector[i++] = - zp.getValue();
            }
            EVanderMonde = EVanderMonde.mulMatrixByScalarsVector(scalarVector);
            return ZpMatrix.getConcatenationMatrix(NVanderMonde, EVanderMonde);
        }


        private static Zp[] getWelchBerlekampConstraintVector(List<Zp> XVlaues, List<Zp> YVlaues, int n, int e, int prime){
                Zp[] bVector = new Zp[n];
                for (int i = 0; i < n; i++){
                        bVector[i] = new Zp (prime, Zp.calculatePower(XVlaues.get(i).getValue(), e, prime) * YVlaues.get(i).getValue());
                }
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
    private static Zp[]  LinearSolve (ZpMatrix A, ZpMatrix B , int prime) {
      int[] invArray = Zp.getFieldInverseArr(prime);
      ZpMatrix C = ZpMatrix.getConcatenationMatrix(A, B);
      int n = C.getRowNumber();
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

      int rank=C.gauss();

      int[][] cContent = C.getContent();
      // can start at rank-1, because below that are all zeroes
      for (int row = rank - 1; row >=0; row--) {
           // remove all the known variables from the equation
           temp = cContent[row][n];
           int col;
           for (col = n-1; col>=firstDeterminedValue; col-- ){
               temp = Zp.modulo(temp - (cContent[row][col] * solution[col]), prime);
           }

            // now we need to find the first nonzero coefficient in this row
            // if it exists before firstDeterminedValue
            // because the matrix is in row echelon form, the first nonzero
            // coefficient cannot be before the diagonal
           for (col=row; col<firstDeterminedValue; col++){
                    if (cContent[row][col] != 0)
                    {
                            break;
                    }
           }

        if (col < firstDeterminedValue)  // this means we found a nonzero coefficient
        {
              // we can determine the variables in position from col to firstDeterminedValue
              // if this loop executes even once, then the system is undertermined
              // we arbitrarily set the undetermined variables to 0, because it make math easier
              for (int j = col+1; j<firstDeterminedValue; j++){
                    solution[j] = 0;
              }  
              // Now determine the variable at the nonzero coefficient
              //div(solution[col], temp, A.getContent()[row][col]);
              solution[col] = temp * invArray[Zp.modulo(cContent[row][col], prime)];
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
      for (int col=0; col<firstDeterminedValue; col++){
            solution[col] = 0;
      }
      Zp[] ResultVec = new Zp[n];
      for (int i = 0; i < n; i ++ ){
          ResultVec[i] = new Zp(prime, solution[i]);
      }
      
      return ResultVec;
    }


       public static void main(String[] args) {

           int numberOfPlayers , polynomDeg, prime = 1231;
            Zp secret =new Zp (prime, 3);
            for (int j = 5; j < 500; j++){
                    numberOfPlayers = j; 
                    polynomDeg =(j-1)/4;
                    List<Zp> shares = Shamir.primitiveShare(secret, numberOfPlayers, polynomDeg);
                    List<Zp> sharesCopy = new ArrayList<Zp>();
                    for(Zp zp: shares){
                        sharesCopy.add(new Zp(zp));
                    }
                    for (int i = 0; i < polynomDeg; i++){
                         sharesCopy.get(i).setValue( (int ) (Math.random() * prime));
                    }
                    int w = Zp.getFieldMinimumPrimitive(prime);
                    List<Zp> XValues = new ArrayList<Zp>();
                    for (int i = 0; i < numberOfPlayers; i++){
                        XValues.add(new Zp(prime, Zp.calculatePower(w, i, prime)));
                    }

                    List<Zp> retCodeWord = decode(XValues, sharesCopy, polynomDeg, polynomDeg, prime);
                    System.out.println("Checking num of players : " + j + "  and poly deg is : " + polynomDeg );
                    for (int i = 0; i < shares.size(); i++){
                            if (! shares.get(i).equals(retCodeWord.get(i)))
                            {
                                    System.out.println("FAILED !!!");
                                    return;
                            }
                    }
            }

       }


}
