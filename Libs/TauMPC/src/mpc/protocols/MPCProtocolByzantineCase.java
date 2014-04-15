/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.protocols;

import mpc.sendables.MultStepBCaseShareBundle;
import mpc.sendables.MultStepBCaseShare;
import mpc.sendables.MultStepVerificationPoly;
import mpc.ui.ProgressLog;
import mpc.circuit.Circuit;
import mpc.communication.BulletinBoard;
import mpc.sendables.PlayerNotification;
import mpc.sendables.SecretPolynomialsBundle;
import mpc.sendables.SecretPolynomials;
import mpc.sendables.Sendable;
import mpc.sendables.ShareObject;
import mpc.finite_field_math.Polynom;
import mpc.finite_field_math.Shamir;
import mpc.finite_field_math.Zp;
import mpc.finite_field_math.ZpMatrix;
import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Map;
import mpc.communication.ConnectionController.Player;


public class MPCProtocolByzantineCase extends MPCProtocol{

    protected boolean [] goodPlayers;
    protected  MPCProtocolByzantineCase callerProtocol;

    public MPCProtocolByzantineCase(Circuit circuit, ProgressLog proglog,int index,int prime) {
        super(circuit, proglog,index,prime);
        initializeGoodPlayers();
        callerProtocol = null;
    }

    
    public MPCProtocolByzantineCase(MPCProtocolByzantineCase protocol, Circuit circuit){
        super(protocol, circuit);
        initializeGoodPlayers(protocol.goodPlayers);
        callerProtocol = protocol;
    }


    @Override
     public boolean init(File xmlFile) {
            if (numberOfPlayers < 5) {
                 throw new IllegalArgumentException("Cannot use Byzantine algoritm - numberOfPlayers <= 4*polynomDeg - " +
                "use regular computation instead");
            }            
            boolean initSucceeded = super.init(xmlFile);
            this.polynomialDeg =  (numberOfPlayers - 1) / 4;    // for simple test  - disable this part
            return initSucceeded;
     }

    @Override
     public boolean init(String serverIP, int port, File xmlFile) {
            if (numberOfPlayers < 5) {
                 throw new IllegalArgumentException("Cannot use Byzantine algoritm - numberOfPlayers <= 4*polynomDeg - " +
                "use regular computation instead");
            }            
            boolean initSucceeded = super.init(serverIP, port,  xmlFile);
            this.polynomialDeg =  (numberOfPlayers - 1) / 4;    // for simple test  - disable this part
            return initSucceeded;
     }
    

    protected int getNumberOfComplaints(List<PlayerNotification> complaints){
        int numOfcomplaints = 0;
       for (PlayerNotification playerNote : complaints){
                if ((playerNote != null) && (playerNote.getMsg() == PlayerNotification.Confirmation.COMPLAINT))
                {
                         numOfcomplaints++;
                }
       }
       return numOfcomplaints;
  }

    protected void removeCheaterPlayer(int playerToVerify) throws IOException {
        goodPlayers[playerToVerify] = false;
        if (callerProtocol != null) {
            callerProtocol.goodPlayers[playerToVerify] = false;
        }
        cController.removePlayer(playerToVerify);
    }

    
    protected void  updateRecvShare(List<SecretPolynomials> recvPublicPolysList, Zp recvSecretShare_i) {
           if ( (recvPublicPolysList != null))
           {
                SecretPolynomials  newRecvSercretPoly = recvPublicPolysList.get(index);
                if (newRecvSercretPoly != null)
                {
                        Zp recvShare = newRecvSercretPoly.getFi_xPolynomial().get(0);
                        if (recvShare != null)
                        {
                            recvSecretShare_i.setValue(recvShare.getValue());
                        }
                }
           }
    }
    
    @Override
    protected void filterPlayers(Map<Integer, Player> players){        
        for (int i = 0; i < goodPlayers.length; i++){
            if (!goodPlayers[i]){
                players.remove(i);
            }                        
        }
    }   


   private void initializeGoodPlayers(){
        goodPlayers = new boolean[numberOfPlayers];
        for (int i = 0; i < numberOfPlayers; i++){
                goodPlayers[i] = true;
        }
  }

   private void initializeGoodPlayers(boolean copyGoodPlayers[]){
        goodPlayers = new boolean[copyGoodPlayers.length];
        for (int i = 0; i < copyGoodPlayers.length; i++){
                goodPlayers[i] = copyGoodPlayers[i];
        }
  }


  private int getNumOfGoodPlayers(){
        int goodPlayerNum = 0;
        for (int i = 0; i < numberOfPlayers; i++){
                goodPlayerNum += goodPlayers[i] ? 1 : 0;
        }
        return goodPlayerNum;
  }

