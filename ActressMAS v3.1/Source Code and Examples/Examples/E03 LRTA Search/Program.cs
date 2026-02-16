/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Learning Real-Time A* (LRTA*) search algorithm using     *
 *               the ActressMas framework                                 *
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

namespace LrtaStar
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new EnvironmentMas();
            var mapAgent = new MapAgent(); env.Add(mapAgent, "map");
            var searchAgent = new SearchAgent(); env.Add(searchAgent, "agent1");

            env.Memory.Add("MapName", "Pendulum");
            //env.Memory.Add("MapName", "StrangeHeuristic");
            env.Memory.Add("Delay", 100);

            env.Start();
        }
    }
}