/**************************************************************************
 *                                                                        *
 *  Description: ActressMas multi-agent framework                         *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Copyright:   (c) 2018-2021, Florin Leon                               *
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
using System.Threading.Tasks;

namespace ActressMas
{
    /// <summary>
    /// An abstract base class for the multiagent environment, where all the agents are executed.
    /// </summary>
    public class EnvironmentMas
    {
        protected Container _container = null;
        private int _delayAfterTurn = 0;
        private int _noTurns = 0;
        private bool _parallel = true;
        private Random _rand;
        private bool _randomOrder = true;
        private static object _locker = new object();

        /// <summary>
        /// The agents in the environment
        /// </summary>
        private AgentCollection Agents;

        /// <summary>
        /// Initializes a new instance of the EnvironmentMas class.
        /// </summary>
        /// <param name="noTurns">The maximum number of turns of the simulation. Setup is considered to be the first turn. The simulation may stop earlier if there are no more agents in the environment. If the number of turns is 0, the simulation runs indefinitely, or until there are no more agents in the environment.</param>
        /// <param name="delayAfterTurn">A delay (in miliseconds) after each turn.</param>
        /// <param name="randomOrder">Whether the agents should be run in a random order (different each turn) or sequentially. If the execution is parallel, agents are always run in random order.</param>
        /// <param name="rand">A random number generator for non-deterministic but repeatable experiments. It should instantiated using a seed. If it is null, a new Random object is created and used.</param>
        /// <param name="parallel">Whether agent behaviors are executed in parallel or sequentially. The code of a single agent in a turn is always executed sequentially.</param>
        public EnvironmentMas(int noTurns = 0, int delayAfterTurn = 0, bool randomOrder = true, Random rand = null, bool parallel = true)
        {
            _noTurns = noTurns;
            _delayAfterTurn = delayAfterTurn;
            _randomOrder = randomOrder;
            _parallel = parallel;

            if (_parallel && !_randomOrder)
                throw new Exception("Sequential order cannot be guaranteed when executing in parallel. You can choose (randomOrder, parallel) to be (true, true), (false, false) or (true, false) (EnvironmentMas.ctor)");

            if (rand == null)
                _rand = new Random();
            else
                _rand = rand;

            Agents = new AgentCollection(_parallel);
            Memory = new Dictionary<string, dynamic>();
        }

        /// <summary>
        /// The name of the container that contains the environment. If the container is not set or not connected to the server,
        /// this method will return the empty string.
        /// </summary>
        public string ContainerName =>
            _container != null ? _container.Name : "";

        /// <summary>
        /// An object that can be used as a shared memory by the agents.
        /// </summary>
        public Dictionary<string, dynamic> Memory { get; set; }

        /// <summary>
        /// The number of agents in the environment
        /// </summary>
        public int NoAgents => Agents.Count;

        /// <summary>
        /// Adds an agent to the environment. The agent should already have a name and its name should be unique.
        /// </summary>
        /// <param name="agent">The concurrent agent that will be added</param>
        public void Add(Agent agent)
        {
            if (agent.Name == null || agent.Name == "")
                throw new Exception("Trying to add an agent without a name (EnvironmentMas.Add(agent))");

            Add(agent, agent.Name);
        }

        /// <summary>
        /// Adds an agent to the environment. Its name should be unique.
        /// </summary>
        /// <param name="agent">The concurrent agent that will be added</param>
        /// <param name="name">The name of the agent</param>
        public void Add(Agent agent, string name)
        {
            if (name == null || name == "")
                throw new Exception("Trying to add an agent without a name (EnvironmentMas.Add(agent))");

            agent.Name = name;

            if (Agents.ContainsKey(agent.Name))
                throw new Exception($"Trying to add an agent with an existing name: {agent.Name} (EnvironmentMas.Add(Agent))");

            agent.Environment = this;
            Agents[agent.Name] = agent;
        }

        /// <summary>
        /// Returns a list with the names of all the agents.
        /// </summary>
        public List<string> AllAgents() =>
            Agents.Keys.ToList();

        /// <summary>
        /// Returns a list with the names of all the containers in the distributed system. This list may change over time,
        /// as some new containers may get connected and existing ones may disconnect.
        /// </summary>
        public List<string> AllContainers() =>
            _container.AllContainers();

        /// <summary>
        /// Continues the simulation for an additional number of turns, after an initial simulation has finished.
        /// The simulation may stop earlier if there are no more agents in the environment.
        /// If the number of turns is 0, the simulation runs indefinitely, or until there are no more agents in the environment.
        /// </summary>
        /// <param name="noTurns">The maximum number of turns of the continued simulation</param>
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
        /// <param name="nameFragment">The name fragment that the agent names should contain</param>
        public List<string> FilteredAgents(string nameFragment) =>
            AllAgents().FindAll(name => name.Contains(nameFragment));

        /// <summary>
        /// Returns the name of a randomly selected agent from the environment
        /// </summary>
        public string RandomAgent()
        {
            var rand = new Random();
            int randomIndex = rand.Next(Agents.Count);
            var item = Agents.ElementAt(randomIndex);
            return item.Key;
        }

        /// <summary>
        /// Returns the name of a randomly selected agent from the environment using a predefined random number generator. This is useful for experiments
        /// involving non-determinism, but which should be repeatable for analysis and debugging.
        /// </summary>
        /// <param name="rand">The random number generator which should be non-null and instantiated using a seed</param>
        public string RandomAgent(Random rand)
        {
            if (rand != null)
            {
                int randomIndex = rand.Next(Agents.Count);
                var item = Agents.ElementAt(randomIndex);
                return item.Key;
            }
            else
                throw new Exception("The random number generator is null (EnvironmentMas.RandomAgent(Random rand))");
        }

        /// <summary>
        /// Stops the execution of the agent and removes it from the environment. Use the Remove method instead of Agent.Stop
        /// when the decision to stop an agent does not belong to the agent itself, but to some other agent or to an external factor.
        /// </summary>
        /// <param name="agent">The agent to be removed</param>
        public void Remove(Agent agent)
        {
            if (Agents.Values.Contains(agent))
                Agents.Remove(agent.Name);
            else
                throw new Exception($"Agent {agent.Name} does not exist (Agent.Remove)");
        }

        /// <summary>
        /// Stops the execution of the agent identified by name and removes it from the environment. Use the Remove method instead of Agent.Stop
        /// when the decision to stop an agent does not belong to the agent itself, but to some other agent or to an external factor.
        /// </summary>
        /// <param name="agentName">The name of the agent to be removed</param>
        public void Remove(string agentName) => 
            Remove(Agents[agentName]);

        /// <summary>
        /// Sends a message from the outside of the multiagent system. Whenever possible, the agents should use the Send method of their own class,
        /// not the Send method of the environment. This method can also be used to simulate a forwarding behavior.
        /// </summary>
        /// <param name="message">The message to be sent</param>
        public void Send(Message message)
        {
            string receiverName = message.Receiver;
            if (Agents.ContainsKey(receiverName))
                Agents[receiverName].Post(message);
        }

        /// <summary>
        /// Sends a message to a remote agent in another container.
        /// </summary>
        /// <param name="receiverContainer">The destination container</param>
        /// <param name="message">The message to be sent</param>
        public void SendRemote(string receiverContainer, Message message) =>
            _container.SendRemoteAgentMessage(receiverContainer, message);

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
                if (_noTurns != 0 && turn >= _noTurns)
                    break;
                if (Agents.Count == 0)
                    break;
            }

            SimulationFinished();
        }

        /// <summary>
        /// A method that may be optionally overriden to perform additional processing after a turn of the simulation has finished.
        /// </summary>
        /// <param name="turn">The turn that has just finished</param>
        public virtual void TurnFinished(int turn)
        {
        }

        internal void AgentHasArrived(AgentState agentState)
        {
            Agent a = Activator.CreateInstance(agentState.AgentType) as Agent;
            a.LoadState(agentState);
            a.Name = agentState.Name;
            a.MustRunSetup = true;
            Add(a);
        }

        internal List<ObservableAgent> GetListOfObservableAgents(string perceivingAgentName, Func<Dictionary<string, string>, bool> PerceptionFilter)
        {
            lock (_locker)
            {
                var observableAgentList = new List<ObservableAgent>();

                var agentsNames = Agents.Keys.ToList();

                for (int i = 0; i < agentsNames.Count; i++)
                {
                    if (Agents.Keys.Contains(agentsNames[i]))
                    {
                        Agent a = Agents[agentsNames[i]];

                        if (a == null || a.Name == perceivingAgentName || a.Observables == null || a.Observables.Count == 0)
                            continue;

                        if (PerceptionFilter(a.Observables))
                            observableAgentList.Add(new ObservableAgent(a.Observables));
                    }
                }

                return observableAgentList;
            }
        }

        internal void RemoteMessageReceived(Message message)
        {
            string[] toks = message.Receiver.Split('@');
            message.Receiver = toks[0];
            Send(message);
        }

        internal void MoveAgent(AgentState agentState, string destination)
        {
            Remove(agentState.Name);
            _container.MoveAgent(agentState, destination);
        }

        internal void SetContainer(Container container) =>
            _container = container;

        private void ExecuteSeeAct(Agent a)
        {
            if (a.UsingObservables)
                a.InternalSee();
            a.InternalAct();
        }

        private void ExecuteSetup(Agent a)
        {
            a.Setup();
            a.MustRunSetup = false;
        }

        private int[] RandomPermutation(int n)
        {
            // Fisher-Yates shuffle

            int[] numbers = new int[n];
            for (int i = 0; i < n; i++)
                numbers[i] = i;

            while (n > 1)
            {
                int k = _rand.Next(n--);
                int temp = numbers[n]; numbers[n] = numbers[k]; numbers[k] = temp;
            }

            return numbers;

            // much faster than Enumerable.Range(0, n).OrderBy(x => _rand.Next()).ToArray();
        }

        private void RunTurn(int turn)
        {
            if (!_parallel)
            {
                var agentOrder = _randomOrder ? RandomPermutation(NoAgents) : SortedPermutation(NoAgents);

                var agentsLeft = new List<string>();
                var agentNames = Agents.Keys.ToArray();
                for (int i = 0; i < NoAgents; i++)
                    agentsLeft.Add(agentNames[agentOrder[i]]);

                while (agentsLeft.Count > 0)
                {
                    string a = agentsLeft[0];
                    agentsLeft.Remove(a);

                    if (Agents.ContainsKey(a)) // agent not stopped or removed
                    {
                        if (Agents[a].MustRunSetup) // first turn runs Setup
                            ExecuteSetup(Agents[a]);
                        else
                            ExecuteSeeAct(Agents[a]);
                    }
                }
            }
            else
            {
                var actions = new List<Action>();

                var agentsLeft = Agents.Keys.ToList();

                while (agentsLeft.Count > 0)
                {
                    string a = agentsLeft[0];
                    agentsLeft.Remove(a);

                    if (Agents.ContainsKey(a)) // agent not stopped or removed
                    {
                        if (Agents[a].MustRunSetup) // first turn runs Setup
                            actions.Add(() => ExecuteSetup(Agents[a]));
                        else
                            actions.Add(() => ExecuteSeeAct(Agents[a]));
                    }
                }

                var aa = new Action[actions.Count];
                for (int i = 0; i < actions.Count; i++)  // faster than .ToArray()
                    aa[i] = actions[i];
                Parallel.Invoke(aa);
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

            // faster than Enumerable.Range(0, n).ToArray();
        }
    }
}