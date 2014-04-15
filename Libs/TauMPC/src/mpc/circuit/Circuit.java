/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.circuit;

import mpc.compiler.Parser;
import mpc.finite_field_math.Zp;
import java.text.ParseException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;
import mpc.circuit.Gate.Operation;


public class Circuit {
    private static Map<Integer, Circuit> exponentialGates = new HashMap<Integer, Circuit>();
    protected List<Gate> gates;
    protected List<String> inputs;
    protected int p;

    public Circuit(int p, List<Gate> gates, List<String> inputs) {
        this.p = p;
        this.gates =  gates;
        this.inputs = inputs;
    }

    public void setGates(List<Gate> gates) {
        this.gates = gates;
    }        
    
    public List<Gate> getGates() {
        assert gates!= null : "run calculateGates first and check output";
        return gates;
    }

    public void setInputs(List<String> inputs) {
        this.inputs = inputs;
    }

    public List<String> getInputs() {
        return inputs;
    }        
    
    //returns a map of all outputs (the int is the output index)
    public Map<Integer, Zp> getOutputs() {
        Map<Integer, Zp> result = new HashMap<Integer, Zp>();
        for (Gate gate : gates) {
            for (Wire wire : gate.getOutputWires()) {
                if (wire.getOutputIndex() != null) {
                    assert gate.isOutputReady();
                    result.put(wire.getOutputIndex(), gate.getOutputValue());
                }
            }
        }
        return result;
    }
    
    //returns the number of inputs declared in code - not all inputs must be used in curcuit
    public int getCircuitInputSize(){
        return inputs.size();        
    }
    
    //returns the number of actual inputs - that are used in the circuit
    public int getCircuitRealInputSize(){
        List<Wire> inputWires = getInputWires();
        Set<Integer> currInputs = new HashSet<Integer>();
        for (Wire wire : inputWires){
            if (wire.getInputIndex() != null){
                currInputs.add(wire.getInputIndex());
            }
        }
        return currInputs.size();        
    }
    
    public boolean isMultipleContained(){
        for (Gate gate : gates){
            if (gate.getOperation() == Operation.MUL || gate.getOperation() == Gate.Operation.DIV){
                return true;
            }
        }
        return false;
    }
    
    public List<Wire> getInputWires(){
        return getInputOrOutputWires(true);
    }
    
    public List<Wire> getOutputWires(){
        return getInputOrOutputWires(false);
    }
    
    private List<Wire> getInputOrOutputWires(final boolean isInput){//todo - check that there are no duplicates
        List<Wire> inputOrOutputWires = new ArrayList<Wire>();
        for (Gate gate : gates){
            List<Wire> wires = isInput ? gate.getInputWires() : gate.getOutputWires();
            for (Wire wire : wires){
                if (wire.isInput() && isInput || wire.isOutput() && !isInput){
                    inputOrOutputWires.add(wire);
                }
            }
        }
        Collections.sort(inputOrOutputWires, new Comparator(){
            public int compare(Object o1, Object o2) {
                Wire w1 = (Wire)o1;
                Wire w2 = (Wire)o2;
                Integer v1 = isInput ? w1.getInputIndex() : w1.getOutputIndex();
                Integer v2 = isInput ? w2.getInputIndex() : w2.getOutputIndex();
                return v1.compareTo(v2);
            }                        
        });
        return inputOrOutputWires;
    }

    public void emptyResult(){
        for (Gate gate : gates){
            gate.deleteOutputValue();
        }
    }        
            
    @Override
    public String toString() {
        StringBuilder sb = new StringBuilder();
        sb.append("Inputs: ");
        List<Wire> inputWires = getInputWires();
        List<Integer> usedInputs = new ArrayList<Integer>();
        StringBuilder inputWiresSB = new StringBuilder();
        for (Wire wire : inputWires) {
            if (usedInputs.contains(wire.getInputIndex())) {
                continue;
            }
            inputWiresSB.append(wire.getInputIndex()).append(", ");
            usedInputs.add(wire.getInputIndex());
        }
        sb.append(inputWiresSB.toString().substring(0, inputWiresSB.length() - 2)).append("\n");
        for (Gate gate : gates){
            sb.append("Gate").append(gates.indexOf(gate)).append(": ").append(gate.display(gates)).append("\n");
        }
        sb.append("Outputs: ");
        List<Wire> outputWires = getOutputWires();
        for (Wire wire : outputWires){
            sb.append("Gate").append(gates.indexOf(wire.getSourceGate()));         
            if (outputWires.indexOf(wire) < outputWires.size() - 1){
                sb.append(", ");
            }
        }
        return sb.toString();
    }
    
    public static Circuit getDivisionExponentialCircuit(int p){
        Circuit c = exponentialGates.get(p);
        if (c == null){            
            c = new ExponentCircuit(p, p - 2).getCircuit();
            if (c == null){
                c = createExponentCircuit(p - 2, p);                
            }          
            exponentialGates.put(p, c);
            return c;
        }
        c.emptyResult();
        return c;
    }
    
