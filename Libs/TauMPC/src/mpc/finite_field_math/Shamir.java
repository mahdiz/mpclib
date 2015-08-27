/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package mpc.finite_field_math;

import mpc.sendables.SecretPolynomials;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;


public class Shamir {

    /* Should evaluate the shared secrets of secret with polynom
    of degree t and numberOfPlayers players  */
    private  static List<Zp> share(Zp secret, int numberOfPlayers, int polynomDeg, boolean  usePrimitiveShare) {

        if (numberOfPlayers <= polynomDeg) {
            throw new IllegalArgumentException("Polynomial degree cannot be bigger or equal to the number of  players");
        }
        //Creating the Random Polynomial - f(x)
        ZpMatrix randomMatrix = ZpMatrix.getRandomMatrix(1, polynomDeg + 1, secret.prime);
        //The free variable in the Random Polynomial( f(x) ) is the seceret
        randomMatrix.setMatrixCell(0, 0, secret);

        //Create vanderMonde matrix
        ZpMatrix vanderMonde;
        if (usePrimitiveShare){
            vanderMonde = ZpMatrix.getPrimitiveVandermondeMatrix(polynomDeg + 1, numberOfPlayers, secret.prime);
        } else {
            vanderMonde = ZpMatrix.getVandermondeMatrix(polynomDeg + 1, numberOfPlayers, secret.prime);
        }

        //compute f(i) for the i-th  player
        Zp[] sharesArr = randomMatrix.times(vanderMonde).getZpVector();

        //List<Zp> sharesList = new ArrayList<Zp>();
        return Arrays.asList(sharesArr);
    }

    /* Should evaluate the shared secrets of secret with polynom
    of degree t and numberOfPlayers players  */
    public  static ShareDetails detailedShare(Zp secret, int numberOfPlayers, int polynomDeg) {

        if (numberOfPlayers <= polynomDeg) {
            throw new IllegalArgumentException("Polynomial degree cannot be bigger or equal to the number of  players");
        }
        //Creating the Random Polynomial - f(x)
        ZpMatrix randomMatrix = ZpMatrix.getRandomMatrix(1, polynomDeg + 1, secret.prime);
        //The free variable in the Random Polynomial( f(x) ) is the seceret
        randomMatrix.setMatrixCell(0, 0, secret);

        //Create vanderMonde matrix
        ZpMatrix vanderMonde = ZpMatrix.getPrimitiveVandermondeMatrix(polynomDeg + 1, numberOfPlayers, secret.prime);
        
        //compute f(i) for the i-th  player
        Zp[] sharesArr = randomMatrix.times(vanderMonde).getZpVector();

        ShareDetails details = new ShareDetails(randomMatrix.getMatrixRow(0), Arrays.asList(sharesArr));

        return details;
    }



    public static List<Zp> share(Zp secret, int numberOfPlayers, int polynomDeg) {
                return share(secret, numberOfPlayers, polynomDeg, false);
    }


    public static List<Zp> primitiveShare(Zp secret, int numberOfPlayers, int polynomDeg) {
            return share(secret, numberOfPlayers, polynomDeg, true);
    }

    /* This function should create a random polynomial f(x,y) and then to create from it for
     * the i-th player two polynomials : fi(x) = f(x,w^i) and gi(y) = f(w^i,y) */
    public static List<SecretPolynomials> shareByzantineCase(Zp secret, int numberOfPlayers, int polynomDeg) {

       if (numberOfPlayers <= 4 * polynomDeg) {
            throw new IllegalArgumentException("Cannot use Byzantine algoritm -- numberOfPlayers <= 4*polynomDeg - " +
                    "use regular computation instead");
        }
        //Creating the Random Polynomial - f(x , y)
        //Note : there are (t+1)^2 coefficiet for the polynomial including the free coefficient (the secret) 
        //first  row  coef are of  (x^0,x^1,x^2,...,x^t)y^0, second  row  coef are (x^0, x1,...,x^t)y^1 and so forth...
        ZpMatrix randomMatrix_f_xy = ZpMatrix.getRandomMatrix(polynomDeg + 1, polynomDeg + 1, secret.prime);
        randomMatrix_f_xy.setMatrixCell(0, 0, secret);
        List<SecretPolynomials> polynomialShares = new ArrayList<SecretPolynomials>();
        SecretPolynomials pSecret;
        
        for (int i = 0; i < numberOfPlayers; i++) {
            pSecret = new SecretPolynomials();
            pSecret.setFi_xPolynomial( generateF_i_xPolynomial(randomMatrix_f_xy, secret , i));
            pSecret.setGi_yPolynomial( generateG_i_yPolynomial(randomMatrix_f_xy, secret, i));
            polynomialShares.add(pSecret);
        }
        
        return polynomialShares;
    }


