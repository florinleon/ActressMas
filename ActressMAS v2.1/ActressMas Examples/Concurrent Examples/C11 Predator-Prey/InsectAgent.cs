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

namespace PredatorPrey
{
    public class InsectAgent : ConcurrentAgent
    {
        protected int _turnsSurvived;
        public int Line, Column; // position on the grid
        protected World _worldEnv;

        protected bool TryToMove()
        {
            Direction direction = (Direction)Utils.RandNoGen.Next(4);
            int newLine, newColumn; // ValidMovement computes newLine and newColumn

            if (_worldEnv.ValidMovement(this, direction, CellState.Empty, out newLine, out newColumn))
            {
                if (Utils.Verbose)
                    Console.WriteLine("Moving " + this.Name);

                _worldEnv.Move(this, newLine, newColumn);

                return true;
            }
            else
                return false;
        }

        protected bool TryToBreed()
        {
            List<Direction> allowedDirections = new List<Direction>();
            int newLine, newColumn;

            for (int i = 0; i < 4; i++)
            {
                if (_worldEnv.ValidMovement(this, (Direction)i, CellState.Empty, out newLine, out newColumn))
                    allowedDirections.Add((Direction)i);
            }

            if (allowedDirections.Count == 0)
                return false;

            int r = Utils.RandNoGen.Next(allowedDirections.Count);
            _worldEnv.ValidMovement(this, allowedDirections[r], CellState.Empty, out newLine, out newColumn);

            _worldEnv.Breed(this, newLine, newColumn);

            return true;
        }
    }
}