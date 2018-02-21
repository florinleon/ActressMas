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
using System.Threading;

namespace Agents4
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new ActressMas.Environment();

            var w = new WriterAgent(); env.Add(w, "writer");
            w.Start();

            int noAgents = 5;

            for (int i = 1; i <= noAgents; i++)
            {
                var a = new MyAgent();
                env.Add(a, "a" + i);
                a.Start();
            }

            Thread.Sleep(100);

            var m = new MonitorAgent();
            env.Add(m, "monitor");
            m.Start();

            while (true) { };
        }
    }

    public class MyAgent : Agent
    {
        private static Random _rand = new Random();

        public override void Act(Message message)
        {
            if (message.Sender == "monitor" && (message.Content == "start" || message.Content == "continue"))
                TakeTurn();
        }

        private void TakeTurn()
        {
            for (int i = 1; i <= 3; i++)
            {
                Send("writer", i.ToString());
                //Console.WriteLine("Agent \'{0}\' says \'{1}\'", this.Name, i);

                int dt = 100 + _rand.Next(400);
                Thread.Sleep(dt); // miliseconds
            }

            Send("monitor", "done");
        }
    }

    public class WriterAgent : Agent
    {
        public override void Act(Message message)
        {
            Console.WriteLine("Agent \'{0}\' says \'{1}\'", message.Sender, message.Content);
        }
    }

    public class MonitorAgent : Agent
    {
        private Dictionary<string, bool> _finished;
        private int _turn;
        private int _maxTurns;
        private static Random _rand = new Random();
        private List<string> _agentNames;

        public override void Setup()
        {
            _maxTurns = 3;
            _finished = new Dictionary<string, bool>();
            _agentNames = new List<string>();
            _turn = 1;

            foreach (Agent a in this.Environment.Agents)
            {
                if (a.Name != this.Name && a.Name != "writer")
                {
                    _agentNames.Add(a.Name);
                    _finished.Add(a.Name, false);
                }
            }

            Send("writer", "start turn 1");
            //Console.WriteLine("Agent \'{0}\' says \'{1}\'", this.Name, "start turn 1");

            int[] randPerm = RandomPermutation(_agentNames.Count);
            for (int i = 0; i < _agentNames.Count; i++)
                Send(_agentNames[randPerm[i]], "start");
                //Send(_agentNames[i], "start");
        }

        public override void Act(Message message)
        {
            if (message.Content == "done")
                _finished[message.Sender] = true;

            if (AllFinished())
            {
                if (++_turn > _maxTurns)
                {
                    Send("writer", "finished");
                    //Console.WriteLine("Agent \'{0}\' says \'{1}\'", this.Name, "finished");
                    return;
                }

                for (int i = 0; i < _agentNames.Count; i++)
                    _finished[_agentNames[i]] = false;

                Send("writer", "start turn " + _turn);
                //Console.WriteLine("Agent \'{0}\' says \'{1}\'", this.Name, "start turn " + _turn);

                int[] randPerm = RandomPermutation(_agentNames.Count);
                for (int i = 0; i < _agentNames.Count; i++)
                    Send(_agentNames[randPerm[i]], "continue");
                    //Send(_agentNames[i], "continue");
            }
        }

        private int[] RandomPermutation(int n)
        {
            int[] numbers = new int[n];
            for (int i = 0; i < n; i++)
                numbers[i] = i;
            int[] randPerm = numbers.OrderBy(x => _rand.Next()).ToArray();
            return randPerm;
        }

        private bool AllFinished()
        {
            foreach (string a in _finished.Keys)
                if (!_finished[a])
                    return false;
            return true;
        }
    }
}