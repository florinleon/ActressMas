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
using System.Collections.Generic;

namespace PredatorPreyGui
{
    public class InsectAgent : Agent
    {
        protected int _turnsSurvived;

        // position on the grid
        public int Line { get; set; }

        public int Column { get; set; }

        protected bool TryToMove()
        {
            World worldEnv = (World)Environment;

            Direction direction = (Direction)Settings.RandNoGen.Next(4);
            int newLine, newColumn; // ValidMovement computes newLine and newColumn

            if (worldEnv.ValidMovement(this, direction, CellState.Empty, out newLine, out newColumn))
            {
                if (Settings.Verbose)
                    Console.WriteLine("Moving " + Name);

                worldEnv.Move(this, newLine, newColumn);

                return true;
            }
            else
                return false;
        }

        protected bool TryToBreed()
        {
            World worldEnv = (World)Environment;

            List<Direction> allowedDirections = new List<Direction>();
            int newLine, newColumn;

            for (int i = 0; i < 4; i++)
            {
                if (worldEnv.ValidMovement(this, (Direction)i, CellState.Empty, out newLine, out newColumn))
                    allowedDirections.Add((Direction)i);
            }

            if (allowedDirections.Count == 0)
                return false;

            int r = Settings.RandNoGen.Next(allowedDirections.Count);
            worldEnv.ValidMovement(this, allowedDirections[r], CellState.Empty, out newLine, out newColumn);

            worldEnv.Breed(this, newLine, newColumn);

            return true;
        }
    }
}