    private static List<Zp> generateF_i_xPolynomial(ZpMatrix f_x_y,  Zp secret,  int playerNum) {

        int  w = Zp.getFieldMinimumPrimitive(secret.prime);
        int w_i = Zp.calculatePower(w, playerNum, secret.prime);

        int[] y_values = new int[f_x_y.getColNumber()];
        for (int i = 0; i < f_x_y.getColNumber(); i++)
        {
            y_values[i] = Zp.calculatePower(w_i, i, secret.prime);
        }
        
        List<Zp> f_x_iShares = Arrays.asList(f_x_y.mulMatrixByScalarsVector( y_values ).SumMatrixRows());
        return f_x_iShares;
    }

    
    private static List<Zp> generateG_i_yPolynomial(ZpMatrix f_x_y,  Zp secret,  int playerNum) {

        int  w = Zp.getFieldMinimumPrimitive(secret.prime);
        int w_i = Zp.calculatePower(w, playerNum, secret.prime);

        Zp[]   x_values= new Zp[f_x_y.getRowNumber()];
        for (int i = 0; i < f_x_y.getRowNumber(); i++)
        {
            x_values[i] = new Zp(secret.prime, Zp.calculatePower(w_i, i, secret.prime));
        }

        Zp[] tempArr = f_x_y.times(new ZpMatrix(x_values, ZpMatrix.VectorType.COLOMN_VECTOR)).getZpVector();

        List<Zp> g_y_iShares = Arrays.asList(tempArr);
        return g_y_iShares;
    }

    
    /* This function Recombine (interpolate) the secret from secret shares  */
    public static Zp recombine(List<Zp> sharedSecrets, int polynomDeg, int prime, boolean  usePrimitiveRecombine) {

        if (sharedSecrets.size() <= polynomDeg) {
            throw new IllegalArgumentException("Polynomial degree cannot be bigger or equal to the number of  shares");
        }
        ZpMatrix A  = null;

        if (usePrimitiveRecombine)
        {
                 A = ZpMatrix.getSymmetricPrimitiveVandermondeMatrix(polynomDeg + 1, prime).getTransposeMatrix();
        }
        else
        {
                A  = ZpMatrix.getShamirRecombineMatrix(polynomDeg + 1, prime);
        }
        Zp[] sharedArr = new Zp[sharedSecrets.size()];
        sharedSecrets.toArray(sharedArr);
        Zp[] truncShare = truncateVector(sharedArr, polynomDeg + 1);
        Zp[] solution = A.solve(truncShare);
        return solution[0];
    }


    public static Zp recombine(List<Zp> sharedSecrets, int polynomDeg, int prime) {
        
            return recombine(sharedSecrets, polynomDeg, prime, false);
    }

   public static Zp primitiveRecombine(List<Zp> sharedSecrets, int polynomDeg, int prime) {

            return recombine(sharedSecrets, polynomDeg, prime, true);
   }

    /* This function creates a random poynomial Qj(x) ,for the j player,and creates a list of elements,
     *such that the i-th element is Qj(i) */
    public static List<Zp> getRandomizedShares(int numberOfPlayers, int polynomDeg, int prime) {
        //polynomial q(x) free element must be zero so it won't effect the final result
        Zp polyfreeElem = new Zp(prime, 0);
        return share(polyfreeElem, numberOfPlayers, polynomDeg);
    }

