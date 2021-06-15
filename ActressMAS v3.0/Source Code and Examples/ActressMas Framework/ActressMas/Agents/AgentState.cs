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

namespace ActressMas
{
    /// <summary>
    /// The class that stores the serializable state of the agent when it moves. It is the Memento in the Memento design pattern,
    /// while the specific Agent class whose state is saved and restored is the Originator. This class should be inherited to add
    /// all the serializable fields specific to a particular agent.
    /// For example, a concurrent agent cannot be serialized directly because MailboxProcessor is not serializable
    /// </summary>
    [Serializable]
    public abstract class AgentState
    {
        /// <summary>
        /// The agent class needed in order to instantiate the agent object after a move
        /// </summary>
        public Type AgentType { get; set; }

        /// <summary>
        /// The agent name
        /// </summary>
        public string Name { get; set; }
    }
}