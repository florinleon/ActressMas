/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Game of Life using the ActressMas framework              *
 *  Copyright:   (c) 2020, Florin Leon                                    *
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
using System.IO;

namespace GameOfLife
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new EnvironmentMas(delayAfterTurn: 10, randomOrder: false, parallel: false);

            var gridAgent = new GridAgent();
            gridAgent.UsingObservables = true;
            env.Add(gridAgent, "GridAgent");

            int size = 25;
            env.Memory["Size"] = size;

            var sr = new StreamReader("patterns.txt");

            for (int y = 0; y < size; y++)
            {
                string[] lineToks = sr.ReadLine().Split();

                for (int x = 0; x < size; x++)
                {
                    string state = (lineToks[x] == "x") ? "Living" : "Dead";
                    var a = new CellAgent(state, x, y);
                    a.UsingObservables = true;
                    env.Add(a, $"Agent-{x}-{y}");
                }
            }

            sr.Close();

            env.Start();
        }
    }
}