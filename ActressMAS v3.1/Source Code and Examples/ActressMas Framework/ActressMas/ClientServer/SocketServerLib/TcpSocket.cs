using System;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net;

namespace SocketServerLib.SocketHandler
{
    /// <summary>
    /// This class represents a Socket (not SSL). Incapsulates the system Socket class in order to allow different implementations.
    /// </summary>
    internal class TcpSocket : IDisposable
    {
        /// <summary>
        /// The underlayer 
        /// </summary>
        protected Socket socket = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="socket">The system socket</param>
        public TcpSocket(Socket socket) 
        {
            this.socket = socket;
            Trace.WriteLine(string.Format("Create TcpSocket for EndPoint {0}", this.socket.RemoteEndPoint));
        }

        #region Properties

        /// <summary>
        /// Get or Set the system socket Keep Alive flag
        /// </summary>
        public bool KeepAlive
        {
            get
            {
                return (bool)this.socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive);
            }
            set
            {
                this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, value);
            }
        }

        /// <summary>
        /// Get or Set the system socket send time out
        /// </summary>
        public int SendTimeout
        {
            get
            {
                return (int)this.socket.SendTimeout;
            }
            set
            {
                this.socket.SendTimeout = value;
            }
        }

        /// <summary>
        /// Get the remote end point.
        /// </summary>
        public EndPoint IPEndPoint
        {
            get
            {
                return socket.RemoteEndPoint;
            }
        }

        /// <summary>
        /// Get if the sockect is connected.
        /// </summary>
        public bool Connected
        {
            get
            {
                return this.socket.Connected;
            }
        }

        #endregion

        /// <summary>
        /// Close the socket.
        /// </summary>
        public virtual void Close()
        {
            try
            {
                if (socket != null)
                {
                    if (socket.Connected)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }
                    socket.Close();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception in Close:{0}", ex));
            }
        }

        #region Methods for Receive/Send data

        /// <summary>
        /// Start the asynchronous receiving data. 
        /// </summary>
        /// <param name="state">The socket state object to receive data</param>
        /// <param name="callback">The callback for the asynchronous receiving</param>
        internal virtual void BeginReceive(SocketStateObject state, AsyncCallback callback)
        {
            this.socket.BeginReceive(state.buffer, 0, SocketStateObject.BufferSize, 0, callback, state);
        }

        /// <summary>
        /// Stop the asynchronous receiving.
        /// </summary>
        /// <param name="result">The socket state object to receive data</param>
        /// <returns>The number of bytes received</returns>
        internal virtual int EndReceive(IAsyncResult result)
        {
            int r = 0;
            try
            {
                if (this.socket.Connected)
                {
                    r = this.socket.EndReceive(result);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception in EndReceive:{0}", ex));
            }
            return r;
        }

        /// <summary>
        /// Start the asynchronous sending data
        /// </summary>
        /// <param name="buffer">The buffer to send</param>
        /// <param name="handler">The socket client handler</param>
        /// <param name="callback">The callback for the asynchronous sending</param>
        internal virtual void BeginSend(byte[] buffer, AbstractTcpSocketClientHandler handler, AsyncCallback callback)
        {
            this.socket.BeginSend(buffer, 0, buffer.Length, 0, callback, handler);
        }

        /// <summary>
        /// Stop the asynchronous sending.
        /// </summary>
        /// <param name="result">The socket client handler</param>
        /// <param name="errorCode">The error code in case of error</param>
        /// <returns>The number of bytes received</returns>
        internal virtual int EndSend(IAsyncResult result, out SocketError errorCode)
        {
            int r = 0;
            errorCode = SocketError.Success;
            try
            {
                r = this.socket.EndSend(result, out errorCode);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception in EndSend:{0}", ex));
            }
            return r;
        }

        /// <summary>
        /// Send a buffer. This method is synchronus.
        /// </summary>
        /// <param name="buffer">The buffer to send</param>
        /// <returns>The number of bytes sent</returns>
        internal virtual int Send(byte[] buffer)
        {
            return this.socket.Send(buffer);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Close and dispose the sockect.
        /// </summary>
        public virtual void Dispose()
        {
            try
            {
                this.Close();
                socket = null;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception in Dispose:{0}", ex));
            }
        }

        #endregion
    }
}
