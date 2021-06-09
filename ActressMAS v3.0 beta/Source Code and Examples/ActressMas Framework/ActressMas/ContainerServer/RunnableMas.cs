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

namespace ActressMas
{
    /// <summary>
    /// An abstract class which should be derived in order to specify the multiagent system with mobile agents that will be run in the environment of a container.
    /// </summary>
    public abstract class RunnableMas
    {
        /// <summary>
        /// Starts the execution of a multiagent environment within a container
        /// </summary>
        /// <param name="env">The multiagent environment</param>
        public virtual void RunMas(EnvironmentMas env)
        {
        }
    }
}