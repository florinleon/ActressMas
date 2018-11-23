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
using System.Linq;
using System.Threading;

namespace ActressMas
{
    /// <summary>
    /// A turn-based environment, where the all the agents are executed sequentially or in a random order during a turn.
    /// </summary>
    public class TurnBasedEnvironment : Environment
    {
        private int _delayAfterTurn = 0;
        private int _numberOfTurns = 0;
        private Random _rand;
        private bool _randomOrder = true;

        /// <summary>
        /// Initializes a new instance of the TurnBasedEnvironment class.
        /// </summary>
        /// <param name="numberOfTurns">The maximum number of turns of the simulation. The simulation may stop earlier if there are no more agents in the environment. If the number of turns is 0, the simulation runs indefinitely, or until there are no more agents in the environment.</param>
        /// <param name="delayAfterTurn">A delay (in miliseconds) after each turn</param>
        /// <param name="randomOrder">Whether the agents should be run in a random order (different each turn) or sequentially</param>
        /// <param name="rand">A random number generator for non-deterministic but repeatable experiments. It should instantiated using a seed. If it is null, a new Random object is created and used.</param>
        public TurnBasedEnvironment(int numberOfTurns = 0, int delayAfterTurn = 0, bool randomOrder = true, Random rand = null)
        {
            _randomOrder = randomOrder;
            _delayAfterTurn = delayAfterTurn;
            _numberOfTurns = numberOfTurns; // Setup is included as the first turn

            if (rand == null)
                _rand = new Random();
            else
                _rand = rand;

            Agents = new List<TurnBasedAgent>();
            AgentsDict = new Dictionary<string, TurnBasedAgent>();
        }

        /// <summary>
        /// The number of agents in the environment
        /// </summary>
        override public int NoAgents
        {
            get { return Agents.Count; }
        }

        protected List<TurnBasedAgent> Agents { get; set; }

        protected Dictionary<string, TurnBasedAgent> AgentsDict { get; set; }

        /// <summary>
        /// Adds an agent to the environment. The agent should already have a name and its name should be unique.
        /// </summary>
        /// <param name="agent">The concurrent agent that will be added</param>
        public void Add(TurnBasedAgent agent)
        {
            if (agent.Name == null || agent.Name == "")
                throw new Exception("Trying to add an agent without a name (TurnBasedEnvironment.Add(agent))");

            if (AgentsDict.ContainsKey(agent.Name))
                throw new Exception("Trying to add an agent with an existing name: " + agent.Name + " (TurnBasedEnvironment.Add(Agent))");

            agent.Environment = this;
            AgentsDict[agent.Name] = agent;
            Agents.Add(agent);
        }

        /// <summary>
        /// Adds an agent to the environment. Its name should be unique.
        /// </summary>
        /// <param name="agent">The concurrent agent that will be added</param>
        /// <param name="name">The name of the agent</param>
        public void Add(TurnBasedAgent agent, string name)
        {
            if (AgentsDict.ContainsKey(name))
                throw new Exception("Trying to add an agent with an existing name: " + name + " (TurnBasedEnvironment.Add(agent, name))");
            else
            {
                agent.Name = name;
                agent.Environment = this;
                Agents.Add(agent);
                AgentsDict[name] = agent;
            }
        }

        /// <summary>
        /// Returns a list with the names of all the agents.
        /// </summary>
        /// <returns></returns>
        override public List<string> AllAgents()
        {
            return AgentsDict.Keys.ToList();
        }

        /// <summary>
        /// Continues the simulation for an additional number of turns, after a simulation has finished.
        /// </summary>
        /// <param name="noTurns">The maximum number of turns of the continued simulation. The simulation may stop earlier if there are no more agents in the environment. If the number of turns is 0, the simulation runs indefinitely, or until there are no more agents in the environment.</param>
        public void Continue(int noTurns = 0)
        {
            int turn = 0;

            while (true)
            {
                RunTurn(turn);
                turn++;
                if (noTurns != 0 && turn >= noTurns) // noTurns = 0 => infinite
                    break;
                if (Agents.Count == 0)
                    break;
            }

            SimulationFinished();
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
            int randomIndex = rand.Next(AgentsDict.Count);
            var item = AgentsDict.ElementAt(randomIndex);
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
                int randomIndex = rand.Next(AgentsDict.Count);
                var item = AgentsDict.ElementAt(randomIndex);
                return item.Key;
            }
            else
                throw new Exception("The random number generator is null (TurnBasedEnvironment.RandomAgent(Random rand))");
        }

