/**************************************************************************
 *                                                                        *
 *  Description: ActressMas multi-agent framework                         *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Copyright:   (c) 2018, Florin Leon                                    *
 *                                                                        *
 *  This program is free software; you can redistribute it and/or modify  *
 *  it under the terms of the GNU General Public License as published by  *
 *  the Free Software Foundation. This program is distributed in the      *
 *  hope that it will be useful, but WITHOUT ANY WARRANTY; without even   *
 *  the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR   *
 *  PURPOSE. See the GNU General Public License for more details.         *
 *                                                                        *
 **************************************************************************/

using System.Collections.Generic;

namespace ActressMas
{
    /// <summary>
    /// The base class for an agent that runs on a turn-based manner in its environment. You must create your own agent classes derived from this abstract class.
    /// </summary>
    public abstract class TurnBasedAgent : Agent
    {
        internal bool MustRunSetup;
        private Queue<Message> _messages;

        public TurnBasedAgent()
        {
            _messages = new Queue<Message>();
            MustRunSetup = true;
        }

        /// <summary>
        /// The environment in which the agent runs. A turn-based agent can only run in a turn-based environment.
        /// </summary>
        public TurnBasedEnvironment Environment { get; set; }

        /// <summary>
        /// This is the method that is called once a turn. This is where the main logic of the agent should be placed. Once a message has been handled, it should
        /// be removed from the queue, using e.g. the Dequeue method.
        /// </summary>
        /// <param name="messages">The messages that the agent has received during the previous turn(s) and should respond to</param>
        public virtual void Act(Queue<Message> messages)
        {
        }

        /// <summary>
        /// Sends a message to all the agents in the environment.
        /// </summary>
        /// <param name="content">The content of the message</param>
        /// <param name="includeSender">Whether the sender itself receives the message or not</param>
        /// <param name="conversationId">A conversation identifier, for the cases when a conversation involves multiple messages that refer to the same topic</param>
        override public void Broadcast(string content, bool includeSender = false, string conversationId = "")
        {
            List<string> receivers = Environment.AllAgents();

            if (includeSender == false)
                receivers.Remove(this.Name);

            foreach (string a in receivers)
                Send(a, content, conversationId);
        }

        /// <summary>
        /// Tests whether the agent can move to a certain remote container.
        /// </summary>
        /// <param name="destination">The name of the container that the agent wants to move to</param>
        /// <returns></returns>
        override public bool CanMove(string destination)
        {
            return (this.Environment.ContainerName != "" && destination != this.Environment.ContainerName && this.Environment.AllContainers().Contains(destination));
        }

        /// <summary>
        /// The method that should be called when the agent wants to move to a different container.
        /// </summary>
        /// <param name="destination">The name of the container that the agent wants to move to</param>
        override public void Move(string destination)
        {
            AgentState state = SaveState();
            state.AgentType = this.GetType();
            state.Name = this.Name;

            Environment.MoveAgent(state, destination);
        }

        /// <summary>
        /// Sends a message to a specific agent, identified by name.
        /// </summary>
        /// <param name="receiver">The agent that will receive the message</param>
        /// <param name="content">The content of the message</param>
        /// <param name="conversationId">A conversation identifier, for the cases when a conversation involves multiple messages that refer to the same topic</param>
        override public void Send(string receiver, string content, string conversationId = "")
        {
            Message message = new Message(this.Name, receiver, content, conversationId);
            this.Environment.Send(message);
        }

        /// <summary>
        /// Sends a message to a specific set of agents, identified by name.
        /// </summary>
        /// <param name="receivers">The list of agents that will receive the message</param>
        /// <param name="content">The content of the message</param>
        /// <param name="conversationId">A conversation identifier, for the cases when a conversation involves multiple messages that refer to the same topic</param>
        override public void SendToMany(List<string> receivers, string content, string conversationId = "")
        {
            foreach (string a in receivers)
                Send(a, content, conversationId);
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
        override public void Stop()
        {
            Environment.Remove(this);
        }

        internal virtual void InternalAct()
        {
            Act(_messages);
        }

        internal void Post(Message message)
        {
            _messages.Enqueue(message);
        }
    }
}