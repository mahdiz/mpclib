/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.protocols;

import mpc.ui.ProgressLog;
import mpc.circuit.Circuit;
import mpc.sendables.SecretPolynomials;
import mpc.sendables.Sendable;
import mpc.finite_field_math.Shamir;
import mpc.finite_field_math.Zp;
import mpc.finite_field_math.ZpMatrix;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;


public class MPCProtocolCheaterPlayer  extends MPCProtocolByzantineCase{

         public MPCProtocolCheaterPlayer(Circuit circuit, ProgressLog proglog,int index, int prime) {
            super(circuit, proglog,index, prime);
            this.polynomialDeg =  (numberOfPlayers - 1) / 4;   // for simple test disable it
        }


        public MPCProtocolCheaterPlayer(MPCProtocolByzantineCase protocol, Circuit circuit){
            super(protocol, circuit);
            this.polynomialDeg =  (numberOfPlayers - 1) / 4;    // for simple test disable it
        }

        
        private List<SecretPolynomials> getByzantineSecretPolynomialList(Zp input){

                List<SecretPolynomials>  byzantineSecretPolysList= Shamir.shareByzantineCase(input, numberOfPlayers, polynomialDeg);
                int i=0;
                for (SecretPolynomials byzantineSecretPolys: byzantineSecretPolysList){

                            if (i == 0){
                                byzantineSecretPolys.setGi_yPolynomial(null);
                            } else {
                                byzantineSecretPolys.setGi_yPolynomial(getRandomZpList(polynomialDeg + 1, input.prime));
                            }
                            i++;
                }
                return byzantineSecretPolysList;
        }

        private List<Zp> getRandomZpList(int length, int prime){
              return  Arrays.asList(ZpMatrix.getRandomMatrix(1, length, prime).getZpVector());
        }

  

    @Override
   protected List<Zp> inputStage(Zp input) throws IOException{

            // Set timeout is 20 seconds
            cController.setTimeOut(2000000);
            // Generated polynomials for users
            List<SecretPolynomials> shareMySecrets = Shamir.shareByzantineCase(input, numberOfPlayers, polynomialDeg);
            int playersNumForPrint = 0;
            String toPrintStr = new String();
            for (SecretPolynomials toSendSecretPoly: shareMySecrets){
                toPrintStr +="\nPolys For Player "  + playersNumForPrint++ + " -  " ;
                toPrintStr += (toSendSecretPoly != null) ?  toSendSecretPoly.toString() : null; // should be change for regualr player
            }
            proglog.printInformation2("My secret polynomials shares  are:" + toPrintStr);

            proglog.printInformation("sharing secrets with other players");

            // Share and receive polynomials from all players - I'm a cheater so I will send a random List that different from my original shares
            // List<SecretPolynomials>  myRevcShare = shareComplex(shareMySecrets);
            List<SecretPolynomials>  myRevcShare =
                        Sendable.asSecretPolynomials(cController.shareSecrets(getByzantineSecretPolynomialList(input), prime));

            playersNumForPrint = 1;
            toPrintStr = "";
            for (SecretPolynomials toRecvSecretPoly: myRevcShare){
                toPrintStr +="\nPolys For Player "  + playersNumForPrint++ + " -  ";
                toPrintStr += (toRecvSecretPoly != null) ?  toRecvSecretPoly.toString() : null;
            }
            proglog.printInformation2("Received polynomials shares  are:"  + toPrintStr);

            List<Zp> recvSharesForComp = new ArrayList<Zp>();

            // Input stage checking - check that each input is legal
            int playerToVerify = 0;
            Zp recvSecretShare_i;
            boolean imTheDealer = false;

            // Each iteration in the loop is a verification of player number  'playerToVerify'
            for (SecretPolynomials  secretPoly_i : myRevcShare ) {

                if (playerToVerify != index)
                {
                        proglog.printInformation2("Verifying input of player number... "  + playerToVerify);
                }
                else    // I'm the dealer
                {
                      proglog.printInformation2("Verifying my input...");
                      imTheDealer = true;
                }

                /* Step 1 - Verify player 'playerToVerify' input  */
                // Extract the received share from the polynomial you received from player Number 'playerToVerify'
                // Assume I'm the j-th player-  Calculate fj(w^i)
                List<Zp> toSendVerificationList_f_j_w_i = null;
                List<Zp> toRecvVerifyList_f_i_w_j = null;
                List<Zp> verifyWithList_g_j_w_i = null;
                List<Coordinate>  wrongCoordinatesList = null;

                // First check if you received a proper polynomial.
                boolean isOrigPolyLegal = isSecretPolynomialsLegal(secretPoly_i);

                if (isOrigPolyLegal)
                {
                        recvSecretShare_i = new Zp(secretPoly_i.getFi_xPolynomial().get(0));
                        toSendVerificationList_f_j_w_i = secretPoly_i.calculateF_i_xValuesForPlayers(numberOfPlayers, prime);
                        toRecvVerifyList_f_i_w_j = shareSimple(toSendVerificationList_f_j_w_i);
                        verifyWithList_g_j_w_i  = secretPoly_i.calculateG_i_yValuesForVerification(numberOfPlayers, prime);
                        // Need to change this comparison and its output
                         wrongCoordinatesList = compareCoordianteList(toRecvVerifyList_f_i_w_j, verifyWithList_g_j_w_i);
                }
                else // Received polynomials are corrupted Send random List and remember to complain
                {
                         proglog.printWarning("Received a corrupted polynomials from player Number : " + playerToVerify);
                        recvSecretShare_i = new Zp(prime, 0);
                        toSendVerificationList_f_j_w_i = ZpMatrix.getRandomMatrix(1, numberOfPlayers, prime).getMatrixRow(0);
                        toRecvVerifyList_f_i_w_j = shareSimple(toSendVerificationList_f_j_w_i);
                        // don't have a need to compare the received results with somthing
                }


                if (imTheDealer)
                {
                    handleDealer(shareMySecrets, recvSecretShare_i);
                }
                else // I'm not the dealer
                {
                    handleNotDealer(isOrigPolyLegal, wrongCoordinatesList, playerToVerify, recvSecretShare_i, secretPoly_i);
                }

                // Finally  Add the secret share
                recvSharesForComp.add(recvSecretShare_i);
                imTheDealer = false;
                playerToVerify++;
            }

            proglog.printInformation("Received secrets shares  are: " + recvSharesForComp.toString());
            return recvSharesForComp;
   }


}