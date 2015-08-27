/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.finite_field_math;

import java.util.ArrayList;
import java.util.List;




final public  class  ZpMatrix {

    public enum VectorType { ROW_VECTOR, COLOMN_VECTOR }

    private final int  rowNum;             // number of rows
    private final int  colNum;             // number of column
    private final int  prime;
    private final int[][] data;               // rowNum-by-colNum array


    
    /*Create M-by-N matrix of zero initialized elements */
    public  ZpMatrix(int rowNum, int colNum, int prime) {
        this.rowNum = rowNum;
        this.colNum = colNum;
        this.prime = prime;
        data = new int[rowNum][colNum];
    }


    /* Create matrix based on 2d array of integers*/
    public  ZpMatrix(int[][] data, int prime) {
        this.rowNum = data.length;
        this.colNum = data[0].length;
        this.prime = prime;
        this.data = new int[rowNum][colNum];

        for (int i = 0; i < rowNum; i++)
            for (int j = 0; j < colNum; j++)
                    this.data[i][j] = data[i][j] ;
    }

      //Creates a  vector matrix from Zp array
     public  ZpMatrix(Zp[] vector,VectorType   vec_type) {

         this.prime = vector[0].prime;
         if (vec_type.equals(VectorType.ROW_VECTOR))
         {
                this.rowNum = 1;
                this.colNum = vector.length;
                this.data = new int[rowNum][colNum];
                for (int j = 0; j < colNum; j++)
                            this.data[0][j] = vector[j].getValue();
         }
         else  //VectorType.COLOMN_VECTOR
         {
                this.rowNum = vector.length;
                this.colNum = 1;
                this.data = new int[rowNum][colNum];
                for (int i = 0; i < rowNum; i++)
                            this.data[i][0] = vector[i].getValue();
         }
    }




    /*Copy CTOR*/
    private  ZpMatrix(ZpMatrix A) {
        this(A.data, A.prime);
    }

   public  int getPrime() {
        return prime;
   }
  
   public  int getRowNumber() {
        return rowNum;
   }

   public  int getColNumber() {
        return colNum;
   }

   public  int[][] getContent() {
           return data;
   }

   
   public Zp[] getZpVector(){
       Zp[] vector = null;
       if (rowNum == 1)
       {
            vector  = new Zp[colNum];
            for(int j = 0;j < colNum; j++)
                    vector[j] = new Zp(prime, data[0][j]);
       }
       else if(colNum == 1 )
       {
            vector  = new Zp[rowNum];
            for(int i = 0;i < rowNum; i++)
                    vector[i] = new Zp(prime, data[i][0]);
       }
       return vector;
   }

      public List<Zp> getMatrixRow(int rowNumber){
          if ( rowNum <= rowNumber )
              throw new IllegalArgumentException("Illegal  matrix  row number.");
             List<Zp> wantedRow = new ArrayList();
            for(int j = 0;j < colNum; j++){
                    wantedRow.add( new Zp(prime, data[rowNumber][j]) );
            }
            return wantedRow;
    }

    /* Create and return the transpose of the invoking matrix */
    public ZpMatrix   getTransposeMatrix() {
        ZpMatrix A = new ZpMatrix(colNum, rowNum, this.prime);
        for (int i = 0; i < rowNum; i++)
            for (int j = 0; j < colNum; j++)
                A.data[j][i] = this.data[i][j];
      
        return A;
    }

    /* return C = A + B */
    public ZpMatrix   plus(ZpMatrix B) {
        ZpMatrix A = this;
        if ((B.rowNum != A.rowNum) || (B.colNum != A.colNum))
             throw new IllegalArgumentException("Illegal  matrix  dimensions.");
        if (A.prime != B.prime)
             throw new IllegalArgumentException("Trying to add Matrix  from different fields.");

        ZpMatrix C = new  ZpMatrix(rowNum, colNum, A.prime);
        for (int i = 0; i < rowNum; i++)
            for (int j = 0; j < colNum; j++)
                C.data[i][j] = modulo(A.data[i][j] + B.data[i][j]);
        
        return C;
    }


