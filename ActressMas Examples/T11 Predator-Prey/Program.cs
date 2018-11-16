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

namespace PredatorPrey
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var worldEnv = new World(Utils.NoTurns); // derived from ActressMas.TurnBasedEnvironment

            int noCells = Utils.GridSize * Utils.GridSize;

            int[] randVect = Utils.RandomPermutation(noCells);

            for (int i = 0; i < Utils.NoDoodlebugs; i++)
            {
                var a = new DoodlebugAgent();
                worldEnv.Add(a, worldEnv.CreateName(a)); // unique name
                worldEnv.AddAgentToMap(a, randVect[i]);
            }

            for (int i = Utils.NoDoodlebugs; i < Utils.NoDoodlebugs + Utils.NoAnts; i++)
            {
                var a = new AntAgent();
                worldEnv.Add(a, worldEnv.CreateName(a));
                worldEnv.AddAgentToMap(a, randVect[i]);
            }

            worldEnv.Start();
        }
    }
}