        /// <summary>
        /// Stops the execution of the agent and removes it from the environment. Use the Remove method instead of Agent.Stop
        /// when the decision to stop an agent does not belong to the agent itself, but to some other agent or to an external factor.
        /// </summary>
        /// <param name="agent">The agent to be removed</param>
        public void Remove(TurnBasedAgent agent)
        {
            if (Agents.Contains(agent))
            {
                Agents.Remove(agent);
                AgentsDict.Remove(agent.Name);
            }
            else
                throw new Exception("Agent " + agent.Name + " does not exist (TurnBasedAgent.Remove)");
        }

        /// <summary>
        /// Stops the execution of the agent identified by name and removes it from the environment. Use the Remove method instead of Agent.Stop
        /// when the decision to stop an agent does not belong to the agent itself, but to some other agent or to an external factor.
        /// </summary>
        /// <param name="agentName">The name of the agent to be removed</param>
        override public void Remove(string agent)
        {
            if (AgentsDict.ContainsKey(agent))
            {
                TurnBasedAgent ag = AgentsDict[agent];
                Agents.Remove(ag);
                AgentsDict.Remove(agent);
            }
            else
                throw new Exception("Agent " + agent + " does not exist (TurnBasedAgent.Remove)");
        }

        /// <summary>
        /// Sends a message from the outside of the multiagent system. Whenever possible, the agents should use the Send method of their own class,
        /// not the Send method of the environment. This method can also be used to simulate a forwarding behavior.
        /// </summary>
        /// <param name="message">The message to be sent</param>
        override public void Send(Message message)
        {
            string receiverName = message.Receiver;

            if (AgentsDict.ContainsKey(receiverName))
                AgentsDict[receiverName].Post(message);
        }

        /// <summary>
        /// A method that may be optionally overriden to perform additional processing after the simulation has finished.
        /// </summary>
        public virtual void SimulationFinished()
        {
        }

        /// <summary>
        /// Starts the simulation.
        /// </summary>
        public void Start()
        {
            int turn = 0;

            while (true)
            {
                RunTurn(turn);
                turn++;
                if (_numberOfTurns != 0 && turn >= _numberOfTurns)
                    break;
                if (Agents.Count == 0)
                    break;
            }

            SimulationFinished();
        }

        /// <summary>
        /// A method that may be optionally overriden to perform additional processing after a turn of the the simulation has finished.
        /// </summary>
        /// <param name="turn">The turn that has just finished</param>
        public virtual void TurnFinished(int turn)
        {
        }

        override internal void AgentHasArrived(AgentState agentState)
        {
            TurnBasedAgent a = Activator.CreateInstance(agentState.AgentType) as TurnBasedAgent;
            a.LoadState(agentState);
            a.Name = agentState.Name;
            a.MustRunSetup = true;
            Add(a);
        }

        private int[] RandomPermutation(int n)
        {
            int[] numbers = new int[n];
            for (int i = 0; i < n; i++)
                numbers[i] = i;
            int[] randPerm = numbers.OrderBy(x => _rand.Next()).ToArray();
            return randPerm;
        }

        private void RunTurn(int turn)
        {
            int[] agentOrder = null;
            if (_randomOrder)
                agentOrder = RandomPermutation(this.NoAgents);
            else
                agentOrder = SortedPermutation(this.NoAgents);

            // address situations when the agent list changes during the turn

            TurnBasedAgent[] agentsCopy = new TurnBasedAgent[this.NoAgents];
            Agents.CopyTo(agentsCopy);

            for (int i = 0; i < agentsCopy.Length; i++)
            {
                int aoi = agentOrder[i];
                if (agentsCopy[aoi] != null && Agents.Contains(agentsCopy[aoi])) // agent not stopped
                {
                    if (agentsCopy[aoi].MustRunSetup) // first turn runs Setup
                    {
                        agentsCopy[aoi].Setup();
                        agentsCopy[aoi].MustRunSetup = false;
                    }
                    else
                        agentsCopy[aoi].InternalAct();
                }
            }

            Thread.Sleep(_delayAfterTurn);

            TurnFinished(turn);
        }

        private int[] SortedPermutation(int n)
        {
            int[] numbers = new int[n];
            for (int i = 0; i < n; i++)
                numbers[i] = i;
            return numbers;
        }
    }
}