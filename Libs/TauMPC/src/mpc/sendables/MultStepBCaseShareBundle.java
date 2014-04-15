/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.sendables;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

public class MultStepBCaseShareBundle extends SendableList<MultStepBCaseShare>{

    public MultStepBCaseShareBundle() {
    }

    public MultStepBCaseShareBundle(List<MultStepBCaseShare> bCaseShareBundle) {
        super(bCaseShareBundle);
    }

    @Override
    public MessageType getMessageType() {
        return MessageType.MULT_STEP_BCASE_BUNDLE;
    }

    @Override
    public MultStepBCaseShare getNewInstrance() {
        return new MultStepBCaseShare(); 
    }
    
    public static void main(String[] args) throws IOException {
        int prime = 7;
        List<MultStepBCaseShare> list = new ArrayList<MultStepBCaseShare>();
        for (int i = 0; i < 6; i++) {
            list.add(MultStepBCaseShare.createRandom(prime));
        }
        MultStepBCaseShareBundle bundle = new MultStepBCaseShareBundle(list);
        byte[] ba = bundle.writeToByteArray();
        Sendable sendable = Sendable.loadFromByteArray(ba, prime);
        boolean b = sendable.equals(bundle);
        int k = 3;
    }       
}
