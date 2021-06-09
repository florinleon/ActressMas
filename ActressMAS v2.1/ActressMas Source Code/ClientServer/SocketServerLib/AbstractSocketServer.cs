using System;
using SocketServerLib.SocketHandler;
using System.Security.Cryptography.X509Certificates;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Diagnostics;
using System.Net.Security;
using SocketServerLib.Message;
using System.Security.Authentication;
using SocketServerLib.Threads;

namespace SocketServerLib.Server
{
    /// <summary>
    /// This abstract class represent a Socket Server.
    /// Implements the method GetHandler to define the socket client handler used in your Socket Server.
    /// </summary>
    internal abstract class AbstractSocketServer : AbstractThread
    {
        /// <summary>
        /// Server certificate for the SSL
        /// </summary>
        private X509Certificate2 serverCertificate = null;
        /// <summary>
        /// Socket listener
        /// </summary>
        private Socket listener = null;
        /// <summary>
        /// TcpListener for SSL
        /// </summary>
        private TcpListener sslListener = null;
        /// <summary>
        /// The thread notification for complete a connection on listener
        /// </summary>
        private ManualResetEvent listenerCompleteConnectionEvent = new ManualResetEvent(false);
        /// <summary>
        /// The Client List
        /// </summary>
        private ClientInfoList clientList = new ClientInfoList();
        /// <summary>
        /// Delegate for a receive message event
        /// </summary>
        private ReceiveMessageDelegate receiveMessageEvent = null;
        /// <summary>
        /// Delegate for a new connection event
        /// </summary>
        private SocketConnectionDelegate connectionEvent = null;
        /// <summary>
        /// Delegate for a close connection event
        /// </summary>
        private SocketConnectionDelegate closeConnectionEvent = null;
        // Lock object for raise event
        private readonly object raiseLock = new object();
        /// <summary>
        /// Keep Alive
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public AbstractSocketServer()
        {
            this.KeepAlive = false;
        }

        #region Properties

        public virtual ClientInfo this[AbstractTcpSocketClientHandler abstractTcpSocketClientHandler]
        {
            get
            {
                return this.clientList[abstractTcpSocketClientHandler];
            }
        }

