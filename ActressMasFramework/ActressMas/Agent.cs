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

namespace ActressMas
{
    public abstract class Agent
    {
        private MailboxProcessor<Message> _mb;
        private bool _alive;

        public string Name { get; set; } // if the name is given in the constructor, a constructor with a string parameter must also be added in the derived classes
        public Environment Environment { get; set; }

        public void Start()
        {
            _alive = true;

            _mb = MailboxProcessor.Start<Message>(async mb =>
            {
                while (_alive)
                {
                    Message message = await mb.Receive();
                    CallAgentMethods(message);
                }
            });

            _mb.Post(new Message(this.Name, this.Name, "callsetupmethod"));
        }

        private void CallAgentMethods(Message message)
        {
            if (message.Content == "callsetupmethod")
                Setup();
            else
                Act(message);
        }

        public void Stop()
        {
            _alive = false;
        }

        public bool IsAlive()
        {
            return _alive;
        }

        public void Send(string receiver, string content)
        {
            Message message = new Message(this.Name, receiver, content);
            this.Environment.Send(message);
        }

        public void Send(string receiver, string content, string conversationId)
        {
            Message message = new Message(this.Name, receiver, content, conversationId);
            this.Environment.Send(message);
        }

        internal void Post(Message message)
        {
            _mb.Post(message);
        }

        public virtual void Setup()
        {
        }

        public virtual void Act(Message message)
        {
        }
    }
}