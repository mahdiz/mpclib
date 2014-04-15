/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.communication;

import mpc.finite_field_math.Zp;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.util.ArrayList;
import java.util.List;
import mpc.sendables.Sendable.MessageType;




public class BitStream {
    
    public static final int BITS_FOR_LENGTH_OF_MESSAGE = 16;//has to be devisible by BitStream.BYTE_LENGTH (8)
    public static final int BYTE_LENGTH = 8;
    public static final int BYTES_FOR_LENGTH_OF_MESSAGE = BITS_FOR_LENGTH_OF_MESSAGE/BYTE_LENGTH;    
    public static final int BOOLEAN_LENGTH = 1;    
    public static final int STRING_LENGTH = 8;
    public static final int LIST_LENGTH = 7;//2^(LIST_LENGTH-1) should be the maximal legnth of a list
    public static final int LENGTH_OF_SECRET = 16;
    public static final int LENGTH_OF_GATE_INDEX = 16;  
    public static final int MESSAGE_TYPE_LENGTH = 6;
    
    ByteArrayOutputStream outputStream = null;
    ByteArrayInputStream inputStream = null;
    private int offset;
    private int offsetLength = 0;
    private final int inputLength;
    private int readPos = 0;
    public static final int[] masks; 

    static {
        masks = new int[33];
        for (int i = 0; i < masks.length; i++) {
            masks[i] =pow(2, i) - 1;
        }
    }

    static private int pow(int num, int power)
    {
        int retVal = 1;
        for (int i = 0; i < power; i++)
        {
            retVal = retVal * num;
        }
        return retVal;
    }
    
    public BitStream() {
        outputStream = new ByteArrayOutputStream();
        inputLength = 0;
    }
    
     public BitStream(byte[] bytes) {
        inputStream = new ByteArrayInputStream(bytes);
        inputLength = bytes.length;
    }
    
    private void checkWrite(boolean write) throws IOException {
        if (write && outputStream == null || !write && inputStream == null) {
            throw new IOException("Invalid operation");
        }
    }
    
    private int getPosition()
    {
        return inputLength - inputStream.available();
    }

    public void writeInt(int numToWrite, int lengthInBits){
  //      assert 0 <= lengthInBits && lengthInBits <= 32;
        if (lengthInBits + offsetLength < 8){ //Not enough bits to create new Byte. Adding the new bits to the offset
            offset =  ((offset << lengthInBits) | (numToWrite & masks[lengthInBits]));
            offsetLength += lengthInBits;
            return;
        }
        //first take 8 bits from the numToWrite (or less if lengthInBits < 8)
        int firstByte = lengthInBits <= 8 ? (numToWrite & masks[lengthInBits]) << (8 - lengthInBits) : (numToWrite >> (lengthInBits - 8)) & masks[8];
        int numOfBitsToWrite = 8 - offsetLength;

        //move the bits to write from the number to the right (by offsetLength)
        int toWriteFromInput = ((firstByte >> offsetLength) & masks[numOfBitsToWrite]);

        //prepare the byte to write. Contains the offset and the new bits from numToWrite
        int toWrite = ((offset << (8 - offsetLength)) | toWriteFromInput);
        outputStream.write(toWrite);

        //calculate the new offsetLength
        offsetLength = lengthInBits > 8 ? offsetLength : lengthInBits + offsetLength - 8;

        //calculate the new offset. Should be composed of all the bits we did not write from firstByte
        offset = (firstByte >> (8 - (numOfBitsToWrite + offsetLength))) & masks[offsetLength];

        //create the new numToWrite for the recursive call.
        numToWrite = numToWrite & masks[lengthInBits - numOfBitsToWrite];

        //do recursive call to continue writing
        writeInt(numToWrite, Math.max(lengthInBits - 8, 0));
    }
    
    public void writeBoolean(boolean b) {
        int toWrite = b ? 1 : 0;
        writeInt(toWrite, 1);
    }

    public void writeLengthOfMessage(int lengthOfMessage) {
        writeInt(lengthOfMessage, BITS_FOR_LENGTH_OF_MESSAGE);
    }
    
    public void writeMessageType(MessageType messageType){
        writeInt(messageType.code, MESSAGE_TYPE_LENGTH);        
    }

       public void writeString(String st) {
           if (st == null || st.length() == 0) {
               writeByte((byte) 0);
               return;
           }
           byte[] bytes;
           try {
               bytes = st.getBytes("UnicodeLittleUnmarked");
           } catch (UnsupportedEncodingException ex) {
               return;
           }
           writeByte((byte) bytes.length);
           writeBytes(bytes);
    }

    public void writeByte(byte b) {
        writeInt((int) b, BYTE_LENGTH);
    }

