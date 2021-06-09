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
    /// <summary>
    /// A concurrent environment, where the agents run in parallel.
    /// </summary>
    public class ConcurrentEnvironment : Environment
    {
        /// <summary>
        /// Initializes a new instance of the ConcurrentEnvironment class.
        /// </summary>
        public ConcurrentEnvironment()
        {
            Agents = new Dictionary<string, ConcurrentAgent>();
            Memory = new Dictionary<string, dynamic>();
        }

        /// <summary>
        /// The number of agents in the environment
        /// </summary>
        override public int NoAgents
        {
            get { return Agents.Count; }
        }

        internal Dictionary<string, ConcurrentAgent> Agents { get; set; }

        /// <summary>
        /// Adds an agent to the environment. The agent should already have a name and its name should be unique.
        /// </summary>
        /// <param name="agent">The concurrent agent that will be added</param>
        public void Add(ConcurrentAgent agent)
        {
            if (agent.Name == null || agent.Name == "")
                throw new Exception("Trying to add an agent without a name (ConcurrentEnvironment.Add(agent))");

            if (Agents.ContainsKey(agent.Name))
                throw new Exception("Trying to add an agent with an existing name: " + agent.Name + " (ConcurrentEnvironment.Add(Agent))");

            agent.Environment = this;
            Agents[agent.Name] = agent;
        }

        /// <summary>
        /// Adds an agent to the environment. Its name should be unique.
        /// </summary>
        /// <param name="agent">The concurrent agent that will be added</param>
        /// <param name="name">The name of the agent</param>
        public void Add(ConcurrentAgent agent, string name)
        {
            if (Agents.ContainsKey(name))
                throw new Exception("Trying to add an agent with an existing name: " + name + " (ConcurrentEnvironment.Add(agent, name))");

            agent.Name = name;
            agent.Environment = this;
            Agents[name] = agent;
        }

        /// <summary>
        /// Returns a list with the names of all the agents.
        /// </summary>
        /// <returns></returns>
        override public List<string> AllAgents()
        {
            return Agents.Keys.ToList();
        }

        /// <summary>
        /// Returns a list with the names of all the agents that contain a certain string.
        /// </summary>
        /// <returns>The name fragment that the agent names should contain</returns>
        override public List<string> FilteredAgents(string nameFragment)
        {
            List<string> agentNames = AllAgents();
            return agentNames.FindAll(name => name.Contains(nameFragment));
        }

        /// <summary>
        /// Returns the name of a randomly selected agent from the environment
        /// </summary>
        /// <returns></returns>
        override public string RandomAgent()
        {
            Random rand = new Random();
            int randomIndex = rand.Next(Agents.Count);
            var item = Agents.ElementAt(randomIndex);
            return item.Key;
        }

        /// <summary>
        /// Returns the name of a randomly selected agent from the environment using a predefined random number generator. This is useful for experiments
        /// involving non-determinism, but which should be repeatable for analysis and debugging.
        /// </summary>
        /// <param name="rand">The random number generator which should be non-null and instantiated using a seed</param>
        /// <returns></returns>
        override public string RandomAgent(Random rand)
        {
            if (rand != null)
            {
                int randomIndex = rand.Next(Agents.Count);
                var item = Agents.ElementAt(randomIndex);
                return item.Key;
            }
            else
                throw new Exception("The random number generator is null (ConcurrentEnvironment.RandomAgent(Random rand))");
        }

        /// <summary>
        /// Stops the execution of the agent and removes it from the environment. Use the Remove method instead of Agent.Stop
        /// when the decision to stop an agent does not belong to the agent itself, but to some other agent or to an external factor.
        /// </summary>
        /// <param name="agent">The agent to be removed</param>
        public void Remove(ConcurrentAgent agent)
        {
            if (Agents.ContainsValue(agent))
                Agents.Remove(agent.Name);
            else
                throw new Exception("Agent " + agent.Name + " does not exist (ConcurrentEnvironment.Remove)");
        }

        /// <summary>
        /// Stops the execution of the agent identified by name and removes it from the environment. Use the Remove method instead of Agent.Stop
        /// when the decision to stop an agent does not belong to the agent itself, but to some other agent or to an external factor.
        /// </summary>
        /// <param name="agentName">The name of the agent to be removed</param>
        override public void Remove(string agentName)
        {
            if (Agents.ContainsKey(agentName))
                Agents.Remove(agentName);
            else
                throw new Exception("Agent " + agentName + " does not exist (ConcurrentEnvironment.Remove)");
        }

        /// <summary>
        /// Sends a message from the outside of the multiagent system. Whenever possible, the agents should use the Send method of their own class,
        /// not the Send method of the environment. This method can also be used to simulate a forwarding behavior.
        /// </summary>
        /// <param name="message">The message to be sent</param>
        override public void Send(Message message)
        {
            string receiverName = message.Receiver;

            if (Agents.ContainsKey(receiverName))
                Agents[receiverName].Post(message);
        }

        /// <summary>
        /// Prevents the program from closing by waiting as long as some agents still run in the environment. This method should be used at the end of the
        /// main program, after all the agents have been added to the environment and started.
        /// </summary>
        public void WaitAll()
        {
            while (Agents.Count > 0)
            {
            }
        }

        override internal void AgentHasArrived(AgentState agentState)
        {
            ConcurrentAgent a = Activator.CreateInstance(agentState.AgentType) as ConcurrentAgent;
            a.LoadState(agentState);
            a.Name = agentState.Name;
            Add(a);
            a.Start();
        }
    }
}