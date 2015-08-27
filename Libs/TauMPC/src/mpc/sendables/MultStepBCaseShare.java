/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.sendables;

import mpc.communication.BitStream;
import mpc.finite_field_math.Zp;
import java.io.IOException;



public class MultStepBCaseShare  extends Sendable {

    private Zp aShare, bShare, abShare, rShare;//nullables

    public MultStepBCaseShare( Zp aShare,  Zp bShare,  Zp abShare,  Zp rShare) {
        this.aShare = aShare;
        this.bShare = bShare;
        this.abShare = abShare;
        this.rShare = rShare;
    }

    public MultStepBCaseShare() {
    }        


    public Zp getAShare() {
        return aShare;
    }

    public Zp getAbShare() {
        return abShare;
    }

    public Zp getBShare() {
        return bShare;
    }

    public Zp getRShare() {
        return rShare;
    }


    public void setAShare(Zp aShare) {
        this.aShare = aShare;
    }

    public void setAbShare(Zp abShare) {
        this.abShare = abShare;
    }

    public void setBShare(Zp bShare) {
        this.bShare = bShare;
    }

    public void setRShare(Zp rShare) {
        this.rShare = rShare;
    }   

    @Override
    public byte[] writeToByteArray() throws IOException {
        BitStream bs = new BitStream();
        bs.writeMessageType(MessageType.MULT_STEP_BCASE);      
        writeToBitStreamNoHeader(bs);
        bs.close();
        return bs.getByteArray();
    }
    
    public void writeToBitStreamNoHeader(BitStream bs){
        writeSecret(bs, aShare);
        writeSecret(bs, bShare);
        writeSecret(bs, abShare);
        writeSecret(bs, rShare);   
    }
    
    public static MultStepBCaseShare readFromBitStreamNoHeader(BitStream bs, int prime) throws IOException{
        MultStepBCaseShare multStepBCaseShare= new MultStepBCaseShare();
        multStepBCaseShare.loadFromByteArrayNoHeader(bs, prime);            
        return multStepBCaseShare;
    }
        
    @Override
    protected void loadFromByteArrayNoHeader(BitStream bs, int prime) throws IOException {
        aShare = readSecret(bs, prime);
        bShare = readSecret(bs, prime);
        abShare = readSecret(bs, prime);
        rShare = readSecret(bs, prime);        
    }

    private void writeSecret(BitStream bs, Zp secret) {
        bs.writeBoolean(secret != null);
        if (secret != null) {
            bs.writeInt(secret.getValue(), BitStream.LENGTH_OF_SECRET);
        }
    }
    
    private Zp readSecret(BitStream bs, int prime) throws IOException{
        if (bs.readBoolean()){
            return new Zp(prime, bs.readInt(BitStream.LENGTH_OF_SECRET));
        }else{
            return null;
        }
    }

    @Override
    public boolean equals(Object obj) {
        if (!(obj instanceof MultStepBCaseShare)){
            return false;
        }
        MultStepBCaseShare second = (MultStepBCaseShare) obj;
        return compareSecrets(aShare, second.aShare) && compareSecrets(bShare, second.bShare) &&
                compareSecrets(abShare, second.abShare) && compareSecrets(rShare, second.rShare);
    }        
    
    public static MultStepBCaseShare createRandom(int prime){//for testing
        double d = Math.random();
        if (d < 0.25){
            return null;
        }
        MultStepBCaseShare multStepBCaseShare = new MultStepBCaseShare();
        multStepBCaseShare.aShare = new Zp(prime, (int)Math.floor(d * 20));
        multStepBCaseShare.bShare = new Zp(prime, (int)Math.floor(d * 40));
        multStepBCaseShare.abShare = new Zp(prime, (int)Math.floor(d * 60));
        multStepBCaseShare.rShare = new Zp(prime, (int)Math.floor(d * 80));
        return multStepBCaseShare;        
    }
    
    public static void main(String[] args) throws IOException{
        int prime = 7;
        MultStepBCaseShare multStep = createRandom(prime);
        byte[] ba = multStep.writeToByteArray();
        Sendable sendable = Sendable.loadFromByteArray(ba, prime);
        boolean b = sendable.equals(multStep);
        int k = 3;
    }
    
    

}
