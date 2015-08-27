/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.protocols;

import mpc.ui.ProgressLog;
import mpc.circuit.Gate;
import mpc.circuit.Circuit;
import mpc.communication.XMLConnectionController;
import mpc.sendables.Sendable;
import mpc.sendables.ShareObject;
import mpc.finite_field_math.Shamir;
import mpc.finite_field_math.Zp;
import mpc.finite_field_math.ZpMatrix;
import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;
import mpc.communication.ConnectionController;
import mpc.communication.ConnectionController.Player;
import mpc.communication.ServerConnectionController;


public class MPCProtocol {
    
    protected  int polynomialDeg;
    protected Circuit circuit;
    protected ConnectionController cController;
    protected int numberOfPlayers;
    protected int index;
    protected int prime;
    protected ProgressLog proglog;
    

    public MPCProtocol(Circuit circuit, ProgressLog proglog, int index, int prime) {
        this.circuit = circuit;
        this.proglog = proglog;
        this.index = index;
        this.prime = prime;
        numberOfPlayers = circuit.getCircuitInputSize();
        // to get the maximum polinom deg we should ask if there is a mul in the circuit
        boolean multipleContained = circuit.isMultipleContained();
        this.polynomialDeg = multipleContained ? (numberOfPlayers - 1) / 2  : numberOfPlayers - 1;
    }
    
    public MPCProtocol(MPCProtocol protocol, Circuit circuit){
        this.circuit = circuit;
        this.proglog = protocol.proglog;
        this.numberOfPlayers = protocol.numberOfPlayers;
        this.index = protocol.index;
         this.prime = protocol.prime;
         this.polynomialDeg = protocol.polynomialDeg;
         this.cController = protocol.cController;
    }

    public boolean init(String serverIP, int port, File xmlFile) {
        cController = new ServerConnectionController();
        boolean res = false;
        try {
            res = ((ServerConnectionController)cController).CreateConnections(serverIP, port, index,prime, xmlFile, proglog);
        } catch (Exception e) {
            proglog.printError(e.getMessage());
            return false;
        }
        return res;
    }

    
    public boolean init(File xmlFile) {
        cController = new XMLConnectionController();
        boolean res = false;
        try {
            res =((XMLConnectionController)cController).CreateConnections(xmlFile, proglog, numberOfPlayers, index);
        } catch (Exception e) {
            proglog.printError(e.getMessage());
            return false;
        }
        return res;
    }
    
    public void close(){
        if (cController != null){
            try {
                cController.close();
            } catch (IOException ex) {
                proglog.printError("Could not close connections properly: " + ex.getMessage());
                return;
            }
            proglog.printInformation("Connection closed");
        }
        cController = null;        
    }

    public int getPolynomialDegree(){
        return polynomialDeg;
    }

    public int getNumberOfPlayers(){
        return numberOfPlayers;
    }
    
    public  Map<Integer, Zp> calculate(Zp input) throws IOException{
        return calculate(input, false,  "");
    }
    
    protected  List<? extends Sendable> getShareObjects(List<Zp> zPs){
        List<ShareObject> shareObjects = new ArrayList<ShareObject>();
        for (Zp zp : zPs){
            shareObjects.add(new ShareObject(zp, null));
        }
        return shareObjects;                
    }
    
    protected  List<Zp> getZPs(List<ShareObject> sharedObjects){
        List<Zp> zPs = new ArrayList<Zp>();
        for (ShareObject shareObject : sharedObjects){
            zPs.add( (shareObject != null) ? shareObject.getSharedSecret() : null);
        }
        return zPs;
    }
    
    protected  List<Zp> shareSimple(List<Zp> sharedSecrets) throws IOException{
        assert sharedSecrets.size() > 0;
        return getZPs(Sendable.asShareObjects(cController.shareSecrets(getShareObjects(sharedSecrets), prime)));
    }

   protected  List<Zp> shareSimple(Zp sharedSecrets) throws IOException{
        return getZPs(Sendable.asShareObjects(cController.shareSecrets(new ShareObject(sharedSecrets, null), prime)));
    }

    protected  List<Zp> inputStage(Zp input) throws IOException{
            List<Zp> myRecvShares;
            List<Zp> sharedSecrets = Shamir.share(input, numberOfPlayers, polynomialDeg);
            proglog.printInformation("My secret shares  are: " + sharedSecrets.toString());
            proglog.printInformation("sharing secrets with other players");
            myRecvShares = shareSimple(sharedSecrets);
            proglog.printInformation("Received secrets shares  are: " + myRecvShares.toString());
            return  myRecvShares;
    }

