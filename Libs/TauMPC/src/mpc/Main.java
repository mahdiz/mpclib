/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc;

import java.util.logging.Level;
import java.util.logging.Logger;
import javax.swing.UIManager;
import javax.swing.UnsupportedLookAndFeelException;
import mpc.ui.UserInterface;


public class Main {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) throws InterruptedException {
        //example for use of Circuit and Gate. Not using MPCProtocol at all...
        /*int p = 233;
        Circuit simpleCircuit = new SimpleCircuit(p, 4);
        for (Gate gate : simpleCircuit.getGates()){
        while (!gate.isReadyToCalculate()){
        Thread.sleep(100);
        }
        assert gate.isReadyToCalculate();
        gate.calculate(Arrays.asList(new Zp(p, 1), new Zp(p, 2), new Zp(p, 3), new Zp(p, 4)));
        assert gate.isOutputReady();
        System.out.print(gate.getOutputValue());         */
        try {            
            UIManager.setLookAndFeel("com.sun.java.swing.plaf.windows.WindowsLookAndFeel");
        } catch (ClassNotFoundException ex) {
            Logger.getLogger(Main.class.getName()).log(Level.SEVERE, null, ex);
        } catch (InstantiationException ex) {
            Logger.getLogger(Main.class.getName()).log(Level.SEVERE, null, ex);
        } catch (IllegalAccessException ex) {
            Logger.getLogger(Main.class.getName()).log(Level.SEVERE, null, ex);
        } catch (UnsupportedLookAndFeelException ex) {
            Logger.getLogger(Main.class.getName()).log(Level.SEVERE, null, ex);
        }
        

        java.awt.EventQueue.invokeLater(new Runnable() {
            public void run() {
                new UserInterface().setVisible(true);
            }
        });
        }
    

}
