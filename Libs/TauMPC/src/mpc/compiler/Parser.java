/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.compiler;

import mpc.circuit.Wire;
import mpc.circuit.Circuit;
import mpc.finite_field_math.Zp;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.Reader;
import java.io.StringReader;
import java.text.ParseException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import mpc.circuit.Gate;
import mpc.circuit.Gate.Operation;


public class Parser {
    
    private static String s_inputsDec = "Inputs";
    private static String s_outputsDec = "Outputs";
    private static String variableExp = "([a-zA-Z][a-zA-Z0-9]*)";
    private static String conditionalExp = "(.*)?(.*):(.*)";
    private static Pattern variablePattern = Pattern.compile(variableExp);
    private static Pattern setArithmeticalValue = Pattern.compile(variableExp + "=(.*)");
    private static Pattern conditionalTerm = Pattern.compile(conditionalExp);
    private static Pattern inputsDecleration = Pattern.compile(s_inputsDec + "=(.*)");
    private static Pattern outputsDecleration = Pattern.compile(s_outputsDec + "=(.*)");
    private String fileNameOrString;
    private List<String> outputVars;
    private Map<String, Wire> varsToWires;
    private int prime;
    private Circuit circuit;
    private List<String> inputs = new ArrayList<String>();

    public Parser(String fileNameOrPath, int prime) {
        this.fileNameOrString = fileNameOrPath;
        this.prime = prime;
    }    
    
    public enum LineType{
        INPUTS_DECLERATION, OUTPUTS_DECLERATION, SETTING_ARITHMETICAL_VALUE, EMPTY_LINE;
    }
    
    public boolean parse() throws Exception{
        File file = new File(fileNameOrString);
        Reader reader;
        if (file.exists()){
            reader = new FileReader(file);
        }else{
            reader = new StringReader(fileNameOrString);            
        }
        BufferedReader bf = new BufferedReader(reader);
        outputVars = new ArrayList<String>();
        varsToWires = new HashMap<String, Wire>();                
        String line = null;
        int lineNumber = 0;
        while (bf.ready()){            
            line = bf.readLine();  
            if (line == null){
                break;
            }
            int commentIndex = line.indexOf("//");
            if (commentIndex != -1){
                line = line.substring(0, commentIndex);
            }
            try{
                line = line.replace("\t", "");//remove tabs...
                readLine(line.replaceAll(" ", ""));//remove white spaces                    
                lineNumber++;
            }catch(Exception e){
                String msg = "Error in line " + lineNumber + "\n" + e.getMessage();
                System.out.print(msg);  
                throw new ParseException(msg, lineNumber);
            }            
        }        
        circuit = createCircuit(new ArrayList<Wire>(getOutputWires()), prime, inputs);
        return circuit != null;
    }

    public Circuit getCircuit() {
        return circuit;
    }        
    
    private void readLine(String line) throws ParseException{
        LineType lineType = getLineType(line);
        if (lineType == null){
            throw new ParseException("Line is not in a valid format", 0);
        }
        switch (lineType){
            case SETTING_ARITHMETICAL_VALUE:
                readSettingArithmeticalValue(line, LineType.SETTING_ARITHMETICAL_VALUE);
                break;    
            case INPUTS_DECLERATION:
                readInputOutputDecleration(line, true);
                break;
            case OUTPUTS_DECLERATION:
                readInputOutputDecleration(line, false);
                break;
            case EMPTY_LINE:
                break;
            default:
                assert false;
        }                
    }
    
    private Wire getWireFromCondition(String line) throws ParseException{
        return getWireFromCondition(line, null);      
    }
    
