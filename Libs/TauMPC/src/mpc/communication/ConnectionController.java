/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package mpc.communication;

import mpc.ui.ProgressLog;
import mpc.sendables.Sendable;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.File;
import java.io.IOException;
import java.net.ServerSocket;
import java.net.Socket;
import java.net.SocketException;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;
import java.util.concurrent.Semaphore;
import javax.net.ServerSocketFactory;
import javax.net.SocketFactory;

public abstract class ConnectionController
{
    public static final int BITS_FOR_MESSAGE_LENGTH = 16;//HAS TO BE DIVIDED BY 8!!!

    protected boolean isInitialized = false;
    protected boolean isErrorInServer;
    protected Semaphore isListeningForConnections;
    protected Semaphore isConnectingSemaphore;
    protected Map<Integer, Player> indexToPlayer;
    protected ServerSocket serverSocket;
    protected File sslKeyFile;
    protected Integer myIndex;

    public ConnectionController()
    {
        indexToPlayer = new TreeMap<Integer, Player>();
        isConnectingSemaphore = new Semaphore(1, true);
        isListeningForConnections = new Semaphore(1, true);
    }

    public Map<Integer, Player> getIndexToPlayer() {
        return indexToPlayer;
    }        

    public void setMyIndex(Integer myIndex)
    {
        this.myIndex = myIndex;
    }

    public void addPlayer(Player player, int index)
    {
        indexToPlayer.put(index, player);

    }
    
    public void removePlayer(int index) throws IOException
    {
        if ( ( index >= indexToPlayer.size() ) || (index<0) )
        {
            throw new IllegalArgumentException("Illegal  index");
        }
        
        Player player = indexToPlayer.get(index);
        
        // close socket and replace it with null
        player.socket.close();
        player.socket = null;
        
    }

    public int getNumOfPlayers()
    {
        return indexToPlayer.size();
    }

    public void close() throws IOException
    {
        CancellCreateConnections();
        for (Player player : indexToPlayer.values())
        {
            if (player.socket != null)
            {
                player.socket.close();
            }
        }
        if (serverSocket != null)
        {
            serverSocket.close();
        }
        isInitialized = false;

    }

    public int findMyIndex()
    {
        if (myIndex == null)
        {
            throw new RuntimeException("No definition to player's index");
        }
        return myIndex;
    }

    private boolean checkInitialized()
    {
        for (int index : indexToPlayer.keySet())
        {
            if ((index != myIndex) && (indexToPlayer.get(index).socket == null))
            {
                return false;
            }
        }
        return true;
    }

    public void setTimeOut(int timeoutValueMS) throws SocketException
    {
        for (Player player : indexToPlayer.values())
        {
            if (player.socket != null)
            {
                player.socket.setSoTimeout(timeoutValueMS);
            }
        }
    }

    /**
     * <p>
     * This function creates the connection (after the list of players is ready - can be initialized from server,XML,File....).
     * The main idea is that each player recieves an index, and initiates the connections only to players with higher indexes,
     * while waiting for connections from the players with lower indexs. This is to prevent 2 connection between 2 players.
     * </p>
     * @param proglog - the logger.
     * @return true if finished without errors or interupted(cancelled), false if error occured.
     */
    public boolean CreateConnections(ProgressLog proglog)
    {
        isConnectingSemaphore.acquireUninterruptibly();

        ServerThread serverThread = new ServerThread(myIndex, indexToPlayer.get(myIndex).port, proglog);
        if (myIndex > 0)
        {
            isListeningForConnections.acquireUninterruptibly();
            serverThread.start();
        }

        for (int index = myIndex + 1; index < indexToPlayer.size(); index++)
        {
            try
            {
                Socket socket = null;

                /* Note here that the user maybe trying to connect to a player which didn't make a connection yet - so we might have to retry */
                SocketFactory socketFactory = BlindSSLSocketFactory.getDefault();
                while (isConnectingSemaphore.availablePermits() == 0)
                {
                    try
                    {
                        socket = socketFactory.createSocket(indexToPlayer.get(index).ip, indexToPlayer.get(index).port);
                        //socket = new Socket(indexToPlayer.get(index).ip, indexToPlayer.get(index).port);
                    } catch (Exception e)
                    {
//                        proglog.printWarning("Couldn't open a socket to player indexed:" + 
//                        		index + " with ip/port:"+ indexToPlayer.get(index).ip +"\\" + 
//                        		indexToPlayer.get(index).port + "\n" + e.getMessage() + ". Retrying...");
                        continue;
                    }
                    break;
                }

                if (socket == null)
                {
                    if (isConnectingSemaphore.availablePermits() == 0)
                    {
                        proglog.printError("Could not connect to player " + index);
                    }
                    return false;
                }

                /* Sending the remote player we just connected to, our index */
                indexToPlayer.get(index).socket = socket;
                DataOutputStream outToPlayer = new DataOutputStream(socket.getOutputStream());
                outToPlayer.write(myIndex);
                proglog.printInformation("Conneced to player (" + index + ")");
            } catch (Exception ex)
            {
                proglog.printError("Cannot connect to some players. Failed to calculate.");
                return false;
            }
        }

        /* wait for connecting thread to finish(get semaphore), and then release semphore */
        isListeningForConnections.acquireUninterruptibly();
        isListeningForConnections.release();

        if (isErrorInServer)
        {
            return false;
        }

        if (isConnectingSemaphore.availablePermits() != 0)
        {
            proglog.printInformation("Connection creation cancelleds");
            return false;
        }

        isInitialized = true;
        isConnectingSemaphore.release();

        proglog.printInformation("Finished connecting process");

        return true;
    }

