/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package mpc.communication;

import mpc.ui.ConfigurationsParser;
import mpc.ui.ProgressLog;
import java.io.File;
import java.io.FileNotFoundException;




public class XMLConnectionController extends ConnectionController
{

    public boolean CreateConnections(File playersXmlFile, ProgressLog proglog, int numberOfPlayers, int myIndex) throws Exception
    {
        ConfigurationsParser parser = new ConfigurationsParser(playersXmlFile, true);
        try
        {
            parser.parse(indexToPlayer);
        }
        catch (FileNotFoundException ex)
        {
            proglog.printError("Error: " + ex.getMessage());
            return false;
        }
        if (numberOfPlayers != indexToPlayer.size())
        {
            proglog.printError("Number of players in circuit (" + numberOfPlayers + ") is different than xml (" + indexToPlayer.size() + ")");
            return false;
        }
        sslKeyFile = parser.getKeyFile();
        assert  sslKeyFile.exists();
        setMyIndex(myIndex);
        return (super.CreateConnections(proglog));
    }
}
