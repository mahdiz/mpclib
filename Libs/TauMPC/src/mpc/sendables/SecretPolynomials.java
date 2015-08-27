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


public class SecretPolynomials extends Sendable {

    private List<Zp> fi_x;
    private List<Zp> gi_y;

    public SecretPolynomials() {
        fi_x = new ArrayList<Zp>();
        gi_y = new ArrayList<Zp>();
    }

    public void setFi_xPolynomial(List<Zp> fi_x) {
        this.fi_x = fi_x;
    }

    public void setGi_yPolynomial(List<Zp> gi_y) {
        this.gi_y = gi_y;
    }

    public List<Zp> getFi_xPolynomial() {
        return fi_x;
    }

    public List<Zp> getGi_yPolynomial() {
        return gi_y;
    }

     public int getFi_xPolynomialLength() {
         if (fi_x != null){
              return fi_x.size();
         }
         return 0;
    }

    public int getGi_yPolynomialLength() {
        if (gi_y != null){
            return gi_y.size();
        }
        return 0;
    }

    @Override
    protected void loadFromByteArrayNoHeader(BitStream bs, int prime) throws IOException {
        if (bs.readBoolean()) {
            fi_x = bs.readList(prime);
        }
        if (bs.readBoolean()) {
            gi_y = bs.readList(prime);
        }
    }
    
    public void writeToBitStreamNoHeader(BitStream bs){
        bs.writeBoolean(fi_x != null);
        if (fi_x != null) {
            bs.writeList(fi_x);
        }
        bs.writeBoolean(gi_y != null);
        if (gi_y != null) {
            bs.writeList(gi_y);
        }        
    }

    public byte[] writeToByteArray() throws IOException {
        BitStream bs = new BitStream();
        bs.writeMessageType(MessageType.ZP_LISTS);
        writeToBitStreamNoHeader(bs);
        bs.close();
        return bs.getByteArray();
    }

    public List<Zp> calculateF_i_xValuesForPlayers(int numOfPlayers, int prime) {

        int w_i, w = Zp.getFieldMinimumPrimitive(prime);

        int value;
        List<Zp> f_i_xValues = new ArrayList<Zp>();
        for (int playerNum = 0; playerNum < numOfPlayers; playerNum++) {
            w_i = Zp.calculatePower(w, playerNum, prime);
            value = 0;
            for (int j = 0; j < fi_x.size(); j++) {
                value += Zp.calculatePower(w_i, j, prime) * fi_x.get(j).getValue();
            }
            f_i_xValues.add(new Zp(prime, value));
        }

        return f_i_xValues;
    }

    public List<Zp> calculateG_i_yValuesForVerification(int numOfPlayers, int prime) {

        int w_i, w = Zp.getFieldMinimumPrimitive(prime);

        int value;
        List<Zp> f_i_xValues = new ArrayList<Zp>();
        for (int playerNum = 0; playerNum < numOfPlayers; playerNum++) {
            w_i = Zp.calculatePower(w, playerNum, prime);
            value = 0;
            for (int j = 0; j < gi_y.size(); j++) {
                value += Zp.calculatePower(w_i, j, prime) * gi_y.get(j).getValue();
            }
            f_i_xValues.add(new Zp(prime, value));
        }

        return f_i_xValues;
    }

    @Override
    public boolean equals(Object obj) {
        if (!(obj instanceof SecretPolynomials)){
            return false;
        }
        SecretPolynomials secretPolynomials = (SecretPolynomials)obj;
        if (secretPolynomials.fi_x.size() != fi_x.size() || secretPolynomials.gi_y.size() != gi_y.size()){
            return false;
        }
        for (int i = 0; i < fi_x.size(); i++){
            if (!(fi_x.get(i).equals(secretPolynomials.fi_x.get(i)))){
                return false;                
            }
        }
        for (int i = 0; i < gi_y.size(); i++){
            if (!(gi_y.get(i).equals(secretPolynomials.gi_y.get(i)))){
                return false;                
            }
        }
        return true;
    }        

    @Override
    public String toString() {
        String outputStr = new String();
        outputStr += "Fi_x  : " + ((fi_x  != null) ?  fi_x.toString() : null) + "\n";
        outputStr += "Gi_y  : " + ((gi_y  != null) ?  gi_y.toString() : null);
        return outputStr;
    }
}