    /* return C = A * B     : matrix    multiplication*/
    public ZpMatrix  times(ZpMatrix B) {
        ZpMatrix A = this;
        if (A.colNum != B.rowNum)
             throw new IllegalArgumentException("Illegal matrix dimensions.");

        if (A.prime != B.prime)
             throw new IllegalArgumentException("Matrix  from different fields.");

        //create initialized matrix (zero value to all elements)
        ZpMatrix C = new  ZpMatrix(A.rowNum, B.colNum, A.prime);
        for (int i = 0; i < C.rowNum; i++)
            for (int j = 0; j < C.colNum; j++)
                for (int k = 0; k < A.colNum; k++)
                       C.data[i][j] =modulo(C.data[i][j] + A.data[i][k] * B.data[k][j])  ;
        
        return C;
    }

    

    /* does A = B ? */
    public boolean eq(ZpMatrix B) {
        ZpMatrix A = this;
        if ((B.rowNum != A.rowNum) || (B.colNum != A.colNum) || (A.prime != B.prime))
        {
            return false;
        }
        for (int i = 0; i < rowNum; i++)
            for (int j = 0; j < colNum; j++)
                if ( A.data[i][j] != B.data[i][j] )
                    return false;
        return true;
    }


public  Zp[]   solve(Zp[] vector) {
        ZpMatrix vecMatrix = new ZpMatrix(vector, ZpMatrix.VectorType.COLOMN_VECTOR);
        ZpMatrix  revA = this.getInverse();
        return revA.times(vecMatrix).getZpVector();
}


// return x = (A^-1) b, assuming A is square and has full rank (not singular)
public  ZpMatrix   solve(ZpMatrix vec) {
        ZpMatrix  revA = this.getInverse();
        return revA.times(vec);
}


  /* r  -   Array of row indices,  j0 -   Initial column index,  j1 -   Final column index
      return     A(r(:),j0:j1)  */
  private  ZpMatrix  getSubMatrix(int[] r, int j0, int j1) {
      ZpMatrix X = new ZpMatrix(r.length ,j1 - j0 + 1, prime);
      int[][] B = X.data;
      
      for (int i = 0; i < r.length; i++) {
          for (int j = j0; j <= j1; j++) {
              B[i][j - j0] = data[r[i]][j];
          }
      }
      return X;
  }


    /* swap rows i and j in the matrix*/
    private void swapRows(int i, int j) {
       int[] temp = data[i];
        data[i] = data[j];
        data[j] = temp;
    }

    
    /*calculate i mod prime */
    private int modulo (int i){
            return  Zp.modulo(i, prime);
    }
    


/*return the inverse matrix of the invoking matrix*/
public  ZpMatrix  getInverse(){
        int[] piv = new int[rowNum];
        int[] fieldInv = Zp.getFieldInverseArr(prime);
        ZpMatrix lu = this.getLUDecomposition(piv , fieldInv);
        return  lu.solveInv(ZpMatrix.getIdentityMatrix(rowNum, prime), piv, fieldInv);
}


private  boolean isNonsingular() {
      for (int i = 0; i < rowNum; i++) {
              if (data[i][i] == 0)
                  return false;
      }
      return true;
}



 private  ZpMatrix  solveInv(ZpMatrix B, int[] piv, int[] fieldInv) {
          if ( B.rowNum != rowNum ) {
                      throw new IllegalArgumentException( "Matrix row dimensions must agree." );
          }
          if ( !this.isNonsingular() ) {
                    throw new IllegalArgumentException( "Matrix is singular." );
          }

          // Copy right hand side with pivoting
          int nx = B.colNum;
          ZpMatrix Xmat = B.getSubMatrix(piv, 0, nx - 1);
          int[][] X = Xmat.data;

          // Solve L*Y = B(piv,:)
          for (int k = 0; k < rowNum; k++) {
              for (int i = k + 1; i < rowNum; i++) {
                  for (int j = 0; j < nx; j++) {
                      X[i][j] = modulo(X[i][j] - X[k][j] * data[i][k]);
                  }
              }
          }
          // Solve U*X = Y;
          for (int k = rowNum - 1; k >= 0; k--) {
              for (int j = 0; j < nx; j++) {
                  X[k][j] = modulo(X[k][j] * fieldInv[data[k][k]]);
              }
              for (int i = 0; i < k; i++) {
                  for (int j = 0; j < nx; j++) {
                      X[i][j] = modulo(X[i][j] - X[k][j] * data[i][k]);
                  }
              }
          }
          return Xmat;
      }
  

