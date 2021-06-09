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
using System.Threading;

namespace Agents1
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new EnvironmentMas(noTurns: 100);
            var a1 = new MyAgent(); env.Add(a1, "agent1");
            var a2 = new MyAgent(); env.Add(a2, "agent2");
            var m = new MonitorAgent(); env.Add(m, "monitor");
            env.Start();
        }
    }

    public class MyAgent : Agent
    {
        private static Random _rand = new Random();

        public override void Setup()
        {
            Console.WriteLine($"{this.Name} starting");

            for (int i = 1; i <= 10; i++)
            {
                Console.WriteLine($"{this.Name} sending {i}");
                Send("monitor", $"{i}");

                int dt = 10 + _rand.Next(90);
                Thread.Sleep(dt); // miliseconds
            }
        }
    }

    public class MonitorAgent : Agent
    {
        public override void Act(Message m)
        {
            Console.WriteLine($"{this.Name} has received {m.Format()}");
        }
    }
}