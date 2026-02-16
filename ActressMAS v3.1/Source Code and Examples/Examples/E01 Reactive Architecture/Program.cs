/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: The reactive architecture using the ActressMas framework *
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

using ActressMas;

namespace Reactive
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new EnvironmentMas(delayAfterTurn: 100); // no turn limit, run in parallel

            var planetAgent = new PlanetAgent();
            env.Add(planetAgent, "planet");

            for (int i = 1; i <= 5; i++) // 5 explorers
            {
                var explorerAgent = new ExplorerAgent();
                env.Add(explorerAgent, $"explorer{i}");
            }

            env.Memory.Add("Size", 11);
            env.Memory.Add("NoResources", 10);

            env.Start();
        }
    }
}