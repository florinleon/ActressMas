using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Diagnostics;
using SocketServerLib.Message;
using SocketServerLib.SocketHandler;

namespace SocketServerLib.Client
{
    /// <summary>
    /// This abstract class represents a Client to a Socket Server. Implement the method GetHandler to create your Socket Client.
    /// </summary>
    internal abstract class AbstractSocketClient
    {
        /// <summary>
        /// The socket client handler
        /// </summary>
        protected AbstractTcpSocketClientHandler handler = null;
        /// <summary>
        /// Delegate for a connection event
        /// </summary>
        private SocketConnectionDelegate connectionEvent = null;
        /// <summary>
        /// Delegate for a close connection event
        /// </summary>
        private SocketConnectionDelegate closeConnectionEvent = null;
        /// <summary>
        /// Delegate for an incoming message event
        /// </summary>
        private SocketConnectionDelegate inReceivingEvent = null;
        /// <summary>
        /// Delegate for a receive message event
        /// </summary>
        private ReceiveMessageDelegate receiveMessageEvent = null;
        /// <summary>
        /// Flag for connection status (connected or not)
        /// </summary>
        private bool connected = false;
        /// <summary>
        /// Flag for check on certification
        /// </summary>
        private bool flagRemoteCertificateNameMismatch = false;
        /// <summary>
        /// Lock object for raise event
        /// </summary>
        private readonly object raiseLock = new object();

        /// <summary>
        /// Default constructor
        /// </summary>
        public AbstractSocketClient()
            : base()
        {
            // Timeout default value
            KeepAlive = false;
        }

        /// <summary>
        /// Implement this method to create a Socket Client class.
        /// </summary>
        /// <param name="handler">The socket client handler</param>
        /// <param name="stream">The ssl stream</param>
        /// <param name="sendHandleTimeout">The send timeout</param>
        /// <param name="socketSendTimeout">The socket send timeout</param>
        /// <returns></returns>
        protected abstract AbstractTcpSocketClientHandler GetHandler(Socket handler, SslStream stream);

        #region Properties

        /// <summary>
        /// Get/set Socket Keep Alive flag.
        /// </summary>
        public bool KeepAlive
        {
            get
            {
                if (this.handler != null)
                {
                    return this.handler.KeepAlive;
                }
                return false;
            }
            set
            {
                if (this.handler != null)
                {
                    this.handler.KeepAlive = value;
                }
            }
        }

        /// <summary>
        /// Get if the client is connected or not
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return connected;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event for a connection.
        /// </summary>
        public event SocketConnectionDelegate ConnectionEvent
        {
            add { lock (raiseLock) { connectionEvent += value; } }
            remove { lock (raiseLock) { connectionEvent -= value; } }
        }

        /// <summary>
        /// Event for a close connection.
        /// </summary>
        public event SocketConnectionDelegate CloseConnectionEvent
        {
            add { lock (raiseLock) { closeConnectionEvent += value; } }
            remove { lock (raiseLock) { closeConnectionEvent -= value; } }
        }

        /// <summary>
        /// Event for an incoming a message.
        /// </summary>
        public event SocketConnectionDelegate InReceivingEvent
        {
            add { lock (raiseLock) { inReceivingEvent += value; } }
            remove { lock (raiseLock) { inReceivingEvent -= value; } }
        }

        /// <summary>
        /// Event for receive a message.
        /// </summary>
        public event ReceiveMessageDelegate ReceiveMessageEvent
        {
            add { lock (raiseLock) { receiveMessageEvent += value; } }
            remove { lock (raiseLock) { receiveMessageEvent -= value; } }
        }

        #endregion

        /// <summary>
        /// Close the Client.
        /// </summary>
        public void Close()
        {
            if (handler == null)
            {
                return;
            }
            handler.Close();
        }

        #region Connect methods

        /// <summary>
        /// Connect the client to the EndPoint on SSL.
        /// </summary>
        /// <param name="endPoint">The remote end point</param>
        /// <param name="clientCertificatePath">The client certificate file</param>
        /// <param name="certificatePassword">The client certifciate password</param>
        public void Connect(IPEndPoint endPoint, string clientCertificatePath, string certificatePassword)
        {
            // Load the client certificte
            X509Certificate2 clientCertificate = new X509Certificate2(clientCertificatePath, certificatePassword);
            X509CertificateCollection clientCertificateList = new X509CertificateCollection();
            clientCertificateList.Add(clientCertificate);
            // Connect the client to the remote end point
            TcpClient sslTcpClient = new TcpClient();
            sslTcpClient.Connect(endPoint);
            sslTcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, this.KeepAlive);
            // Open a ssl stream for the communication
            SslStream sslStream = new SslStream(sslTcpClient.GetStream(), false, new RemoteCertificateValidationCallback(this.OnVerifyCertificate));
            sslStream.AuthenticateAsClient("NONE", clientCertificateList, SslProtocols.Ssl3, false); //TODO: params from config for mutual auth, protocol and revocation
            // Create the socket client handler, add the callback for the event and start to receiving
            Socket client = sslTcpClient.Client;
            client.Blocking = true;
            handler = GetHandler(client, sslStream);
            handler.ReceiveMessageEvent += new ReceiveMessageDelegate(handler_ReceiveMessageEvent);
            handler.CloseConnectionEvent += new SocketConnectionDelegate(handler_CloseConnectionEvent);
            handler.InReceivingEvent += new SocketConnectionDelegate(handler_InReceivingEvent);
            connected = true;
            this.OnConnection(handler);
            handler.StartReceive();
        }