    private   ZpMatrix  getLUDecomposition(int[] pivot ,int[] fieldInv) {

            // Use a "left-looking", dot-product, Crout/Doolittle algorithm.
            ZpMatrix LU = new ZpMatrix(this);
           int [][] LUArr = LU.data;
           
           int[] piv = pivot;
           for (int i = 0; i < rowNum; i++) {
                  piv[i] = i;
           }
           int pivsign = 1;
           int[] LUrowi;
           int[] LUcolj = new int[rowNum];

           // Outer loop.
           for (int j = 0; j < rowNum; j++) {

               // Make a copy of the j-th column to localize references.
               for (int i = 0; i < rowNum; i++) {
                   LUcolj[i] = LUArr[i][j];
               }

               // Apply previous transformations.
               for (int i = 0; i < rowNum; i++) {
                   LUrowi = LUArr[i];

                   // Most of the time is spent in the following dot product.
                   int kmax = Math.min(i, j);
                   int s = 0;
                   for (int k = 0; k < kmax; k++) 
                           s = modulo(s + LUrowi[k] * LUcolj[k]);
                  
                   LUrowi[j] = LUcolj[i] = modulo(LUcolj[i] - s);
               }

               // Find pivot and exchange if necessary.
               int p = j;
               for (int i = j + 1; i < rowNum; i++) {
                   if ((LUcolj[i]) >(LUcolj[p])) {
                       p = i;
                   }
               }
               if (p != j) {
                   for (int k = 0; k < rowNum; k++) {
                       int t = LUArr[p][k];
                      LUArr[p][k] = LUArr[j][k];
                      LUArr[j][k] = t;
                  }
                  int k = piv[p];
                  piv[p] = piv[j];
                  piv[j] = k;
                  pivsign = -pivsign;
              }

              // Compute multipliers.
              if (j < rowNum & LUArr[j][j] != 0) {
                  for (int i = j + 1; i < rowNum; i++) {
                      LUArr[i][j] = modulo(LUArr[i][j]  * fieldInv[modulo(LUArr[j][j])]);
                  }
              }
          }
          return  LU;
      }



    /*Create and return a random rowNum-by-colNum  matrix with values between '0' and 'prime-1' */
    public static ZpMatrix  getRandomMatrix(int rowNum, int colNum, int prime) {
        ZpMatrix A = new ZpMatrix(rowNum, colNum, prime);
        for (int i = 0; i < rowNum; i++)
            for (int j = 0; j < colNum; j++)
               A.data[i][j]= Zp.modulo((int) ( Math.random() * (prime)), prime);

        return A;
   }


    /* Create and return the N-by-N identity matrix */
    public static  ZpMatrix  getIdentityMatrix(int matrixSize, int prime) {
        ZpMatrix I = new ZpMatrix(matrixSize, matrixSize, prime);
        for (int i = 0; i < matrixSize; i++)
             I.data[i][i] = 1;

        return I;
    }

