/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Iterated Prisoner's Dilemma using ActressMas framework   *
 *  Copyright:   (c) 2018-2021, Florin Leon                               *
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
    public class PoliceAgent : Agent
    {
        private Dictionary<string, string> _responses;

        public PoliceAgent()
        {
            _responses = new Dictionary<string, string>();
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine("\t[{1} -> {0}]: {2}", Name, message.Sender, message.Content);

                message.Parse1P(out string p, out string chosenAction); // p = "play"
                HandlePlay(message.Sender, chosenAction);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void HandlePlay(string sender, string action)
        {
            _responses.Add(sender, action);

            if (_responses.Count == 2)
            {
                var e = _responses.GetEnumerator();
                e.MoveNext(); var p1 = e.Current;
                e.MoveNext(); var p2 = e.Current;

                ComputeOutcome(p1.Value, p2.Value, out int outcome1, out int outcome2);

                Send(p1.Key, $"outcome {outcome1}");
                Send(p2.Key, $"outcome {outcome2}");

                _responses.Clear();
            }
        }

        private void ComputeOutcome(string action1, string action2, out int outcome1, out int outcome2)
        {
            outcome1 = outcome2 = 0;

            if (action1 == "confess" && action2 == "confess")
            {
                outcome1 = outcome2 = -3;
            }
            else if (action1 == "deny" && action2 == "deny")
            {
                outcome1 = outcome2 = -1;
            }
            else if (action1 == "confess" && action2 == "deny")
            {
                outcome1 = 0; outcome2 = -5;
            }
            else if (action1 == "deny" && action2 == "confess")
            {
                outcome1 = -5; outcome2 = 0;
            }
        }
    }
}