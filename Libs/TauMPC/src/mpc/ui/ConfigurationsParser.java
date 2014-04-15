/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package mpc.ui;

import java.io.File;
import java.io.FileNotFoundException;
import java.text.ParseException;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import mpc.communication.ConnectionController.Player;
import org.w3c.dom.DOMException;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;


public class ConfigurationsParser {

    private File file;
    private File keyFile;
    private boolean isConnectionsDetailsMandatory;

    public ConfigurationsParser(File file, boolean isConnectionsDetailsMandatory) {
        this.file = file;
        this.isConnectionsDetailsMandatory = isConnectionsDetailsMandatory;
    }

    public File getKeyFile() {
        return keyFile;
    }
    
    private void checkUniqueness(NodeList nodeList) throws ParseException{
        if (nodeList.getLength() == 0){
            throw new ParseException("No SSL Key file configurations found", 0);
        }
        if (nodeList.getLength() > 1){
            throw new ParseException("More than 1 SSL Key file configurations found", 0);
        }        
    }

    public void parse(Map<Integer, Player> players) throws Exception {
        if (!file.exists()) {
            throw new FileNotFoundException("Configuration file not found");
        }
        DocumentBuilderFactory docBuilderFactory = DocumentBuilderFactory.newInstance();
        DocumentBuilder docBuilder = docBuilderFactory.newDocumentBuilder();
        Document doc = docBuilder.parse(this.file);

       doc.getDocumentElement().normalize();
        if (players != null) {
            parsePlayers(doc, players);
        }

        NodeList sslConfigurations = doc.getElementsByTagName("SSL_Configurations");
       checkUniqueness(sslConfigurations);
       Element sslElement = (Element) sslConfigurations.item(0);
        NodeList listOfSSLFiles = sslElement.getElementsByTagName("SSL_Key_File_Path");
        checkUniqueness(listOfSSLFiles);
        Element sslFileCOnfigurationsElement = (Element) listOfSSLFiles.item(0);
        NodeList nodeList = sslFileCOnfigurationsElement.getChildNodes();
        String keyFilePath = ((Node) nodeList.item(0)).getNodeValue().trim();
        keyFile = new File(keyFilePath);
        if (!keyFile.exists()){
            throw new FileNotFoundException("Cannot find ssl key file in " + keyFilePath);
        }        
    }
    
    private void parsePlayers(Document doc, Map<Integer, Player> players) throws NumberFormatException, DOMException, ParseException {
        NodeList playersConfigurations = doc.getElementsByTagName("PlayersConfigurations");
       checkUniqueness(playersConfigurations);
       Element playersElement = (Element) playersConfigurations.item(0);
       NodeList listOfPersons = playersElement.getElementsByTagName("Player");
        for (int s = 0; s < listOfPersons.getLength(); s++) {
            Node firstPersonNode = listOfPersons.item(s);
            if (firstPersonNode.getNodeType() == Node.ELEMENT_NODE) {
                Element firstPersonElement = (Element) firstPersonNode;
                NodeList ipList = firstPersonElement.getElementsByTagName("ip");
                Element ipElement = (Element) ipList.item(0);
                boolean hasIPData = ipElement != null && ipElement.getChildNodes() != null && ipElement.getChildNodes().item(0) != null;
                if (isConnectionsDetailsMandatory && !hasIPData){
                    throw new ParseException("IP is missing for player", 0);
                }
                String ip = null;
                NodeList nodeList;
                if (hasIPData) {
                    nodeList = ipElement.getChildNodes();
                    ip = ((Node) nodeList.item(0)).getNodeValue().trim();
                }           

                NodeList portList = firstPersonElement.getElementsByTagName("port");
                Element portElement = (Element) portList.item(0);
                boolean hasPortData = portElement != null && portElement.getChildNodes() != null  && portElement.getChildNodes().item(0) != null;
                if (isConnectionsDetailsMandatory && !hasPortData){
                    throw new ParseException("Port is missing for player", 0);
                }                
                int port = 0;
                if (hasPortData){
                    nodeList = portElement.getChildNodes();   
                    String portString = ((Node) nodeList.item(0)).getNodeValue().trim();
                    port = Integer.parseInt(portString);
                }
                
                NodeList indexList = firstPersonElement.getElementsByTagName("index");
                Element indexElement = (Element) indexList.item(0);
                nodeList = indexElement.getChildNodes();
                String indexString = ((Node) nodeList.item(0)).getNodeValue().trim();
                int index = Integer.parseInt(indexString);
                
                NodeList outputsIndexList = firstPersonElement.getElementsByTagName("outputs");
                Element outputsIndexElement = (Element) outputsIndexList.item(0);
                List<Integer> outputIndexes;
                nodeList = outputsIndexElement.getChildNodes();
                if (nodeList.getLength() == 0){
                    outputIndexes = new ArrayList<Integer>();                    
                } else {
                    String outputsIndexString = ((Node) nodeList.item(0)).getNodeValue().trim();
                    outputIndexes = parseOutputIndexes(outputsIndexString);
                }
                players.put(index, new Player(ip, port, outputIndexes));
            }
        }
    }

    private List<Integer> parseOutputIndexes(String outputIndexes) {
        String[] indexes = outputIndexes.split(",");        
        List<Integer> result = new ArrayList<Integer>();
        if (indexes == null){
            return result;
        }
        for (String stringIndex : indexes){
            result.add(Integer.parseInt(stringIndex.trim()));            
        }
        return result;
    }

    public static void main(String[] args) throws Exception {
        ConfigurationsParser p = new ConfigurationsParser(new File("players.xml"), true);
        p.parse(new TreeMap<Integer, Player>());
    }

    
}
