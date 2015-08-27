/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.sendables;

import mpc.communication.BitStream;
import mpc.finite_field_math.Zp;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;


public class MultStepVerificationPoly extends Sendable{

    List<Zp> RxPolynomial;

    public MultStepVerificationPoly() {
    }


    public MultStepVerificationPoly(List<Zp> RxPolynomial) {
        this.RxPolynomial = RxPolynomial;
    }


    public List<Zp> getRxPolynomial() {
        return RxPolynomial;
    }

    public void setRxPolynomial(List<Zp> RxPolynomial) {
        this.RxPolynomial = RxPolynomial;
    }

    @Override
    public byte[] writeToByteArray() throws IOException {
        BitStream bs = new BitStream();
        bs.writeMessageType(MessageType.MULT_STEP_VERIFY_POLY);      
        writeToBitStreamNoHeader(bs);        
        bs.close();
        return bs.getByteArray();
    }

    @Override
    protected void loadFromByteArrayNoHeader(BitStream bs, int prime) throws IOException {
        if (bs.readBoolean()) {
            RxPolynomial = bs.readList(prime);
        }
    }

    @Override
    public void writeToBitStreamNoHeader(BitStream bs) {
        bs.writeBoolean(RxPolynomial != null);
        if (RxPolynomial != null){
            bs.writeList(RxPolynomial);
        }
    }  
    
    @Override
    public boolean equals(Object obj) {
        if (!(obj instanceof MultStepVerificationPoly)){
            return false;
        }
        MultStepVerificationPoly second = (MultStepVerificationPoly)obj;
        if (RxPolynomial == null && second.RxPolynomial != null || RxPolynomial != null && second.RxPolynomial == null) {
            return false;
        }
        if (RxPolynomial == null && second.RxPolynomial == null) {
            return true;
        }
        if (RxPolynomial.size() != second.RxPolynomial.size()) {
            return false;
        }
        for (int i = 0; i < RxPolynomial.size(); i++) {
            if (!(compareSecrets(RxPolynomial.get(i), second.RxPolynomial.get(i)))) {
                return false;
            }
        }
        return true;
    }
    
    public static void main(String[] args) throws IOException{
        int prime = 7;
        List<Zp> rxPoly = new ArrayList<Zp>();
        for (int i = 0; i < 6; i++) {
            if ((i%2) == 0){
                rxPoly.add(null);
                continue;
            }
            rxPoly.add(new Zp(prime, 3 * i));
        }
        MultStepVerificationPoly multStep = new MultStepVerificationPoly(rxPoly);
        byte[] ba = multStep.writeToByteArray();
        Sendable sendable = Sendable.loadFromByteArray(ba, prime);
        boolean b = sendable.equals(multStep);
        int k = 3;
    }
}