    private static class ExponentCircuit{
        private int prime;
        private int exp;
        private Map<Integer, Wire> calculatedValues = new HashMap<Integer, Wire>();

        public ExponentCircuit(int prime, int exp) {
            this.prime = prime;
            this.exp = exp;
        }
        
        public Circuit getCircuit(){
            Wire outputWire = getWireForExp(exp);
            outputWire.setOutputIndex(0);
            try {
                return Parser.createCircuit(Arrays.asList(outputWire), prime, Arrays.asList("x"));
            } catch (ParseException ex) {
                assert false;//should not happen
                return null;
            }                        
        }
        
        private Wire getWireForExp(int exp){
            Wire wire = calculatedValues.get(exp);
            if (wire != null){
                return wire.clone();
            }          
            if (exp == 1){
                return new Wire(0, true);
            }
            int biggestExponential = getBiggest2Exponential(exp);
            if (biggestExponential == exp){
                return getWireFor2ExponentialExp(exp);                
            }
            Wire firstWire = getWireForExp(biggestExponential);
            Wire secondWire = getWireForExp(exp - biggestExponential);
            return createGate(firstWire, secondWire);            
        }
        
        private Wire getWireFor2ExponentialExp(int exp){
            int numberOfIterations = get2Log(exp);
            Wire firstWire = new Wire(0, true);
            Wire secondWire = new Wire(0, true);      
            Wire outputWire = createGate(firstWire, secondWire);
            calculatedValues.put(2, outputWire);
            for (int i = 1; i < numberOfIterations; i++) {
                firstWire = outputWire;
                secondWire = outputWire.clone();
                outputWire = createGate(firstWire, secondWire);
                calculatedValues.put((int)Math.pow(2, i + 1), outputWire);
            }
            calculatedValues.put(exp, outputWire);
            return outputWire;            
        }
        
        private Wire createGate(Wire firstWire, Wire secondWire) {
            Wire outputWire = new Wire();
            Gate gate = new Gate(Arrays.asList(firstWire, secondWire), outputWire, Operation.MUL, prime);
            firstWire.setTargetGate(gate);
            secondWire.setTargetGate(gate);
            outputWire.setSourceGate(gate);
            return outputWire;
        }
        
        private int getBiggest2Exponential(int max){
            if (max == 0){
                throw new RuntimeException("Cannot find 2 exponential");
            }            
            int i = 1;
            for (i = 1; i <= max; i = 2 * i){}
            return i/2;
        }
        
        private int get2Log(int num){
            assert num == getBiggest2Exponential(num);
            int j = 0;
            for (int i = 1; i <= num; i = 2 * i){
                j++;
            }
            return j - 1;
        }
        
    }
    
    public static Circuit createExponentCircuit(int exp, int p){
        List<Wire> currWires = new ArrayList<Wire>();
        for (int i = 0; i < exp; i++){
            currWires.add(new Wire(0, true));
        }
        List<Gate> gates = new ArrayList<Gate>();
        while(currWires.size() > 1){
            List<Wire> outputWires = new ArrayList<Wire>();
            for (int i = 0; i < currWires.size() - 1; i = i + 2){
                Wire outputWire = new Wire();
                Gate gate = new Gate(Arrays.asList(currWires.get(i), currWires.get(i + 1)), outputWire, Operation.MUL, p);
                currWires.get(i).setTargetGate(gate);
                currWires.get(i + 1).setTargetGate(gate);
                outputWire.setSourceGate(gate);
                outputWires.add(outputWire);
                gates.add(gate);
            }
            if (currWires.size() % 2 != 0){
                outputWires.add(currWires.get(currWires.size() - 1));
            }
            currWires = outputWires;
        }
        currWires.get(0).setOutputIndex(0);
        return new Circuit(p, gates, Arrays.asList("x"));
    }

    //returns a List of all outputs - for internal use only
    public  List<Zp>  internalCalculate(String inputs, int prime)  {

        inputs = inputs.replaceAll(" ", "");//remove white spaces
        String[] tokens = inputs.split(",");
        List<Zp> usersInputs = new ArrayList<Zp>();
        for (int i=0; i< tokens.length; i++) {
              usersInputs.add( new Zp(prime, Integer.parseInt(tokens[i])));
         }

        for (Gate gate : getGates()){
            gate.internalCalculate(usersInputs);
        }

        Map<Integer, Zp> outputsMap = getOutputs();

        int numOfOutputs = outputsMap.size();
        Zp tempRes = null;
        List<Zp> resultList = new ArrayList<Zp>();

        for (int i = 0; i < numOfOutputs; i++){
            tempRes = outputsMap.get(new Integer(i));
            resultList.add(tempRes);
        }
        return resultList;
    }
    
    public static void main(String[] args){
        Circuit circuit = Circuit.getDivisionExponentialCircuit(61);
        System.out.print(circuit.toString());        
    }
  }
