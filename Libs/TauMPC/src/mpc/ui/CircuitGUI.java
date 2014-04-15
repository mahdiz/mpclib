/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.ui;

import mpc.compiler.Parser;
import mpc.circuit.Gate;
import mpc.circuit.Wire;
import mpc.circuit.Circuit;
import java.util.List;
import java.awt.*;
import java.util.ArrayList;
import java.util.HashMap;
import javax.swing.*;
import no.geosoft.cc.geometry.Geometry;
import no.geosoft.cc.graphics.*;



public class CircuitGUI extends JFrame implements GInteraction{

   private GScene  scene;
   private Circuit circuit;
   private float[] layerHorizontalArr;
   private float circleSize = 50;
   private float  verticalInd= 50;
   private int layerNumber;
   private int screenHeight= 500, screenWidth =500;
   private Component Canvas;

   public CircuitGUI(Circuit circuit) {
    super ("Circuit Graph");
    this.circuit = circuit;
    setDefaultCloseOperation (JFrame.DO_NOTHING_ON_CLOSE);

    // Create the GUI
    JPanel topLevel = new  JPanel();
    topLevel.setLayout (new BorderLayout());
    getContentPane().add (topLevel);

    JScrollBar hScrollBar = new JScrollBar (JScrollBar.HORIZONTAL);
    getContentPane().add (hScrollBar, BorderLayout.SOUTH);
    JScrollBar vScrollBar = new JScrollBar (JScrollBar.VERTICAL);
    getContentPane().add (vScrollBar, BorderLayout.EAST);



    // Create the graphic canvas
    GWindow window = new GWindow(Color.WHITE);
    Canvas = window.getCanvas();
    topLevel.add (Canvas, BorderLayout.CENTER);
    //topLevel.add (window.getCanvas(), BorderLayout.CENTER);
    
     // Create scene with default viewport and world extent settings
    scene = new GScene (window, "Scene");

    double w0[] = {0.0,  1200.0, 0.0};
    double w1[] = {1200.0, 1200.0, 0.0};
    double w2[] = {0.0,  0.0, 0.0};
    scene.setWorldExtent (w0, w1, w2);

    GStyle style = new GStyle();
    style.setForegroundColor (new Color (0, 0, 0));
    style.setBackgroundColor (new Color (255, 255, 255));
    style.setFont (new Font ("Dialog", Font.BOLD, 14));
    scene.setStyle (style);

    convertCircuitToGraph();

    pack();
    setSize (new Dimension (screenWidth, screenHeight));
    setVisible (false);

     // Start zoom interaction
    GStyle zoomStyle = new GStyle();
    zoomStyle.setForegroundColor (new Color (0, 0, 0));
    zoomStyle.setBackgroundColor (new Color (0.8f, 1.0f, 0.8f, 0.3f));
    window.startInteraction (new ZoomInteraction (scene, zoomStyle));
    //window.startInteraction (this);

    
    scene.shouldWorldExtentFitViewport (false);
    scene.shouldZoomOnResize (false);
    scene.installScrollHandler (hScrollBar, vScrollBar);
  }
  
public Component getCanvas()
{
    return Canvas;
   
}

