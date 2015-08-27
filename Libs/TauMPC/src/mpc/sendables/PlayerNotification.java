/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package mpc.sendables;

import mpc.communication.BitStream;
import java.io.IOException;

public class PlayerNotification extends Sendable {
    public static final int CONFIRMATION_LENGTH = 1;

    private Confirmation msg;

    public PlayerNotification(Confirmation confMsg) {
        msg = confMsg;
    }

    public PlayerNotification() {
    }

    public Confirmation getMsg() {
        return msg;
    }

    public void setMsg(Confirmation msg) {
        this.msg = msg;
    }        

    public enum Confirmation {
        APPROVAL(0),
        COMPLAINT(1);
        private final int code;
        private Confirmation(int code) {
            this.code = code;
        }
        public static Confirmation getByCode(int code){
            for (Confirmation confirmation : values()){
                if (confirmation.code == code){
                    return confirmation;
                }                
            }
            throw new RuntimeException("Unknown Confirmation Type: " + code);
        }              
    }

    @Override
    public byte[] writeToByteArray() throws IOException {
        BitStream bs = new BitStream();
        writeToBitStreamNoHeader(bs);
        bs.close();
        return bs.getByteArray();
    }

    @Override
    public void writeToBitStreamNoHeader(BitStream bs) {
        bs.writeMessageType(MessageType.MESSAGE);
        assert msg != null;
        bs.writeInt(msg.code, CONFIRMATION_LENGTH);
    }        

    @Override
    protected void loadFromByteArrayNoHeader(BitStream bs, int prime) throws IOException {
        msg = Confirmation.getByCode(bs.readInt(CONFIRMATION_LENGTH));
    }
    
    public static void main(String[] args) throws IOException{
        int prime = 7;
        PlayerNotification note1 = new PlayerNotification(Confirmation.APPROVAL);
        PlayerNotification note2 = new PlayerNotification(Confirmation.COMPLAINT);
        PlayerNotification note11 = (PlayerNotification)Sendable.loadFromByteArray(note1.writeToByteArray(), prime);
        PlayerNotification note22 = (PlayerNotification)Sendable.loadFromByteArray(note2.writeToByteArray(), prime);
        int k = 3;
    }
}
