/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Iterated Prisoner's Dilemma using ActressMas framework   *
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

namespace IteratedPrisonersDilemma
{
    public abstract class PrisonerAgent : Agent
    {
        protected int _points = 0;
        protected int _lastOutcome = 0;

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out string parameters);

                switch (action)
                {
                    case "outcome":
                        HandleOutcome(Convert.ToInt32(parameters));
                        break;

                    case "end":
                        HandleEnd();
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override void ActDefault()
        {
            HandleTurn();
        }

        protected abstract string ChooseAction(int lastOutcome);

        private void HandleOutcome(int outcome)
        {
            _lastOutcome = outcome;
            _points += _lastOutcome;
        }

        private void HandleTurn()
        {
            string action = ChooseAction(_lastOutcome);
            Send("police", $"play {action}");
        }

        private void HandleEnd()
        {
            Console.WriteLine($"[{Name}]: I have {_points} points");
            Stop();
        }
    }
}