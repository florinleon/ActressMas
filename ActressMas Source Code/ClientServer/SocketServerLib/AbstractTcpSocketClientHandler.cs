using System;
using System.Threading;
using System.Net.Sockets;
using System.Net.Security;
using System.Diagnostics;
using SocketServerLib.Message;
using System.Collections.Concurrent;

namespace SocketServerLib.SocketHandler
{

    #region Defines delegates

    /// <summary>
    /// Defines the delegate for a received message event
    /// </summary>
    /// <param name="handler">The socket client handler where the message has been received</param>
    /// <param name="message">The received message</param>
    internal delegate void ReceiveMessageDelegate(AbstractTcpSocketClientHandler handler, AbstractMessage message);
   
    /// <summary>
    /// Defines the delegate for a connection event
    /// </summary>
    /// <param name="handler">The socket client handler that has been connected</param>
    internal delegate void SocketConnectionDelegate(AbstractTcpSocketClientHandler handler);

    #endregion

    /// <summary>
    /// This abstract class represents a socket client handler with/without a receive queue and a send queue. 
    /// With the receive queue, each received message is saved in a queue and a dequeuer thread
    /// get the messages one by one from the queue and raise the receive message event.
    /// Without the receieve queue, each time you receive a message a thread is created to raise the receive event.
    /// You have to implement the method GetMessageInstance to define in your socket client handler the message class.
    /// </summary>
    internal abstract class AbstractTcpSocketClientHandler : IDisposable
    {
        /// <summary>
        /// The socket
        /// </summary>
        protected TcpSocket socket = null;
        /// <summary>
        /// auto resent events to synch send
        /// </summary>
        private AutoResetEvent sendDone = new AutoResetEvent(true);
        private AutoResetEvent sendWaitHandle = new AutoResetEvent(true);
        /// <summary>
        /// Receive queue
        /// </summary>
        private BlockingCollection<ReceiveMessageStateObject> receiveQueue = null;
        /// <summary>
        /// Cancellation token for the dequeuer from the receive queue
        /// </summary>
        private CancellationTokenSource cancellationToken = null;
        /// <summary>
        /// Flag to stop the dequeuer
        /// </summary>
        private bool flagShutdown = false;
        /// <summary>
        /// Dequeuer thread
        /// </summary>
        private Thread dequeuerThread = null;
        /// <summary>
        /// Delegate for received message event
        /// </summary>
        private ReceiveMessageDelegate receiveMessageEvent = null;
        /// <summary>
        /// Delegate for close connection event
        /// </summary>
        private SocketConnectionDelegate closeConnectionEvent = null;
        /// <summary>
        /// Delegate for incoming message event
        /// </summary>
        private SocketConnectionDelegate inReceivingEvent = null;
        // Lock object for raise event
        private readonly object raiseLock = new object();

        /// <summary>
        /// Constructor for a socket client handler on SSL
        /// </summary>
        /// <param name="handler">The socket handler</param>
        /// <param name="stream">The ssl stream</param>
        /// <param name="useReceiveQueue">If true the message receiving is throw a queue, otherwise each message is handle by a different thread</param>
        public AbstractTcpSocketClientHandler(Socket handler, SslStream stream, bool useReceiveQueue)
            : base()
        {
            if (stream != null)
            {
                this.socket = new SSLSocket(handler, stream);
            }
            else
            {
                this.socket = new TcpSocket(handler);
            }
            // If the socket client handler has to use the receive queue, the necessary classes are created
            if (useReceiveQueue)
            {
                receiveQueue = new BlockingCollection<ReceiveMessageStateObject>();
                cancellationToken = new CancellationTokenSource();
                // The dereive dequeuer is started
                this.dequeuerThread = new Thread(new ThreadStart(this.ReceiveDequeuer));
                this.dequeuerThread.Start();
            }
        }

        /// <summary>
        /// Constructor for a socket client handler on SSL without receive queue
        /// </summary>
        /// <param name="handler">The socket handler</param>
        /// <param name="stream">The ssl stream</param>
        public AbstractTcpSocketClientHandler(Socket handler, SslStream stream)
            : this(handler, stream, false)
        {
        }

        /// <summary>
        /// Constructor for a socket client handler (not SSL) without receive queue
        /// </summary>
        /// <param name="handler">The socket handler</param>
        public AbstractTcpSocketClientHandler(Socket handler)
            : this(handler, null, false)
        {
        }

        /// <summary>
        /// Implement this method to create a socket client handler class.
        /// </summary>
        /// <returns>Return an empty message instance</returns>
        protected abstract AbstractMessage GetMessageInstance();

