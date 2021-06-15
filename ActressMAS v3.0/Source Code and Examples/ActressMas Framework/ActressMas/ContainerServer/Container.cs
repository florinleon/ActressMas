/**************************************************************************
 *                                                                        *
 *  Description: ActressMas multi-agent framework                         *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Copyright:   (c) 2018-2021, Florin Leon                               *
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

using AsyncClientServerLib.Client;
using AsyncClientServerLib.Message;
using SocketServerLib.Message;
using SocketServerLib.SocketHandler;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace ActressMas
{
    /// <summary>
    /// A container contains an environment and is connected to a server. It facilitates the move of agents in a distributed system.
    /// </summary>
    public class Container
    {
        private List<string> _allContainers;
        private BasicSocketClient _client = null;
        private Guid _clientGuid = Guid.Empty;
        private EnvironmentMas _environment;
        private string _name;
        private string _serverIP;
        private int _serverPort;
        private static object _locker = new object();

        /// <summary>
        /// Initializes a new instance of the Container class.
        /// </summary>
        /// <param name="serverIP">The IP address of the server</param>
        /// <param name="serverPort">The port number of the server</param>
        /// <param name="name">The name of the container. The name of the container should be unique and cannot contain spaces.</param>
        public Container(string serverIP, int serverPort, string name)
        {
            _serverIP = serverIP;
            _serverPort = serverPort;
            _name = name;

            _allContainers = new List<string>();
        }

        /// <summary>
        /// An event handler for the ongoing messages provided by the container.
        /// </summary>
        public event NewTextEventHandler NewText;

        /// <summary>
        /// The name of the container. If the container is not connected to the server,
        /// this method will return the empty string.
        /// </summary>
        public string Name =>
            _client != null ? _name : "";

        /// <summary>
        /// Returns a list with the names of all the containers in the distributed system. This list may change over time,
        /// as some new containers may get connected and existing ones may disconnect.
        /// </summary>
        public List<string> AllContainers() =>
            _allContainers;

        /// <summary>
        /// Starts the execution of the multiagent system defined in the environment.
        /// </summary>
        /// <param name="environment">The multiagent environment</param>
        /// <param name="mas">The multiagent system to be executed</param>
        public void RunMas(EnvironmentMas environment, RunnableMas mas)
        {
            _environment = environment;
            _environment.SetContainer(this);

            RaiseNewTextEvent("Running MAS...\r\n");

            var t = new Thread(new ThreadStart(() => mas.RunMas(environment)));
            t.Start();
        }

        /// <summary>
        /// Tries to connect to the server and activates the container.
        /// </summary>
        public void Start()
        {
            try
            {
                _clientGuid = Guid.NewGuid();
                _client = new BasicSocketClient();
                _client.ReceiveMessageEvent += new ReceiveMessageDelegate(client_ReceiveMessageEvent);
                _client.ConnectionEvent += new SocketConnectionDelegate(client_ConnectionEvent);
                _client.CloseConnectionEvent += new SocketConnectionDelegate(client_CloseConnectionEvent);

                if (_name.Contains(" "))
                {
                    RaiseNewTextEvent("Do not use space in container name!\r\n");
                    return;
                }

                var ipEndPoint = new IPEndPoint(IPAddress.Parse(_serverIP), _serverPort);

                RaiseNewTextEvent("Container connecting to server...");

                _client.Connect(ipEndPoint);
            }
            catch (Exception ex)
            {
                RaiseNewTextEvent($"Exception: {ex.Message}.");
            }
        }

        /// <summary>
        /// Disconnects from the server and deactivates the container.
        /// </summary>
        public void Stop()
        {
            if (_client != null)
            {
                _client.Close();
                _client.Dispose();
                _client = null;
            }
        }

        internal void AgentHasArrived(string agentState)
        {
            var state = ContainerMessage.Deserialize(agentState) as AgentState;
            _environment.AgentHasArrived(state);
        }

        internal void RemoteMessageReceived(string content)
        {
            var message = ContainerMessage.Deserialize(content) as Message;
            _environment.RemoteMessageReceived(message);
        }

        internal void MoveAgent(AgentState state, string destination)
        {
            string serializedAgentState = ContainerMessage.Serialize(state);
            var cm = new ContainerMessage(_name, destination, "Request Move Agent", serializedAgentState);
            Send(cm);
        }

        internal void SendRemoteAgentMessage(string receiverContainer, Message message)
        {
            message.Sender = $"{message.Sender}@{Name}";
            string serializedMessage = ContainerMessage.Serialize(message);
            var cm = new ContainerMessage(_name, receiverContainer, "Send Remote Message", serializedMessage);
            Send(cm);
        }

        private void client_CloseConnectionEvent(AbstractTcpSocketClientHandler handler)
        {
            try
            {
                RaiseNewTextEvent("Container disconnected from server.");
            }
            catch (Exception ex)
            {
                RaiseNewTextEvent(ex.Message);
            }
        }

        private void client_ConnectionEvent(AbstractTcpSocketClientHandler handler)
        {
            try
            {
                RaiseNewTextEvent("Container connected to server.");
                Register();
                RaiseNewTextEvent($"{_name} registered.");
            }
            catch (Exception ex)
            {
                RaiseNewTextEvent(ex.Message);
            }
        }

        private void client_ReceiveMessageEvent(AbstractTcpSocketClientHandler handler, AbstractMessage message)
        {
            try
            {
                var receivedMessage = (BasicMessage)message;
                byte[] buffer = receivedMessage.GetBuffer();
                string s = ASCIIEncoding.Unicode.GetString(buffer);

                if (ContainerMessage.Deserialize(s) is ContainerMessage deserializedMessage)
                    ProcessMessage(deserializedMessage);
            }
            catch (Exception ex)
            {
                RaiseNewTextEvent(ex.Message);
            }
        }

        private void ProcessMessage(ContainerMessage message)
        {
            RaiseNewTextEvent($"Message received [{message.Format()}]");

            if (message.Sender == "Server" && message.Info == "Inform Invalid Name")
            {
                RaiseNewTextEvent("Container name refused by server.\r\n");
            }
            else if (message.Sender == "Server" && message.Info == "Inform Containers")
            {
                string[] toks = message.Content.Split();
                _allContainers.Clear();
                foreach (string t in toks)
                {
                    if (t != "" && t != " ")
                        _allContainers.Add(t);
                }
            }
            else if (message.Info == "Request Move Agent")
            {
                AgentHasArrived(message.Content);
            }
            else if (message.Info == "Send Remote Message")
            {
                RemoteMessageReceived(message.Content);
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

        private void Register()
        {
            var message = new ContainerMessage(_name, "Server", "Request Register", "");
            Send(message);
        }

        private void Send(ContainerMessage message)
        {
            try
            {
                RaiseNewTextEvent($"Sending message [{message.Format()}]");

                string s = ContainerMessage.Serialize(message);
                byte[] buffer = ASCIIEncoding.Unicode.GetBytes(s);
                var serializedMessage = new BasicMessage(_clientGuid, buffer);
                _client.SendAsync(serializedMessage);
            }
            catch (Exception ex)
            {
                RaiseNewTextEvent(ex.Message);
            }
        }
    }
}