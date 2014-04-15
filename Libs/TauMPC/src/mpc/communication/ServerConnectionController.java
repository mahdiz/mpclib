/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.communication;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.net.Socket;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;
import mpc.sendables.Sendable;
import mpc.ui.ConfigurationsParser;
import mpc.ui.ProgressLog;

public class ServerConnectionController extends ConnectionController 
{
    public boolean CreateConnections(String ServerIPAddress, int ServerPort,
            int MyIndex,int MyPrime, File playersXmlFile, ProgressLog proglog) throws Exception
    {
        ConfigurationsParser parser = new ConfigurationsParser(playersXmlFile, false);
        
        Map<Integer, Player> players = new TreeMap<Integer, Player>();
        try
        {
            parser.parse(players);
        }
        catch (FileNotFoundException ex)
        {
            proglog.printError("Cannot find players.xml file");
            return false;
        }
        
        Player Me = players.get(MyIndex);
        int ExpectedPlayers = players.size();
        
        if (Me == null)
        {
           proglog.printError("Can't find this players index");
           return false;
        }
        
        //make the output list of all the players.
        List<List<Integer>> OutputList = new LinkedList<List<Integer>>();
        for (Player player: players.values())
        {
            List<Integer> playerList = new LinkedList<Integer>();

            for(Integer singleOutput : player.outputIndexes)
            {
                playerList.add(singleOutput);
            }
            OutputList.add(playerList);
        }
        //Connect to server
        Socket socket = new Socket(ServerIPAddress, ServerPort);
        //Socket socket = BlindSSLSocketFactory.getDefault().createSocket(ServerIPAddress, ServerPort);
   
        //SendToServer     
        ToServerObject toServer = new ToServerObject(Me.ip, Me.port, MyIndex, OutputList, ExpectedPlayers, MyPrime);
        
        //socket.getOutputStream().write(toServer.writeToByteArray());
        DataOutputStream dos = new DataOutputStream(socket.getOutputStream());
        dos.write(toServer.writeToByteArray());
        
        System.out.println("sent " +toServer.writeToByteArray().length);
        
        //RecevList(add size instead of doing this?) 
        byte[] byteArray = new byte[10000];
        
        DataInputStream din = new DataInputStream(socket.getInputStream());
        int numberOfBytes = din.read(byteArray);
       // int numberOfBytes = socket.getInputStream().read(byteArray);
         byte[] byteArr2 = java.util.Arrays.copyOf(byteArray, numberOfBytes);
        FromServerObject singleObject = (FromServerObject) Sendable.loadFromByteArray(byteArr2, 0);
        
        if (!singleObject.isValid)
        {
            proglog.printError(singleObject.serverMsg);
            return false;
        }
        
        List<ToServerObject> completeList = singleObject.getList();
        
        socket.close();
                
        //Fill map
        for (int i=0; i< completeList.size(); i++)
        {
            ToServerObject CurrPlayerObj = completeList.get(i);
            List<Integer> ExptedOutPutforIPlayer = CurrPlayerObj.MyOutputs.get(i);
            indexToPlayer.put(CurrPlayerObj.MyIndex, new Player(CurrPlayerObj.MyIp, CurrPlayerObj.MyPort, ExptedOutPutforIPlayer));
        }
        
        
        sslKeyFile = parser.getKeyFile();
        assert  sslKeyFile.exists();
        setMyIndex(MyIndex);
        return (super.CreateConnections(proglog));
    }
    
    
    
    
    public static void main(String[] args) throws IOException, Exception
    {
     
        String serverIP = "127.0.0.1";
        int port = 3000;
        
        System.out.println("type your index\n");
        String MyIndex = new DataInputStream(System.in).readLine();
        
        File XmlFile = new File("Configurations.xml");
        
        ServerConnectionController serverConnection = new ServerConnectionController();
        
        serverConnection.CreateConnections(serverIP, port, Integer.parseInt(MyIndex),233, XmlFile, null);
        
    }
}