    public void CancellCreateConnections()
    {
        isListeningForConnections.release();
        isConnectingSemaphore.release();
    }

    /**
     * 
     * @param sharedSecret
     * @param index
     * @throws java.io.IOException
     * <p> Sends "sharedSecret" object to the player with "index". </p>
     */
    public void sendSercrets(Sendable sharedSecret, int index) throws IOException
    {
        if (!isInitialized)
        {
            throw new RuntimeException("Connections have not been initialized");
        }

        Player player = indexToPlayer.get(index);
        if (player.socket != null)
        {
            DataOutputStream outToPlayer = new DataOutputStream(player.socket.getOutputStream());
            byte[] toSend = sharedSecret.writeToByteArray();
            try
            {
            BitStream bs = new BitStream();
            bs.writeInt(toSend.length,BITS_FOR_MESSAGE_LENGTH);
            outToPlayer.write(bs.getByteArray());//todo - we should send a more general Object with header containing more data
            outToPlayer.write(toSend, 0, toSend.length);//todo - we should send a more general Object with header containing more data
            }
            catch (Exception e)
            {
                // do nothing. just ignore player
            }
        }
        else
        {
            throw new RuntimeException("Trying to send secret to a closed socket !");
        }
    }

    /**
     * 
     * @param index
     * @param p
     * @return
     * @throws java.io.IOException
     * <p> Recvs and returns a sendable object from player with "index", if player doesn't send after a time out - returns null </p>
     */
    public Sendable recieveSecrets(int index, int p) throws IOException
    {
        Player player = indexToPlayer.get(index);
        if (player.socket != null)
        {
            DataInputStream inFromPlayer = new DataInputStream(player.socket.getInputStream());
        
            byte[] ba;
            try
            {
            byte[] lengthArray = new byte[BITS_FOR_MESSAGE_LENGTH/8];
            inFromPlayer.read(lengthArray, 0, BITS_FOR_MESSAGE_LENGTH/8);
            BitStream bs = new BitStream(lengthArray);
            int length = bs.readInt(BITS_FOR_MESSAGE_LENGTH);
             ba = new byte[length];
        
                inFromPlayer.read(ba, 0, ba.length);
            }
            catch(Exception ex)
            {
                return null;
            }
            
            Sendable value = Sendable.loadFromByteArray(ba, p);
            return value;

        } else
        {
            throw new RuntimeException("Trying to recieve secret to self!");
        }
    }

    /**
     * 
     * @param sharedSecret
     * @throws java.io.IOException
     * <p> Sends sharedSecret to all player. </p>
     */
    public void sendSercrets(Sendable sharedSecret) throws IOException
    {
        for (int index : indexToPlayer.keySet())
        {
            if (index != myIndex)
            {
                sendSercrets(sharedSecret, index);
            } else
            {
                // send to all but to self
            }
        }
    }
    
        /**
     * 
     * @param sharedSecret
     * @throws java.io.IOException
     * <p> Sends sharedSecret to all player. </p>
     */
    public void sendSercrets(Sendable sharedSecret, boolean[] IndexToSend) throws IOException
    {
        for (int index : indexToPlayer.keySet())
        {
            if (IndexToSend[index] == false)
            {
                continue;
            }
            
            if (index != myIndex)
            {
                sendSercrets(sharedSecret, index);
            } else
            {
                // send to all but to self
            }
        }
    }

