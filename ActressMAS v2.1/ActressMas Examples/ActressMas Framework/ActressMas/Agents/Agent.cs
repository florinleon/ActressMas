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

using System.Collections.Generic;

namespace ActressMas
{
    /// <summary>
    /// An abstract base class for agents. You must define your own agent classes derived from ConcurrentAgent or TurnBasedAgent.
    /// </summary>
    public abstract class Agent
    {
        /// <summary>
        /// The name of the agent. Each agent must have a unique name in its environment. Most operations are performed using agent names rather than agent objects.
        /// </summary>
        public string Name { get; set; } // if the name is given in the constructor, a constructor with a string parameter must also be added in the derived classes

        public abstract void Broadcast(string content, bool includeSender = false, string conversationId = "");

        public abstract bool CanMove(string destination);

        /// <summary>
        /// Imports the state of the agent, after it has moved from another container.
        /// </summary>
        /// <param name="state">The state of the agent</param>
        public virtual void LoadState(AgentState state)
        {
        }

        public abstract void Move(string destination);

        /// <summary>
        /// Exports the state of the agent, so it can be serialized when moving to another container.
        /// </summary>
        /// <returns></returns>
        public virtual AgentState SaveState()
        {
            return null;
        }

        public abstract void Send(string receiver, string content, string conversationId = "");

        public abstract void SendToMany(List<string> receivers, string content, string conversationId = "");

        public abstract void Stop();
    }
}