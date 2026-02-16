/**************************************************************************
 *                                                                        *
 *  Description: ActressMas multi-agent framework                         *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Copyright:   (c) 2018-2026, Florin Leon                               *
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ActressMas
{
    internal class AgentCollection
    {
        private ConcurrentDictionary<string, Agent> _agentsC;
        private Dictionary<string, Agent> _agentsS;
        private bool _parallel;

        public AgentCollection(bool parallel)
        {
            _parallel = parallel;
            if (_parallel)
                _agentsC = new ConcurrentDictionary<string, Agent>();
            else
                _agentsS = new Dictionary<string, Agent>();
        }

        public int Count =>
            _parallel ? _agentsC.Count : _agentsS.Count;

        public bool ContainsKey(string name) =>
            _parallel ? _agentsC.ContainsKey(name) : _agentsS.ContainsKey(name);

        public Agent this[string name]
        {
            get
            {
                return _parallel ? _agentsC[name] : _agentsS[name];
            }
            set
            {
                if (_parallel)
                    _agentsC[name] = value;
                else
                    _agentsS[name] = value;
            }
        }

        public ICollection<string> Keys =>
            _parallel ? _agentsC.Keys : _agentsS.Keys;

        public ICollection<Agent> Values =>
            _parallel ? _agentsC.Values : _agentsS.Values;

        public KeyValuePair<string, Agent> ElementAt(int index) =>
            _parallel ? _agentsC.ElementAt(index) : _agentsS.ElementAt(index);

        public bool TryGetValue(string name, out Agent agent)
        {
            if (_parallel)
                return _agentsC.TryGetValue(name, out agent);
            else
                return _agentsS.TryGetValue(name, out agent);
        }

        public bool Remove(string name)
        {
            if (_parallel)
            {
                bool ok = _agentsC.TryRemove(name, out Agent a);
                if (!ok)
                    throw new Exception($"Agent {name} could not be removed (EnvironmentMas.Remove(string) -> AgentCollection(parallel).Remove)");
            }
            else
            {
                return _agentsS.Remove(name);
            }
            return false;
        }
    }
}