    public void writeBytes(byte[] bytes) {
        for (int i = 0; i < bytes.length; i++) {
            writeByte(bytes[i]);
        }
    }
    
    public void writeList(List<Zp> valuesToWrite){
        writeInt(valuesToWrite.size(), LIST_LENGTH);
        for(Zp zp : valuesToWrite){
            writeBoolean(zp != null);
            if(zp != null){
                writeInt(zp.getValue(), LENGTH_OF_SECRET);                
            }            
        }       
    }
    

    public String getFormattedByteAsString(int num) {
        String st = Integer.toHexString(num);
        if (st.length() == 1) {
            return "0" + st;
        }
        return st;
    }

    public int readLengthOfMessage() throws IOException {
        return readInt(BITS_FOR_LENGTH_OF_MESSAGE);
    }   

    public byte readByte() throws IOException {
        return (byte) readInt(BYTE_LENGTH);
    }

    public boolean readBoolean() throws IOException {
        int value = readInt(1);
        return value == 1;
    }

    public byte[] readBytes(int numberOfBytes) throws IOException {
        byte[] bytes = new byte[numberOfBytes];
        for (int i = 0; i < numberOfBytes; i++) {
            bytes[i] = readByte();
        }
        return bytes;
    }
    
    public MessageType readMessageType() throws IOException{
        return MessageType.getByCode(readInt(MESSAGE_TYPE_LENGTH));
    }
    
    public List<Zp> readList(int prime) throws IOException{
        int length = readInt(LIST_LENGTH);
        List<Zp> valuesRead = new ArrayList<Zp>();
        for (int i = 0; i < length; i++){
            if (readBoolean()){
                valuesRead.add(new Zp(prime, readInt(LENGTH_OF_SECRET)));                                        
            }else{
                valuesRead.add(null);
            }            
        }
        return valuesRead;
    }

    public String readString() throws IOException {
        int length = readByte();
        if (length == 0) {
            return "";
        }
        byte[] StringBytes = readBytes(length);
        String st = null;
        try {
            st = new String(StringBytes, "UnicodeLittleUnmarked");
        } catch (UnsupportedEncodingException ex) {
       //     Logger.getLogger(BitStream.class.getName()).log(Level.SEVERE, null, ex);
        }
        return st;
    }

    public int readInt(int numberOfBits) throws IOException {
        checkWrite(false);
        inputStream.mark(inputStream.available());
        if (readPos + numberOfBits > inputLength * BYTE_LENGTH) {
            throw new RuntimeException("Cannot read " + numberOfBits + " bits");
        }
        byte[] bytes = new byte[5];
        inputStream.read(bytes, 0, 5);
        int readPosInByte = readPos % BYTE_LENGTH;
        int value = getIntegerFromByteBuffer(bytes, readPosInByte, numberOfBits);
        readPos += numberOfBits;
        inputStream.reset();
        inputStream.read(bytes, 0, (readPosInByte + numberOfBits) / BYTE_LENGTH);        
        return value;
    }

    private int getIntegerFromByteBuffer(byte[] bb, int bitPosition, int numOfBits) {
     //   assert numOfBits > 0;
        int bytePos = bitPosition / 8;
        int rightBitOff = 40 - numOfBits - (bitPosition % 8);
        byte byte1 = bb.length > bytePos + 1 ? bb[bytePos + 1] : (byte) 0;
        byte byte2 = bb.length > bytePos + 2 ? bb[bytePos + 2] : (byte) 0;
        byte byte3 = bb.length > bytePos + 3 ? bb[bytePos + 3] : (byte) 0;
        byte byte4 = bb.length > bytePos + 4 ? bb[bytePos + 4] : (byte) 0;
        long rawLong = makeLong(bb[bytePos], byte1, byte2, byte3, byte4);
        int mask1 = BitStream.masks[numOfBits];
        int result = (int)((rawLong >> rightBitOff) & mask1);
        return result;

    }

    private long makeLong(byte b1, byte b2, byte b3, byte b4, byte b5) {
        return (long)((int)b1 & 0xff) << 32 |
                (long)(b2 & 0xff) << 24 |
                (long)(b3 & 0xff) << 16 |
                (long)(b4 & 0xff) << 8 |
                (long)(b5 & 0xff);

    }
    
    public boolean canRead() throws IOException {
        return canRead(0);
    }

    public boolean canRead(int numberOfBits) throws IOException {
        checkWrite(false);
        return readPos + numberOfBits < inputLength * BYTE_LENGTH;
    }

    public void close() throws IOException {
        if (offsetLength > 0){
            int toWrite = offset << (8 - offsetLength);
            outputStream.write(toWrite);
        }        
        outputStream.close();
    }

    public byte[] getByteArray() {
        return outputStream.toByteArray();
    }
    
    public ByteArrayOutputStream getStream(){
        return outputStream;
    } 
}
