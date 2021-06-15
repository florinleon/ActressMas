/**************************************************************************
 *                                                                        *
 *  Description: ActressMas multi-agent framework                         *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Copyright:   (c) 2018-2021, Florin Leon                               *
 *                                                                        *
 *  This program is free software; you can redistribute it and/or modify  *
 *  it under the terms of the GNU General Public License as published by  *
 *  the Free Software Foundation. This program is distributed in the      *
 *  hope that it will be useful, but WITHOUT ANY WARRANTY; without even   *
 *  the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR   *
 *  PURPOSE. See the GNU General Public License for more details.         *
 *                                                                        *
 **************************************************************************/

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ActressMas
{
    /// <summary>
    /// The base class for an agent that runs on a turn-based manner in its environment. You must create your own agent classes derived from this abstract class.
    /// </summary>
    public abstract class Agent
    {
        internal bool MustRunSetup;
        private ConcurrentQueue<Message> _messages;

        public Agent()
        {
            _messages = new ConcurrentQueue<Message>();
            MustRunSetup = true;
            Observables = new Dictionary<string, string>();
            UsingObservables = false;
        }

        /// <summary>
        /// The environment in which the agent runs.
        /// </summary>
        public EnvironmentMas Environment { get; set; }

        /// <summary>
        /// The name of the agent. Each agent must have a unique name in its environment. Most operations are performed using agent names rather than agent objects.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The properties of an agent which can be visible from the outside, i.e. perceivable by other agents.
        /// </summary>
        public Dictionary<string, string> Observables { get; set; }

        /// <summary>
        /// Whether the agent uses the observable feature. The default value is false and it must be explicitly set to true before using observables.
        /// </summary>
        public bool UsingObservables { get; set; }

        /// <summary>
        /// This is the method that is called when the agent receives a message and is activated. This is where the main logic of the agent should be placed.
        /// </summary>
        /// <param name="message">The message that the agent has received and should respond to</param>
        public virtual void Act(Message message)
        {
        }

        /// <summary>
        /// This is the method that is called when the agent does not receive any messages at the end of a turn.
        /// </summary>
        public virtual void ActDefault()
        {
        }

        /// <summary>
        /// Sends a message to all the agents in the environment.
        /// </summary>
        /// <param name="content">The content of the message</param>
        /// <param name="includeSender">Whether the sender itself receives the message or not</param>
        /// <param name="conversationId">A conversation identifier, for the cases when a conversation involves multiple messages that refer to the same topic</param>
        public void Broadcast(string content, bool includeSender = false, string conversationId = "")
        {
            List<string> receivers = Environment.AllAgents();

            if (includeSender == false)
                receivers.Remove(Name);

            foreach (string a in receivers)
                Send(a, content, conversationId);
        }

        /// <summary>
        /// Sends a message to all the agents in the environment.
        /// </summary>
        /// <param name="contentObj">The content of the message</param>
        /// <param name="includeSender">Whether the sender itself receives the message or not</param>
        /// <param name="conversationId">A conversation identifier, for the cases when a conversation involves multiple messages that refer to the same topic</param>
        public void Broadcast(dynamic contentObj, bool includeSender = false, string conversationId = "")
        {
            List<string> receivers = Environment.AllAgents();

            if (includeSender == false)
                receivers.Remove(Name);

            foreach (string a in receivers)
                Send(a, contentObj, conversationId);
        }

        /// <summary>
        /// Tests whether the agent can move to a certain remote container.
        /// </summary>
        /// <param name="destination">The name of the container that the agent wants to move to</param>
        public bool CanMove(string destination) =>
            Environment.ContainerName != "" && destination != Environment.ContainerName && Environment.AllContainers().Contains(destination);

        /// <summary>
        /// Imports the state of the agent, after it has moved from another container.
        /// </summary>
        /// <param name="state">The state of the agent</param>
        public virtual void LoadState(AgentState state)
        {
        }

        /// <summary>
        /// The method that should be called when the agent wants to move to a different container.
        /// </summary>
        /// <param name="destination">The name of the container that the agent wants to move to</param>
        public void Move(string destination)
        {
            AgentState state = SaveState();
            state.AgentType = GetType();
            state.Name = Name;

            Environment.MoveAgent(state, destination);
        }

        /// <summary>
        /// The function that identifies which properties and conditions must be satisfied by the Observables of other agents
        /// in order to be perceived by the observing agent. It must return true for the observables that will be available to the agent.
        /// </summary>
        /// <param name="observed">A dictionary with name-value pairs of observed properties</param>
        public virtual bool PerceptionFilter(Dictionary<string, string> observed) =>
            false; // default: no observables

        /// <summary>
        /// Exports the state of the agent, so it can be serialized when moving to another container.
        /// </summary>
        public virtual AgentState SaveState() =>
            null;

        /// <summary>
        /// This method provides the agents whose observable properties are visible. It is called once a turn, before Act.
        /// </summary>
        /// <param name="observableAgents">The list of agents which have at least one observable property desired by the observing agent. The desired properties are also available, from the ObservableAgent objects.</param>
        public virtual void See(List<ObservableAgent> observableAgents)
        {
        }

        /// <summary>
        /// Sends a message to a specific agent, identified by name.
        /// </summary>
        /// <param name="receiver">The agent that will receive the message. If the agent is in another container, use: agent@container</param>
        /// <param name="content">The content of the message</param>
        /// <param name="conversationId">A conversation identifier, for the cases when a conversation involves multiple messages that refer to the same topic</param>
        public void Send(string receiver, string content, string conversationId = "")
        {
            if (!receiver.Contains("@"))
            {
                var message = new Message(Name, receiver, content, conversationId);
                Environment.Send(message);
            }
            else // remote message
            {
                string[] toks = receiver.Split('@');
                string container = toks[1];
                var message = new Message(Name, receiver, content, conversationId);
                Environment.SendRemote(container, message);
            }
        }

        /// <summary>
        /// Sends a message to a specific agent, identified by name.
        /// </summary>
        /// <param name="receiver">The agent that will receive the message</param>
        /// <param name="contentObj">The content of the message</param>
        /// <param name="conversationId">A conversation identifier, for the cases when a conversation involves multiple messages that refer to the same topic</param>
        public void Send(string receiver, dynamic contentObj, string conversationId = "")
        {
            if (receiver.Contains("@"))
                throw new System.Exception("Sending objects to remote agents is not supported. Serialize the object and send it as a string.");

            var message = new Message(Name, receiver, contentObj, conversationId);
            Environment.Send(message);
        }

        /// <summary>
        /// Sends a message to a specific set of agents, identified by name.
        /// </summary>
        /// <param name="receivers">The list of agents that will receive the message</param>
        /// <param name="content">The content of the message</param>
        /// <param name="conversationId">A conversation identifier, for the cases when a conversation involves multiple messages that refer to the same topic</param>
        public void SendToMany(List<string> receivers, string content, string conversationId = "")
        {
            foreach (string a in receivers)
                Send(a, content, conversationId);
        }
        /// <summary>
        /// Sends a message to a specific set of agents, identified by name.
        /// </summary>
        /// <param name="receivers">The list of agents that will receive the message</param>
        /// <param name="contentObj">The content of the message</param>
        /// <param name="conversationId">A conversation identifier, for the cases when a conversation involves multiple messages that refer to the same topic</param>
        public void SendToMany(List<string> receivers, dynamic contentObj, string conversationId = "")
        {
            foreach (string a in receivers)
                Send(a, contentObj, conversationId);
        }

        /// <summary>
        /// This method is called as the first turn or right after an agent has moved to a new container.
        /// It is similar to the constructor of the class, but it may be used for agent-related logic, e.g. for sending initial message(s).
        /// </summary>
        public virtual void Setup()
        {
        }

        /// <summary>
        /// Stops the execution of the agent and removes it from the environment. Use the Stop method instead of Environment.Remove
        /// when the decision to be stopped belongs to the agent itself.
        /// </summary>
        public void Stop() =>
            Environment.Remove(this);

        internal virtual void InternalAct()
        {
            if (_messages.Count > 0)
            {
                while (_messages.Count > 0)
                {
                    bool ok = _messages.TryDequeue(out Message m);
                    if (ok)
                        Act(m);
                }
            }
            else
            {
                ActDefault();
            }
        }

        internal virtual void InternalSee() =>
            See(Environment.GetListOfObservableAgents(Name, PerceptionFilter));

        internal void Post(Message message) =>
            _messages.Enqueue(message);
    }
}