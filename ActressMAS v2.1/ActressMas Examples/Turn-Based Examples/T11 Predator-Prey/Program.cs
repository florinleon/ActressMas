/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Predator-prey simulation (ants and doodlebugs) using     *
 *               ActressMas framework                                     *
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
using System.Linq;

namespace PredatorPrey
{
    public class Program
    {
        private static Random _rand = new Random();

        private static void Main(string[] args)
        {
            var worldEnv = new WorldEnvironment(Settings.NoTurns); // derived from ActressMas.TurnBasedEnvironment
            var world = worldEnv.Memory["World"];

            int noCells = Settings.GridSize * Settings.GridSize;

            int[] randVect = RandomPermutation(noCells);

            for (int i = 0; i < Settings.NoDoodlebugs; i++)
            {
                var a = new DoodlebugAgent();
                worldEnv.Add(a, world.CreateName(a)); // unique name
                world.AddAgentToMap(a, randVect[i]);
            }

            for (int i = Settings.NoDoodlebugs; i < Settings.NoDoodlebugs + Settings.NoAnts; i++)
            {
                var a = new AntAgent();
                worldEnv.Add(a, world.CreateName(a));
                world.AddAgentToMap(a, randVect[i]);
            }

            worldEnv.Start();
        }

        private static int[] RandomPermutation(int n)
        {
            int[] numbers = new int[n];
            for (int i = 0; i < n; i++)
                numbers[i] = i;
            int[] randPerm = numbers.OrderBy(x => _rand.Next()).ToArray();
            return randPerm;
        }
    }
}