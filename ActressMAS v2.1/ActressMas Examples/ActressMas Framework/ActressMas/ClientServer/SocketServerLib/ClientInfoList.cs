using System;
using System.Collections;
using System.Diagnostics;
using SocketServerLib.SocketHandler;

namespace SocketServerLib.Server
{
    /// <summary>
    /// This class contains a list of client in a Socket Server.
    /// </summary>
    internal class ClientInfoList : IDisposable
    {
        /// <summary>
        /// Hashtable to store the clients
        /// </summary>
        private Hashtable clientList = Hashtable.Synchronized(new Hashtable());

        #region Methods to add/remove a client

        /// <summary>
        /// Add a client to the list.
        /// </summary>
        /// <param name="client">The client to add</param>
        public void AddClient(ClientInfo client)
        {
            lock (this)
            {
                if (!clientList.ContainsKey(client))
                {
                    clientList.Add(client.TcpSocketClientHandler, client);
                    Trace.WriteLine(string.Format("{0} added to the server", client));
                }
            }
        }

        /// <summary>
        /// Remove a client from the list.
        /// </summary>
        /// <param name="client">The client to remove</param>
        public void RemoveClient(ClientInfo client)
        {
            if (client == null)
            {
                return;
            }
            lock (this)
            {
                if (clientList.ContainsKey(client.TcpSocketClientHandler))
                {
                    clientList.Remove(client.TcpSocketClientHandler);
                    Trace.WriteLine(string.Format("{0} removed from the server", client));
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the number of clients.
        /// </summary>
        public virtual int Count
        {
            get
            {
                lock (this)
                {
                    return this.clientList.Count;
                }
            }
        }

        /// <summary>
        /// Get from the client list the Client Info connected to the specific socket client handler.
        /// </summary>
        /// <param name="abstractTcpSocketClientHandler">The socket client handler to find</param>
        /// <returns>The client info of the socket client handler</returns>
        public virtual ClientInfo this[AbstractTcpSocketClientHandler abstractTcpSocketClientHandler]
        {
            get
            {
                lock (this)
                {
                    if (!this.clientList.ContainsKey(abstractTcpSocketClientHandler))
                    {
                        return null;
                    }
                    return (ClientInfo)this.clientList[abstractTcpSocketClientHandler];
                }
            }
        }

        /// <summary>
        /// Get from the client list the Client Info of the specific ClientUID.
        /// </summary>
        /// <param name="clientUID">The client UID to find</param>
        /// <returns>The client info of the client UID</returns>
        public virtual ClientInfo this[string clientUID]
        {
            get
            {
                lock (this)
                {
                    ClientInfo ret = null;
                    foreach (ClientInfo client in this.clientList.Values)
                    {
                        if(client.ClientUID.Equals(clientUID))
                        {
                            ret = client;
                            break;
                        }
                    }
                    return ret;
                }
            }
        }

        #endregion

        /// <summary>
        /// Return a clone of the client list.
        /// </summary>
        /// <returns>A clone of the client list as a Client Info array</returns>
        public ClientInfo[] CloneClientList()
        {
            lock (this)
            {
                ClientInfo[] handlerList = new ClientInfo[clientList.Count];
                clientList.Values.CopyTo(handlerList, 0);
                return handlerList;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Dispose all the contained clients.
        /// </summary>
        public void Dispose()
        {
            lock (this)
            {
                foreach(ClientInfo clientInfo in this.clientList.Values)
                {
                    if (clientInfo.TcpSocketClientHandler != null)
                    {
                        clientInfo.TcpSocketClientHandler.Dispose();
                    }
                }
            }
        }

        #endregion
    }
}
