/**************************************************************************
 *                                                                        *
 *  Description: Simple example of using the ActressMas framework         *
 *  Website:     https://github.com/florinleon/ActressMas                 *
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
using System.Linq;

namespace Agents3
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new MyEnvironment(noTurns: 10, delayAfterTurn: 100, parallel: true);
            //var env = new MyEnvironment(noTurns: 10, delayAfterTurn: 100, randomOrder: false, parallel: false);

            int noAgents = 4;

            for (int i = 1; i <= noAgents; i++)
            {
                var a = new MyAgent();
                env.Add(a, $"agent{i}");
            }

            var m = new MonitorAgent();
            env.Add(m, "monitor");

            env.Start();
        }
    }

    public class MyEnvironment : EnvironmentMas
    {
        public MyEnvironment(int noTurns = 0, int delayAfterTurn = 0, bool randomOrder = true, Random rand = null, bool parallel = true)
            : base(noTurns, delayAfterTurn, randomOrder, rand, parallel)
        {
        }

        public override void TurnFinished(int turn)
        {
            Console.WriteLine($"\nTurn {turn + 1} finished\n");
        }

        public override void SimulationFinished()
        {
            Console.WriteLine("\nSimulation finished\n");
        }
    }

    public class MyAgent : Agent
    {
        public override void Act(Message message)
        {
            //Console.WriteLine($"{Name} acting");
            Console.WriteLine($"\t{message.Format()}");

            if (message.Sender == "monitor" && (message.Content == "start" || message.Content == "continue"))
                Send("monitor", "done");
        }
    }

    public class MonitorAgent : Agent
    {
        private int _round, _maxRounds;
        private Dictionary<string, bool> _finished;
        private List<string> _agentNames;
        private static Random _rand = new Random();

        public override void Setup()
        {
            _finished = new Dictionary<string, bool>();
            _agentNames = new List<string>();
            _maxRounds = 3;
            _round = 0;

            Console.WriteLine("monitor: start round 1 in setup");

            foreach (var a in Environment.FilteredAgents("agent"))
            {
                _agentNames.Add(a);
                _finished.Add(a, false);
            }

            for (int i = 0; i < _agentNames.Count; i++)
            {
                Console.WriteLine($"-> sending to {_agentNames[i]}");
                Send(_agentNames[i], "start");
            }
        }

        public override void Act(Message message)
        {
            Console.WriteLine($"\t{message.Format()}");

            if (message.Content == "done")
                _finished[message.Sender] = true;

            if (AllFinished())
            {
                if (++_round >= _maxRounds)
                    return;

                //Console.WriteLine($"{Name} acting scenario");

                for (int i = 0; i < _agentNames.Count; i++)
                    _finished[_agentNames[i]] = false;

                Console.WriteLine($"\r\nmonitor: start round {_round + 1} in act");

                for (int i = 0; i < _agentNames.Count; i++)
                {
                    Console.WriteLine($"-> sending to {_agentNames[i]}");
                    Send(_agentNames[i], "continue");
                }
            }
        }

        private bool AllFinished() =>
            _finished.Keys.All(a => _finished[a]);
    }
}