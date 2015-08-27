
package mpc.communication;

import mpc.sendables.Sendable;
import java.io.IOException;
import java.util.List;


/* This class implementation is a naive implementation  -  can be replaced by a real bullein board protocol in the future*/
public class  BulletinBoard {

        public static  Sendable read(int publisher, int prime ,ConnectionController conCtrl) throws IOException{
                return conCtrl.recieveSecrets(publisher, prime);
        }

        public static  void publish(Sendable toSend, boolean [] sendToPlayers, ConnectionController conCtrl) throws IOException{
                conCtrl.sendSercrets(toSend, sendToPlayers);
        }

        
        public static   List<? extends Sendable> publishAndRead(Sendable toSend, int prime, boolean [] sendToPlayers ,ConnectionController conCtrl) throws IOException{
                return  conCtrl.shareSecrets(toSend, prime, sendToPlayers);
        }
       
}