  protected Zp getRecombinedResult(List<Zp> recvList, int prime){
            return  Shamir.recombine(recvList, polynomialDeg, prime);
  }

   // Returns a List of all outputs 
    public  Map<Integer, Zp> calculate(Zp input, boolean partialCircuit, String numPrefix) throws IOException{
        List<Zp> myRecvShares;

        if (partialCircuit){
                myRecvShares = Arrays.asList(input);
        } else {
                myRecvShares = inputStage(input);
        }

        int k = 1; // temp only - needed to be index of gate
        for (Gate gate : circuit.getGates()){
            proglog.printInformation("calculating gate number "  + numPrefix + k +  " : a  '" +gate.getOperation().toString() + "' gate");
            gate.calculate(myRecvShares, this, k + ".");
            k++;
        }

        Map<Integer, Zp> outputsMap = circuit.getOutputs();
        if (partialCircuit){
            return outputsMap;
        }
        int numOfOutputs = outputsMap.size();
        Zp tempRes = null;
        List<Zp> advertiseList = new ArrayList<Zp>();
        List<Zp> recvList = null;
        Map<Integer, Zp> resultList = new TreeMap<Integer, Zp>();

        for (int i = 0; i < numOfOutputs; i++){
            tempRes = outputsMap.get(i);
            for (int j = 0;j < numberOfPlayers; j++){
                    advertiseList.add(tempRes);
            }
            proglog.printInformation("sharing results  with other players to recombine output number " +  (i+1));
            Map<Integer, Player> players = cController.getIndexToPlayer();
            filterPlayers(players);//remove unwanted players if necessary...
            
            //share result only with players that allow to calculate this output
            for (Integer currIndex : players.keySet()){
                if (currIndex == index){
                    continue;
                }
                Player player = players.get(currIndex);
                if (player.outputIndexes.contains(i)){
                    cController.sendSercrets(new ShareObject(advertiseList.get(currIndex), null), currIndex);
                }
            }
            
            recvList = new ArrayList<Zp>();
            //calculate output only if this player is allowed to calculate it.
            if (players.get(index).outputIndexes.contains(i)) {
                for (int currIndex = 0; currIndex < numberOfPlayers ; currIndex++){
                    if (!players.containsKey(currIndex)){
                        recvList.add(null);
                        continue;
                    }
                    if (currIndex == index){
                        recvList.add(advertiseList.get(currIndex));                        
                    }else{  
                        ShareObject share = cController.recieveSecrets(currIndex, prime).asShareObject();
                        recvList.add(share == null ? null : share.getSharedSecret());
                    }                                        
                }
                tempRes = getRecombinedResult(recvList, input.prime);
                resultList.put(i, tempRes);
            }
            advertiseList.clear();
        }
        return resultList;
    }
    
    protected void filterPlayers(Map<Integer, Player> players){        
    }


