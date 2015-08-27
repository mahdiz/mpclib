/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.sendables;

import mpc.communication.BitStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;


public abstract class SendableList<T extends Sendable> extends Sendable{
    
    public static final int BITS_FOR_LENGTH_OF_LIST = 5;
    protected List<T> sendableList;
    
    public abstract MessageType getMessageType();
    
    public abstract T getNewInstrance();
        
    public SendableList(List<T> sendableList) {
        this.sendableList = sendableList;
    }

    public SendableList() {
    }

    public  List<T> getList(){
        return sendableList;
    }

    public void setSendableList(List<T> sendableList) {
        this.sendableList = sendableList;
    }        

    @Override
    public byte[] writeToByteArray() throws IOException {
        assert sendableList != null;
        BitStream bs = new BitStream();
        writeToBitStreamNoHeader(bs);
        bs.close();
        return bs.getByteArray();
    }

    @Override
    public void writeToBitStreamNoHeader(BitStream bs) {
        bs.writeMessageType(getMessageType());
        bs.writeInt(sendableList.size(), BITS_FOR_LENGTH_OF_LIST);
        for (T senable : sendableList) {
                bs.writeBoolean(senable != null);
                if (senable != null)
                {
                       senable.writeToBitStreamNoHeader(bs);
                }
        }
    }
    
    @Override
    protected void loadFromByteArrayNoHeader(BitStream bs, int prime) throws IOException {
        sendableList = new ArrayList<T>();
        int length = bs.readInt(BITS_FOR_LENGTH_OF_LIST);
        for (int i = 0; i < length; i++) {
            if (bs.readBoolean()) {
                T sendable = getNewInstrance();
                sendable.loadFromByteArrayNoHeader(bs, prime);
                sendableList.add(sendable);
            } else {
                sendableList.add(null);
                }
        }
    }

    @Override
    public boolean equals(Object obj) {
        if (!(obj instanceof SendableList)){
            return false;
        }
        SendableList bundle = (SendableList)obj;
        if (sendableList.size() != bundle.sendableList.size()){
            return false;
        }
        for (int i = 0; i < sendableList.size(); i++){
            if  ((sendableList.get(i) == null) && (bundle.sendableList.get(i) == null)){
                continue;
            }
            if (sendableList.get(i) == null && sendableList.get(i) != null || sendableList.get(i) != null && sendableList.get(i) == null){
                return false;
            }
            if (!(sendableList.get(i).equals(bundle.sendableList.get(i)))){
                return false;
            }
        }
        return true;
    }                  
}