    @Override
    protected Zp getRecombinedResult(List<Zp> recvList, int prime) {
            // Scan recvList - if there are null elements replace them arbitrarily to Zp with zero value
            for (int i = 0;i < recvList.size(); i++){
                 if (recvList.get(i) == null)
                 {
                        recvList.set(i, new Zp(prime, 0));
                 }
            }
            List<Zp> xVlaues = new ArrayList<Zp>();
            int w = Zp.getFieldMinimumPrimitive(prime);
            for (int i = 0;i < recvList.size(); i++){
                 xVlaues.add(new Zp(prime, Zp.calculatePower(w, i, prime) ));
            }
            // Should call Welch-Berlekamp Decoder to fix error at last stage
            List<Zp> fixedShares = WelchBerlekampDecoder.decode(xVlaues, recvList, polynomialDeg, polynomialDeg, prime);
            if (fixedShares == null)
            {
                    String errorStr = "There were more then polynomialDegree = " + polynomialDeg + " Cheaters - cannot extract results.";
                    proglog.printError(errorStr);
                    throw new IllegalStateException(errorStr);
            }     
            return  Shamir.primitiveRecombine(fixedShares, polynomialDeg, prime);
    }



    @Override
   protected List<Zp> inputStage(Zp input) throws IOException{

            // Set timeout is 20 seconds
            cController.setTimeOut(20000);
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

            // Share and receive polynomials from all players
            List<SecretPolynomials>  myRevcShare = Sendable.asSecretPolynomials(cController.shareSecrets(shareMySecrets, prime, goodPlayers));
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
                // First check if the verified dealer had already been caught as a cheater
                if (! goodPlayers[playerToVerify])
                {
                        // No need to check a the cheater player input
                        recvSharesForComp.add(new Zp(prime, 0));
                        imTheDealer = false;
                        playerToVerify++;
                        continue;
                }
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
    
    protected void handleDealer(List<SecretPolynomials> shareMySecrets, Zp recvSecretShare_i) throws IOException {
        // Should send&receive complaints on the bulletin board  1 - I'm not complaining on  myself
        PlayerNotification myDemoComplaint = new PlayerNotification(PlayerNotification.Confirmation.APPROVAL);
        List<PlayerNotification> complaintesOnMe = Sendable.asPlayerNotifications(BulletinBoard.publishAndRead(myDemoComplaint, prime, goodPlayers, cController));

        // Prepare wanted polys according to players complaintes
        List<SecretPolynomials> wantedPolysList = new ArrayList<SecretPolynomials>();
        boolean recvCompOnMe = createWantedPolynomials(wantedPolysList, complaintesOnMe, shareMySecrets);

        if (recvCompOnMe) {
            // Publish wanted polynomials on the bulletin board
            SecretPolynomialsBundle secretBundle = new SecretPolynomialsBundle(wantedPolysList);
            BulletinBoard.publish(secretBundle, goodPlayers, cController);

            // Should send&receive complaints on the bulletin board  2 - I'm not complaining on  myself
            complaintesOnMe = Sendable.asPlayerNotifications(BulletinBoard.publishAndRead(myDemoComplaint, prime, goodPlayers, cController));

            // Prepare wanted polys according to players complaintes
            recvCompOnMe = createWantedPolynomials(wantedPolysList, complaintesOnMe, shareMySecrets);

            if (recvCompOnMe) {
                // Publish wanted polynomials on the bulletin board
                secretBundle = new SecretPolynomialsBundle(wantedPolysList);
                BulletinBoard.publish(secretBundle, goodPlayers, cController);

                // Should send&receive complaints on the bulletin board  3 - I'm not complaining on  myself
                complaintesOnMe = Sendable.asPlayerNotifications(BulletinBoard.publishAndRead(myDemoComplaint, prime, goodPlayers, cController));

                // Count complaints number
                int numOfcomplaints = getNumberOfComplaints(complaintesOnMe);

                if (numOfcomplaints > polynomialDeg) {
                    // Take the zero polynomial as my input (Not mandatory) and throw exception
                    recvSecretShare_i.setValue(0);
                    String errorStr = "Other players decided that I'm a cheater  :- (  -  taking my input as zero.";
                    proglog.printError(errorStr);
                    throw new IllegalStateException(errorStr);
                }
            }
        }
    }

    protected void handleNotDealer(boolean isOrigPolyLegal, List<Coordinate> wrongCoordinatesList, int playerToVerify, Zp recvSecretShare_i, SecretPolynomials secretPoly_i) throws IOException {
        /* Step 2 - Check and advertise results to players on the bulletin board  */
        boolean foundWrongValue1 = (!isOrigPolyLegal) || (wrongCoordinatesList.size() != 0);
        PlayerNotification complaintValue = new PlayerNotification(foundWrongValue1 ? PlayerNotification.Confirmation.COMPLAINT : PlayerNotification.Confirmation.APPROVAL);
        List<PlayerNotification> recvComplaintesList = Sendable.asPlayerNotifications(BulletinBoard.publishAndRead(complaintValue, prime, goodPlayers, cController));

        // Count complaints number
        int numOfcomplaints = getNumberOfComplaints(recvComplaintesList);

        /* Step 3 - If there is a need, receive polynomials for the complainig sides and verify it */
        // currently check only polynomials - not wrong coordinates
        if (numOfcomplaints != 0) {
            proglog.printWarning("Inconsistency 1 found ! checking if player " + playerToVerify + " is a cheater");
            // Should get a list of the public polynomials from player  'playerToVerify'  -  get info from the bulletin board
            Sendable sendablePolysRecv = BulletinBoard.read(playerToVerify, prime, cController);

            List<SecretPolynomials> recvPublicPolysList1 = null;
            if (sendablePolysRecv != null) {
                recvPublicPolysList1 = sendablePolysRecv.asSecretPolynomialsBundle().getList();
            }

            if (foundWrongValue1) {
                updateRecvShare(recvPublicPolysList1, recvSecretShare_i);
            }
            boolean foundWrongValue2 = (!isOrigPolyLegal) || isPublicDataContradictPrivate(secretPoly_i, recvPublicPolysList1, recvComplaintesList, recvSecretShare_i);


            // Check and advertise results to players on the bulletin board
            complaintValue = new PlayerNotification(foundWrongValue2 ? PlayerNotification.Confirmation.COMPLAINT : PlayerNotification.Confirmation.APPROVAL);

            recvComplaintesList = Sendable.asPlayerNotifications(BulletinBoard.publishAndRead(complaintValue, prime, goodPlayers, cController));

            // Count complaints number
            numOfcomplaints = getNumberOfComplaints(recvComplaintesList);

            /* Step 4 - If there is a need, recieve polynomials for the complainig sides and verify it */
            if (numOfcomplaints != 0) {
                proglog.printWarning("Inconsistency 2 found ! checking if player " + playerToVerify + " is a cheater");
                // Should get a list of the public polynomials from player  'playerToVerify' -  get info from the bulletin board
                sendablePolysRecv = BulletinBoard.read(playerToVerify, prime, cController);
                List<SecretPolynomials> recvPublicPolysList2 = null;
                if (sendablePolysRecv != null) {
                    recvPublicPolysList2 = sendablePolysRecv.asSecretPolynomialsBundle().getList();
                }

                if (foundWrongValue2) {
                    updateRecvShare(recvPublicPolysList2, recvSecretShare_i);
                }
                boolean foundWrongValue3 = (!isOrigPolyLegal) || isPublicDataContradictPrivate(secretPoly_i, recvPublicPolysList2, recvComplaintesList, recvSecretShare_i);
                foundWrongValue3 = foundWrongValue3 || isNewPublicDataContradictOld(recvPublicPolysList1, recvPublicPolysList2);
                // Check and advertise results to players on the bulletin board
                complaintValue = new PlayerNotification(foundWrongValue3 ? PlayerNotification.Confirmation.COMPLAINT : PlayerNotification.Confirmation.APPROVAL);
                recvComplaintesList = Sendable.asPlayerNotifications(BulletinBoard.publishAndRead(complaintValue, prime, goodPlayers, cController));

                // Count complaints number
                numOfcomplaints = getNumberOfComplaints(recvComplaintesList);

                /* Step 5 - Check if there is more than 'polynomialDeg' complaintes or recption timeout occured  */
                if (numOfcomplaints > polynomialDeg) {
                    proglog.printWarning("Found a cheater ! player  " + playerToVerify + "  - taking its input as zero.");
                    // Take the zero polynomial as this user input
                    recvSecretShare_i.setValue(0);
                     removeCheaterPlayer(playerToVerify);//don't send and receive from this player anymore...
                }
            }
        }
    }


    // Implementation according to GRR 
    public  Zp  reductionRandomizationStep(Zp a, Zp b, Zp ab) throws IOException{
        proglog.printInformation("performing reduction & randomization step");

        /* Create a detailed share of a,b and ab */
        Shamir.ShareDetails aSharesDetails = Shamir.detailedShare(a, numberOfPlayers, polynomialDeg);
        Shamir.ShareDetails bSharesDetails = Shamir.detailedShare(b, numberOfPlayers, polynomialDeg);
        Shamir.ShareDetails abSharesDetails = Shamir.detailedShare(ab, numberOfPlayers, polynomialDeg);
        /* Share a random polynomial r(x) of degree 2t-1 */
        Shamir.ShareDetails rSharesDetails = Shamir.detailedShare(new Zp(prime, (int) (Math.random() *  prime)), numberOfPlayers, 2*polynomialDeg - 1);
        MultStepVerificationPoly RxPolynomial = new MultStepVerificationPoly(constructRxPolynomial(aSharesDetails, bSharesDetails, abSharesDetails, rSharesDetails));

        /* Build verified shares for users */
        List<Zp> aShares = aSharesDetails.getCreatedShares();
        List<Zp> bShares = bSharesDetails.getCreatedShares();
        List<Zp> abShares = abSharesDetails.getCreatedShares();
        List<Zp> rShares = rSharesDetails.getCreatedShares();
        List<MultStepBCaseShare> toSendmultStepShares = new ArrayList<MultStepBCaseShare>();
        for (int i = 0; i < numberOfPlayers; i++){
                MultStepBCaseShare  playerShares = new MultStepBCaseShare();
                playerShares.setAShare(aShares.get(i));
                playerShares.setBShare(bShares.get(i));
                playerShares.setAbShare(abShares.get(i));
                playerShares.setRShare(rShares.get(i));
                toSendmultStepShares.add(playerShares);
        }

        /* Send and receive shares from all players */
        List<MultStepBCaseShare> toRecvmultStepShares =
                 Sendable.asMultStepBCaseShares(cController.shareSecrets(toSendmultStepShares, prime, goodPlayers));

        /* Start verifying player shares and construct a list of the received shares of ab  - this list will be used to compute the regular multStep*/
        int playerToVerify = 0;
        List<Zp> abVerifiedList = new ArrayList<Zp>();
        for (MultStepBCaseShare  recvShareFromPlayer_i : toRecvmultStepShares){
              if (playerToVerify == index)
              {
                    // I'm the dealer
                    // Put Rx polynomial on the bulletin board
                   BulletinBoard.publish(RxPolynomial, goodPlayers, cController);
                   
                    // Should advertise a demo complaint on the bulletin board - I'm not complaining about myself
                    PlayerNotification myDemoComplaint = new PlayerNotification(PlayerNotification.Confirmation.APPROVAL);
                    
                    List<PlayerNotification> complaintesOnMe = 
                                                    Sendable.asPlayerNotifications(BulletinBoard.publishAndRead(myDemoComplaint, prime, goodPlayers ,cController));
                    // Build a message containing all the requested data
                    List<MultStepBCaseShare> requstedData = new ArrayList<MultStepBCaseShare>();
                    for (int i = 0; i < numberOfPlayers; i++){
                             if ((complaintesOnMe.get(i) != null) && (complaintesOnMe.get(i).getMsg().equals(PlayerNotification.Confirmation.COMPLAINT))){
                                        requstedData.add(toSendmultStepShares.get(i));
                             }
                             else
                             {
                                        requstedData.add(null);
                             }
                    }
                    MultStepBCaseShareBundle requstedDataToPublish = new MultStepBCaseShareBundle(requstedData);
                    // Put the requested shares on the bulletin board . 
                    // Note : even if you are a cheater you should publish the real shares here otherwise you  will be considered as cheater
                    BulletinBoard.publish(requstedDataToPublish, goodPlayers, cController);

                    // Add my ab share to abShares any way
                    abVerifiedList.add(toSendmultStepShares.get(index).getAbShare());
              }
              else
              {
                      if (goodPlayers[playerToVerify])
                      {
                            proglog.printInformation2("Mult Step - verifying player number " +playerToVerify+ " share");
                            boolean dealerIsCheater = false;
                            PlayerNotification  myComplaint;
                            List<PlayerNotification> recvComplaints;
                            MultStepBCaseShareBundle recvPublicShares;
                            // Get Rx polynomial of the 'playerToVerify' from bulletin board
                            MultStepVerificationPoly recvVerificationPolynomial = BulletinBoard.read(playerToVerify, prime, cController).asMultStepVerificationPoly();
                            if (isMultStepPolynomialLegal(recvVerificationPolynomial))
                            {
                                    if (!isMultStepShareLegal(recvShareFromPlayer_i, recvVerificationPolynomial))
                                    {
                                        // Should prepare a massege that te dealer is a cheater
                                        myComplaint = new PlayerNotification(PlayerNotification.Confirmation.COMPLAINT);
                                    }
                                    else
                                    {
                                        // Should prepare a massege that te dealer is not a cheater
                                        myComplaint = new PlayerNotification(PlayerNotification.Confirmation.APPROVAL);
                                    }

                                    // Help verify complaining users shares  :
                                    // Should advertise what you thinks about the  dealer on the bulletin board
                                    recvComplaints = Sendable.asPlayerNotifications(BulletinBoard.publishAndRead(myComplaint, prime, goodPlayers ,cController));
                                    // Get complaining players share from the bulletin board - it is now a public share
                                    recvPublicShares =  BulletinBoard.read(playerToVerify, prime, cController).asMultStepBCaseShareBundle();
                                    // Check that the dealer had published all the requested data and that the data is consistent
                                    List<MultStepBCaseShare>  publishedShareList = null;
                                    if (recvPublicShares != null)
                                    {
                                            publishedShareList  = recvPublicShares.getList();
                                    }
                                    MultStepBCaseShare  publishedShare;
                                   for (int i = 0; i < numberOfPlayers; i++){
                                            if ((recvComplaints.get(i) != null) && (recvComplaints.get(i).getMsg().equals(PlayerNotification.Confirmation.COMPLAINT)))
                                            {
                                                    publishedShare = publishedShareList.get(i);
                                                    if (!isMultStepShareLegal(publishedShare, recvVerificationPolynomial))
                                                    {
                                                            // No need to send any more complaints or aproval- the shares and the R(x) polynomial is public data
                                                            dealerIsCheater = true;
                                                            cController.removePlayer(playerToVerify);//don't send and receive from this player anymore...
                                                            break;
                                                    }
                                            }
                                    }
                            }
                            else  // Mult step Polynomial , R(x) isn't legal
                            {
                                    dealerIsCheater = true;
                            }
                            
                            if (dealerIsCheater)
                            {
                                    proglog.printWarning("Found a cheater in  multiplication step. Cheater is player number :" + playerToVerify);
                                    removeCheaterPlayer(playerToVerify);
                            }
                            else
                            {
                                    abVerifiedList.add(recvShareFromPlayer_i.getAbShare());
                            }
                      }
              }
              playerToVerify++;
        }

        // Finally
        List<Zp> firstLineAtInvVanderMonde = getMultStepCoeffsForCheaters(0); // Contains only the needed coeffs
               
        // Calculate the value of the  polynomial H(x)  at i = H(i) as defined at GRR
        Zp tempSecret = new Zp(prime, 0);
        int goodPlayerNum = getNumOfGoodPlayers();
        for (int i =0; i < goodPlayerNum ;i++){
                tempSecret.add( abVerifiedList.get(i).mul(firstLineAtInvVanderMonde.get(i)));
        }
        return   tempSecret;
    }


    
    protected List<Zp> getMultStepCoeffsForCheaters(int j){
            boolean[] rowsToRemove = new boolean[numberOfPlayers];
            for (int i = 0; i < numberOfPlayers; i++){
                rowsToRemove[i] = !goodPlayers[i];
            }
            ZpMatrix vanderMonde = ZpMatrix.getSymmetricPrimitiveVandermondeMatrix(numberOfPlayers, prime).getTransposeMatrix();
            ZpMatrix filteredMatrix =vanderMonde.removeRowsFromMatrix(rowsToRemove).getInverse();
            return filteredMatrix.getMatrixRow(j);
    }


    
    protected List<Zp> constructRxPolynomial(Shamir.ShareDetails aSharesDetails, Shamir.ShareDetails bSharesDetails, Shamir.ShareDetails abSharesDetails,
                                                                                    Shamir.ShareDetails rSharesDetails){

        List<Zp> fax = aSharesDetails.getRandomPolynomial();
        List<Zp> fbx = bSharesDetails.getRandomPolynomial();
        List<Zp> hx = abSharesDetails.getRandomPolynomial();
        List<Zp> rx = rSharesDetails.getRandomPolynomial();

        Zp[] RxPolynomial = new Zp[2*polynomialDeg + 1];
        /* Initialize RxPolynomial coefs with zeros  */
        for (int i = 0; i < 2*polynomialDeg + 1; i++){
                RxPolynomial[i] = new Zp(prime, 0);
        }
        /* First calculate fax*fbx - hx */
        for (int i = 0; i< fax.size(); i++){
                Zp temp = fax.get(i);
                for (int j = 0; j< fax.size(); j++){
                        RxPolynomial[i+j].add(temp.constMul(fbx.get(j)));
                }
                RxPolynomial[i].sub(hx.get(i));
        }
        /* Calculate x*rx+fax*fbx - hx*/
        for (int i = 0; i< rx.size(); i++){
                RxPolynomial[i+1].add(rx.get(i));
        }
        return Arrays.asList(RxPolynomial);
    }


    protected boolean isPolynomialLegal(List<Zp> polynomial, int polySize){
                if ((polynomial == null)  || (polynomial.size() != polySize))
                {
                        return false;
                }
                for (Zp zp : polynomial) {
                        if (zp == null)
                        {
                            return false;
                        }
                }
                return true;
    }

    protected boolean isMultStepPolynomialLegal(MultStepVerificationPoly recvVerifcationPolynomial){
             if (recvVerifcationPolynomial != null)
            {
                    List<Zp> RxPolynomial = recvVerifcationPolynomial.getRxPolynomial();
                   if  (!isPolynomialLegal(RxPolynomial, 2*polynomialDeg +1))
                   {
                       return false;
                   }
             }
            else
            {
                    return false;
            }
            return true;
    }

    private boolean isRecvShareLegal(MultStepBCaseShare recvShareFromPlayer) {
            if ((recvShareFromPlayer == null) || (recvShareFromPlayer.getAShare() == null) || (recvShareFromPlayer.getBShare() == null) ||
                    (recvShareFromPlayer.getAbShare() == null) || (recvShareFromPlayer.getRShare() == null))
            {
                    return false;
            }
            return true;
    }

    
    protected boolean  isMultStepShareLegal(MultStepBCaseShare recvShareFromPlayer_i,  MultStepVerificationPoly recvVerifcationPolynomial){

            if (!isRecvShareLegal(recvShareFromPlayer_i))
            {
                    return false;
            }
            List<Zp> RxPolynomial = recvVerifcationPolynomial.getRxPolynomial();
            Zp Ratpoint0 = Zp.evalutePolynomialAtPoint(RxPolynomial, new Zp(prime, 0));
            if (!Ratpoint0.equals(new Zp(prime, 0)))
            {
                    return false;
            }
            int w = Zp.getFieldMinimumPrimitive(prime);
            Zp w_InMyIndex = new Zp(prime, Zp.calculatePower(w, index, prime));
            Zp RjFromPublicPolynomial = Zp.evalutePolynomialAtPoint(RxPolynomial, w_InMyIndex);

            Zp temp = recvShareFromPlayer_i.getAShare().constMul(recvShareFromPlayer_i.getBShare()).constSub(recvShareFromPlayer_i.getAbShare());
            Zp RjFromRecvPrivateInfo = w_InMyIndex.constMul(recvShareFromPlayer_i.getRShare()).constAdd(temp);
            if (!RjFromPublicPolynomial.equals(RjFromRecvPrivateInfo))
            {
                return false;
            }
            return true;
    }

    

    /* This function checks if the received polynomials are not null, from the right size and have no null elements */
    protected boolean isSecretPolynomialsLegal(SecretPolynomials secretPolynomial){

            if (secretPolynomial == null)
            {
                    return false;
            }
            else 
            {
                    List<Zp> fi_xPoly = secretPolynomial.getFi_xPolynomial();
                    List<Zp> gi_yPoly = secretPolynomial.getGi_yPolynomial();

                    if (! isPolynomialLegal(fi_xPoly, polynomialDeg + 1) ||  ! isPolynomialLegal(gi_yPoly, polynomialDeg + 1))
                    {
                            return false;
                    }
            }
            return true;
    }

    
protected boolean  createWantedPolynomials(List<SecretPolynomials> wantedPolysList, List<PlayerNotification> complaintesOnMe,
                                                                                                                            List<SecretPolynomials> shareMySecrets) {
        boolean recvCompOnMe =false;
        wantedPolysList.clear();
        for (int j = 0; j < complaintesOnMe.size(); j++) {
            if ((complaintesOnMe.get(j) != null) && (complaintesOnMe.get(j).getMsg() == PlayerNotification.Confirmation.COMPLAINT))
            {
                wantedPolysList.add(shareMySecrets.get(j));
                recvCompOnMe = true;
            }
            else
            {
                wantedPolysList.add(null);
            }
        }
        return recvCompOnMe;
    }


    @Override
    protected  List<Zp> shareSimple(List<Zp> sharedSecrets) throws IOException{
        assert sharedSecrets.size() > 0;
        return getZPs(Sendable.asShareObjects(cController.shareSecrets(getShareObjects(sharedSecrets), prime, goodPlayers)));
    }
    
   @Override
   protected  List<Zp> shareSimple(Zp sharedSecrets) throws IOException{
        return getZPs(Sendable.asShareObjects(cController.shareSecrets(new ShareObject(sharedSecrets, null), prime , goodPlayers)));
    }


    
    protected boolean isNewPublicDataContradictOld(List<SecretPolynomials> oldPublicData, List<SecretPolynomials> newPublicData){
            for (int i = 0; i < numberOfPlayers; i++){
                    SecretPolynomials secretToCheck = oldPublicData.get(i);
                    if ( (secretToCheck != null)  &&  (!secretToCheck.equals(newPublicData.get(i))) )
                    {
                           return  true;
                    }
            }
            return false;
    }


    

    protected boolean isPublicDataContradictPrivate(SecretPolynomials myRecvPolys, List<SecretPolynomials> recvPublicPolysList, List<PlayerNotification> recvComplaintesList, Zp myRecvShare) {
        if  ((recvPublicPolysList == null) || (recvPublicPolysList.size() != numberOfPlayers))
        {
            return true;
        }
        List<Zp> myG_j_w_iValues = myRecvPolys.calculateG_i_yValuesForVerification(numberOfPlayers, prime);
        List<Zp> myF_j_w_iValues = myRecvPolys.calculateF_i_xValuesForPlayers(numberOfPlayers, prime);
        for (int k = 0; k < numberOfPlayers; k++) {
            if ((recvComplaintesList.get(k) != null) && (recvComplaintesList.get(k).getMsg() == PlayerNotification.Confirmation.COMPLAINT)) {
                SecretPolynomials playerKNewPolys = recvPublicPolysList.get(k);
                // Check if the dealer  didn't publish all the required data or published a corrupted data
                if (!isSecretPolynomialsLegal(playerKNewPolys)) {
                    return true;
                }
                // Verify that the public information doesn't contradict itself - check that : f(w^k, w^k) = fk(w^k) = gk(w^k) = f(w^k, w^k)
                List<Zp> playerKNewFk_x_w_iValues = playerKNewPolys.calculateF_i_xValuesForPlayers(numberOfPlayers, prime);
                List<Zp> playerKNewGk_y_w_iValues = playerKNewPolys.calculateG_i_yValuesForVerification(numberOfPlayers, prime);
                if (!playerKNewFk_x_w_iValues.get(k).equals(playerKNewGk_y_w_iValues.get(k))) {
                    return true;
                }
                if (k == index) {
                    // Verify that the new public polynomials equals the old polynomials
                    if (!isSecretPolynomialsLegal(myRecvPolys) || !myRecvPolys.equals(playerKNewPolys)) {
                        return true;
                    }
                } else {
                    // Verify that the public information doesn't contradict the old information :
                    // f(w^j, w^k) = fk(w^j) = gj(w^k) = f(w^j, w^k) && f(w^k, w^j) = fj(w^k) = gk(w^j) = f(w^k, w^j)
                    if ((!myG_j_w_iValues.get(k).equals(playerKNewFk_x_w_iValues.get(index))) || (!myF_j_w_iValues.get(k).equals(playerKNewGk_y_w_iValues.get(index)))) {
                        return true;
                    }
                }
            }
        }
        return false;
    }


       protected   List<Coordinate>  compareCoordianteList(List<Zp> f_iValues, List<Zp> g_jValues){
            List<Coordinate> wrongCoords = new ArrayList<Coordinate>();
            for (int i = 0;i<f_iValues.size(); i++ )
            {
                    if (goodPlayers[i]){
                        Zp g_jVal = g_jValues.get(i);
                        Zp f_iVal = f_iValues.get(i);
                        if ((f_iVal == null) || (g_jVal == null) ||  (!f_iVal.equals(g_jVal)))
                        {
                                wrongCoords.add(new Coordinate(i, index));
                        }
                    }
            }
            return wrongCoords;
      }



        public static class Coordinate {

            public int i;
            public int j;;

            public Coordinate(int i, int j) {
                this.i = i;
                this.j = j;
            }
        }


    // Implementation according to Ran Canetti
    @Override
    public  Zp  reductionRandomizationStep(Zp ab) throws IOException{
        proglog.printInformation("performing Improved reduction & randomization step");

        boolean cheaterFound = true;
        List<Zp> recvSharesFromPlayers = null;

        while(cheaterFound)
        {
                  cheaterFound = false;
                 
                /* Share secret by VSS */
                recvSharesFromPlayers = inputStage(ab);
                /* Generate a t degree polynomial, hi(x) , with a  free coeef  that equals 'ab' and create share for users from it  */                
                //List<Zp> shareResultWithPlayers = Shamir.primitiveShare(ab, numberOfPlayers , polynomialDeg);
                /* Send to the j-th user hi(j) and receive from every other k player hk(i)  */
                //recvSharesFromPlayers = shareSimple(shareResultWithPlayers);

                /* Check if there were some null elements - from not playing players or cheater players and put zero instead - arbitrarily */
                for (int i = recvSharesFromPlayers.size() - 1; i >=0 ; i--)
                {
                    if (recvSharesFromPlayers.get(i) == null)
                    {
                        recvSharesFromPlayers.set(i, new Zp(prime, 0));
                    }
                    if (!goodPlayers[i])
                    {
                                recvSharesFromPlayers.remove(i);
                    }
                }
                

                List<Zp> calculationPolyCoeffs = new ArrayList<Zp>();
                /* Fill the first 2t+1 coeff with zero arbitrarily  */
                for (int i= 0; i < 2 * polynomialDeg + 1; i++)
                {
                    calculationPolyCoeffs.add(new Zp(prime, 0));
                }

                /* Perform the following iteration to calculate the 2t+1...n  coeffs of the  calculation polynomial*/
                 for (int k = 2 * polynomialDeg + 1; k < recvSharesFromPlayers.size(); k++)
                 {
                               List<Zp> K_LineAtInvVanderMonde = getMultStepCoeffsForCheaters(k);
                               /* Calculate your share of the K-th coeff at the calculation polynomial */
                               Zp myK_CoeffShare = new Zp(prime, 0);
                               for (int i =0; i < recvSharesFromPlayers.size(); i++)
                               {    
                                     myK_CoeffShare.add( recvSharesFromPlayers.get(i).constMul(K_LineAtInvVanderMonde.get(i)));
                               }

                               /* Send this to all other players so all of you could recombine the real k-th coeff  - no need to use the bulletin board */
                               List<Zp> K_CoeffShares = shareSimple(myK_CoeffShare);
                               /* Fix the received codeword and get the Recombined result */
                               calculationPolyCoeffs.add(getRecombinedResult(K_CoeffShares, prime));
                 }
                 Polynom calculationPoly = new Polynom(calculationPolyCoeffs);

                 if (calculationPoly.getDegree() == -1 )
                 {
                     /* No one cheated at this stage  */
                     break;
                 }

                List<Zp> XValues = new ArrayList<Zp>();
                int w = Zp.getFieldMinimumPrimitive(prime);
                for (int i = 0;i < numberOfPlayers; i++)
                {
                     XValues.add(new Zp(prime, Zp.calculatePower(w, i, prime) ));
                }

                /* Create the distorted code word */
                List<Zp> distortedCodeword = new ArrayList<Zp>();
                for (int i = 0; i < numberOfPlayers; i++)
                {
                        distortedCodeword.add(calculationPoly.Sample(XValues.get(i)));
                }
                List<Zp> fixedCodeword =  WelchBerlekampDecoder.decode(XValues, distortedCodeword, polynomialDeg, 2*polynomialDeg, prime);
                // Check For exception in codeword fixing
                if (fixedCodeword == null)
                {
                      String errorStr = "There were more then polynomialDegree = " + polynomialDeg + " Cheaters - cannot complete  mult step.";
                      proglog.printError(errorStr);
                      throw new IllegalStateException(errorStr);
                }

               for (int i = 0; i < fixedCodeword.size(); i++)
               {
                        if (goodPlayers[i]  && !fixedCodeword.get(i).equals(distortedCodeword.get(i)))
                        {
                                proglog.printWarning("Player Number " + i + " is a mult step cheater ! ");
                                cheaterFound = true;
                                removeCheaterPlayer(i);
                                // Continue at iteration till no one will cheat
                        }
               }
        }

       /* Finally calculate your share  - we will get here if no one had tried to cheat */
       List<Zp> firstLineAtInvVanderMonde = getMultStepCoeffsForCheaters(0);

       /* Calculate your share of the K-th coeff at the calculation polynomial */
       Zp tempSecret = new Zp(prime, 0);
       for (int i =0; i < recvSharesFromPlayers.size(); i++)
       {
             tempSecret.add( recvSharesFromPlayers.get(i).constMul(firstLineAtInvVanderMonde.get(i)));
       }
        return   tempSecret;
    }


public static void main(String[] args) {

        int prime = 19, polynomialDeg = 3, playerNum = 15;
        List<Integer> lyersList = Arrays.asList(2, 3, 4);//indexes of lyers
        Zp secret1 = new Zp(prime, 2);
        Zp secret2 = new Zp(prime, 3);

        List<Zp> share0 = Shamir.primitiveShare(secret1, playerNum, polynomialDeg);
        List<Zp> share1 = Shamir.primitiveShare(secret2, playerNum, polynomialDeg);

        List<List<Zp>> pShares = new ArrayList<List<Zp>>();
        for (int i = 0; i < playerNum; i++) {
            pShares.add(Arrays.asList(share0.get(i), share1.get(i)));
        }

        List<Zp> pMuls = new ArrayList<Zp>();
        for (int i = 0; i < playerNum; i++) {
            Zp zp = new Zp(pShares.get(i).get(0).mul(pShares.get(i).get(1)));
            if (!lyersList.contains(i)) {
                pMuls.add(zp);
            } else {
                pMuls.add(new Zp(zp).add(secret1));
            }
        }

        List<List<Zp>> shareResultWithPlayers = new ArrayList<List<Zp>>();
        for (int i = 0; i < playerNum; i++) {
            shareResultWithPlayers.add(Shamir.primitiveShare(pMuls.get(i), playerNum, polynomialDeg));
        }

        /* Send to the j-th user hi(j) and receive from every other k player hk(i)  */
        List<List<Zp>> recvShares = new ArrayList<List<Zp>>();
        for (int i = 0; i < playerNum; i++) {
            ArrayList<Zp> list = new ArrayList<Zp>();
            for (int j = 0; j < playerNum; j++) {
                list.add(shareResultWithPlayers.get(j).get(i));
            }
            recvShares.add(list);
        }

        ArrayList<ArrayList<Zp>> resVectors = new ArrayList<ArrayList<Zp>>();
        for (int i = 0; i < playerNum; i++) {
            resVectors.add(new ArrayList<Zp>());
        }


        for (int j = 0; j < playerNum; j++) {
            List<Zp> firstLineAtInvVanderMonde =
                    ZpMatrix.getSymmetricPrimitiveVandermondeMatrix(playerNum, prime).getTransposeMatrix().getInverse().getMatrixRow(j);

            // Calculate the value of the  polynomial H(x)  at i = H(i) as defined at GRR

            for (int k = 0; k < playerNum; k++) {
                Zp tempSecret = new Zp(prime, 0);
                for (int i = 0; i < playerNum; i++) {
                    tempSecret.add(recvShares.get(k).get(i).constMul(firstLineAtInvVanderMonde.get(i)));
                }
                resVectors.get(k).add(tempSecret);
            }

            List<Zp> tempList = new ArrayList<Zp>();
            for (int i = 0; i < playerNum; i++) {
                tempList.add(resVectors.get(i).get(0));
            }
            Zp Erez = Shamir.primitiveRecombine(tempList, polynomialDeg, prime);
            Zp Doron = null;
        }


        List<Zp> XValues = new ArrayList<Zp>();
        int w = Zp.getFieldMinimumPrimitive(prime);
        for (int i = 0; i < playerNum; i++) {
            XValues.add(new Zp(prime, Zp.calculatePower(w, i, prime)));
        }

        List<Zp> resVector = new ArrayList<Zp>();
        for (int k = 0; k < playerNum; k++) {
            List<Zp> recoList = new ArrayList<Zp>();
            for (int i = 0; i < playerNum; i++) {
                recoList.add(resVectors.get(i).get(k));
            }
            //List<Zp> recoListCons = WelchBerlekampDecoder.decode(XValues, recoList, polynomialDeg, polynomialDeg , prime);
            resVector.add(Shamir.primitiveRecombine(recoList, polynomialDeg, prime));
        }
        resVector.get(0).setValue(0);
        resVector.get(1).setValue(0);
        resVector.get(2).setValue(0);

        Polynom poly = new Polynom(resVector);
        List<Zp> malCodeWord = new ArrayList<Zp>();

        for (int i = 0; i < playerNum; i++) {
            malCodeWord.add(poly.Sample(XValues.get(i)));
        }
        System.out.println(malCodeWord);
        System.out.println(WelchBerlekampDecoder.decode(XValues, malCodeWord, polynomialDeg, 2 * polynomialDeg, prime));

    }


}
