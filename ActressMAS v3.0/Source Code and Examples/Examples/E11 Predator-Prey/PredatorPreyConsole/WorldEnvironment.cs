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

using ActressMas;
using System;
using System.IO;

namespace PredatorPrey
{
    public class WorldEnvironment : EnvironmentMas
    {
        private StreamWriter _sw;

        public WorldEnvironment(int noTurns) 
            : base(noTurns, parallel: false)
        {
            string worldStateFileName = "world-state.txt";
            _sw = new StreamWriter(worldStateFileName);
            _sw.WriteLine("Doodlebugs\tAnts");

            Memory["World"] = new World();
        }

        public override void TurnFinished(int turn)
        {
            World w = Memory["World"];

            w.CountInsects(out int noDoodlebugs, out int noAnts);

            Console.WriteLine($"Turn {turn + 1}: {noDoodlebugs} doodlebugs, {noAnts} ants");

            _sw.WriteLine($"{noDoodlebugs}\t{noAnts}");
            _sw.Flush();

            if (Settings.ShowWorld)
            {
                Console.WriteLine(w.PrintMap());
                Console.WriteLine("\r\nPress ENTER to continue ");
                Console.ReadLine();
            }
        }

        public override void SimulationFinished()
        {
            Console.WriteLine("\r\nSimulation finished.");
            _sw.Close();
        }
    }
}