using System;
using SocketServerLib.SocketHandler;

namespace SocketServerLib.Server
{
    /// <summary>
    /// This is the class for a Client Info. Extend this class to add new properties.
    /// </summary>
    internal class ClientInfo : IDisposable
    {
        /// <summary>
        /// The socket client handler connected as client
        /// </summary>
        protected AbstractTcpSocketClientHandler abstractTcpSocketClientHandler = null;
        /// <summary>
        /// The string for the client identification. For default is abstractTcpSocketClientHandler.ToString()
        /// </summary>
        protected string clientUID = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="abstractTcpSocketClientHandler">The socket client handler</param>
        public ClientInfo(AbstractTcpSocketClientHandler abstractTcpSocketClientHandler)
        {
            this.abstractTcpSocketClientHandler = abstractTcpSocketClientHandler;
            this.clientUID = this.abstractTcpSocketClientHandler.ToString();
        }

        #region Properties

        /// <summary>
        /// Get the underlayer socket client handler.
        /// </summary>
        public AbstractTcpSocketClientHandler TcpSocketClientHandler
        {
            get
            {
                return this.abstractTcpSocketClientHandler;
            }
        }

        /// <summary>
        /// Get the cient UID.
        /// </summary>
        public string ClientUID
        {
            get
            {
                return this.clientUID;
            }
        }

        #endregion

        /// <summary>
        /// Print on string the information.
        /// </summary>
        /// <returns>The client information</returns>
        public override string ToString()
        {
            if (this.abstractTcpSocketClientHandler == null)
            {
                return "Client Info: null";
            }
            return string.Format("Client Info: {0}", this.clientUID);
        }

        #region IDisposable Members

        /// <summary>
        /// Dispose the client.
        /// </summary>
        public void Dispose()
        {
            if (this.abstractTcpSocketClientHandler == null)
            {
                return;
            }
            this.abstractTcpSocketClientHandler.Dispose();
            this.abstractTcpSocketClientHandler = null;
        }

        #endregion
    }
}
