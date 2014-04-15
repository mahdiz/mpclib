/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.sendables;

import mpc.communication.BitStream;
import mpc.finite_field_math.Zp;
import java.io.IOException;


public class ShareObject extends Sendable{    
    
    private Zp sharedSecret;
    private Integer gateIndex;

    public ShareObject(Zp sharedSecret, Integer gateIndex) {
        this.sharedSecret = sharedSecret;
        this.gateIndex = gateIndex;        
    }

    public ShareObject() {
    }        
    
    public byte[] writeToByteArray() throws IOException{
        BitStream bs = new BitStream();
        bs.writeMessageType(MessageType.SIMPLE_ZP);
        writeToBitStreamNoHeader(bs);        
        bs.close();
        return bs.getByteArray();        
    }

    @Override
    public void writeToBitStreamNoHeader(BitStream bs) {
        bs.writeBoolean(sharedSecret != null);
        if (sharedSecret != null){
            bs.writeInt(sharedSecret.getValue(), BitStream.LENGTH_OF_SECRET);            
        }
        
        bs.writeBoolean(gateIndex != null);
        if (gateIndex != null){
            bs.writeInt(gateIndex, BitStream.LENGTH_OF_GATE_INDEX);            
        }
    }
    
    
    
    protected void loadFromByteArrayNoHeader(BitStream bs, int prime) throws IOException{
        if (bs.readBoolean()) {
            int shared = bs.readInt(BitStream.LENGTH_OF_SECRET);
            sharedSecret = new Zp(prime, shared);
        }
        if (bs.readBoolean()) {
            gateIndex = bs.readInt(BitStream.LENGTH_OF_GATE_INDEX);
        }                
    }

    public int getGateIndex() {
        return gateIndex;
    }

    public Zp getSharedSecret() {
        return sharedSecret;
    }    
}
