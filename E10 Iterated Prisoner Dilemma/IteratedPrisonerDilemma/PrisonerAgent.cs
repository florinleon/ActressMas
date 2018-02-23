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
        protected int points = 0;
        protected int lastOutcome = 0;

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                string action; string parameters;
                Utils.ParseMessage(message.Content, out action, out parameters);

                switch (action)
                {
                    case "turn":
                        HandleTurn();
                        break;

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

        private void HandleTurn()
        {
            string action = ChooseAction(lastOutcome);
            Send("police", Utils.Str("play", action));
        }

        protected abstract string ChooseAction(int lastOutcome);

        private void HandleOutcome(int outcome)
        {
            lastOutcome = outcome;
            points += lastOutcome;
        }

        private void HandleEnd()
        {
            Console.WriteLine("[{0}]: I have {1} points", this.Name, points);
        }
    }
}