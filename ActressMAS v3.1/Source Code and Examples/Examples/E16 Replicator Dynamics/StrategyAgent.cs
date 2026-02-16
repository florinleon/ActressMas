/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Replicator Dynamics using the ActressMas framework       *
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

namespace ReplicatorDynamics
{
    public class StrategyAgent : Agent
    {
        private readonly string _coordinatorName;
        private readonly int _index;
        private double _proportion;

        public StrategyAgent(string coordinatorName, int index, double initialProportion)
        {
            _coordinatorName = coordinatorName;
            _index = index;
            _proportion = initialProportion;
        }

        public override void Setup()
        {
            Console.WriteLine($"{Name} (strategy {_index}) initial proportion = {$"{_proportion:F4}"}");
        }

        public override void Act(Message message)
        {
            try
            {
                message.Parse(out string action, out string parameters);

                switch (action)
                {
                    case "tick":
                        HandleTick(parameters);
                        break;

                    case "payoff":
                        HandlePayoff(parameters);
                        break;

                    case "stop":
                        HandleStop();
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Name} error: {ex.Message}");
            }
        }

        private void HandleTick(string parameters)
        {
            // parameters: time step (optional, not used here)
            Send(_coordinatorName, $"state {$"{_proportion:F4}"}");
        }

        private void HandlePayoff(string parameters)
        {
            // parameters: "<ui> <ubar>"
            var toks = parameters.Trim().Split();

            if (toks.Length < 2)
            {
                Console.WriteLine($"{Name}: payoff message malformed: '{parameters}'");
                return;
            }

            double ui = double.Parse(toks[0]);
            double uBar = double.Parse(toks[1]);

            if (uBar <= 0.0)
            {
                Console.WriteLine($"{Name}: average payoff non-positive ({uBar}), proportion not updated.");
                return;
            }

            double oldProportion = _proportion;

            // Discrete-time replicator update: x_i' = x_i * (u_i / u_bar)
            _proportion = _proportion * (ui / uBar);

            Console.WriteLine($"{Name}: ui = {ui:F2}, ubar = {uBar:F2}, x(old) = {oldProportion:F4}, x(new) = {_proportion:F2}");
        }

        private void HandleStop()
        {
            Console.WriteLine($"Final proportion for {Name}: {_proportion:F2}");
            Stop();
        }
    }
}