    /* Create and return N-by-N  matrix that its first "trucToSize" elements in
      its diagonal is "1" and the rest of the matrix is "0"*/
    public static  ZpMatrix  getTruncationMatrix(int  matrixSize, int truncToSize ,int prime) {
        ZpMatrix  I = new ZpMatrix(matrixSize, matrixSize, prime);
        for (int i = 0; i < truncToSize; i++)
            I.data[i][i] = 1;

        return I;
    }


public static ZpMatrix  getVandermondeMatrix(int rowNum, int colNum, int prime) {
        ZpMatrix A = new ZpMatrix(rowNum, colNum ,prime);
        
        for (int j = 0; j < colNum; j++)
                A.data[0][j] = 1;

        if (rowNum == 1) 
        {
            return A;
        }

        for (int j = 0; j < colNum; j++)
                A.data[1][j] = j + 1;

        for (int j = 0; j < colNum; j++)
            for (int i = 2; i < rowNum; i++)
                A.data[i][j] = Zp.modulo(A.data[i - 1][j] * A.data[1][j], prime);

        return A;
}

public static ZpMatrix  getVandermondeMatrix(int rowNum, List<Zp> values, int prime){
    int colNum = values.size();
    ZpMatrix A = new ZpMatrix(rowNum, colNum ,prime);

        for (int j = 0; j < colNum; j++)
                A.data[0][j] = 1;

        if (rowNum == 1)
        {
            return A;
        }
       
        for (int j = 0; j < colNum; j++)
            for (int i = 1; i < rowNum; i++)
                A.data[i][j] = Zp.modulo(A.data[i - 1][j] * values.get(j).getValue(), prime);

        return A;
}


 public static ZpMatrix  getSymmetricVandermondeMatrix(int matrixSize, int prime) {
            return getVandermondeMatrix(matrixSize, matrixSize, prime);
 }



   public static ZpMatrix  getShamirRecombineMatrix(int matrixSize, int prime) {

        ZpMatrix A = new ZpMatrix(matrixSize, matrixSize ,prime);
        if (matrixSize == 1)
        {
                A.data[0][0] = 1;
                return  A;
        }

        for (int i = 0; i < matrixSize; i++)
                A.data[i][0] = 1;

        for (int i = 0; i < matrixSize; i++)
                A.data[i][1] = i + 1;

        for (int i = 0; i < matrixSize; i++)
            for (int j = 2; j < matrixSize; j++)
                 A.data[i][j] = Zp.modulo(A.data[i][j- 1] * A.data[i][1], prime);
       return A;
    }




    public static ZpMatrix  getPrimitiveVandermondeMatrix(int rowNum, int colNum, int prime) {

        int primitive = Zp.getFieldMinimumPrimitive(prime);
        if (primitive == 0)
                      throw new IllegalArgumentException( "Cannot create a primitive Vandermonde matrix from a non-prime number. " );

        ZpMatrix A = new ZpMatrix(rowNum, colNum ,prime);

        for (int j = 0; j < colNum; j++)
                A.data[0][j] = 1;

        if (rowNum == 1)
        {
            return A;
        }
        
        /*  This variable represents  primitive^j  for the j-th player*/
        int primitive_j = 1;
        for (int j = 0; j < colNum; j++)
        {
                A.data[1][j] =  primitive_j;
                primitive_j = Zp.modulo(primitive_j * primitive, prime);
        }            

        for (int j = 0; j < colNum; j++)
            for (int i = 2; i < rowNum; i++)
                A.data[i][j] = Zp.modulo(A.data[i - 1][j] * A.data[1][j], prime);

        return A;
}


 public static ZpMatrix  getSymmetricPrimitiveVandermondeMatrix(int matrixSize, int prime) {
            return getPrimitiveVandermondeMatrix(matrixSize, matrixSize, prime);
 }

 // Change the name !!!!
 public ZpMatrix  removeRowsFromMatrix(boolean[]  toRemove){
        if (this.rowNum != toRemove.length)
                     throw new IllegalArgumentException("Illegal row number.");
       int numOfRowsToRemove = 0;
       for (int i = 0; i < toRemove.length; i++)
       {
            numOfRowsToRemove += toRemove[i] ? 1 : 0 ;
       }
       int[][] dataCopy = new int[rowNum - numOfRowsToRemove][colNum - numOfRowsToRemove];
       int rowIndex = 0;
       for (int i = 0; i < rowNum; i++)
       {
            if (toRemove[i])
            {
                    continue;
            }
            for (int j = 0; j < colNum - numOfRowsToRemove; j++)
            {
                   dataCopy[rowIndex][j] = data[i][j] ;
            }
            rowIndex++;
        }
       return new ZpMatrix(dataCopy, prime);
 }
 
