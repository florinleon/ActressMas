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
    public class AntAgent : InsectAgent
    {
        public override void Setup()
        {
            _turnsSurvived = 0;
            _world = Environment.Memory["World"];

            if (Settings.Verbose)
                Console.WriteLine($"AntAgent {Name} started in ({Line},{Column})");
        }

        public override void ActDefault()
        {
            //try
            {
                AntAction();
            }
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
        }

        private void AntAction()
        {
            /*
              • Move: For every time step, the ants randomly try to move up, down, left, or right. If the neighboring
                cell in the selected direction is occupied or would move the ant off the grid, then the ant stays in the
                current cell.
                • Breed: If an ant survives for three time steps, at the end of the time step (i.e., after moving) the ant will
                breed. This is simulated by creating a new ant in an adjacent (up, down, left, or right) cell that is
                empty. If there is no empty cell available, no breeding occurs. Once an offspring is produced, an ant
                cannot produce an offspring again until it has survived three more time steps.
             */

            _turnsSurvived++;

            // move
            TryToMove(); // implemented in base class InsectAgent

            // breed
            if (_turnsSurvived >= 3)
            {
                if (TryToBreed()) // implemented in base class InsectAgent
                    _turnsSurvived = 0;
            }
        }
    }
}