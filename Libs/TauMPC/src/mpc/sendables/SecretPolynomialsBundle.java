/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package mpc.sendables;

import mpc.finite_field_math.Zp;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

public class SecretPolynomialsBundle extends SendableList<SecretPolynomials> {
    
    public SecretPolynomialsBundle(List<SecretPolynomials> secretPolysList) {
        super(secretPolysList);
    }

    public SecretPolynomialsBundle() {
    }

    @Override
    public MessageType getMessageType() {
        return MessageType.ZPS_BUNDLE;
    }

    @Override
    public SecretPolynomials getNewInstrance() {
        return new SecretPolynomials();
    }

    public static void main(String[] args) throws IOException {
        int prime = 7;
        List<SecretPolynomials> polys = new ArrayList<SecretPolynomials>();
        for (int i = 0; i < 6; i++) {
            if ((i%2) == 0){
                polys.add(null);
                continue;
            }
            List<Zp> x = new ArrayList<Zp>();
            List<Zp> y = new ArrayList<Zp>();
            for (int j = 10; j < 15; j++) {
                x.add(new Zp(prime, j * i));
                y.add(new Zp(prime, 30 - j * i));
            }
            SecretPolynomials secretPolynomials = new SecretPolynomials();
            secretPolynomials.setFi_xPolynomial(x);
            secretPolynomials.setGi_yPolynomial(y);
            polys.add(secretPolynomials);
        }
        polys.add(null);
        polys.add(null);
        SecretPolynomialsBundle bundle = new SecretPolynomialsBundle(polys);
        byte[] ba = bundle.writeToByteArray();
        Sendable sendable = Sendable.loadFromByteArray(ba, prime);
        boolean b = sendable.equals(bundle);
        int k = 3;
    }
}
