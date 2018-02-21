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
            // === Init ===

            var env = new ActressMas.Environment();
            var a1 = new MyAgent(); env.Add(a1, "agent1");
            var a2 = new MyAgent(); env.Add(a2, "agent2");
            var m = new MonitorAgent(); env.Add(m, "monitor");

            // === Run ===

            m.Start(); a1.Start(); a2.Start();

            // === Wait ===

            while (true) { };
        }
    }

    public class MyAgent : Agent
    {
        private static Random _rand = new Random();

        public override void Setup()
        {
            for (int i = 1; i <= 10; i++)
            {
                Send("monitor", i.ToString());

                int dt = 100 + _rand.Next(900);
                Thread.Sleep(dt); // miliseconds
            }
        }
    }

    public class MonitorAgent : Agent
    {
        public override void Act(Message message)
        {
            Console.WriteLine("{0}: {1}", message.Sender, message.Content);
        }
    }
}