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
    public class DoodlebugAgent : InsectAgent
    {
        private int _lastEaten;

        public override void Setup()
        {
            _turnsSurvived = 0;
            _lastEaten = 0;
            _world = Environment.Memory["World"];

            if (Settings.Verbose)
                Console.WriteLine($"DoodlebugAgent {Name} started in ({Line},{Column})");
        }

        public override void ActDefault()
        {
            //try
            {
                DoodlebugAction();
            }
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
        }

        private void DoodlebugAction()
        {
            /*
             • Move. For every time step, the doodlebug will move to an adjacent cell containing an ant and eat the
                ant. If there are no ants in adjoining cells, the doodlebug moves according to the same rules as the
                ant. Note that a doodlebug cannot eat other doodlebugs.
                • Breed. If a doodlebug survives for eight time steps, at the end of the time step it will spawn off a new
                doodlebug in the same manner as the ant.
                • Starve. If a doodlebug has not eaten an ant within three time steps, at the end of the third time step it
                will starve and die. The doodlebug should then be removed from the grid of cells.
             */

            _turnsSurvived++;
            _lastEaten++;

            // eat
            bool success = TryToEat();
            if (success)
                _lastEaten = 0;

            // move
            if (!success)
                TryToMove(); // implemented in base class InsectAgent

            // breed
            if (_turnsSurvived >= 8)
            {
                if (TryToBreed()) // implemented in base class InsectAgent
                    _turnsSurvived = 0;
            }

            // starve
            if (_lastEaten >= 3)
                Die();
        }

        private bool TryToEat()
        {
            var allowedDirections = new List<Direction>();
            int newLine, newColumn;

            for (int i = 0; i < 4; i++)
            {
                if (_world.ValidMovement(this, (Direction)i, CellState.Ant, out newLine, out newColumn))
                    allowedDirections.Add((Direction)i);
            }

            if (allowedDirections.Count == 0)
                return false;

            int r = _rand.Next(allowedDirections.Count);
            _world.ValidMovement(this, allowedDirections[r], CellState.Ant, out newLine, out newColumn);

            AntAgent ant = _world.Eat(this, newLine, newColumn);
            Environment.Remove(ant);

            return true;
        }

        private void Die()
        {
            // removing the doodlebug

            if (Settings.Verbose)
                Console.WriteLine($"Removing {Name}");

            _world.Die(this);
            Stop();
        }
    }
}