    private Wire getWireFromCondition(String line, Wire conditionWire) throws ParseException{
        int index1 = line.indexOf("?");
        int index2 = line.indexOf(":");
        assert index1 != -1 && index2 != -1 && index1 < index2;
        //get all 3 expressions wires
        if (conditionWire == null){
            conditionWire = getWireFromExpression(line.substring(0, index1));            
        }        
        Wire firstWire = getWireFromExpression(line.substring(index1 + 1, index2));
        Wire secondWire = getWireFromExpression(line.substring(index2 + 1));
        
        //normalizedConditionWire = conditionWire/conditionWire
        Wire normalizedConditionWire = new Wire();
        setGate(Arrays.asList(conditionWire, conditionWire), normalizedConditionWire, Operation.DIV);
        
        //reverseCondition = 1 - normalizedConditionWire
        Wire reverseCondition = new Wire();
        setGate(Arrays.asList(new Wire(new Zp(prime, 1)), normalizedConditionWire), reverseCondition, Operation.SUB);
        
        //firstWireMultiplied = firstWire * normalizedConditionWire
        Wire firstWireMultiplied = new Wire();                
        setGate(Arrays.asList(firstWire, normalizedConditionWire), firstWireMultiplied, Operation.MUL);
        
        //secondWireMultiplied = secondWire * reverseCondition
        Wire secondWireMultiplied = new Wire();
        setGate(Arrays.asList(secondWire, reverseCondition), secondWireMultiplied, Operation.MUL);
        
        //result = firstWireMultiplied + secondWireMultiplied
        Wire resultWire = new Wire();
        setGate(Arrays.asList(firstWireMultiplied, secondWireMultiplied), resultWire, Operation.ADD);
        return resultWire;                                                        
    }
    
    private boolean readInputOutputDecleration(String line, boolean isInput) throws ParseException{
        int index = line.indexOf("=");
        assert index != -1;
        String[] tokens = line.substring(index + 1).split(",");
        for (int i = 0; i < tokens.length; i++){
            String token = tokens[i];
            checkVariableSynthax(token);
            Wire wire = new Wire(i, isInput);
            if (varsToWires.containsKey(token)){
                if (isInput){
                    varsToWires.get(token).setInputIndex(index);
                }else{
                    varsToWires.get(token).setOutputIndex(index);
                }
            }else{
                varsToWires.put(token, wire);                
            }            
            if (!isInput){
                outputVars.add(token);
            }else{
                inputs.add(token);
            }
        }
        return true;
    }
    
    private void checkVariableSynthax(String var) throws ParseException{
        Matcher matcher = variablePattern.matcher(var);
        if (!matcher.matches()){
            throw new ParseException("Variable '" + var + "' is not a valid variable name", 0);
        }            
    }
    
    private boolean readSettingArithmeticalValue(String line, LineType lineType) throws ParseException{
        checkParanthesisSyntax(line);
        Matcher matcher = setArithmeticalValue.matcher(line);
        assert matcher.matches();
        int index = line.indexOf("=");
        String targetVar = line.substring(0, index);
        checkVariableSynthax(targetVar);        
        Wire wire = null;   
        switch(lineType){
            case SETTING_ARITHMETICAL_VALUE:
                wire = getWireFromExpression(line.substring(index + 1));     
                break;
            default:
                assert false;
        }
        varsToWires.put(targetVar, wire);
        return true;        
    }
    
    private int getIndexOfMinusOrPlus(String line, int fromIndex){
        int index1 = fromIndex == -1 ? line.indexOf(Operation.ADD.toString()) : line.indexOf(Operation.ADD.toString(), fromIndex);
        int index2 = fromIndex == -1 ? line.indexOf(Operation.SUB.toString()) : line.indexOf(Operation.SUB.toString(), fromIndex);
        return index1 == -1 ? index2 : index2 == -1 ? index1 : index1 < index2 ? index1 : index2;
    }
    
    private int getIndexOfDivOrMul(String line, int fromIndex){
        int index1 = fromIndex == -1 ? line.indexOf(Operation.MUL.toString()) : line.indexOf(Operation.MUL.toString(), fromIndex);
        int index2 = fromIndex == -1 ? line.indexOf(Operation.DIV.toString()) : line.indexOf(Operation.DIV.toString(), fromIndex);
        return index1 == -1 ? index2 : index2 == -1 ? index1 : index1 < index2 ? index1 : index2;
    }
    
