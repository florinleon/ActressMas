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

using System;
using System.Collections.Generic;

namespace ActressMas
{
    public class Environment
    {
        public List<Agent> Agents { get; set; }

        public Environment()
        {
            Agents = new List<Agent>();
        }

        public void Add(Agent a)
        {
            a.Environment = this;
            Agents.Add(a);
        }

        public void Add(Agent a, string name)
        {
            a.Name = name;
            a.Environment = this;
            Agents.Add(a);
        }

        public void Remove(Agent a)
        {
            Agents.Remove(a);
        }

        public bool SomeAlive()
        {
            foreach (Agent a in Agents)
            {
                if (a.IsAlive())
                    return true;
            }
            return false;
        }

        public void Send(Message message)
        {
            // this method could have been internal, but if access from outside is allowed, one can simulate the forwarding behavior

            if (message.Content.Contains("callsetupmethod"))
                throw new Exception("\"callsetupmethod\" is a reserved keyword!");

            foreach (Agent a in Agents)
            {
                if (a.Name == message.Receiver)
                    a.Post(message);
            }
        }
    }
}