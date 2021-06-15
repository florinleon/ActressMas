/**************************************************************************
 *                                                                        *
 *  Description: ActressMas multi-agent framework                         *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Copyright:   (c) 2018, Florin Leon                                    *
 *                                                                        *
 *  Using the library for a socket client/server system by                *
 *  Alessandro Lentini (2015): https://www.codeproject.com/Articles/      *
 *  563627/A-full-library-for-a-Socket-Client-Server-system               *
 *                                                                        *
 *  This program is free software; you can redistribute it and/or modify  *
 *  it under the terms of the GNU General Public License as published by  *
 *  the Free Software Foundation. This program is distributed in the      *
 *  hope that it will be useful, but WITHOUT ANY WARRANTY; without even   *
 *  the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR   *
 *  PURPOSE. See the GNU General Public License for more details.         *
 *                                                                        *
 **************************************************************************/

using AsyncClientServerLib.Message;
using AsyncClientServerLib.Server;
using SocketServerLib.Message;
using SocketServerLib.Server;
using SocketServerLib.SocketHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace ActressMas
{
    /// <summary>
    /// A server that ensures the communication of containers, e.g. for the movement of agents, in a distributed system.
    /// </summary>
    public class Server
    {
        private string _containerList;
        private Dictionary<ClientInfo, string> _containerNames;
        private int _count = 0;
        private int _port, _ping;
        private BasicSocketServer _server = null;
        private Guid _serverGuid = Guid.Empty;
        private Timer _timer;
        private static object _locker = new object();

        /// <summary>
        /// Initializes a new instance of the Server class.
        /// </summary>
        /// <param name="port">The port number of the server</param>
        /// <param name="ping">The time interval (in miliseconds) for the ping messages, needed to check if the containers are still alive</param>
        public Server(int port, int ping)
        {
            _port = port;
            _ping = ping; // ms
            _containerNames = new Dictionary<ClientInfo, string>();
            _containerList = "";
        }

        /// <summary>
        /// An event handler for the ongoing messages provided by the server.
        /// </summary>
        public event NewTextEventHandler NewText;

        /// <summary>
        /// Tries to start the server
        /// </summary>
        public void Start()
        {
            try
            {
                _serverGuid = Guid.NewGuid();
                _server = new BasicSocketServer();
                _server.ReceiveMessageEvent += new ReceiveMessageDelegate(server_ReceiveMessageEvent);
                _server.ConnectionEvent += new SocketConnectionDelegate(server_ConnectionEvent);
                _server.CloseConnectionEvent += new SocketConnectionDelegate(server_CloseConnectionEvent);

                _server.Init(new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port));
                _server.StartUp();

                _timer = new Timer();
                _timer.Interval = _ping;
                _timer.Elapsed += _timer_Elapsed;
                _timer.Start();

                RaiseNewTextEvent($"Server started on port {_port}.");
            }
            catch (Exception ex)
            {
                RaiseNewTextEvent($"Exception in Server.Start: {ex.Message}.");
            }
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public void Stop()
        {
            try
            {
                if (_server != null)
                    _server.Dispose();

                RaiseNewTextEvent("Server stopped.");
            }
            catch (Exception ex)
            {
                RaiseNewTextEvent($"Exception in Server.Stop: {ex.Message}.");
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_server == null)
                    return;

                ClientInfo[] clientList = _server.GetClientList();

                if (clientList.Length != 0)
                {
                    string containerListAsString = "";
                    foreach (string cn in _containerNames.Values.ToList())
                    {
                        if (cn.StartsWith("Unknown container")) // ignore container until it registers
                            continue;

                        containerListAsString += cn + " ";
                    }

                    foreach (ClientInfo ci in clientList)
                    {
                        if (_containerNames.ContainsKey(ci))
                        {
                            var m = new ContainerMessage("Server", _containerNames[ci], "Inform Containers", containerListAsString);
                            Send(ci, m);
                        }
                    }

                    DisplayContainerList();
                }
            }
            catch (Exception ex)
            {
                RaiseNewTextEvent($"Exception in Server.TimerElapsed: {ex.Message}.");
            }
        }

        private void DisplayContainerList()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Containers:");

            ClientInfo[] clientList = _server.GetClientList();

            if (clientList.Length != 0)
            {
                foreach (ClientInfo ci in clientList)
                {
                    if (_containerNames.ContainsKey(ci))
                    {
                        if (_containerNames[ci].StartsWith("Unknown "))
                            continue;
                        sb.AppendLine("\t" + _containerNames[ci] + " -> " + ci.ClientUID.ToString().Substring(43));
                    }
                }
            }

            string newContainerList = sb.ToString().Trim();

            if (newContainerList != _containerList)
            {
                _containerList = newContainerList;
                RaiseNewTextEvent(_containerList);
            }
        }

        private void ProcessMessage(ContainerMessage message, ClientInfo sender)
        {
            if (message.Sender == "Server")
                return;

            RaiseNewTextEvent($"Message received [{message.Format()}]");

            if (message.Receiver != "Server" && message.Info == "Request Move Agent")
                RaiseNewTextEvent($"Moving agent from {message.Sender} to {message.Receiver}");

            if (message.Receiver == "Server" && message.Info == "Request Register")
            {
                bool exists = false;

                foreach (string s in _containerNames.Values)
                {
                    if (s == message.Sender)
                    {
                        exists = true;
                        break;
                    }
                }

                if (exists)
                {
                    var m = new ContainerMessage("Server", message.Sender, "Inform Invalid Name", "");
                    Send(sender, m);
                    sender.TcpSocketClientHandler.Close();
                }
                else
                {
                    _containerNames[sender] = message.Sender;
                    DisplayContainerList();
                }
            }
            else if (message.Info == "Request Move Agent" || message.Info == "Send Remote Message")
            {
                foreach (string s in _containerNames.Values)
                {
                    if (s == message.Sender)
                    {
                        ClientInfo[] clientList = _server.GetClientList();

                        if (clientList.Length == 0)
                            return;

                        foreach (ClientInfo ci in clientList)
                        {
                            if (_containerNames.ContainsKey(ci) && _containerNames[ci] == message.Receiver)
                                Send(ci, message);
                        }
                    }
                }
            }
        }

        private void RaiseNewTextEvent(string text)
        {
            if (NewText != null)
            {
                lock (_locker)
                {
                    NewText(this, new NewTextEventArgs(text));
                }
            }
        }

        private void Send(ClientInfo client, ContainerMessage message)
        {
            try
            {
                RaiseNewTextEvent($"Sending message [{message.Format()}]");

                string s = ContainerMessage.Serialize(message);

                byte[] buffer = ASCIIEncoding.Unicode.GetBytes(s);
                var m = new BasicMessage(_serverGuid, buffer);
                client.TcpSocketClientHandler.SendAsync(m);
            }
            catch (SocketException se)
            {
                // "An existing connection was forcibly closed by the remote host"
                RaiseNewTextEvent($"Container {_containerNames[client]} is disconnected.");
                _containerNames.Remove(client);
                client.TcpSocketClientHandler.Close();
                RaiseNewTextEvent($"Exception in Server.Send: {se.Message}.");
            }
            catch (Exception ex)
            {
                RaiseNewTextEvent($"Exception in Server.Send: {ex.Message}.");
            }
        }

        private void server_CloseConnectionEvent(AbstractTcpSocketClientHandler handler)
        {
            try
            {
                foreach (ClientInfo ci in _containerNames.Keys)
                {
                    if (ci.TcpSocketClientHandler == handler)
                    {
                        _containerNames.Remove(ci);
                        DisplayContainerList();
                        RaiseNewTextEvent("Container disconnected from server.");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseNewTextEvent($"Exception in Server.CloseConnectionEvent: {ex.Message}.");
            }
        }

        private void server_ConnectionEvent(AbstractTcpSocketClientHandler handler)
        {
            try
            {
                if (_server == null)
                    return;

                ClientInfo[] clientList = _server.GetClientList();

                if (clientList.Length != 0)
                {
                    _count++;

                    foreach (ClientInfo ci in clientList)
                    {
                        if (!_containerNames.ContainsKey(ci))
                        {
                            _containerNames.Add(ci, "Unknown container " + _count);
                            break;
                        }
                    }
                }

                DisplayContainerList();
                RaiseNewTextEvent("Container connected to server.");
            }
            catch (Exception ex)
            {
                RaiseNewTextEvent($"Exception in Server.ConnectionEvent: {ex.Message}.");
            }
        }

        private void server_ReceiveMessageEvent(AbstractTcpSocketClientHandler handler, AbstractMessage message)
        {
            try
            {
                var receivedMessage = (BasicMessage)message;
                byte[] buffer = receivedMessage.GetBuffer();
                string s = System.Text.ASCIIEncoding.Unicode.GetString(buffer);

                ClientInfo[] clientList = _server.GetClientList();
                if (clientList.Length == 0)
                    return;

                ClientInfo sender = null;

                foreach (ClientInfo ci in clientList)
                    if (ci.TcpSocketClientHandler == handler)
                    {
                        sender = ci;
                        break;
                    }

                var deserializedMessage = ContainerMessage.Deserialize(s) as ContainerMessage;
                if (deserializedMessage != null)
                    ProcessMessage(deserializedMessage, sender);
            }
            catch (Exception ex)
            {
                RaiseNewTextEvent($"Exception in Server.ReceiveMessageEvent: {ex.Message}.");
            }
        }
    }
}