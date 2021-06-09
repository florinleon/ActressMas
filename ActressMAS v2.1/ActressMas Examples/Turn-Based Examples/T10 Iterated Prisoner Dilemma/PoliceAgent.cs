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
using System.Collections.Generic;

namespace IteratedPrisonersDilemma
{
    public class PoliceAgent : TurnBasedAgent
    {
        public override void Act(Queue<Message> messages)
        {
            try
            {
                if (messages.Count < 2)
                    return;

                Message m1 = messages.Dequeue();
                Message m2 = messages.Dequeue();

                Console.WriteLine($"\t{m1.Format()}");
                Console.WriteLine($"\t{m2.Format()}");

                m1.Parse1P(out string p, out string chosenAction1); // p = "play"
                m2.Parse1P(out p, out string chosenAction2); // p = "play"

                ComputeOutcome(chosenAction1, chosenAction2, out int outcome1, out int outcome2);

                Send(m1.Sender, $"outcome {outcome1}");
                Send(m2.Sender, $"outcome {outcome2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ComputeOutcome(string action1, string action2, out int outcome1, out int outcome2)
        {
            outcome1 = outcome2 = 0;

            if (action1 == "confess" && action2 == "confess")
            {
                outcome1 = -3;
                outcome2 = -3;
            }
            else if (action1 == "deny" && action2 == "deny")
            {
                outcome1 = -1;
                outcome2 = -1;
            }
            else if (action1 == "confess" && action2 == "deny")
            {
                outcome1 = 0;
                outcome2 = -5;
            }
            else if (action1 == "deny" && action2 == "confess")
            {
                outcome1 = -5;
                outcome2 = 0;
            }

            outcome1 += 5;
            outcome2 += 5; // to have positive payoffs
        }
    }
}