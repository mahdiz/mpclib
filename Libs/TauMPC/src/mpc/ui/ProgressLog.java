/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.ui;

import java.awt.Color;
import java.util.logging.Level;
import java.util.logging.Logger;
import javax.swing.JTextPane;
import javax.swing.text.BadLocationException;
import javax.swing.text.StyleConstants;
import javax.swing.text.StyleContext;
import javax.swing.text.StyledDocument;


public class ProgressLog {
    private JTextPane progressTextPane;
    private StyledDocument doc;
   private String[] initStyles = { "black", "blue", "red","green","gray"};

    public ProgressLog(JTextPane textPane) {
        this.progressTextPane=textPane;
        doc=progressTextPane.getStyledDocument();
        initStyles();
     

    }

    private void initStyles()
    {
        javax.swing.text.Style def = StyleContext.getDefaultStyleContext().getStyle(StyleContext.DEFAULT_STYLE);
        javax.swing.text.Style black = doc.addStyle("black", def);
        StyleConstants.setForeground(black, Color.black);

        javax.swing.text.Style blue = doc.addStyle("blue", def);
        StyleConstants.setForeground(blue,Color.blue);

        javax.swing.text.Style red = doc.addStyle("red", def);
        StyleConstants.setForeground(red,Color.red);

        javax.swing.text.Style green = doc.addStyle("green", def);
        StyleConstants.setForeground(green,Color.green);

        javax.swing.text.Style gray = doc.addStyle("gray", def);
        StyleConstants.setForeground(gray,Color.gray);

      
    }

    public void printError(String text) {
        try {
            doc.insertString(doc.getLength(), "Error:  "+text +"\n" , doc.getStyle(initStyles[2]));
        } catch (BadLocationException ex) {
            Logger.getLogger(ProgressLog.class.getName()).log(Level.SEVERE, null, ex);
        }
    }

  public void printWarning(String text) {
        try {
            doc.insertString(doc.getLength(),"Warning: "+ text + "\n", doc.getStyle(initStyles[3]));
        } catch (BadLocationException ex) {
            Logger.getLogger(ProgressLog.class.getName()).log(Level.SEVERE, null, ex);
        }
    }

    public void printInformation(String text) {
        try {
            doc.insertString(doc.getLength(), "Info: "+text + "\n", doc.getStyle(initStyles[0]));
        } catch (BadLocationException ex) {
            Logger.getLogger(ProgressLog.class.getName()).log(Level.SEVERE, null, ex);
        }
    }

    public void printInformation2(String text) {
        try {
            doc.insertString(doc.getLength(), "Info: "+text + "\n", doc.getStyle(initStyles[4]));
        } catch (BadLocationException ex) {
            Logger.getLogger(ProgressLog.class.getName()).log(Level.SEVERE, null, ex);
        }
    }


public void printResult(String text) {
        try {
            doc.insertString(doc.getLength(), "Result: "+text + "\n", doc.getStyle(initStyles[1]));
        } catch (BadLocationException ex) {
            Logger.getLogger(ProgressLog.class.getName()).log(Level.SEVERE, null, ex);
        }
    }

public void clearProgressLog()
{
    int length= doc.getLength();
        try {
            doc.remove(0, length);
        } catch (BadLocationException ex) {
            Logger.getLogger(ProgressLog.class.getName()).log(Level.SEVERE, null, ex);
        }
}
}
