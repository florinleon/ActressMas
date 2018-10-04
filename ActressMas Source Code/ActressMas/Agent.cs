/**************************************************************************
 *                                                                        *
 *  Description: ActressMas multi-agent framework                         *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Copyright:   (c) 2018, Florin Leon                                    *
 *                                                                        *
 *  Using Actress, a C# port of the F# MailboxProcessor                   *
 *  by Kevin Thompson: https://github.com/kthompson/Actress               *
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
    public abstract class Agent
    {
        private MailboxProcessor<Message> _mbProc;
        private bool _alive;

        public string Name { get; set; } // if the name is given in the constructor, a constructor with a string parameter must also be added in the derived classes
        public Environment Environment { get; set; }

        public void Start()
        {
            _alive = true;
            Environment.NoAliveAgents++;

            _mbProc = MailboxProcessor.Start<Message>(async mb =>
            {
                Setup();
                while (_alive)
                {
                    Message message = await mb.Receive();
                    Act(message);
                }
            });
        }

        public void Stop()
        {
            BeforeStop();
            _alive = false;
            Environment.NoAliveAgents--;
        }

        public bool IsAlive()
        {
            return _alive;
        }

        public void Send(string receiver, string content, string conversationId = "")
        {
            Message message = new Message(this.Name, receiver, content, conversationId);
            BeforeSend();
            this.Environment.Send(message);
            AfterSend();
        }

        internal void Post(Message message)
        {
            _mbProc.Post(message);
        }

        public void Broadcast(string content, bool includeSender = false, string conversationId = "")
        {
            List<string> receivers = Environment.AllAgents();

            if (includeSender == false)
                receivers.Remove(this.Name);

            BeforeSend();

            foreach (string a in receivers)
                Send(a, content, conversationId);

            AfterSend();
        }

        public void SendToMany(List<string> receivers, string content, string conversationId = "")
        {
            BeforeSend();

            foreach (string a in receivers)
                Send(a, content, conversationId);

            AfterSend();
        }

        public virtual void Setup()
        {
        }

        public virtual void Act(Message message)
        {
        }

        public virtual void BeforeSend()
        {
        }

        public virtual void AfterSend()
        {
        }

        public virtual void BeforeStop()
        {
        }
    }
}