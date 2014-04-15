/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.protocols;

import java.io.IOException;
import mpc.circuit.Circuit;
import mpc.finite_field_math.Zp;
import mpc.ui.ProgressLog;


public class MPCProtocolMultStepCheaterPlayer extends MPCProtocolByzantineCase{

    public MPCProtocolMultStepCheaterPlayer(MPCProtocolByzantineCase protocol, Circuit circuit) {
        super(protocol, circuit);
    }

    public MPCProtocolMultStepCheaterPlayer(Circuit circuit, ProgressLog proglog, int index, int prime) {
        super(circuit, proglog, index, prime);
    }

    
    @Override
    public Zp reductionRandomizationStep(Zp ab) throws IOException {
        int randomNum = (int) (prime * Math.random());
        if (randomNum > 2/3*prime)
        {
              /* Don't send 'ab' - send a fabrication of it */
              ab = new Zp(prime, randomNum);
        }
        return super.reductionRandomizationStep(ab);
    }


}
