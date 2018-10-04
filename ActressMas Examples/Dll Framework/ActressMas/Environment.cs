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
using System.Linq;

namespace ActressMas
{
    public class Environment
    {
        internal Dictionary<string, Agent> Agents { get; set; }

        internal int NoAliveAgents = 0;

        public Environment()
        {
            Agents = new Dictionary<string, Agent>();
        }

        public int NoAgents
        {
            get
            {
                return Agents.Count;
            }
        }

        public void Add(Agent a)
        {
            a.Environment = this;
            Agents[a.Name] = a;
        }

        public void Add(Agent a, string name)
        {
            a.Name = name;
            a.Environment = this;
            Agents[name] = a;
        }

        public void Remove(Agent a)
        {
            Agents.Remove(a.Name);
        }

        public void Remove(string a)
        {
            Agents.Remove(a);
        }

        public bool SomeAlive()
        {
            return NoAliveAgents > 0;
        }

        public void Send(Message message)
        {
            // this method could have been internal, but if access from outside is allowed, one can simulate the forwarding behavior

            string receiverName = message.Receiver;

            if (Agents.ContainsKey(receiverName))
                Agents[receiverName].Post(message);
            else
                throw new Exception("Environment: could not find agent \"" + receiverName + "\"");
        }

        public void StopAll()
        {
            foreach (var kvp in Agents)
            {
                var a = kvp.Value;
                if (a.IsAlive())
                    a.Stop();
            }
        }

        public void Restart(string a)
        {
            Agents[a].Start();
        }

        public void WaitAll()
        {
            while (SomeAlive())
            {
            }
        }

        public List<string> AllAgents()
        {
            return Agents.Keys.ToList();
        }

        public List<string> FilteredAgents(string agentNameFragment)
        {
            List<string> agentNames = AllAgents();
            return agentNames.FindAll(name => name.Contains(agentNameFragment));
        }

        public string RandomAgent()
        {
            Random rand = new Random();
            int randomIndex = rand.Next(Agents.Count);
            var item = Agents.ElementAt(randomIndex);
            return item.Key;
        }

        public string RandomAgent(Random rand)
        {
            int randomIndex = rand.Next(Agents.Count);
            var item = Agents.ElementAt(randomIndex);
            return item.Key;
        }
    }
}