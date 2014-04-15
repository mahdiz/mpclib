/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.circuit;

import mpc.finite_field_math.Zp;


public class Wire {
    private  Integer inputIndex;
    private Integer outputIndex;
    private   Gate sourceGate;
    private   Gate targetGate;
    private Zp constValue;

    public Wire() {
        inputIndex = null;
        outputIndex = null;
    }        
        

    public Wire(Zp constValue) {
        this.constValue = constValue;
        inputIndex = null;
        outputIndex = null;
    }        

    public Wire(Integer inputOrOutputIndex, boolean isInput) {
        if (isInput) {
            this.inputIndex = inputOrOutputIndex;
            outputIndex = null;
        } else {
            this.outputIndex = inputOrOutputIndex;
            inputIndex = null;
        }
    }
       
    public Wire(Integer inputIndex, Gate targetGate) {
        this.inputIndex = inputIndex;
        this.targetGate = targetGate;
        outputIndex = null;
        sourceGate = null;        
    }

    public Wire(Gate sourceGate, Integer outputIndex) {
        this.outputIndex = outputIndex;
        this.sourceGate = sourceGate;
        inputIndex = null;
        targetGate = null;
    }

    public Wire(Gate sourceGate, Gate targetGate) {
        this.sourceGate = sourceGate;
        this.targetGate = targetGate;
        inputIndex = null;
        outputIndex = null;
    }

    public Wire(Integer inputIndex, Integer outputIndex) {
        this.inputIndex = inputIndex;
        this.outputIndex = outputIndex;
        sourceGate = null;
        targetGate = null;
    }
    
    public boolean isValid(){
        return inputIndex != null || sourceGate != null && sourceGate.isReadyToCalculate();
    }

    public boolean isInput() {
        return inputIndex != null;
    }

    public boolean isOutput() {
        return outputIndex != null;
    }

    public Gate getTargetGate() {
        return targetGate;
    }

    public void setTargetGate(Gate targetGate) {
        assert outputIndex == null : "output wire should not have target gate";
        this.targetGate = targetGate;
    }

    public Gate getSourceGate() {
        return sourceGate;
    }

    public void setSourceGate(Gate sourceGate) {
        assert inputIndex == null : "input wire should not have source gate";
        this.sourceGate = sourceGate;
    }

    public Zp getConstValue() {
        return constValue;
    }

    public void setConstValue(Zp constValue) {
        this.constValue = constValue;
    }        

    public Integer getOutputIndex() {
        return outputIndex;
    }

    public void setOutputIndex(Integer outputIndex) {
        this.outputIndex = outputIndex;
    }

    public Integer getInputIndex() {
        return inputIndex;
    }

    public void setInputIndex(Integer inputIndex) {
        this.inputIndex = inputIndex;
    }        
    
    @Override
    public Wire clone(){
        Wire wire = new Wire(inputIndex, outputIndex);
        wire.setSourceGate(sourceGate);
        wire.setTargetGate(targetGate);     
        wire.setConstValue(constValue);
        if (wire.getSourceGate() != null){
            wire.getSourceGate().addOutputWire(wire);            
        }      
        return wire;                
    }
}