        public virtual ClientInfo this[string clientUID]
        {
            get
            {
                return this.clientList[clientUID];
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event for receive a message.
        /// </summary>
        public event ReceiveMessageDelegate ReceiveMessageEvent
        {
            add { lock (raiseLock) { receiveMessageEvent += value; } }
            remove { lock (raiseLock) { receiveMessageEvent -= value; } }
        }

        /// <summary>
        /// Event for a new connection.
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
        
        #endregion

        #region Methods for Init

        /// <summary>
        /// Init a listener for a SSL communication.
        /// </summary>
        /// <param name="endPoint">The listener IP/Port</param>
        /// <param name="serverCertificateFilename">The server certification file</param>
        /// <param name="certificatePassword">The server certification password</param>
        public void Init(IPEndPoint endPoint, string serverCertificateFilename, string certificatePassword)
        {
            serverCertificate = new X509Certificate2(serverCertificateFilename, certificatePassword);
            Trace.WriteLine(string.Format("Init ssl listener on {0}", endPoint));
            sslListener = new TcpListener(endPoint);
            sslListener.Start();
            Trace.WriteLine(string.Format("Start ssl listener on {0}", endPoint));
            Init();
        }

        /// <summary>
        /// Init a lister for a not SSL communication
        /// </summary>
        /// <param name="endPoint">The listener IP/Port</param>
        public void Init(IPEndPoint endPoint)
        {
            Trace.WriteLine(string.Format("Init listener on {0}", endPoint));
            listener = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(endPoint);
            listener.Listen(10);
            Trace.WriteLine(string.Format("Start listener on {0}", endPoint));
            Init();
        }

        #endregion

        #region Accept incoming connection

        /// <summary>
        /// The thread loop is used to accecpt new incoming connection. The Accept method is asynchronous.
        /// </summary>
        protected override void ThreadLoop()
        {
            listenerCompleteConnectionEvent.Reset();
            try
            {
                // Check if it's a not SSL listener
                if (listener != null)
                {
                    Trace.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), this);
                }
                // Check if it's a SSL listener
                else if (sslListener != null)
                {
                    Trace.WriteLine("Waiting for a ssl connection...");
                    sslListener.BeginAcceptTcpClient(new AsyncCallback(AcceptSSLCallback), this);
                }
                else
                {
                    Trace.WriteLine("None listener is started");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Failed to start listener on {0}. Exception {1}", listener.LocalEndPoint, ex));
                // Set semaphore to go back to the start of thread loop and wait for a incoming connection
                listenerCompleteConnectionEvent.Set();
            }
            listenerCompleteConnectionEvent.WaitOne();
        }

        /// <summary>
        /// Accept callback method for not SSL connection
        /// </summary>
        /// <param name="ar">The socket server</param>
        private void AcceptCallback(IAsyncResult ar)
        {
            AbstractSocketServer server = (AbstractSocketServer)ar.AsyncState;
            try
            {
                // Get the socket that handles the client request.
                Socket handler = server.listener.EndAccept(ar);
                Trace.WriteLine("Start incoming connection ...");
                handler.Blocking = true;
                AbstractTcpSocketClientHandler clientHandler = this.GetHandler(handler, null);
                clientHandler.KeepAlive = this.KeepAlive;
                clientHandler.ReceiveMessageEvent += new ReceiveMessageDelegate(server.OnReceiveMessage);
                clientHandler.CloseConnectionEvent += new SocketConnectionDelegate(server.clientHandler_CloseConnectionEvent);
                // Add the clilent to the client list
                server.clientList.AddClient(this.GetClientInfo(clientHandler));
                // Notify the connection event
                server.OnConnection(clientHandler);
                // Start to receive data from this client
                clientHandler.StartReceive();
                Trace.WriteLine("New connection completed");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Failed to accept incoming connection. Exception", ex));
            }
            finally
            {
                // Signal the main thread to continue.
                server.listenerCompleteConnectionEvent.Set();
            }
        }

        /// <summary>
        /// Accept callback method for SSL connection.
        /// </summary>
        /// <param name="ar">The socket server</param>
        private void AcceptSSLCallback(IAsyncResult ar)
        {
            AbstractSocketServer server = (AbstractSocketServer)ar.AsyncState;
            SslStream sslStream = null;
            try
            {
                // Get the socket that handles the client request.
                TcpClient sslTcpClient = server.sslListener.EndAcceptTcpClient(ar);
                Trace.WriteLine("Start incoming ssl connection ...");
                sslStream = new SslStream(sslTcpClient.GetStream(), false, new RemoteCertificateValidationCallback(server.OnVerifyClientCertificate));
                sslStream.AuthenticateAsServer(server.serverCertificate, true, SslProtocols.Ssl3, false);
                Socket handler = sslTcpClient.Client;
                handler.Blocking = true;
                AbstractTcpSocketClientHandler clientHandler = this.GetHandler(handler, sslStream);
                clientHandler.KeepAlive = this.KeepAlive;
                clientHandler.ReceiveMessageEvent += new ReceiveMessageDelegate(server.OnReceiveMessage);
                clientHandler.CloseConnectionEvent += new SocketConnectionDelegate(server.clientHandler_CloseConnectionEvent);
                server.OnConnection(clientHandler);
                server.clientList.AddClient(this.GetClientInfo(clientHandler));
                clientHandler.StartReceive();
                Trace.WriteLine("New connection completed");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Failed to accept incoming connection. Exception", ex));
                try
                {
                    if (sslStream != null)
                    {
                        sslStream.Close();
                        sslStream.Dispose();
                    }
                }
                catch (Exception ex2)
                {
                    Trace.WriteLine(ex2);
                }
            }
            finally
            {
                // Signal the main thread to continue.
                server.listenerCompleteConnectionEvent.Set();
            }
        }

        #endregion

        /// <summary>
        /// Implement this method to return a socket client handler instance
        /// </summary>
        /// <param name="handler">The socket handler</param>
        /// <param name="sslStream">The ssl stream</param>
        /// <param name="sendHandleTimeout">The send time out</param>
        /// <param name="socketSendTimeout">The socket send time out</param>
        /// <returns></returns>
        protected abstract AbstractTcpSocketClientHandler GetHandler(Socket handler, SslStream sslStream);

        /// <summary>
        /// Return a clones client list.
        /// </summary>
        /// <returns>A cloned client list</returns>
        public ClientInfo[] GetClientList()
        {
            return this.clientList.CloneClientList();
        }

        /// <summary>
        /// Override this method to change the instace used for the Client Info
        /// </summary>
        /// <param name="abstractTcpSocketClientHandler">The socket client handler for the cient info</param>
        /// <returns>The client info contains the socket clilent handler</returns>
        protected virtual ClientInfo GetClientInfo(AbstractTcpSocketClientHandler abstractTcpSocketClientHandler)
        {
            return new ClientInfo(abstractTcpSocketClientHandler);
        }

        /// <summary>
        /// Override this method to chage the vVerification of the client certificate
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="certificate">The cliet certificate</param>
        /// <param name="chain">The chain</param>
        /// <param name="sslPolicyErrors">The policy</param>
        /// <returns></returns>
        protected virtual bool OnVerifyClientCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            lock (this)
            {
                if (sslPolicyErrors != SslPolicyErrors.None)
                {
                    return false;
                }
                return true;
            }
        }