 public ZpMatrix removeRowFromMatrix(int index)
 {
    if ( (index < 0) || (index > rowNum-1) )
    {
        throw new IllegalArgumentException("Illegal row number.");
    }
    
    int[][] dataCopy = new int[rowNum - 1][colNum];
    for (int i = 0; i < rowNum-1; i++)
    {
        if (i == index)
        {
            continue;
        }
        for (int j=0; j<colNum-1; j++)
        {
            dataCopy[i][j] = data[i][j];
        }
    }
    return new ZpMatrix(dataCopy,prime);
 }
 
  public ZpMatrix removeColFromMatrix(int index)
 {
    if ( (index < 0) || (index > colNum-1) )
    {
        throw new IllegalArgumentException("Illegal col number.");
    }
    
    int[][] dataCopy = new int[rowNum][colNum-1];
    for (int i = 0; i < colNum-1; i++)
    {
        if (i == index)
        {
            continue;
        }
        for (int j=0; j<rowNum-1; j++)
        {
            dataCopy[j][i] = data[j][i];
        }
    }
    return new ZpMatrix(dataCopy,prime);
 }
  
  public int Determinant()
  {
     if ((rowNum == 1) && (colNum == 1))
     {
        return data[0][0];
     }
     
     Zp det = new Zp(prime,0);
     
     for (int i=0; i<colNum; i++)
     {
         int SubDet = removeRowFromMatrix(0).removeColFromMatrix(i).Determinant();
         Zp  SubDetAi = new Zp(prime, SubDet*data[0][i]);
         if (i%2 == 0)
         {
             det.add(SubDetAi);
         }
         else
         {
             det.sub(SubDetAi);
         }
     }
     
     return det.getValue();
  }


 private void mulRowByscalar (int rowNumber, int scalar){
        if (this.rowNum <= rowNumber)
             throw new IllegalArgumentException("Illegal matrix row number.");

        for (int j = 0;j < colNum;j++)
        {
            data[rowNumber][j] = modulo(data[rowNumber][j] * scalar);
        }      
 }

 /* Multiplying each row by different scalar from the scalars vector */
  public  ZpMatrix  mulMatrixByScalarsVector (int[] scalarsVector){
        if (this.rowNum != scalarsVector.length)
             throw new IllegalArgumentException("incompatible vector length and matrix row number.");

        ZpMatrix B = new ZpMatrix(this);
        for (int i = 0;i < rowNum;i++)
        {
            B.mulRowByscalar(i, scalarsVector[i]);
        }
        return B;
 }


public Zp[]  SumMatrixRows(){

    int[] sum = new int[colNum];

    for (int i = 0; i< rowNum; i++)
    {
        for (int j = 0; j< colNum; j ++)
        {
            sum[j] += data[i][j];
        }
    }

    Zp[] zpSum = new Zp[colNum];
    for (int j = 0; j < colNum; j++)
    {
        zpSum[j] = new Zp(prime, sum[j]);
    }

    return zpSum;
}

public void setMatrixCell(int rowNumber, int colNumber, Zp value){
        if ( (this.rowNum <= rowNumber) || (this.colNum <= colNumber) )
             throw new IllegalArgumentException("Illegal matrix cell.");

        data[rowNumber][colNumber] = value.getValue();
}


  public  static ZpMatrix  getConcatenationMatrix(ZpMatrix A, ZpMatrix B) {
      
      if (A.rowNum != B.rowNum)
            throw new IllegalArgumentException("Illegal matrix dimensions - cannot perform concatenation.");

      if (A.prime != B.prime)
             throw new IllegalArgumentException("Trying to concatenate Matrix  from different fields.");

      ZpMatrix C = new ZpMatrix(A.rowNum, A.colNum + B.colNum, A.prime);

      // Copy A
      for (int i = 0; i < A.rowNum; i++){
            for (int j = 0; j < A.colNum; j++){
                    C.data[i][j] = A.data[i][j];
            }
      }
      // Copy B
      for (int i = 0; i < A.rowNum; i++){
            for (int j = A.colNum; j < A.colNum + B.colNum; j++){
                    C.data[i][j] = B.data[i][j - A.colNum];
            }
      }
      return C;
  }