        #region Events

        /// <summary>
        /// Event for a received message.
        /// </summary>
        public event ReceiveMessageDelegate ReceiveMessageEvent
        {
            add { lock (raiseLock) { receiveMessageEvent += value; } }
            remove { lock (raiseLock) { receiveMessageEvent -= value; } }
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

        
        #endregion

        #region Properties

        /// <summary>
        /// Socket Keep Alive flag
        /// </summary>
        public bool KeepAlive
        {
            get
            {
                return this.socket.KeepAlive;
            }
            set
            {
                this.socket.KeepAlive = value;
            }
        }

        /// <summary>
        /// Socket Send Timeout
        /// </summary>
        public int SendTimeout
        {
            get
            {
                return this.socket.SendTimeout;
            }
            set
            {
                this.socket.SendTimeout = value;
            }
        }
        
        #endregion

        #region Methods to raise events

        /// <summary>
        /// Handle a received message. A received message event is raise but in a new thread in order to not block the main thread.
        /// Override this method to change the behavior.
        /// </summary>
        /// <param name="rcvObj">The received object</param>
        protected virtual void OnReceiveMessage(ReceiveMessageStateObject rcvObj)
        {
            if (this.receiveQueue != null)
            {
                Trace.WriteLine(string.Format("Add Message {0} from Client {1} in the receive queue", rcvObj.Message.MessageUID, rcvObj.Message.ClientUID));
                this.receiveQueue.Add(rcvObj);
                return;
            }
            ThreadPool.QueueUserWorkItem(new WaitCallback(rcvObj.Handler.RaiseReceiveMessageEvent), rcvObj);
        }

        /// <summary>
        /// Raise a received message event. This method is running in the same thread of the caller. 
        /// </summary>
        /// <param name="stateObj">The receive message state object</param>
        protected void RaiseReceiveMessageEvent(object stateObj)
        {
            if (receiveMessageEvent != null)
            {
                ReceiveMessageStateObject rcvObj = (ReceiveMessageStateObject)stateObj;
                receiveMessageEvent(rcvObj.Handler, rcvObj.Message);
            }
        }

        /// <summary>
        /// Handle a close connection. A close connection event is raise but in a new thread in order to not block the main thread.
        /// Override this method to change the behavior.
        /// </summary>
        /// <param name="closedHandler">The socket client handler closed</param>
        protected virtual void OnCloseConnection(AbstractTcpSocketClientHandler closedHandler)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(closedHandler.RaiseCloseConnectionEvent), closedHandler);
        }

        /// <summary>
        /// Raise a received message event. This method is running in the same thread of the caller. 
        /// </summary>
        /// <param name="stateObj">The socket client handler closed</param>
        private void RaiseCloseConnectionEvent(object stateObj)
        {
            if (closeConnectionEvent != null)
            {
                AbstractTcpSocketClientHandler closedHandler = (AbstractTcpSocketClientHandler)stateObj;
                closeConnectionEvent(closedHandler);
            }
        }

        /// <summary>
        /// Raise an incoming message event. This method is running in the same thread of the caller.
        /// </summary>
        /// <param name="handler">The socket client handler that is receiving an incoming message</param>
        protected virtual void OnReceivingMessage(object handler)
        {
            if (inReceivingEvent != null)
            {
                inReceivingEvent((AbstractTcpSocketClientHandler)handler);
            }
        }

        #endregion

        #region Asinchronous receiving

        /// <summary>
        /// Start asynchronous receiving.
        /// </summary>
        internal virtual void StartReceive()
        {
            SocketStateObject state = new SocketStateObject();
            state.workHandler = this;
            socket.BeginReceive(state, new AsyncCallback(ReadCallback));
        }

        /// <summary>
        /// Stop asynchronous receiving.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private int EndReceive(IAsyncResult result)
        {
            if (socket == null)
            {
                return 0;
            }
            return socket.EndReceive(result);
        }