    // Implementation according to GRR 
    public  Zp  reductionRandomizationStep(Zp oldSecret) throws IOException{
        proglog.printInformation("performing reduction & randomization step");
        /* Generate a t degree polynomial, hi(x) , with a  free coeef  that equals 'ab' and create share for users from it  */
        List<Zp> shareResultWithPlayers = Shamir.share(oldSecret, numberOfPlayers , polynomialDeg);
        /* Send to the j-th user hi(j) and receive from every other k player hk(i)  */
        List<Zp> recvSharesFromPlayers = shareSimple(shareResultWithPlayers);
       
        List<Zp> firstLineAtInvVanderMonde =
                ZpMatrix.getSymmetricVandermondeMatrix(numberOfPlayers, oldSecret.prime).getTransposeMatrix().getInverse().getMatrixRow(0);

        // Calculate the value of the  polynomial H(x)  at i = H(i) as defined at GRR
        Zp tempSecret = new Zp(oldSecret.prime, 0);
        for (int i =0; i < numberOfPlayers;i++){
            tempSecret.add( recvSharesFromPlayers.get(i).mul(firstLineAtInvVanderMonde.get(i)));
        }        
        return   tempSecret;
    }



   
public static void main(String[] args) throws IOException {

    /* int prime = 61;
    int numOfPlayers = 2;
    int polynomDeg = 2;
    SimpleConCtrl simpleCon = new SimpleConCtrl();
    Zp secret1 = new Zp(prime,  9);
    Zp secret2 = new Zp(prime,  5);

    MPCProtocol mpc1 = null;
   Parser parser1 = new Parser("test1.txt", UserInterface.prime);
   try{
            parser1.parse();
            mpc1 = new MPCProtocol(parser1.getCircuit(),null);
   }catch(Exception e){
            int i = 5;
   }
    MPCProtocol mpc2 = null;
   Parser parser2 = new Parser("test1.txt", UserInterface.prime);
   try{
            parser2.parse();
            mpc2 = new MPCProtocol(parser2.getCircuit(),null);
   }catch(Exception e){
            int i = 5;
   }




   ///
   List<Zp> sharedSecrets1 = Shamir.share(secret1, mpc1.getNumberOfPlayers(), mpc1.getPolynomialDegree());
   List<Zp> sharedSecrets2 = Shamir.share(secret2, mpc2.getNumberOfPlayers(), mpc2.getPolynomialDegree());

   List<Zp> newShared1 = new ArrayList<Zp>();
   newShared1.add(sharedSecrets1.get(0));
   newShared1.add(sharedSecrets2.get(0));

   List<Zp> newShared2 = new ArrayList<Zp>();
   newShared2.add(sharedSecrets1.get(1));
   newShared2.add(sharedSecrets2.get(1));

   Zp res1 = new Zp (newShared1.get(0)).add(newShared1.get(1));
   Zp res2 = new Zp (newShared2.get(0)).add(newShared2.get(1));


   List<Zp> randomForUsers1 = Shamir.getRandomizedShares(mpc1.getNumberOfPlayers(), mpc1.getPolynomialDegree(), res1.prime);
   List<Zp> randomForUsers2 = Shamir.getRandomizedShares(mpc2.getNumberOfPlayers(), mpc2.getPolynomialDegree(), res2.prime);

   res1.add(randomForUsers1.get(0)).add(randomForUsers2.get(0));
   res2.add(randomForUsers1.get(1)).add(randomForUsers2.get(1));

   //reduction
   List<Zp> sharedTmpSec1 = Shamir.share(res1, mpc1.getNumberOfPlayers(), mpc1.getPolynomialDegree());
   List<Zp> sharedTmpSec2 = Shamir.share(res2, mpc2.getNumberOfPlayers(), mpc2.getPolynomialDegree());

   List<Zp> tempShares1 = new ArrayList<Zp>();
   tempShares1.add(sharedTmpSec1.get(0));
   tempShares1.add(sharedTmpSec2.get(0));

   List<Zp> tempShares2 = new ArrayList<Zp>();
   tempShares2.add(sharedTmpSec1.get(1));
   tempShares2.add(sharedTmpSec2.get(1));

  Zp[]  sharesArr1 = new Zp[tempShares1.size()];
  tempShares1.toArray(sharesArr1);
  Zp[]  sharesArr2 = new Zp[tempShares2.size()];
  tempShares2.toArray(sharesArr2);
              
  ZpMatrix sharesMat1 = new ZpMatrix(sharesArr1, ZpMatrix.VectorType.ROW_VECTOR);
  ZpMatrix sharesMat2 = new ZpMatrix(sharesArr2, ZpMatrix.VectorType.ROW_VECTOR);

  //calculate the matrix  multiplication in order to get the new shares of the players secrets
  ZpMatrix reductionMat = mpc1.getReductionStepMatrix(mpc1.getNumberOfPlayers(), mpc1.getPolynomialDegree(), secret1.prime);
  
  Zp[] sharedRes1 = sharesMat1.times(reductionMat).getZpVector();
  Zp[] sharedRes2 = sharesMat2.times(reductionMat).getZpVector();

 List<Zp> recvList1 = new ArrayList<Zp>();
recvList1.add(sharedRes1[0]);
recvList1.add(sharedRes2[0]);

List<Zp> recvList2 = new ArrayList<Zp>();
recvList2.add(sharedRes1[1]);
recvList2.add(sharedRes2[1]);

  Zp newSecret1 = Shamir.recombine(recvList1 , mpc1.getPolynomialDegree(), secret1.prime);
  Zp newSecret2 = Shamir.recombine(recvList2, mpc2.getPolynomialDegree(), secret2.prime);
  
   List<Zp> recvList = new ArrayList<Zp>();
   recvList.add(newSecret1);
   recvList.add(newSecret2);


   Zp res = Shamir.recombine(recvList, mpc1.getPolynomialDegree(), secret1.prime);

   System.out.println("the result is : " + res.getValue()); */

}    
}