     /* This function creates a random poynomial Qj(x) ,for the j player,and creates a list of elements,
       *such that the i-th element is Qj(i) */
    public static List<Zp> getRandomizedSharesByzantineCase(int numberOfPlayers, int polynomDeg, int prime) {
         //polynomial q(x) free element must be zero so it won't effect the final result
        Zp polyfreeElem = new Zp(prime, 0);
        return primitiveShare(polyfreeElem, numberOfPlayers, polynomDeg);
    }
    
    /* The i-th user gets  a Qj(i) List from users, each user j calculated Qj(i) - the j element in the List */
    public static Zp calculateRandomShare(Zp myShare, List<Zp> polyUpdate) {
        Zp newShare = new Zp(myShare);
        newShare.addListContent(polyUpdate);
        return newShare;
    }

    private static Zp[] truncateVector(Zp[] vector, int toSize) {
        if (vector.length < toSize) {
            return null;
        }

        Zp[] truncVec = new Zp[toSize];
        for (int i = 0; i < toSize; i++) {
            truncVec[i] = new Zp(vector[i]);
        }
        return truncVec;
    }

    public static boolean checkSharedSecrets(Zp secret, int numberOfPlayers, int polynomDeg, int prime) {
        return secret.equals(recombine(share(secret, numberOfPlayers, polynomDeg), polynomDeg, prime));
    }


    public static class ShareDetails {
            List<Zp> randomPolynomial;
            List<Zp> createdShares;

        public ShareDetails(List<Zp> randomPolynomial, List<Zp> createdShares) {
            this.randomPolynomial = randomPolynomial;
            this.createdShares = createdShares;
        }

        public List<Zp> getCreatedShares() {
            return createdShares;
        }

        public void setCreatedShares(List<Zp> createdShares) {
            this.createdShares = createdShares;
        }

        public List<Zp> getRandomPolynomial() {
            return randomPolynomial;
        }

        public void setRandomPolynomial(List<Zp> randomPolynomial) {
            this.randomPolynomial = randomPolynomial;
        }

    }


