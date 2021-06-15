using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ActressMas
{
    internal class AgentCollection
    {
        private ConcurrentDictionary<string, Agent> AgentsC;
        private Dictionary<string, Agent> AgentsS;
        private bool _parallel;

        public AgentCollection(bool parallel)
        {
            _parallel = parallel;
            if (_parallel)
                AgentsC = new ConcurrentDictionary<string, Agent>();
            else
                AgentsS = new Dictionary<string, Agent>();
        }

        public int Count =>
            _parallel ? AgentsC.Count : AgentsS.Count;

        public bool ContainsKey(string name) =>
            _parallel ? AgentsC.ContainsKey(name) : AgentsS.ContainsKey(name);

        public Agent this[string name]
        {
            get
            {
                return _parallel ? AgentsC[name] : AgentsS[name];
            }
            set
            {
                if (_parallel)
                    AgentsC[name] = value;
                else
                    AgentsS[name] = value;
            }
        }

        public ICollection<string> Keys =>
            _parallel ? AgentsC.Keys : AgentsS.Keys;

        public ICollection<Agent> Values =>
            _parallel ? AgentsC.Values : AgentsS.Values;

        public KeyValuePair<string, Agent> ElementAt(int index) =>
            _parallel ? AgentsC.ElementAt(index) : AgentsS.ElementAt(index);

        public void Remove(string name)
        {
            if (_parallel)
            {
                bool ok = AgentsC.TryRemove(name, out Agent a);
                if (!ok)
                    throw new Exception($"Agent {name} could not be removed (EnvironmentMas.Remove(string) -> AgentCollection(parallel).Remove)");
            }
            else
            {
                AgentsS.Remove(name);
            }
        }
    }
}