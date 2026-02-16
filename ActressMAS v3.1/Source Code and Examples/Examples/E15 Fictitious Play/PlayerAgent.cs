/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Fictitious Play using the ActressMas framework           *
 *  Copyright:   (c) 2026, Florin Leon                                    *
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

namespace FictitiousPlay
{
    public class PlayerAgent : Agent
    {
        private string _gameAgentName;
        private string _opponentName;
        private Dictionary<string, int> _opponentActionCounts;
        private Dictionary<string, int> _myActionCounts;
        private Random _rand;
        private int _round;

        public PlayerAgent(string gameAgentName, string opponentName)
        {
            _gameAgentName = gameAgentName;
            _opponentName = opponentName;

            _rand = new Random();
        }

        public override void Setup()
        {
            _opponentActionCounts = new Dictionary<string, int>();
            foreach (var a in Game.Actions)
                _opponentActionCounts[a] = 1; // Laplace smoothing

            _myActionCounts = new Dictionary<string, int>();
            foreach (var a in Game.Actions)
                _myActionCounts[a] = 0;

            _round = 0;
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out string parameters);

                switch (action)
                {
                    case "play":
                        HandlePlay(parameters);
                        break;

                    case "result":
                        HandleResult(parameters);
                        break;

                    case "stop":
                        Console.WriteLine($"{Name}: stopping.");
                        PrintStrategyProbabilities();
                        Stop();
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

        private void HandlePlay(string parameters)
        {
            _round++;

            string chosenAction = ChooseAction();

            if (_myActionCounts.ContainsKey(chosenAction))
                _myActionCounts[chosenAction]++;
            else
                _myActionCounts[chosenAction] = 1;

            Console.WriteLine($"{Name} (round {_round}) plays {chosenAction}");

            Send(_gameAgentName, $"action {chosenAction}");
        }

        private void PrintStrategyProbabilities()
        {
            // Belief about opponent strategy (based on counts, includes Laplace smoothing)
            int oppTotal = 0;
            foreach (int v in _opponentActionCounts.Values)
                oppTotal += v;

            double oppPA = (double)_opponentActionCounts[Game.ActionA] / oppTotal;
            double oppPB = (double)_opponentActionCounts[Game.ActionB] / oppTotal;

            // Empirical strategy based on my own play history (does not include smoothing)
            int myTotal = 0;
            foreach (int v in _myActionCounts.Values)
                myTotal += v;

            double myPA = myTotal > 0 ? (double)_myActionCounts[Game.ActionA] / myTotal : 0.0;
            double myPB = myTotal > 0 ? (double)_myActionCounts[Game.ActionB] / myTotal : 0.0;

            Console.WriteLine($"{Name}: belief about {_opponentName}: P(A)={oppPA:F2}, P(B)={oppPB:F2} " +
                $"[counts A={_opponentActionCounts[Game.ActionA]}, B={_opponentActionCounts[Game.ActionB]}]");

            Console.WriteLine($"{Name}: empirical strategy: P(A)={myPA:F2}, P(B)={myPB:F2} " +
                $"[counts A={_myActionCounts[Game.ActionA]}, B={_myActionCounts[Game.ActionB]}, rounds={myTotal}]");
        }

        private void HandleResult(string parameters)
        {
            // parameters: "<opponentAction> <myPayoff>"
            var toks = parameters.Trim().Split();

            if (toks.Length >= 1)
            {
                string opponentAction = toks[0];

                if (_opponentActionCounts.ContainsKey(opponentAction))
                    _opponentActionCounts[opponentAction]++;
                else
                    _opponentActionCounts[opponentAction] = 1;

                double payoff = 0.0;
                if (toks.Length >= 2)
                    payoff = Convert.ToDouble(toks[1]);

                Console.WriteLine($"{Name} observes opponent played {opponentAction}, payoff = {payoff:F2}");
            }
        }

        private string ChooseAction()
        {
            int total = 0;
            foreach (int v in _opponentActionCounts.Values)
                total += v;

            double pA = (double)_opponentActionCounts[Game.ActionA] / total;
            double pB = (double)_opponentActionCounts[Game.ActionB] / total;

            double expectedA = pA * Game.Payoff(Game.ActionA, Game.ActionA) + pB * Game.Payoff(Game.ActionA, Game.ActionB);
            double expectedB = pA * Game.Payoff(Game.ActionB, Game.ActionA) + pB * Game.Payoff(Game.ActionB, Game.ActionB);

            if (Math.Abs(expectedA - expectedB) < 1e-6)  // tie: randomize
                return _rand.NextDouble() < 0.5 ? Game.ActionA : Game.ActionB;

            return expectedA > expectedB ? Game.ActionA : Game.ActionB;
        }
    }
}