  private void convertCircuitToGraph(){
        if (circuit == null)
            throw new IllegalArgumentException("Cannot draw an empty circuit.");

        HashMap<Integer, Integer>  layerCount = new HashMap<Integer, Integer>() ;
        HashMap<Integer, ArrayList<Wire>>  hirMap = new HashMap<Integer, ArrayList<Wire>>() ; // 
            
        List<Wire> outputWires = circuit.getOutputWires();
        int currentLayer = 0;
        for (Wire wire : outputWires){                
                 buildGuiTree(hirMap, layerCount, wire, currentLayer);
        }

        layerNumber = layerCount.size();
        layerHorizontalArr  = new float[layerNumber];
        for (int i = 0;i < layerNumber;i++){
            layerHorizontalArr[i] = (float)  Math.pow((double) 2, (double) layerNumber - i ) * circleSize ;
        }

        connectGuiElements(hirMap, layerCount);
  }

  
  private void connectGuiElements(HashMap<Integer, ArrayList<Wire>>  hirMap, HashMap<Integer, Integer>  layerCount){
            int layer = 0;
            int i = 1;
            float left = screenWidth/2 - layerHorizontalArr[layer];
            float right = screenWidth/2 + layerHorizontalArr[layer];
            List<Wire> currentLayerWire = hirMap.get(new Integer(layer));

            for (Wire wire : currentLayerWire){
                    GObject  output = new Gate_GUI ("out" + i++, scene,  (left + right)/2, verticalInd);
                    recursiveDraw(wire, output, 1,  left, right ,verticalInd);
                    left  += screenWidth + 2 * layerHorizontalArr[layer];
                    right += screenWidth + 2 * layerHorizontalArr[layer];
            }
  }

  
  private void recursiveDraw(Wire wire, GObject parent, int parentLayer, float left, float right, float c_vertical){
      float middle = (left + right)/2;
      c_vertical += layerHorizontalArr[layerNumber/2] + verticalInd;
      Gate gate = wire.getSourceGate();
      if (gate != null){
                
                GObject  currGate = new Gate_GUI (gate.getOperation().toString(), parent,  middle, c_vertical);
                c_vertical += layerHorizontalArr[layerNumber/2];
                List<Wire> currentLayerWire = gate.getInputWires();

                recursiveDraw(currentLayerWire.get(0),  currGate, parentLayer + 1, left,  middle - circleSize/2,c_vertical);
                recursiveDraw(currentLayerWire.get(1),  currGate, parentLayer + 1, middle + circleSize/2, right,c_vertical);

      }else {   // An  input/constant  wire
           if (wire.getConstValue() == null){
                  GObject  input = new Gate_GUI ("{" +wire.getInputIndex().toString() + "}", parent,  middle, c_vertical);
           } else {
               GObject  input = new Gate_GUI (wire.getConstValue().toString(), parent,  middle, c_vertical);
           }
      }
  }
  



  private  void  buildGuiTree(HashMap<Integer, ArrayList<Wire>>  hirMap, HashMap<Integer, Integer>  layerCount, Wire wire, int currLayer){

            Integer key = new Integer(currLayer);

            if (layerCount.get(key) == null) {
                         layerCount.put(key, new Integer(1));
            }else {
                         layerCount.put(key,layerCount.get(key) + 1);
            }

            ArrayList<Wire>  newList = null;
            if (hirMap.get(key) == null) {
                          newList = new ArrayList<Wire>();
            }else {
                         newList = hirMap.get(key);
            }
            newList.add(wire);
            hirMap.put(key,newList);

            Gate gate = wire.getSourceGate();
            if (gate == null){
                return;
            }
           List<Wire> gateInputWires = gate.getInputWires();
           for (Wire tmpWire : gateInputWires) {     
                buildGuiTree(hirMap, layerCount, tmpWire, currLayer + 1);
           }
  }

  

  public void event (GScene scene, int event, int x, int y){
    if (event == GWindow.BUTTON1_UP ||
        event == GWindow.BUTTON2_UP) {
      boolean isSelected = event == GWindow.BUTTON1_UP;

      GSegment selectedSegment = this.scene.findSegment (x, y);
      if (selectedSegment == null) return;

      GStyle style = selectedSegment.getOwner().getStyle();
      if (style == null) return;

      if (isSelected)
        style.setBackgroundColor (new Color ((float) Math.random(),
                                             (float) Math.random(),
                                             (float) Math.random()));
      else
        style.unsetBackgroundColor();

      this.scene.refresh();
    }
  }


private class Gate_GUI extends GObject {
      
    private Gate_GUI    parent;
    private double          x, y;
    private GSegment    circle;
    private GSegment    line;


    Gate_GUI (String name, GObject parent, double x, double y) {
      this.parent = parent instanceof Gate_GUI ? (Gate_GUI) parent : null;

      this.x = x;
      this.y = y;
      
      line = new GSegment();
      addSegment (line);

      circle = new GSegment();
      addSegment (circle);

      circle.setText (new GText (name, GPosition.MIDDLE));

      GStyle style = new GStyle();
      Color color = new Color (220,  220,  220);
     style.setBackgroundColor (color);
     style.setFont (new Font (null, Font.BOLD, 14));
     setStyle (style);
     parent.add (this);
    
    }

    public  double getX() {
      return x;
    }

    public double getY() {
      return y;
    }


        @Override
    public void draw() {
      if (parent != null){
        line.setGeometry (parent.getX(), parent.getY(), x, y);
      }
      circle.setGeometryXy (Geometry.createCircle (x, y, circleSize));  
    }


  }



  public static void main (String[] args)
  {
         Parser parser = new Parser("test1.txt", UserInterface.prime);
        try{
                parser.parse();
                new CircuitGUI(parser.getCircuit());
        }catch(Exception e){
                int i = 5;
        }
  }
  
}
