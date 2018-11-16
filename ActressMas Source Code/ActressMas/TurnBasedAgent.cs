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

using System;
using System.Collections.Generic;

namespace ActressMas
{
    public abstract class TurnBasedAgent
    {
        public string Name { get; set; } // if the name is given in the constructor, a constructor with a string parameter must also be added in the derived classes
        public TurnBasedEnvironment Environment { get; set; }

        private Queue<Message> _messages;

        internal bool RunSetup;

        public TurnBasedAgent()
        {
            _messages = new Queue<Message>();
            RunSetup = true;
        }

        public void Stop()
        {
            Environment.Remove(this);
        }

        public void Send(string receiver, string content, string conversationId = "")
        {
            Message message = new Message(this.Name, receiver, content, conversationId);
            this.Environment.Send(message);
        }

        internal void Post(Message message)
        {
            _messages.Enqueue(message);
        }

        public void Broadcast(string content, bool includeSender = false, string conversationId = "")
        {
            List<string> receivers = Environment.AllAgents();

            if (includeSender == false)
                receivers.Remove(this.Name);

            foreach (string a in receivers)
                Send(a, content, conversationId);
        }

        public void SendToMany(List<string> receivers, string content, string conversationId = "")
        {
            foreach (string a in receivers)
                Send(a, content, conversationId);
        }

        public virtual void Setup()
        {
        }

        internal virtual void InternalAct()
        {
            Act(_messages);
        }

        public virtual void Act(Queue<Message> messages)
        {
        }
    }
}