/**************************************************************************
 *                                                                        *
 *  Description: ActressMas multi-agent framework                         *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Copyright:   (c) 2018, Florin Leon                                    *
 *  Acknowledgement: Marius Gavrilescu                                    *
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
    public class ConcurrentEnvironment
    {
        internal Dictionary<string, ConcurrentAgent> Agents { get; set; }

        public ConcurrentEnvironment()
        {
            Agents = new Dictionary<string, ConcurrentAgent>();
        }

        public int NoAgents
        {
            get { return Agents.Count; }
        }

        public void Add(ConcurrentAgent agent)
        {
            if (agent.Name == null || agent.Name == "")
                throw new Exception("Trying to add an agent without a name (ConcurrentEnvironment.Add(agent))");

            if (Agents.ContainsKey(agent.Name))
                throw new Exception("Trying to add an agent with an existing name: " + agent.Name + " (ConcurrentEnvironment.Add(Agent))");

            agent.Environment = this;
            Agents[agent.Name] = agent;
        }

        public void Add(ConcurrentAgent agent, string name)
        {
            if (Agents.ContainsKey(name))
                throw new Exception("Trying to add an agent with an existing name: " + name + " (ConcurrentEnvironment.Add(agent, name))");

            agent.Name = name;
            agent.Environment = this;
            Agents[name] = agent;
        }

        public void Remove(ConcurrentAgent agent)
        {
            if (Agents.ContainsValue(agent))
                Agents.Remove(agent.Name);
            else
                throw new Exception("Agent " + agent.Name + " does not exist (ConcurrentEnvironment.Remove)");
        }

        public void Remove(string agentName)
        {
            if (Agents.ContainsKey(agentName))
                Agents.Remove(agentName);
            else
                throw new Exception("Agent " + agentName + " does not exist (ConcurrentEnvironment.Remove)");
        }

        public void Send(Message message)
        {
            // this method could have been internal, but if access from outside is allowed, one can simulate the forwarding behavior

            string receiverName = message.Receiver;

            if (Agents.ContainsKey(receiverName))
                Agents[receiverName].Post(message);
        }

        public void WaitAll()
        {
            while (Agents.Count > 0)
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