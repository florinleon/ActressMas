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
    public class PoliceAgent : Agent
    {
        private int turns = 0;
        private Dictionary<string, string> responses;

        public override void Setup()
        {
            turns++;
            Console.WriteLine("[{0}] Turn {1}", this.Name, turns);
            Broadcast("turn");
            responses = new Dictionary<string, string>();
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                string action; string parameters;
                Utils.ParseMessage(message.Content, out action, out parameters);

                switch (action)
                {
                    case "play":
                        HandlePlay(message.Sender, parameters);
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

        private void HandlePlay(string sender, string action)
        {
            responses.Add(sender, action);

            if (responses.Count == 2)
            {
                var e = responses.GetEnumerator();
                e.MoveNext(); var p1 = e.Current;
                e.MoveNext(); var p2 = e.Current;

                int outcome1, outcome2;
                ComputeOutcome(p1.Value, p2.Value, out outcome1, out outcome2);

                Send(p1.Key, Utils.Str("outcome", outcome1));
                Send(p2.Key, Utils.Str("outcome", outcome2));

                responses.Clear();

                turns++;
                if (turns > Utils.NoTurns)
                    Broadcast("end");
                else
                {
                    Console.WriteLine("[{0}] Turn {1}", this.Name, turns);
                    Broadcast("turn");
                }
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
                outcome1 = 0;
                outcome2 = -5;
            }
            else if (action1 == "deny" && action2 == "confess")
            {
                outcome1 = -5;
                outcome2 = 0;
            }
        }
    }
}