        /// <summary>
        /// Callback for asynchronous receiving.
        /// </summary>
        /// <param name="ar">The socket state object for receiving data</param>
        protected static void ReadCallback(IAsyncResult ar)
        {
            SocketStateObject state = (SocketStateObject)ar.AsyncState;
            AbstractTcpSocketClientHandler handler = state.workHandler;

            try
            {
                // Read data from the client socket.
                int read = handler.EndReceive(ar);
                Trace.WriteLine(string.Format("Receive {0} bytes", read));

                // Data was read from the client socket.
                if (read > 0)
                {
                    // Fire event for incoming message
                    handler.OnReceivingMessage(handler);
                    while (true)
                    {
                        AbstractMessage message = AbstractMessage.TryReadMessage(handler.GetMessageInstance(), state, read);
                        // Fire event for received message
                        if (message != null)
                        {
                            ReceiveMessageStateObject rcvObj = new ReceiveMessageStateObject() { Handler = handler, Message = message };
                            handler.OnReceiveMessage(rcvObj);
                        }
                        if (state.pendingBuffer == null)
                        {
                            break;
                        }
                        read = 0;
                    }
                    handler.socket.BeginReceive(state, new AsyncCallback(ReadCallback));
                }
                else
                {
                    handler.Close();
                }
            }
            catch (MessageException mex)
            {
                // Invalid message
                Trace.WriteLine(string.Format("Exception {0}. Connection will be closed", mex));
                state.message = null;
                state.pendingBuffer = null;
                handler.Close();
            }
            catch (SocketException sex)
            {
                if (sex.SocketErrorCode == SocketError.ConnectionReset || sex.SocketErrorCode == SocketError.ConnectionAborted)
                {
                    Trace.WriteLine(string.Format("Socket error for disconnection {0} : {1} : {2}. Client will be disconnected", sex.ErrorCode, sex.SocketErrorCode, sex.Message));
                    state.message = null;
                    state.pendingBuffer = null;
                    handler.Close();
                }
                else
                {
                    Trace.WriteLine(string.Format("Exception {0}. Connection will be closed", sex));
                    state.message = null;
                    state.pendingBuffer = null;
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception {0}. Connection will be closed", ex));
                state.message = null;
                state.pendingBuffer = null;
                handler.Close();
            }
        }
        
        #endregion

        #region Synchronous send methods

        /// <summary>
        /// Send a buffer. It's a synchronous operation. The previous send has to be completed.
        /// You can define a timeout on the previous send and skip this one in case of time out.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="previousSendTimeout">Timeout on the previous send in ms</param>
        /// <returns>True if the message has been sent, otherwise false</returns>
        public virtual bool Send(AbstractMessage message, int previousSendTimeout)
        {
            return SendMessage(message, previousSendTimeout);
        }

        /// <summary>
        /// Send a buffer. It's a synchronous operation. The previous send has to be completed.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns>True if the message has been sent, otherwise false</returns>
        public virtual bool Send(AbstractMessage message)
        {
            return Send(message, 0);
        }

        /// <summary>
        /// Send a buffer. It's a synchronous operation. The previous send has to be completed.
        /// You can define a timeout on the previous send and skip this one in case of time out.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="previousSendTimeout">Timeout on the previous send in ms</param>
        /// <returns>True if the message has been sent, otherwise false</returns>
        internal bool SendMessage(AbstractMessage message, int previousSendTimeout)
        {
            byte[] buffer = message.GetEnvelope();
            Trace.WriteLine(string.Format("Sending message {0} len {1}", message.MessageUID, message.MessageLength));
            bool r = SendBuffer(this, buffer, previousSendTimeout);
            return r;
        }

        /// <summary>
        /// Send a buffer. It's a synchronous operation. The previous send has to be completed.
        /// You can define a timeout on the previous send and skip this one in case of time out.
        /// </summary>
        /// <param name="clientHandler">The socket client handler for send</param>
        /// <param name="byteData">The buffer to send</param>
        /// <param name="previousSendTimeout">Timeout on the previous send in ms</param>
        /// <returns>True if the message has been sent, otherwise false</returns>
        private static bool SendBuffer(AbstractTcpSocketClientHandler clientHandler, byte[] byteData, int previousSendTimeout)
        {
            bool ret = false;
            bool flagRaiseDisconnection = false;
            bool flagReset = true;
            try
            {
                // Check if client is connected
                if (!clientHandler.socket.Connected)
                {
                    throw new TcpSocketException("Client disconnected", true);
                }
                // Wait for previous send
                if (previousSendTimeout != 0)
                {
                    if (!clientHandler.sendWaitHandle.WaitOne(previousSendTimeout))
                    {
                        throw new TcpSocketException("Send skip for timeout");
                    }
                }
                else
                {
                    clientHandler.sendWaitHandle.WaitOne();
                }
                // Sending the data to the remote device.
                Trace.WriteLine(string.Format("Sending {0} bytes", byteData.Length));
                int n = clientHandler.socket.Send(byteData);
                if (n != byteData.Length)
                {
                    Trace.WriteLine(string.Format("Invalid send: sent {0} bytes on {1} bytes", n, byteData.Length));
                    throw new TcpSocketException("Invalid send");
                }
                ret = true;
            }
            catch (SocketException sex)
            {
                Trace.WriteLine(string.Format("Exception {0}. Connection will be closed", sex));
                flagRaiseDisconnection = true;
            }
            catch (TcpSocketException netEx)
            {
                Trace.WriteLine(string.Format("Exception {0}. Connection will be closed", netEx));
                if (netEx.DisconnectionRequired)
                {
                    flagRaiseDisconnection = true;
                }
                // Not release the sendwaithandle in case of send timeout
                else
                {
                    flagReset = false;
                }
            }
            catch (ObjectDisposedException dex)
            {
                Trace.WriteLine(string.Format("Exception {0}. Connection will be closed", dex));
                flagRaiseDisconnection = true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception {0}.", ex));
            }
            finally
            {
                if (flagReset)
                {
                    Trace.WriteLine("release sendwaithandle");
                    clientHandler.sendWaitHandle.Set();
                }
                if (flagRaiseDisconnection)
                {
                    Trace.WriteLine("Client will be disconnected.");
                    clientHandler.OnCloseConnection(clientHandler);
                }
            }
            return ret;
        }

        #endregion

        #region Asinchronous send methods

        public virtual void SendAsync(AbstractMessage message)
        {
            this.sendDone.WaitOne();
            byte[] buffer = message.GetEnvelope();
            Trace.WriteLine(string.Format("Sending asynch message {0} len {1}", message.MessageUID, message.MessageLength));
            this.socket.BeginSend(buffer, this, SendCallback);
        }

        protected static void SendCallback(IAsyncResult ar)
        {
            AbstractTcpSocketClientHandler handler = null;
            try
            {
                SocketError errorCode = SocketError.Success;
                // Retrieve the socket from the state object.
                handler = (AbstractTcpSocketClientHandler)ar.AsyncState;
                // Complete sending the data to the remote device.
                int bytesSent = handler.socket.EndSend(ar, out errorCode);
                Trace.WriteLine(string.Format("Complete asynch send. Byte {0} sent.", bytesSent));
            }
            catch (SocketException sex)
            {
                Trace.WriteLine(string.Format("Socket error, the connection will be closed. Error {0} : {1} : {2}", sex.ErrorCode, sex.SocketErrorCode, sex.Message));
                handler.OnCloseConnection(handler);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception during asynch send {0}", ex));
            }
            finally
            {
                // Signal that all bytes have been sent.
                handler.sendDone.Set();
            }
        }

        #endregion

        #region Receive Dequeuer

        /// <summary>
        /// Dequeuer. Read the receive queue and raise the receive event for each message
        /// </summary>
        /// <param name="stateObj">The received message</param>
        protected virtual void ReceiveDequeuer()
        {
            while (!this.flagShutdown)
            {
                ReceiveMessageStateObject message = null;
                try
                {
                    message = receiveQueue.Take(this.cancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    Trace.WriteLine("A cancel operation was request. A shutdown will be done.");
                    this.flagShutdown = true;
                }
                if (flagShutdown)
                {
                    break;
                }
                if (message != null)
                {
                    Trace.WriteLine(string.Format("Remove Message {0} Client {1} from the receive queue and raise event", message.Message.MessageUID, message.Message.ClientUID));
                    message.Handler.RaiseReceiveMessageEvent(message);
                }
            }
        }

        #endregion

        /// <summary>
        /// Close the socket.
        /// </summary>
        public virtual void Close()
        {
            if (this.receiveQueue != null)
            {
                this.flagShutdown = true;
                this.cancellationToken.Cancel();
                this.dequeuerThread.Join();
            }
            if (this.socket != null && this.socket.Connected)
            {
                this.socket.Close();
                this.OnCloseConnection(this);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Close and dispose the socket client handler.
        /// </summary>
        public void Dispose()
        {
            this.Close();
            if (this.receiveQueue != null)
            {
                this.receiveQueue.Dispose();
                this.receiveQueue = null;
            }
            if (this.cancellationToken != null)
            {
                this.cancellationToken.Dispose();
                this.cancellationToken = null;
            }
            if (this.socket != null)
            {
                socket.Dispose();
                socket = null;
            }
        }

        #endregion

        /// <summary>
        /// Print on a string the basic information of the socket client handler - the remote end point.
        /// </summary>
        /// <returns>The string with the information</returns>
        public override string ToString()
        {
            if (this.socket == null)
            {
                return string.Format("{0} with IPEndPoint = none", this.GetType().Name);
            }
            return string.Format("{0} with IPEndPoint = {1}", this.GetType().Name, this.socket.IPEndPoint.ToString());
        }
    }
}
