using System;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net;

namespace SocketServerLib.SocketHandler
{
    /// <summary>
    /// This class represents a Socket (not SSL). Incapsulates the system Socket class in order to allow different implementations.
    /// </summary>
    internal class TcpSocketAsync : TcpSocket
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="socket">The system socket</param>
        public TcpSocketAsync(Socket socket) 
            : base(socket)
        {
            this.socket = socket;
            Trace.WriteLine("Create SSLSocket over TcpSocket");
            Trace.WriteLine(string.Format("Create TcpSocketAsync instead of TcpSocket for EndPoint {0}", this.socket.RemoteEndPoint));
        }

        #region Methods for Receive/Send data

        /// <summary>
        /// Start the asynchronous receiving data. 
        /// </summary>
        /// <param name="state">The socket state object to receive data</param>
        /// <param name="callback">The callback for the asynchronous receiving</param>
        internal override void BeginReceive(SocketStateObject state, AsyncCallback callback)
        {
            throw new Exception("Method not valid for this class.");
        }

        /// <summary>
        /// Stop the asynchronous receiving.
        /// </summary>
        /// <param name="result">The socket state object to receive data</param>
        /// <returns>The number of bytes received</returns>
        internal override int EndReceive(IAsyncResult result)
        {
            throw new Exception("Method not valid for this class.");
        }

        /// <summary>
        /// Receiving asynchronous data. 
        /// </summary>
        /// <param name="e">The SocketAsyncEventArgs</param>
        internal virtual bool ReceiveAsync(SocketAsyncEventArgs e)
        {
            return this.socket.ReceiveAsync(e);
        }

        #endregion

    }
}
