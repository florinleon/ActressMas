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

namespace ActressMas
{
    /// <summary>
    /// An abstract base class for environments. You must use ConcurrentEnvironment or TurnBasedEnvironment.
    /// </summary>
    public abstract class Environment
    {
        protected Container _container = null;

        /// <summary>
        /// The name of the container that contains the environment. If the container is not set or not connected to the server,
        /// this method will return the empty string.
        /// </summary>
        public string ContainerName
        {
            get
            {
                if (_container == null)
                    return "";
                else
                    return _container.Name;
            }
        }

        public abstract int NoAgents { get; }

        public abstract List<string> AllAgents();

        /// <summary>
        /// Returns a list with the names of all the containers in the distributed system. This list may change over time, 
        /// as some new containers may get connected and existing ones may disconnect.
        /// </summary>
        /// <returns></returns>
        public List<string> AllContainers()
        {
            return _container.AllContainers();
        }

        public abstract List<string> FilteredAgents(string nameFragment);

        public abstract string RandomAgent();

        public abstract string RandomAgent(Random rand);

        public abstract void Remove(string agentName);

        public abstract void Send(Message message);

        internal abstract void AgentHasArrived(AgentState agentState);

        internal void MoveAgent(AgentState agentState, string destination)
        {
            Remove(agentState.Name);
            _container.MoveAgent(agentState, destination);
        }

        internal void SetContainer(Container container)
        {
            _container = container;
        }
    }
}