    private Wire getWireFromExpression(String line) throws ParseException{        
        int mulIndex = findCorrectIndex(line, SearchFor.DIV_MUL);
        int addIndex = findCorrectIndex(line, SearchFor.ADD_MIN);
        int equalsIndex = findCorrectIndex(line, SearchFor.EQULAS);
        int questionMark = findCorrectIndex(line, SearchFor.QUESTION_MARK);
        int parIndex = line.indexOf("(");
        if (questionMark != -1 && conditionalTerm.matcher(line).matches() && 
                parIndex != 0 && (addIndex == -1 || addIndex > questionMark || addIndex > equalsIndex && addIndex < questionMark && !line.substring(equalsIndex, questionMark).contains("(")) && 
                (mulIndex == -1 || mulIndex > questionMark || mulIndex > equalsIndex && mulIndex < questionMark && !line.substring(equalsIndex, questionMark).contains("("))){
            return getWireFromCondition(line);
        }
        if (mulIndex == -1 && addIndex == -1 && equalsIndex == -1 && parIndex != 0){
            return getWireFromString(line);
        }
        if (addIndex > 0 && parIndex != 0 && equalsIndex == -1 && (mulIndex == -1 || addIndex < mulIndex)){//handle add gate
            Wire firstWire = getWireFromString(line.substring(0, addIndex));  
            return createAddGates(line.substring(addIndex), firstWire);            
        }
   
        assert parIndex == 0 || mulIndex < parIndex;//this exp should start with ( or * must be before (

        if (parIndex == 0) {
            return createParenthesesWire(line, true);
        }       
        if (equalsIndex != -1){
            return createEqualsGate(line);            
        }
        if (addIndex == -1){
            return createMulGate(null, null, line);            
        }else{
            return createAddGates(line.substring(addIndex), createMulGate(null, null, line.substring(0, addIndex)));
        }        
    }
    
    private Wire createEqualsGate(String line) throws ParseException{
        return createEqualsGate(line, null);        
    }
    
    private Wire createEqualsGate(String line, Wire firstWire) throws ParseException{
        int equalsIndex = line.indexOf("==");
        assert equalsIndex != -1;
        if (firstWire == null){
            firstWire = getWireFromExpression(line.substring(0, equalsIndex));            
        }        
        Wire secondWire = getWireFromExpression(line.substring(equalsIndex + 2));
        Wire compareWire = new Wire();
        setGate(Arrays.asList(firstWire, secondWire), compareWire, Operation.SUB);
        Wire normilizedWire = new Wire();
        setGate(Arrays.asList(compareWire, compareWire), normilizedWire, Operation.DIV);
        Wire resultWire = new Wire();
        setGate(Arrays.asList(new Wire(new Zp(prime, 1)), normilizedWire), resultWire, Operation.SUB);
        return resultWire;        
    }
    
    private enum SearchFor{
        ADD_MIN, DIV_MUL, EQULAS, QUESTION_MARK;
    }
    
    private int findCorrectIndex(String line, SearchFor searchFor) {
        int index = searchFor == SearchFor.ADD_MIN ? getIndexOfMinusOrPlus(line, 1) : searchFor == SearchFor.DIV_MUL ? 
            getIndexOfDivOrMul(line, 1) : searchFor == SearchFor.EQULAS ? line.indexOf("==") : line.indexOf("?");
        int parIndex = line.indexOf("(");
        if (index == -1 || parIndex == -1 || index < parIndex) {
            return index;
        }
        int parEndIndex = parIndex + findEndingParantheses(line.substring(parIndex));
        int correctIndex = findCorrectIndex(line.substring(parEndIndex), searchFor);
        return correctIndex != -1 ? parEndIndex + correctIndex : -1;
    }
    
    private void checkParanthesisSyntax(String line) throws ParseException{
        int opened = 0;
        for (char c : line.toCharArray()){
            if (c == '('){
                opened++;
            }
            if (c == ')'){
                if (opened == 0){
                    throw new ParseException("Closing ) without openning (", 0);
                }else{
                    opened--;
                }
            }
        }
        if (opened > 0){
            throw new ParseException("Openning '(' without closing ')", opened);
        }
    }
    
    private int findEndingParantheses(String line){
        assert line.startsWith("(");
        int numOfOpennings = 0;
        for (int i = 1; i < line.length(); i++){
            char c = line.charAt(i);
            if (c == '('){
                numOfOpennings++;
            }
            if (c == ')'){
                if (numOfOpennings == 0){
                    return i;
                }else{
                    numOfOpennings--;
                }
            }
        }
        throw new RuntimeException("Openning '(' without closing ')");
    }

    private Wire createParenthesesWire(String line, boolean keepCalculating) throws ParseException {
        int parEndIndex = findEndingParantheses(line);
        Wire firstWire = getWireFromExpression(line.substring(1, parEndIndex));
        line = line.substring(parEndIndex + 1);
        if (!keepCalculating){
            return firstWire;
        }
        if (line.startsWith("+") || line.startsWith("-")) {
            return createAddGates(line, firstWire);
        }
        if (line.startsWith("*")) {            
            return createMulGate(firstWire, Operation.MUL, line.substring(1));
        }
        if (line.startsWith("/")){
            return createMulGate(firstWire, Operation.DIV, line.substring(1));
        }
        if (line.equals("")){
            return firstWire;
        }
        if(line.startsWith("==")){
            return createEqualsGate(line, firstWire);            
        }
        if(line.startsWith("?")){
            return getWireFromCondition(line, firstWire);
        }
        throw new ParseException("No operation after closing )", 0);
    }
    