    /**
     * <p> recvis from all players. </p>
     * @param p
     * @return
     * @throws java.io.IOException
     */
    public List<Sendable> recieveSecrets(int p) throws IOException
    {
        List<Sendable> receivedSecrets = new ArrayList<Sendable>();
        for (int index : indexToPlayer.keySet())
        {
            if (index != myIndex)
            {
                receivedSecrets.add(recieveSecrets(index, p));
            } else
            {
                // recv all but from self
            }
        }
        return receivedSecrets;
    }
    
    
    // Bar please change this function
    public List<? extends Sendable> shareSecrets(Sendable sharedSecret, int prime) throws IOException
    {
        List<Sendable> sendableList = new ArrayList<Sendable>();
        for (int i = 0; i < indexToPlayer.size(); i++)
        {
            sendableList.add(sharedSecret);
        }
        return shareSecrets(sendableList, prime);
    }

    
    // Bar please change this function
    public List<? extends Sendable> shareSecrets(Sendable sharedSecret, int prime, boolean[] IndexToSend) throws IOException
    {
        List<Sendable> sendableList = new ArrayList<Sendable>();
        for (int i = 0; i < indexToPlayer.size(); i++)
        {
            sendableList.add(sharedSecret);
        }
        return shareSecrets(sendableList, prime, IndexToSend);
    }


    
    public List<? extends Sendable> shareSecrets(List<? extends Sendable> sharedSecrets, int prime, boolean[] IndexToSend) throws IOException
    {
        assert IndexToSend.length == indexToPlayer.size();
        Sendable MyShareObject = null;
        
        for (int index = 0; index < IndexToSend.length; index++)
        {// send to all but self
            if (IndexToSend[index] == true) 
            {
                if (index != myIndex)
                {
                    sendSercrets(sharedSecrets.get(index), index);
                }
                else
                {
                    MyShareObject = sharedSecrets.get(index);
                }
            }
        }
        
        List<Sendable> receivedSecrets = new ArrayList<Sendable>();
        for (int index = 0; index < IndexToSend.length; index++)
        {
            if (IndexToSend[index] == true)
            {
                    if (index != myIndex)
                    {
                        Sendable value = recieveSecrets(index, prime);
                        receivedSecrets.add(value);
                    }
                    else
                    {
                        receivedSecrets.add(MyShareObject);
                    }
            }
            else
            {
                    receivedSecrets.add(null);
            }
        }
        
        return receivedSecrets;
    }
    
    
    public List<? extends Sendable> shareSecrets(List<? extends Sendable> sharedSecrets, int prime) throws IOException
    {
        assert sharedSecrets.size() == indexToPlayer.size() && sharedSecrets.size() > 0;
        
        boolean[] IndexToSend = new boolean [sharedSecrets.size()];
        for (int i = 0; i<IndexToSend.length; i++)
        {
            IndexToSend[i] = true;
        }
        
        return shareSecrets(sharedSecrets,prime,IndexToSend);        
    }

    
    
    
    public static class Player
    {

        public final String ip;
        public final int port;
        public Socket socket = null;
        public final List<Integer> outputIndexes;
        //public final boolean isThisMe;
        public Player(String ip, int port, List<Integer> outputIndexes)
        {
            this.ip = ip;
            this.port = port;
            this.outputIndexes = outputIndexes;        
        }
    }

    public class ServerThread extends Thread
    {

        private int myIndex;
        private int myPort;
        private ProgressLog proglog;

        public ServerThread(int myIndex, int myPort, ProgressLog proglog)
        {
            this.myIndex = myIndex;
            this.myPort = myPort;
            this.proglog = proglog;
        }

        @Override
        public void run()
        {
            isErrorInServer = false;
            int connectionsOpened = 0;
            try
            {
                ServerSocketFactory serverSocketFactory = BlindSSLServerSocketFactory.getDefault(sslKeyFile);
                serverSocket = serverSocketFactory.createServerSocket(myPort);
                //serverSocket = new ServerSocket(myPort);


                while ((connectionsOpened < myIndex) && (isListeningForConnections.availablePermits() == 0))
                {
                    Socket socket = serverSocket.accept();
                    DataInputStream inFromPlayer = new DataInputStream(socket.getInputStream());
                    int index = (int) inFromPlayer.readByte();
                    if (index >= myIndex)
                    {
                        continue;//invalid socket. this socket should be opened from this side
                    }
                    indexToPlayer.get(index).socket = socket;
                    proglog.printInformation("player (" + index + ") is now connected");
                    connectionsOpened++;
                }
            } catch (IOException ex)
            {
                if (isListeningForConnections.availablePermits() == 0)
                {
                    proglog.printError("a player didn't manage to open a socket:" + ex);
                    isErrorInServer = true;
                }
            }

            proglog.printInformation("Stopped listening to connections");
            isListeningForConnections.release();
        }
    }
}
