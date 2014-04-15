/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.circuit;

import mpc.protocols.MPCProtocol;
import mpc.protocols.MPCProtocolByzantineCase;
import mpc.finite_field_math.Zp;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import mpc.protocols.MPCProtocolMultStepCheaterPlayer;


public class Gate {
    private List<Wire> inputWires;
    private List<Wire> outputWires;
    private Operation operation;
    private Zp outputValue;
    private int p;//the prime number

    public Gate(List<Wire> inputWires, Wire outputWire, Operation operation, int p) {
        this(inputWires, Arrays.asList(outputWire), operation, p);
    }
    public Gate(List<Wire> inputWires, List<Wire> outputWires, Operation operation, int p) {
        assert operation != Operation.MUL || inputWires.size() <= 2;//max 2 inputs for mul gate
        this.inputWires = inputWires;
        this.outputWires = new ArrayList<Wire>(outputWires);
        this.operation = operation;
        this.p = p;
    }    
            
    public enum Operation{
        MUL("*"),
        ADD("+"),
        DIV("/"),
        SUB("-");
        private String displayName;
        private Operation(String displayName) {
            this.displayName = displayName;
        }
        @Override
        public String toString() {
            return displayName;
        }        
        
        public static Operation getOperationByName(String name){
            for (Operation op : Operation.values()){
                if (op.toString().equals(name)){
                    return op;
                }
            }
            return null;
        }
    }
    
    public boolean isReadyToCalculate(){
        for (Wire wire : inputWires){
            if (!wire.isValid()){
                return false;
            }
        }
        return true;
    }
    
    public boolean isOutputReady(){
        return outputValue != null;
    }

    public Zp getOutputValue() {
        return outputValue;
    }

    public void deleteOutputValue() {
        this.outputValue = null;
    }
    
    
            
    //inputs are all the inputs to the circuit. Gate should know to choose the inputs it needs.
    public Zp calculate(List<Zp> inputs, MPCProtocol protocol, String gatePrefix) throws IOException{
        List<Zp> values = new ArrayList<Zp>();
        for (Wire wire : inputWires){
            if (wire.isInput()){
                if (inputs.size() <= wire.getInputIndex()){
                    throw new RuntimeException("Input " + wire.getInputIndex() + " is expected - not found in the list given");
                }
                values.add(inputs.get(wire.getInputIndex()));
            }else{
                assert wire.getSourceGate() != null && wire.getSourceGate().isOutputReady();
                values.add(wire.getConstValue() != null ? wire.getConstValue() : wire.getSourceGate().getOutputValue());
            }
        }
        outputValue = new Zp(values.get(0));
        values.remove( 0 );

        for (Zp value : values) {
            Zp currValue;
            if (operation == Operation.DIV){
                MPCProtocol divProtocol;
                if (protocol  instanceof  MPCProtocolMultStepCheaterPlayer) {
                        divProtocol = new MPCProtocolMultStepCheaterPlayer((MPCProtocolMultStepCheaterPlayer) protocol, Circuit.getDivisionExponentialCircuit(p));
                } else if (protocol  instanceof MPCProtocolByzantineCase){
                        divProtocol = new MPCProtocolByzantineCase((MPCProtocolByzantineCase) protocol, Circuit.getDivisionExponentialCircuit(p));
                }else{
                        divProtocol = new MPCProtocol(protocol, Circuit.getDivisionExponentialCircuit(p));
                }
                currValue = new Zp(p, divProtocol.calculate(value, true, gatePrefix).get(0).getValue());
            }else{
                currValue = new Zp(p, value.getValue());
            }
            outputValue.Calculate(currValue, operation == Operation.DIV ? Operation.MUL : operation);
        }        
        if (isPolynomDegreeReducingNeeded()){
                        outputValue = protocol.reductionRandomizationStep(outputValue);
        }
        return outputValue;
    }



        //inputs are all the inputs to the circuit. Gate should know to choose the inputs it needs. - for internal use only
    public Zp internalCalculate(List<Zp> inputs)  {
        List<Zp> values = new ArrayList<Zp>();
        for (Wire wire : inputWires){
            if (wire.isInput()){
                if (inputs.size() <= wire.getInputIndex()){
                    throw new RuntimeException("Input " + wire.getInputIndex() + " is expected - not found in the list given");
                }
                values.add(inputs.get(wire.getInputIndex()));
            }else{
                assert wire.getSourceGate() != null && wire.getSourceGate().isOutputReady();
                values.add(wire.getConstValue() != null ? wire.getConstValue() : wire.getSourceGate().getOutputValue());
            }
        }
        outputValue = new Zp(values.get(0));
        values.remove( 0 );

        for (Zp value : values) {
            outputValue.Calculate(value, operation == Operation.DIV && value.getValue() == 0 ? Operation.MUL : operation);
        }
        
        return outputValue;
    }



    public boolean isPolynomDegreeReducingNeeded(){
        if (getOperation() != Operation.MUL && getOperation() != Operation.DIV){
            return false;
        }
        boolean foundMul = false;
        for (Wire wire : getInputWires()){
            if (wire.getConstValue() == null){
                if (foundMul){
                    return true;//we have found 2 input that are not constant values
                }
                foundMul = true;
            }
        }
        return false;
    }

    public List<Wire> getInputWires() {
        return inputWires;
    }

    public List<Wire> getOutputWires() {
        return outputWires;
    }
    
    public void addOutputWire(Wire wire){
        outputWires.add(wire);
    }
    
    public void addInputWire(Wire wire){
        inputWires.add(wire);
    }

    public Operation getOperation() {
        return operation;
    }

    public String display(List<Gate> gates) {
        StringBuilder sb = new StringBuilder();
        for (Wire wire : inputWires) {
            sb.append(wire.isInput() ? "{"+wire.getInputIndex()+"}" : wire.isOutput() ? "{"+wire.getOutputIndex()+"}" : wire.getConstValue() != null ?
                wire.getConstValue().toString() : "Gate" + gates.indexOf(wire.getSourceGate()));
            sb.append(" ").append(operation.toString()).append(" ");
        }
        String toString = sb.toString();
        return toString.substring(0, toString.length()  - operation.toString().length() - 1);
    }        
}
