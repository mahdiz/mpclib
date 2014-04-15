/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.communication;

import java.io.IOException;
import java.util.LinkedList;
import java.util.List;
import mpc.sendables.Sendable;


public class ToServerObject extends Sendable{
    
    public int MyPort;
    public String MyIp;
    public int MyIndex;
    public int MyPrime;
    public List<List<Integer>> MyOutputs;
    public int ExpectedPlayers;

    public ToServerObject()
    {
      
    }


    ToServerObject(String MyIp, int MyPort, int MyIndex, List<List<Integer>> MyOutputs, int ExpectedPlayers, int MyPrime)
    {
        this.MyPort = MyPort;
        this.MyIndex = MyIndex;
        this.MyOutputs = MyOutputs;
        this.MyIp = MyIp;
        this.MyPrime = MyPrime;
        this.ExpectedPlayers = ExpectedPlayers;
    }
    
    @Override
    public byte[] writeToByteArray() throws IOException
    {
        BitStream bs = new BitStream();
        bs.writeMessageType(MessageType.TO_SERVER_OBJECT);
        writeToBitStreamNoHeader(bs);        
        bs.close();
        return bs.getByteArray();  
    }

     @Override
    protected void loadFromByteArrayNoHeader(BitStream bs, int prime) throws IOException
    {
        ExpectedPlayers = bs.readInt(30);
        MyIndex = bs.readInt(30);
        MyPort = bs.readInt(30);
        MyIp = bs.readString();
        MyPrime = bs.readInt(30);
        MyOutputs = new LinkedList<List<Integer>>();
        for(int i=0; i<ExpectedPlayers; i++)
        {     
            int ArrLength = bs.readInt(30);
            if (ArrLength == 0)
            {
                MyOutputs.add(new LinkedList<Integer>());
                continue;
            }
            
            List<Integer> intList = new LinkedList<Integer>();
            for (int j=0;j<ArrLength;j++)
            {
                intList.add(bs.readInt(30));
            }    
            MyOutputs.add(intList);
        }
    }

    @Override
    public void writeToBitStreamNoHeader(BitStream bs)
    {
        bs.writeInt(ExpectedPlayers, 30);
        bs.writeInt(MyIndex, 30);
        bs.writeInt(MyPort, 30);
        bs.writeString(MyIp);
        bs.writeInt(MyPrime,30);
        for (List<Integer> intList : MyOutputs)
        {
            if(intList==null)
            {
                bs.writeInt(0, 30);
                continue;
            }
            
            bs.writeInt(intList.size(), 30);
            for(int i=0; i<intList.size(); i++)
            {
                bs.writeInt(intList.get(i), 30);
            }
        }
    }

}
