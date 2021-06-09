using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net.Security;
using SocketServerLib.SocketHandler;
using BasicClientServerLib.Message;
using SocketServerLib.Message;

namespace BasicClientServerLib.SocketHandler
{
    /// <summary>
    /// Basic Socket Client Handler. Implements the AbstractTcpSocketClientHandler class. The message class is BasicMessage.
    /// </summary>
    internal class BasicSocketClientHandlerReceiveQueue : AbstractTcpSocketClientHandler
    {
        /// <summary>
        /// The constructor for SSL connection.
        /// </summary>
        /// <param name="handler">The socket cient handler</param>
        /// <param name="stream">The ssl stream</param>
        public BasicSocketClientHandlerReceiveQueue(Socket handler, SslStream stream)
            : base(handler, stream, true)
        {
        }

        /// <summary>
        /// The constructor for not SSL connection.
        /// </summary>
        /// <param name="handler">The socket cient handler</param>
        public BasicSocketClientHandlerReceiveQueue(Socket handler)
            : base(handler, null, true)
        {
        }

        /// <summary>
        /// Return a BasicMessage empty instance.
        /// </summary>
        /// <returns>The BasicMessage instance</returns>
        protected override AbstractMessage GetMessageInstance()
        {
            return new BasicMessage();
        }
    }
}
