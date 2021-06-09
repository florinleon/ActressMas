using System;
using System.Threading;
using System.Net.Sockets;
using System.Net.Security;
using System.Diagnostics;
using SocketServerLib.Message;

namespace SocketServerLib.SocketHandler
{

    /// <summary>
    /// This abstract class changes the AbstractTcpSocketClientHandler behavior. The asynchronous receiving is by ReceiveAsync method
    /// instead of the Being/EndReceive.
    /// You have to implement the method GetMessageInstance to define in your socket client handler the message class.
    /// </summary>
    internal abstract class AbstractAsyncTcpSocketClientHandler : AbstractTcpSocketClientHandler
    {
        /// <summary>
        /// Object for async read operation.
        /// </summary>
        private SocketAsyncEventArgs readAsyncEventArgs = null;
        /// <summary>
        /// Object for async read state.
        /// </summary>
        private SocketStateObject stateObject = null;

        /// <summary>
        /// Constructor for a socket client handler on SSL
        /// </summary>
        /// <param name="handler">The socket handler</param>
        /// <param name="stream">The ssl stream</param>
        /// <param name="useReceiveQueue">If true the message receiving is throw a queue, otherwise eash message is handle by a different thread</param>
        public AbstractAsyncTcpSocketClientHandler(Socket handler, SslStream stream, bool useReceiveQueue)
            : base(handler, stream, useReceiveQueue)
        {
            // Change the TcpSocket for the non SSL
            if (stream == null)
            {
                this.socket = new TcpSocketAsync(handler);
                this.stateObject = new SocketStateObject();
                this.stateObject.workHandler = this;
                this.readAsyncEventArgs = new SocketAsyncEventArgs();
                this.readAsyncEventArgs.SetBuffer(this.stateObject.buffer, 0, this.stateObject.buffer.Length);
                this.readAsyncEventArgs.UserToken = this;
                this.readAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(socketIOAsyncEvent_Completed);
            }
        }

        /// <summary>
        /// Constructor for a socket client handler on SSL without receive queue
        /// </summary>
        /// <param name="handler">The socket handler</param>
        /// <param name="stream">The ssl stream</param>
        public AbstractAsyncTcpSocketClientHandler(Socket handler, SslStream stream)
            : this(handler, stream, false)
        {
        }

        /// <summary>
        /// Constructor for a socket client handler (not SSL) without receive queue
        /// </summary>
        /// <param name="handler">The socket handler</param>
        public AbstractAsyncTcpSocketClientHandler(Socket handler)
            : this(handler, null, false)
        {
        }

        private void socketIOAsyncEvent_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    break;
            }
        }

        #region Asinchronous receiving

        /// <summary>
        /// Start asynchronous receiving.
        /// </summary>
        internal override void StartReceive()
        {
            bool flagPending = ((TcpSocketAsync)socket).ReceiveAsync(this.readAsyncEventArgs);
            if (!flagPending)
            {
                ProcessReceive(this.readAsyncEventArgs);
            }
        }

        /// <summary>
        /// Callback for asynchronous receiving.
        /// </summary>
        /// <param name="e">The socket state object for receiving data</param>
        private static void ProcessReceive(SocketAsyncEventArgs e)
        {
            AbstractAsyncTcpSocketClientHandler handler = (AbstractAsyncTcpSocketClientHandler)e.UserToken;

            try
            {
                while (true)
                {
                    Trace.WriteLine(string.Format("Receive {0} bytes", e.BytesTransferred));
                    if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                    {
                        int read = e.BytesTransferred;
                        // Fire event for incoming message
                        handler.OnReceivingMessage(handler);
                        while (true)
                        {
                            AbstractMessage message = AbstractMessage.TryReadMessage(handler.GetMessageInstance(), handler.stateObject, read);
                            // Fire event for received message
                            if (message != null)
                            {
                                ReceiveMessageStateObject rcvObj = new ReceiveMessageStateObject() { Handler = handler, Message = message };
                                handler.OnReceiveMessage(rcvObj);
                            }
                            if (handler.stateObject.pendingBuffer == null)
                            {
                                break;
                            }
                            read = 0;
                        }
                        bool flagPending = ((TcpSocketAsync)handler.socket).ReceiveAsync(e);
                        if (flagPending)
                        {
                            break;
                        }
                    }
                    else
                    {
                        Trace.WriteLine(string.Format("SocketError {0}.", e.SocketError));
                        handler.Close();
                        break;
                    }
                }
            }
            catch (MessageException mex)
            {
                // Invalid message
                Trace.WriteLine(string.Format("Exception {0}. Connection will be closed", mex));
                handler.Close();
            }
            catch (SocketException sex)
            {
                if (sex.SocketErrorCode == SocketError.ConnectionReset || sex.SocketErrorCode == SocketError.ConnectionAborted)
                {
                    Trace.WriteLine(string.Format("Socket error for disconnection {0} : {1} : {2}. Client will be disconnected", sex.ErrorCode, sex.SocketErrorCode, sex.Message));
                    handler.stateObject.message = null;
                    handler.stateObject.pendingBuffer = null;
                    handler.Close();
                }
                else
                {
                    Trace.WriteLine(string.Format("Exception {0}. Connection will be closed", sex));
                    handler.stateObject.message = null;
                    handler.stateObject.pendingBuffer = null;
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception {0}. Connection will be closed", ex));
                handler.stateObject.message = null;
                handler.stateObject.pendingBuffer = null;
                handler.Close();
            }
        }
        
        #endregion
        
    }
}
