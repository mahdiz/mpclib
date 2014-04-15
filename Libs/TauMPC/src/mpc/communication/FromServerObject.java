/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.communication;

import java.io.IOException;
import java.util.List;
import mpc.sendables.SendableList;


public class FromServerObject extends SendableList<ToServerObject> {
    boolean isValid;
    String  serverMsg;

    public FromServerObject()
    {
        
    }
    
    public FromServerObject(List<ToServerObject> ToServerList, boolean isValid, String serverMsg)
    {
        this.isValid = isValid;
        this.serverMsg = serverMsg;
        this.sendableList = ToServerList;
    }

    @Override
    public MessageType getMessageType()
    {
        return MessageType.FROM_SERVER_OBJECT;
    }

    @Override
    public ToServerObject getNewInstrance()
    {
        return new ToServerObject();
    }
    
    @Override
    public void writeToBitStreamNoHeader(BitStream bs)
    {
        super.writeToBitStreamNoHeader(bs);
        bs.writeBoolean(isValid);
        bs.writeString(serverMsg);
    }
    
    @Override
    protected void loadFromByteArrayNoHeader(BitStream bs, int prime) throws IOException 
    {
       super.loadFromByteArrayNoHeader(bs, prime);
       isValid = bs.readBoolean();
       serverMsg = bs.readString();
    }


    


}