    private Wire createMulGate(Wire firstWireOrNull, Operation firstOpOrNull, String line) throws ParseException{
        int addIndex = findCorrectIndex(line, SearchFor.ADD_MIN);
        if (addIndex == -1){
            return createMulGates(firstWireOrNull, firstOpOrNull, line);            
        }
        Wire firstWire = createMulGates(firstWireOrNull, firstOpOrNull, line.substring(0, addIndex));
        return createAddGates(line.substring(addIndex), firstWire);
    }

    private void splitMul(String line, List<String> operands, List<Operation> operations){
        int index = findCorrectIndex(line, SearchFor.DIV_MUL);
        while (index != -1){
            operands.add(line.substring(0, index));
            operations.add(Operation.getOperationByName(line.substring(index, index + 1)));
            line = line.substring(index + 1);
            index = findCorrectIndex(line, SearchFor.DIV_MUL);
        }
        operands.add(line);
    }
    
    private Wire createMulGates(Wire firstWireOrNull, Operation firstOpOrNull, String line) throws ParseException{
        List<String> tokens = new ArrayList<String>();
        List<Operation> operations = new ArrayList<Operation>();
        splitMul(line, tokens, operations);
        assert tokens.size() >= 2;
        Wire firstWire = firstWireOrNull == null ? getWireFromString(tokens.get(0)) : firstWireOrNull;
        Operation currOperation = firstOpOrNull;
        if (currOperation == null){
            currOperation = operations.get(0);
        }
        for (int i = firstWireOrNull == null ? 1 : 0; i < tokens.size(); i++){
            Wire secondWire = getWireFromString(tokens.get(i));
            Wire outputWire = new Wire();            
            setGate(Arrays.asList(firstWire, secondWire), outputWire, currOperation);
            firstWire = outputWire;
            if (operations.size() <= i){
                break;
            } 
            currOperation = operations.get(i);
        }
        return firstWire;        
    }
    
    private void setGate(List<Wire> inputWires, Wire outputWire, Operation op){
        Gate gate = new Gate(inputWires, Arrays.asList(outputWire), op, prime);
        for (Wire wire : inputWires){
            wire.setTargetGate(gate);
        }
        outputWire.setSourceGate(gate);        
    }
    
    private int getFixedIndex(int index){
        return index == -1 ? Integer.MAX_VALUE : index;
    }
    
    private int getFirstParamterIndex(String line){
        int addIndex = getIndexOfMinusOrPlus(line, 0);
        int mulIndex = getIndexOfDivOrMul(line, 0);
        int parIndex = line.indexOf("(");
        int equalsIndex = line.indexOf("==");
        int questionMarkIndex = line.indexOf("?");
        int index = Math.min(getFixedIndex(addIndex), Math.min(getFixedIndex(mulIndex), Math.min(getFixedIndex(parIndex), 
                Math.min(getFixedIndex(equalsIndex), getFixedIndex(questionMarkIndex)))));
        return index == Integer.MAX_VALUE ? -1 : index;
    }
    
    private Wire createAddGates(String line, Wire firstWire) throws ParseException {
        Operation op = Operation.getOperationByName(line.substring(0, 1));
        line = line.substring(1);
       while (op == Operation.SUB){
            int firstIndex = getFirstParamterIndex(line);
            if (firstIndex == -1){
                break;
            }
            if (firstIndex == 0 && !line.startsWith("(") || firstIndex == -1){
                break;                                
            }
            if (getIndexOfDivOrMul(line, 0) == firstIndex){
                break;
            }            
            boolean isPar = line.startsWith("(");
            Wire secondWire = isPar ? createParenthesesWire(line, false) : getWireFromExpression(line.substring(0, firstIndex));
            Wire outputWire = new Wire();
            setGate(Arrays.asList(firstWire, secondWire), outputWire, op);
            line = line.substring(isPar ? findEndingParantheses(line) + 1 : firstIndex);
            if (line.length() == 0){
                return outputWire;
            }
            op = Operation.getOperationByName(line.substring(0, 1));
            line = line.substring(1);
            firstWire = outputWire;
        }
        if (op != Operation.ADD && op != Operation.SUB){
            return createMulGate(firstWire, op, line);
        }
        Wire secondWire = getWireFromExpression(line);
        Wire outputWire = new Wire();
        setGate(Arrays.asList(firstWire, secondWire), outputWire, op);
        return outputWire;
    }