        /// <summary>
        /// Connect the client to the EndPoint.
        /// </summary>
        /// <param name="endPoint">The remote end point</param>
        public void Connect(IPEndPoint endPoint)
        {
            // Connect the client to the remote end point
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, this.KeepAlive);
            client.Connect(endPoint);
            client.Blocking = true;
            // Create the socket client handler, add the callback for the event and start to receiving
            handler = GetHandler(client, null);
            handler.ReceiveMessageEvent += new ReceiveMessageDelegate(handler_ReceiveMessageEvent);
            handler.CloseConnectionEvent += new SocketConnectionDelegate(handler_CloseConnectionEvent);
            handler.InReceivingEvent += new SocketConnectionDelegate(handler_InReceivingEvent);
            connected = true;
            this.OnConnection(handler);
            handler.StartReceive();
        }

        #endregion

        #region Method to verify remote certificate

        /// <summary>
        /// Method to verify the certificate
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="certificate">The certificate</param>
        /// <param name="chain">The chain</param>
        /// <param name="sslPolicyErrors">The error policy</param>
        /// <returns></returns>
        private bool OnVerifyCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            lock (this)
            {
                //Return true if the server certificate is ok
                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    return true;
                }

                bool acceptCertificate = true;
                string msg = "The server could not be validated for the following reason(s):\r\n";

                //The server did not present a certificate
                if ((sslPolicyErrors &
                    SslPolicyErrors.RemoteCertificateNotAvailable) == SslPolicyErrors.RemoteCertificateNotAvailable)
                {
                    msg = msg + "\r\n    -The server did not present a certificate.\r\n";
                    acceptCertificate = false;
                }
                else
                {
                    //The certificate does not match the server name
                    if ((sslPolicyErrors &
                        SslPolicyErrors.RemoteCertificateNameMismatch) == SslPolicyErrors.RemoteCertificateNameMismatch)
                    {
                        if (flagRemoteCertificateNameMismatch)
                        {
                            msg = msg + "\r\n    -The certificate name does not match the authenticated name.\r\n";
                            acceptCertificate = false;
                        }
                        else
                        {
                            Trace.WriteLine("The certificate name does not match the authenticated name");
                        }
                    }

                    //There is some other problem with the certificate
                    if ((sslPolicyErrors &
                        SslPolicyErrors.RemoteCertificateChainErrors) == SslPolicyErrors.RemoteCertificateChainErrors)
                    {
                        foreach (X509ChainStatus item in chain.ChainStatus)
                        {
                            if (item.Status != X509ChainStatusFlags.RevocationStatusUnknown &&
                                item.Status != X509ChainStatusFlags.OfflineRevocation)
                            {
                                break;
                            }

                            if (item.Status != X509ChainStatusFlags.NoError)
                            {
                                msg = msg + "\r\n    -" + item.StatusInformation;
                                acceptCertificate = false;
                            }
                        }
                    }
                }
                if (acceptCertificate == false)
                {
                    Trace.WriteLine(msg);
                }
                return acceptCertificate;
            }
        }

        #endregion

        #region Raise event methods

        /// <summary>
        /// Raise a connect event.
        /// </summary>
        /// <param name="abstractTcpSocketClientHandler">The socket client handler closed</param>
        protected virtual void OnConnection(AbstractTcpSocketClientHandler abstractTcpSocketClientHandler)
        {
            if (connectionEvent != null)
            {
                connectionEvent(handler);
            }
        }

        /// <summary>
        /// Raise a close connection event.
        /// </summary>
        /// <param name="handler">The socket client handler of the close connection</param>
        void handler_CloseConnectionEvent(AbstractTcpSocketClientHandler handler)
        {
            connected = false;
            if (closeConnectionEvent != null)
            {
                closeConnectionEvent(handler);
            }
        }

        /// <summary>
        /// Raise an incoming message event.
        /// </summary>
        /// <param name="handler">The socket client handler of the close connection</param>
        void handler_InReceivingEvent(AbstractTcpSocketClientHandler handler)
        {
            if (inReceivingEvent != null)
            {
                inReceivingEvent(handler);
            }
        }

        /// <summary>
        /// Raise a received message event.
        /// </summary>
        /// <param name="handler">The socket client handler of the close connection</param>
        /// <param name="message">The message received</param>
        void handler_ReceiveMessageEvent(AbstractTcpSocketClientHandler handler, AbstractMessage message)
        {
            OnReceiveMessage(handler, message);
        }

        /// <summary>
        /// Override this method to change the raise event on a received message event
        /// </summary>
        /// <param name="handler">The socket client handler of the close connection</param>
        /// <param name="message">The message received</param>
        protected virtual void OnReceiveMessage(AbstractTcpSocketClientHandler handler, AbstractMessage message)
        {
            if (receiveMessageEvent != null)
            {
                receiveMessageEvent(handler, message);
            }
        }

        #endregion

        /// <summary>
        /// Send a message
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns>True if the message has been sent, otherwise false</returns>
        public bool Send(AbstractMessage message)
        {
            if (handler == null)
            {
                return false;
            }
            return handler.Send(message);
        }

        /// <summary>
        /// Send asynchronous message
        /// </summary>
        /// <param name="message">The message to send</param>
        public void SendAsync(AbstractMessage message)
        {
            if (handler == null)
            {
                return;
            }
            handler.SendAsync(message);
        }

        #region IDisposable Members

        /// <summary>
        /// Close and dispose the client.
        /// </summary>
        public void Dispose()
        {
            if (handler != null)
            {
                handler.Close();
                handler.Dispose();
                handler = null;
            }
        }

        #endregion
    }
}