    public  int gauss(){
       
        int invArr[] = Zp.getFieldInverseArr(prime);

        // Gaussian elimination with partial pivoting
        int i, j;
        i = j = 0;
        while( (i < rowNum) && (j < colNum - 1))
         {
            // find pivot row and swap
            int max = i;
            for (int k = i + 1; k < rowNum; k++){
                if (this.data[k][j]  >  this.data[max][j])
                {
                    max = k;
                }
            }
                
            if (this.data[max][j] != 0)
            {
                    this.swapRows(i, max);
                    int toMul = invArr[this.data[i][j]];
                    for (int k = 0; k < colNum; k++){
                            this.data[i][k] = modulo(this.data[i][k] * toMul);
                    }

                    for (int u = i + 1; u < rowNum; u++)
                    {
                        int  m = modulo(this.data[u][j]);
                        for (int v = 0; v < colNum; v++)
                        {
                            this.data[u][v] = modulo(this.data[u][v] - this.data[i][v]  *  m);
                        }
                        this.data[u][j] = 0;
                    }
                i++;
            }
            j++;
        }

       // Get number of lines differrent from zero
        int num = 0;

        for (int k = 0; k < rowNum; k++){
                for (int v = 0; v < colNum; v++){
                        if (this.data[k][v] != 0){
                            num++;
                            break;
                        }
                }
        }
        return num;
    }



    /* Print matrix to standard output */
    public void print() {
        for (int i = 0; i < rowNum; i++) {
            for (int j = 0; j < colNum; j++)
                System.out.printf(data[i][j] + "   ");
            System.out.println();
        }
        System.out.println();
    }

//    // test     matrix
    public static void main(String[] args) {

        
        int prime = 7, numOfPlayers = 5, polyDeg = 1;
        int[][] data1 = {{2,2,2,2,2},{1,1,1,2,2},{0,0,2,1,3}};
        ZpMatrix mt = new ZpMatrix(data1, prime);
        //mt.print();
        int num0 = mt.gauss();
        mt.print();
        Zp secret = new Zp(prime, 9);
        List<Zp> shares = Shamir.primitiveShare(secret, numOfPlayers, polyDeg);

       ZpMatrix prim = getSymmetricPrimitiveVandermondeMatrix(numOfPlayers,  prime);
       ZpMatrix res = new ZpMatrix((Zp[]) shares.toArray(), VectorType.COLOMN_VECTOR);
        ZpMatrix concat = ZpMatrix.getConcatenationMatrix(prim, res);
        int num = concat.gauss();

        ZpMatrix regVanderMatrix = getSymmetricVandermondeMatrix(4, 5);
        //check solving algoritm
        int till = 100;
        for (int i = 2; i < till; i++)
        {
            for (int j = 1; j <= i; j++)
            {
                     if (Zp.isPrime(i))
                     {
                            System.out.println("current prime is : " + i +"  and matrix size is :" + j);
                            ZpMatrix  A = ZpMatrix.getShamirRecombineMatrix(j, i);
                            ZpMatrix  b = ZpMatrix.getRandomMatrix(j, 1, i);
                            ZpMatrix  x = A.solve(b);
                            if (!b.eq(A.times(x)))
                            {
                                System.out.println("failed !!!!");
                                return;
                            }
                     }


            }
        }

        //check inverse&times&eq functions - a long time (change 500 to 100 for fast execution)
        till = 200;
        for (int i = 2; i < till; i++)
        {
                    for (int j = 1; j <= i; j++)
                    {
                            if (Zp.isPrime(i))
                            {
                                    System.out.println("current prime is : " + i +"  and matrix size is :" + j);
                                    ZpMatrix A = ZpMatrix.getSymmetricVandermondeMatrix(j, i);
                                    ZpMatrix B = A.times(A.getInverse());
                                    if (!B.eq(ZpMatrix.getIdentityMatrix(B.rowNum, B.prime)))
                                    {
                                            System.out.println("failed !!!!");
                                            return;
                                    }
                            }
                    }
             }


    }
}