    private Wire getWireFromString(String st) throws ParseException {
        if (st.startsWith("(")){
            return createParenthesesWire(st, false);
        }
        Wire wire = varsToWires.get(st);
        if (wire != null && wire.isOutput() && wire.getSourceGate() == null){
            throw new ParseException("Variable " + st + " was not initialized", 0);
        }
        if (wire != null) {
            if (wire.getTargetGate() != null) {
                return wire.clone();
            } else {
                return wire;
            }
        }
        int value;
        try {
            value = Integer.parseInt(st);
        } catch (Exception e) {
            if (st.contains("(")){
                throw new ParseException("No operation before opening ( in expression: " + st, 0);
            }
            throw new ParseException("variable '" + st + "' is not defined", 0);
        }
        return new Wire(new Zp(prime, value));
    }
    
    private LineType getLineType(String line){
        if (line.equals("")){
            return LineType.EMPTY_LINE;
        }
        Matcher matcher = inputsDecleration.matcher(line);
        if (matcher.matches()){
            return LineType.INPUTS_DECLERATION;
        }
        matcher = outputsDecleration.matcher(line);
        if (matcher.matches()){
            return LineType.OUTPUTS_DECLERATION;
        }
//        matcher = setConditionValue.matcher(line);
//        if (matcher.matches()){
//            return LineType.SETTING_CONDITION_VALUE;
//        }
        matcher = setArithmeticalValue.matcher(line);
        if (matcher.matches()){
            return LineType.SETTING_ARITHMETICAL_VALUE;
        }
        return null;        
    }
    
    public static  Circuit createCircuit(List<Wire> outputWires, int prime, List<String> inputs) throws ParseException{
        List<Wire> currWires = new ArrayList<Wire>(outputWires);
        List<Gate> gates = new ArrayList<Gate>();
        while(!currWires.isEmpty()){
            List<Wire> toRemove = new ArrayList<Wire>();
            List<Wire> toAdd = new ArrayList<Wire>();
            for (Wire wire : currWires) {
                if (wire.isInput() || wire.getConstValue() != null) {
                    if (wire.isOutput() && wire.isInput()){
                        throw new ParseException("Forbidden: Input " + wire.getInputIndex() + " is also output " + wire.getOutputIndex(), 0);
                    }
                    if (wire.isOutput() && wire.getConstValue() != null){
                        throw new ParseException("Forbidden: Output " + wire.getOutputIndex() + " has a const value " + wire.getConstValue(), 0);                        
                    }
                    toRemove.add(wire);
                    continue;
                }
                if (!isTargetInGates(gates, wire) && !wire.isOutput()){
                    continue;
                }
                assert wire.getSourceGate() != null;
                gates.remove(wire.getSourceGate());//remove source if it is List already - it means the it's output is going to 2 gates
                gates.add(0, wire.getSourceGate());
                toRemove.add(wire);  
                if (wire.getSourceGate() == null){
                    throw new ParseException("Output " + wire.getOutputIndex() + " has no source gate", 0);
                }
                toAdd.addAll(wire.getSourceGate().getInputWires());
            }
            currWires.removeAll(toRemove);
            currWires.addAll(toAdd);
        }
        if (gates.isEmpty()){
            throw new RuntimeException("No valid gates in the circuit");
        }
        return new Circuit(prime, gates, inputs);
    }
    
    private List<Wire> getOutputWires() throws ParseException{
        List<Wire> outputWires = new ArrayList<Wire>();
        for (String varName : outputVars){
            Wire outputWire = getWireFromString(varName);
            outputWire.setOutputIndex(outputVars.indexOf(varName));
            outputWires.add(outputWire);            
        }
        return outputWires;
    }
    
    private static boolean isTargetInGates(List<Gate> gates, Wire outputWire){
        for (Gate gate : gates){
            if (outputWire.getTargetGate() == gate){
                return true;
            }
        }
        return false;        
    }            
    
    public static void main (String[] args){
        Parser parser = new Parser("Inputs=x,y,w\nOutputs=z\nz=x==2 ? y : w", 231);
        try{
            parser.parse();
            System.out.print(parser.getCircuit().toString());
        }catch(Exception e){
           int i = 5;
        }
    }
}
