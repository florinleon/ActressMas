using ActressMas;
using System;
using System.Collections.Generic;

namespace PredatorPrey
{
    public class InsectAgent : Agent
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