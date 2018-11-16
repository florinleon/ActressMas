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
    public class TurnBasedEnvironment
    {
        private bool _randomOrder = true;
        private int _delayAfterTurn = 0;
        private int _numberOfTurns = 0;
        private Random _rand;

        protected List<TurnBasedAgent> Agents { get; set; }

        protected Dictionary<string, TurnBasedAgent> AgentsDict { get; set; }

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

        public int NoAgents
        {
            get { return Agents.Count; }
        }

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

        public void Remove(string agent)
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

        public void Send(Message message)
        {
            // this method could have been internal, but if access from outside is allowed, one can simulate the forwarding behavior

            string receiverName = message.Receiver;

            if (AgentsDict.ContainsKey(receiverName))
                AgentsDict[receiverName].Post(message);
        }

        public List<string> AllAgents()
        {
            return AgentsDict.Keys.ToList();
        }

        public List<string> FilteredAgents(string agentNameFragment)
        {
            List<string> agentNames = AllAgents();
            return agentNames.FindAll(name => name.Contains(agentNameFragment));
        }

        public string RandomAgent()
        {
            Random rand = new Random();
            int randomIndex = rand.Next(AgentsDict.Count);
            var item = AgentsDict.ElementAt(randomIndex);
            return item.Key;
        }

        public string RandomAgent(Random rand)
        {
            int randomIndex = rand.Next(AgentsDict.Count);
            var item = AgentsDict.ElementAt(randomIndex);
            return item.Key;
        }

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
                    if (agentsCopy[aoi].RunSetup) // first turn runs Setup
                    {
                        agentsCopy[aoi].Setup();
                        agentsCopy[aoi].RunSetup = false;
                    }
                    else
                        agentsCopy[aoi].InternalAct();
                }
            }

            Thread.Sleep(_delayAfterTurn);

            TurnFinished(turn);
        }

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

        public virtual void TurnFinished(int turn)
        {
        }

        public virtual void SimulationFinished()
        {
        }

        private int[] RandomPermutation(int n)
        {
            int[] numbers = new int[n];
            for (int i = 0; i < n; i++)
                numbers[i] = i;
            int[] randPerm = numbers.OrderBy(x => _rand.Next()).ToArray();
            return randPerm;
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