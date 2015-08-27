/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package mpc.sendables;

import mpc.finite_field_math.Zp;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import mpc.communication.*;


public abstract class Sendable {
    
    public enum MessageType{
        SIMPLE_ZP(0), 
        ZP_LISTS(1),
        ZPS_BUNDLE(2),
        MESSAGE(3),
        MULT_STEP_BCASE(4),
       MULT_STEP_BCASE_BUNDLE(5),
       MULT_STEP_VERIFY_POLY(6),
       TO_SERVER_OBJECT(7),
       FROM_SERVER_OBJECT(8);
        
        public final int code;
        private MessageType(int code) {
            this.code = code;
        }        
        public static MessageType getByCode(int code){
            for (MessageType messageType : values()){
                if (messageType.code == code){
                    return messageType;
                }                
            }
            throw new RuntimeException("Unknown Message Type: " + code);
        }        
    }
    public final static Sendable loadFromByteArray(byte[] ba, int prime) throws IOException{
        BitStream bs = new BitStream(ba);
        MessageType messageType = bs.readMessageType();
        Sendable sendable;
        switch(messageType){
            case SIMPLE_ZP:
                sendable = new ShareObject();
                break;
            case ZP_LISTS:
                sendable = new SecretPolynomials();
                break;
            case ZPS_BUNDLE:
                sendable = new SecretPolynomialsBundle();
                break;
            case MESSAGE:
                sendable = new PlayerNotification();
                break;
            case MULT_STEP_BCASE:
                sendable = new MultStepBCaseShare();
                break;
            case MULT_STEP_BCASE_BUNDLE:
                sendable = new MultStepBCaseShareBundle();
                break;
            case MULT_STEP_VERIFY_POLY:
                sendable = new MultStepVerificationPoly();
                break;
            case TO_SERVER_OBJECT:
                sendable = new ToServerObject();
                break;
            case FROM_SERVER_OBJECT:
                sendable = new FromServerObject();
                break;
                
            default:
                assert false;
                return null;                     
        }                
        sendable.loadFromByteArrayNoHeader(bs, prime);
        return sendable;       
    }
    
    public static List<ShareObject> asShareObjects(List<? extends Sendable> sendables){
        List<ShareObject> shareObjects = new ArrayList<ShareObject>();
        for (Sendable sendable : sendables){
            if (sendable != null){
                    shareObjects.add(sendable.asShareObject());
            } else {
                    shareObjects.add(null);
            }
        }
        return shareObjects;        
    }
    
    public static List<SecretPolynomials> asSecretPolynomials(List<? extends Sendable> sendables){
        List<SecretPolynomials> SecretPolynomials = new ArrayList<SecretPolynomials>();
        for (Sendable sendable : sendables){
             if (sendable != null){
                    SecretPolynomials.add(sendable.asSecretPolynomials());
             } else {
                    SecretPolynomials.add(null);
             }
        }
        return SecretPolynomials;        
    }
    
    public static List<SecretPolynomialsBundle> asSecretPolynomialsBundles(List<? extends Sendable> sendables){
        List<SecretPolynomialsBundle> SecretPolynomialsBundles = new ArrayList<SecretPolynomialsBundle>();
        for (Sendable sendable : sendables){
            if (sendable != null){
                    SecretPolynomialsBundles.add(sendable.asSecretPolynomialsBundle());
            } else {
                    SecretPolynomialsBundles.add(null);
            }
        }
        return SecretPolynomialsBundles;        
    }
    
    public static List<PlayerNotification> asPlayerNotifications(List<? extends Sendable> sendables){
        List<PlayerNotification> PlayerNotifications = new ArrayList<PlayerNotification>();
        for (Sendable sendable : sendables){
            if (sendable != null){
                    PlayerNotifications.add(sendable.asPlayerNotification());
            }else {
                    PlayerNotifications.add(null);
            }
        }
        return PlayerNotifications;        
    }
    
    public static List<MultStepBCaseShare> asMultStepBCaseShares(List<? extends Sendable> sendables){
        List<MultStepBCaseShare> multStepVerifys = new ArrayList<MultStepBCaseShare>();
        for (Sendable sendable : sendables){
            if (sendable != null){
                    multStepVerifys.add(sendable.asMultStepBCaseShare());
            }else {
                    multStepVerifys.add(null);
            }
        }
        return multStepVerifys;        
    }
    
    public static List<MultStepBCaseShareBundle> asMultStepBCaseShareBundles(List<? extends Sendable> sendables){
        List<MultStepBCaseShareBundle> multStepBCaseShareBundles = new ArrayList<MultStepBCaseShareBundle>();
        for (Sendable sendable : sendables){
            if (sendable != null){
                    multStepBCaseShareBundles.add(sendable.asMultStepBCaseShareBundle());
            }else {
                    multStepBCaseShareBundles.add(null);
            }
        }
        return multStepBCaseShareBundles;        
    }
    
    public static List<MultStepVerificationPoly> asMultStepVerificationPolys(List<? extends Sendable> sendables){
        List<MultStepVerificationPoly> multStepVerificationPolys = new ArrayList<MultStepVerificationPoly>();
        for (Sendable sendable : sendables){
            if (sendable != null){
                    multStepVerificationPolys.add(sendable.asMultStepVerificationPoly());
            }else {
                    multStepVerificationPolys.add(null);
            }
        }
        return multStepVerificationPolys;        
    }
    
    
    public ShareObject asShareObject() {
        if (this instanceof ShareObject){
            return (ShareObject)this;
        }
        return null;
    }
    
    public SecretPolynomials asSecretPolynomials() {
        if (this instanceof SecretPolynomials){
            return (SecretPolynomials)this;
        }
        return null;
    }
    
    public SecretPolynomialsBundle asSecretPolynomialsBundle() {
        if (this instanceof SecretPolynomialsBundle){
            return (SecretPolynomialsBundle)this;
        }
        return null;
    }
    
    public PlayerNotification asPlayerNotification() {
        if (this instanceof PlayerNotification){
            return (PlayerNotification)this;
        }
        return null;
    }
    
    public MultStepBCaseShare asMultStepBCaseShare() {
        if (this instanceof MultStepBCaseShare){
            return (MultStepBCaseShare)this;
        }
        return null;
    }  
    
    public MultStepBCaseShareBundle asMultStepBCaseShareBundle() {
        if (this instanceof MultStepBCaseShareBundle){
            return (MultStepBCaseShareBundle)this;
        }
        return null;
    }      
    
    public MultStepVerificationPoly asMultStepVerificationPoly() {
        if (this instanceof MultStepVerificationPoly){
            return (MultStepVerificationPoly)this;
        }
        return null;
    }    
    
    public abstract byte[] writeToByteArray() throws IOException;
    
    protected abstract void loadFromByteArrayNoHeader(BitStream bs, int prime) throws IOException;    
    
    public abstract void writeToBitStreamNoHeader(BitStream bs);
    
    protected boolean compareSecrets(Zp s1, Zp s2){
        if (s1 == null && s2 != null || s1 != null && s2 == null){
            return false;
        }
        if (s1 != null && s2 != null){
            return s1.equals(s2);
        }
        return true;
    }
}

