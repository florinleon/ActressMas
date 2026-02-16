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

namespace Agents4
{
    public class Program
    {
        private static void Main(string[] args)
        {
            int noAgents = 5;

            var env = new EnvironmentMas(noTurns: 5, delayAfterTurn: 1000);
            env.Add(new WriterAgent(), "writer");
            for (int i = 1; i <= noAgents; i++)
                env.Add(new MyAgent(), $"a{i}");
            env.Start();
        }
    }

    public class MyAgent : Agent
    {
        private int _turn = 1;

        public override void ActDefault()
        {
            if (_turn > 3)
                Stop();

            for (int i = 1; i <= 3; i++)
                Send("writer", $"Agent {Name} turn {_turn} index {i}");

            _turn++;
        }
    }

    public class WriterAgent : Agent
    {
        public override void Act(Message m)
        {
            Console.WriteLine(m.Format());
        }
    }
}