using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Diagnostics;

namespace SocketServerLib.SocketHandler
{
    /// <summary>
    /// This class extends the TcpSocket to define a SSL socket.
    /// </summary>
    internal class SSLSocket : TcpSocket
    {
        /// <summary>
        /// The ssl stream
        /// </summary>
        private SslStream sslStream = null;

        /// <summary>
        /// Construct
        /// </summary>
        /// <param name="socket">The system sockect</param>
        /// <param name="sslStream">The system ssl stream</param>
        public SSLSocket(Socket socket, SslStream sslStream) 
            : base(socket)
        {
            Trace.WriteLine("Create SSLSocket over TcpSocket");
            this.sslStream = sslStream;
        }

        /// <summary>
        /// Close the socket.
        /// </summary>
        public override void Close()
        {
            try
            {
                if (sslStream != null)
                {
                    sslStream.Close();
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
        internal override void BeginReceive(SocketStateObject state, AsyncCallback callback)
        {
            this.sslStream.BeginRead(state.buffer, 0, SocketStateObject.BufferSize, callback, state);
        }

        /// <summary>
        /// Stop the asynchronous receiving.
        /// </summary>
        /// <param name="result">The socket state object to receive data</param>
        /// <returns>The number of bytes received</returns>
        internal override int EndReceive(IAsyncResult result)
        {
            int r = 0;
            try
            {
                if (this.sslStream.CanRead)
                {
                    r = this.sslStream.EndRead(result);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception in EndReceive:{0}", ex));
            }
            return r;
        }

        /// <summary>
        /// Send a buffer. This method is synchronus.
        /// </summary>
        /// <param name="buffer">The buffer to send</param>
        /// <returns>The number of bytes sent</returns>
        internal override int Send(byte[] buffer)
        {
            int n = 0;
            this.sslStream.Write(buffer);
            n = buffer.Length;
            return n;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Close and dispose the socket and the ssl stream.
        /// </summary>
        public override void Dispose()
        {
            try
            {
                this.Close();
                if (sslStream != null)
                {
                    sslStream.Dispose();
                    sslStream = null;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception in Dispose:{0}", ex));
            }
        }

        #endregion
    }
}
