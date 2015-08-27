package mpc.communication;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.net.InetAddress;
import java.net.ServerSocket;
import java.security.GeneralSecurityException;
import java.security.KeyStore;
import java.security.cert.X509Certificate;

import javax.net.ServerSocketFactory;
import javax.net.SocketFactory;
import javax.net.ssl.KeyManager;
import javax.net.ssl.KeyManagerFactory;
import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;

/**
 * BlindSSLSocketFactoryTest
 *  Simple test to show an Active Directory (LDAP)
 *  and HTTPS connection without verifying the 
 *  server's certificate.
 *  
 * @author Mike McKinney, Platinum Solutions, Inc.
 */
public class BlindSSLServerSocketFactory extends ServerSocketFactory
{

    private static ServerSocketFactory blindFactory = null;
    private static boolean initialized = false;
    /**
     * Builds an all trusting "blind" ssl socket factory.
     */
    
    public static void init(File sslKeyFile) {
//    static
//    {
        // create a trust manager that will purposefully fall down on the
        // job
        TrustManager[] blindTrustMan = new TrustManager[]{
            new X509TrustManager() {

                public X509Certificate[] getAcceptedIssuers() {
                    return null;
                }

                public void checkClientTrusted(X509Certificate[] c, String a) {
                }

                public void checkServerTrusted(X509Certificate[] c, String a) {
                }
            }
        };

        // create our "blind" ssl socket factory with our lazy trust manager
        try {
            assert sslKeyFile.exists();
            char[] passphrase = "123456".toCharArray();
            KeyStore keystore = KeyStore.getInstance("JKS");
            keystore.load(new FileInputStream(sslKeyFile), passphrase);

            KeyManagerFactory kmf = KeyManagerFactory.getInstance("SunX509", "SunJSSE");
            kmf.init(keystore, passphrase);
            KeyManager[] keyManagers = kmf.getKeyManagers();

            SSLContext sc = SSLContext.getInstance("SSL");

            sc.init(keyManagers, blindTrustMan, new java.security.SecureRandom());
            blindFactory = sc.getServerSocketFactory();
        } catch (GeneralSecurityException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        }
        initialized = true;        
    }

    /**
     * @see javax.net.SocketFactory#getDefault()
     */
    public static ServerSocketFactory getDefault(File sslKeyFile)
    {
        if (!initialized){
            BlindSSLServerSocketFactory.init(sslKeyFile);
        }
        return new BlindSSLServerSocketFactory();
    }

    @Override
    public ServerSocket createServerSocket(int arg0) throws IOException
    {
        return blindFactory.createServerSocket(arg0);
    }

    @Override
    public ServerSocket createServerSocket(int arg0, int arg1) throws IOException
    {
        return blindFactory.createServerSocket(arg0, arg1);
    }

    @Override
    public ServerSocket createServerSocket(int arg0, int arg1, InetAddress arg2) throws IOException
    {
        return blindFactory.createServerSocket(arg0, arg1, arg2);
    }

}
