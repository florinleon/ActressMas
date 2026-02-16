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
using System.Collections.Generic;
using System.Linq;

namespace ReplicatorDynamics
{
    public class GameAgent : Agent
    {
        private readonly List<string> _strategyNames;
        private readonly Dictionary<string, int> _nameToIndex;
        private readonly int _noStrategies;
        private readonly int _maxRounds;
        private Dictionary<string, double> _currentProportions;
        private int _currentRound;

        public GameAgent(IEnumerable<string> strategyNames, int maxRounds)
        {
            _strategyNames = new List<string>(strategyNames);
            _noStrategies = _strategyNames.Count;
            _maxRounds = maxRounds;

            _nameToIndex = new Dictionary<string, int>();

            for (int i = 0; i < _noStrategies; i++)
                _nameToIndex[_strategyNames[i]] = i;
        }

        public override void Setup()
        {
            _currentRound = 0;
            _currentProportions = new Dictionary<string, double>();

            foreach (var name in _strategyNames)
                _currentProportions[name] = double.NaN;

            Console.WriteLine($"Replicator dynamics with {_noStrategies} strategies for {_maxRounds} rounds.");
            StartNextRound();
        }

        public override void Act(Message message)
        {
            try
            {
                message.Parse(out string action, out string parameters);

                switch (action)
                {
                    case "state":
                        HandleState(message.Sender, parameters);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GameAgent error: {ex.Message}");
            }
        }

        private void StartNextRound()
        {
            _currentRound++;

            if (_currentRound > _maxRounds)
            {
                Console.WriteLine();
                Console.WriteLine("Simulation finished.");
                foreach (var name in _strategyNames)
                    Send(name, "stop");

                Stop();
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"--- Round {_currentRound} ---");

            foreach (var name in _strategyNames)
                _currentProportions[name] = double.NaN;

            foreach (var name in _strategyNames)
                Send(name, $"tick {_currentRound}");
        }

        private void HandleState(string sender, string parameters)
        {
            double xi = double.Parse(parameters.Trim());
            _currentProportions[sender] = xi;

            if (!AllProportionsReceived())
                return;

            var x = new double[_noStrategies];

            for (int i = 0; i < _noStrategies; i++)
            {
                string name = _strategyNames[i];
                x[i] = _currentProportions[name];
            }

            double sum = x.Sum();

            if (sum <= 0.0)
            {
                Console.WriteLine("Population proportions sum to non-positive value.");
                foreach (var name in _strategyNames)
                    Send(name, "stop");

                Stop();
                return;
            }

            if (Math.Abs(sum - 1.0) > 1e-6)
            {
                for (int i = 0; i < _noStrategies; i++)
                    x[i] /= sum;

                Console.WriteLine($"State normalized: sum before normalization = {sum:F2)}");
            }

            var u = Game.ComputeExpectedPayoffs(x);
            double uBar = Game.ComputeAveragePayoff(x, u);

            Console.Write("x = [");
            for (int i = 0; i < _noStrategies; i++)
            {
                Console.Write($"{x[i]:F2}");
                if (i < _noStrategies - 1)
                    Console.Write(", ");
            }
            Console.WriteLine("]");

            Console.Write("u = [");
            for (int i = 0; i < _noStrategies; i++)
            {
                Console.Write($"{u[i]:F2}");
                if (i < _noStrategies - 1)
                    Console.Write(", ");
            }
            Console.WriteLine("]");

            Console.WriteLine($"ubar = {uBar:F2}");

            for (int i = 0; i < _noStrategies; i++)
            {
                string name = _strategyNames[i];
                string msg = $"payoff {u[i]} {uBar}";
                Send(name, msg);
            }

            StartNextRound();
        }

        private bool AllProportionsReceived()
        {
            foreach (var kvp in _currentProportions)
            {
                if (double.IsNaN(kvp.Value))
                    return false;
            }

            return true;
        }
    }
}