        #region Methods to raise events

        /// <summary>
        /// Raise the close connection event.
        /// </summary>
        /// <param name="handler">The socket client handler disconnected</param>
        protected virtual void clientHandler_CloseConnectionEvent(AbstractTcpSocketClientHandler handler)
        {
            clientList.RemoveClient(clientList[handler]);
            if (closeConnectionEvent != null)
            {
                closeConnectionEvent(handler);
            }
        }

        /// <summary>
        /// Raise the receive message event.
        /// </summary>
        /// <param name="handler">The socket client handler for the received message</param>
        /// <param name="abstractMessage">The message received</param>
        protected virtual void OnReceiveMessage(AbstractTcpSocketClientHandler handler, AbstractMessage abstractMessage)
        {
            if (receiveMessageEvent != null)
            {
                receiveMessageEvent(handler, abstractMessage);
            }
        }

        /// <summary>
        /// Raise the new conection event.
        /// </summary>
        /// <param name="handler">The socket client handler connected</param>
        protected virtual void OnConnection(AbstractTcpSocketClientHandler handler)
        {
            if (connectionEvent != null)
            {
                connectionEvent(handler);
            }
        }

        #endregion

        #region Thread methods overridden

        /// <summary>
        /// Shutdown the server and stop the main thread.
        /// </summary>
        public override void Shutdown()
        {
            Trace.WriteLine("Stop listener");
            // Check if it's a not SSL listener
            if (this.listener != null)
            {
                this.listener.Close();
                this.listener.Dispose();
                this.listener = null;
            }
            // Check if it's a SSL listener
            if (this.sslListener != null)
            {
                this.sslListener.Stop();
                this.sslListener = null;
            }
            Trace.WriteLine("Disconnect pending client...");
            foreach (ClientInfo clientInfo in this.clientList.CloneClientList())
            {
                try
                {
                    clientInfo.TcpSocketClientHandler.Close();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("Exception in disconnet pending client. Exception {0}", ex));
                }
            }
            Trace.WriteLine("All pending client are disconnected");
            base.Shutdown();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            this.clientList.Dispose();
            base.Dispose();
        }

        #endregion

    }
}