    public static void main(String[] args) {

        int prime = 233;
        int numOfPlayers = 5;
        int polynomDeg = 1;

        //check share Byzantine case

        Zp my_secret = new Zp(prime, 34);

        List<SecretPolynomials> polyShares =  Shamir.shareByzantineCase(my_secret, numOfPlayers, polynomDeg);

        List<Zp> polyList = new ArrayList<Zp>();
        for (SecretPolynomials secPoly : polyShares)
        {
                polyList.add(secPoly.getFi_xPolynomial().get(0));
        }
        
        Zp polyResult = Shamir.primitiveRecombine(polyList, polynomDeg, prime);

        // Verify that  fi(w^j) == gj(w^i)
        for (int i = 1 ; i < numOfPlayers; i++)
        {
                System.out.println("Verifying shares of player number " + i);
                List<Zp> player_i_forVerify = polyShares.get(i).calculateF_i_xValuesForPlayers(numOfPlayers, prime);
                for (int  j= 1 ; j < numOfPlayers; j++)
                {
                        List<Zp> player_j_VerificationList = polyShares.get(j).calculateG_i_yValuesForVerification(numOfPlayers, prime);
                        if  (!player_i_forVerify.get(j).equals(player_j_VerificationList.get(i)))
                        {
                                    System.out.println("Failed !!!");
                                    return;
                        }
                }
        }

        //check multiplication of 2 secrets between n players

//        Zp secret1 = new Zp(prime, 9);
//        Zp secret2 = new Zp(prime, 5);
//
//        List<Zp> shares1 = Shamir.share(secret1, numOfPlayers, polynomDeg);
//        List<Zp> shares2 = Shamir.share(secret2, numOfPlayers, polynomDeg);
//
//
//        List<Zp> resultShare = new ArrayList<Zp>();
//
//        for (int i = 0; i < numOfPlayers; i++) {
//            resultShare.add(shares1.get(i).mul(shares2.get(i)));
//        }
//
//        //after multiply - need to perform reduction and randomization step
//
//        //each players generates a random List which he send to the user (just its part)
//        List<Zp> r1 = getRandomizedShares(numOfPlayers, polynomDeg, prime);
//        List<Zp> r2 = getRandomizedShares(numOfPlayers, polynomDeg, prime);
//        List<Zp> r3 = getRandomizedShares(numOfPlayers, polynomDeg, prime);
//        List<Zp> r4 = getRandomizedShares(numOfPlayers, polynomDeg, prime);
//        List<Zp> r5 = getRandomizedShares(numOfPlayers, polynomDeg, prime);
//        List<Zp> r6 = getRandomizedShares(numOfPlayers, polynomDeg, prime);
//
//        //every user need update its share so it would feet the new polynomial
//        List<Zp> resultShare2 = new ArrayList<Zp>();
//        for (int i = 0; i < r1.size(); i++) {
//            resultShare2.add(resultShare.get(i).add(r1.get(i)).add(r2.get(i)).add(r3.get(i)).add(r4.get(i)).add(r5.get(i)).add(r6.get(i)));
//        }
//
//        //here each user should participate in calulation and sending the parts of each user
//        //didn't do it
//
//        ZpMatrix constMat = ZpMatrix.getReductionStepMatrix(numOfPlayers, polynomDeg, prime);
//        Zp[] results = new Zp[resultShare.size()];
//        resultShare.toArray(results);
//        ZpMatrix sharesMat = new ZpMatrix(results, ZpMatrix.VectorType.ROW_VECTOR);
//
//        ZpMatrix finalRes = sharesMat.times(constMat);
//
//        Zp reveal = recombine(Arrays.asList(finalRes.getZpVector()), polynomDeg, prime);
//
//        System.out.println("the result of the mul is : " + reveal.toString());
//
//        ////////////////////////////////////////////////////////
//
//        //check addition of 2 secrets between n players
//        Zp secret10 = new Zp(prime, 5);
//        Zp secret20 = new Zp(prime, 2);
//        Zp secret30 = new Zp(prime, 3);
//        Zp secret40 = new Zp(prime, 4);
//        Zp secret50 = new Zp(prime, 2);
//        Zp secret60 = new Zp(prime, 3);
//
//        List<Zp> shares10 = Shamir.share(secret10, numOfPlayers, polynomDeg);
//        List<Zp> shares20 = Shamir.share(secret20, numOfPlayers, polynomDeg);
//        List<Zp> shares30 = Shamir.share(secret30, numOfPlayers, polynomDeg);
//        List<Zp> shares40 = Shamir.share(secret40, numOfPlayers, polynomDeg);
//        List<Zp> shares50 = Shamir.share(secret50, numOfPlayers, polynomDeg);
//        List<Zp> shares60 = Shamir.share(secret60, numOfPlayers, polynomDeg);
//
//        List<Zp> result_Share = new ArrayList<Zp>();
//
//        for (int i = 0; i < shares1.size(); i++) {
//            result_Share.add(shares10.get(i).add(shares20.get(i)).add(shares30.get(i)).add(shares40.get(i)).add(shares50.get(i)).add(shares60.get(i)));
//        }
//
//        Zp result = Shamir.recombine(result_Share, polynomDeg, prime);
//
//        System.out.println("the result of the sum is : " + result.toString());
//
//        //////////////////////////////////////////////////////
////check solving algoritm
//        int till = 100;
//        for (int i = 2; i < till; i++) // prime numbers
//        {
//            for (int j = 2; j < i; j++) // polinomial degree
//            {
//                for (int l = j + 1; l < 2 * j; l++) // l - num of users - must be at least as big as the polinomial degree
//                {
//                    if (Zp.isPrime(i)) {
//                        Zp secret = new Zp(i, j);
//                        System.out.println("current prime is : " + i + "  polynomial degree is : " + j + "  secret is : " + j + "  num of users : " + l);
//
//                        if (!checkSharedSecrets(secret, l, j, i)) {
//                            System.out.println("failed !!!!");
//                            return;
//                        }
//                    }
//                }
//            }
//        }
    }
}


