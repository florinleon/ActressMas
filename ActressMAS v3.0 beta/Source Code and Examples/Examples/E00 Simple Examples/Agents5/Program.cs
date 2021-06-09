/**************************************************************************
 *                                                                        *
 *  Description: Simple example of using the ActressMas framework         *
 *  Website:     https://github.com/florinleon/ActressMas                 *
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
using System.Threading;

namespace Agents5
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new EnvironmentMas();
            var a1 = new Agent1(); env.Add(a1, "a1");
            var a2 = new Agent2(); env.Add(a2, "a2");
            env.Start();
        }
    }

    public class Agent1 : Agent
    {
        public override void Setup()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"Setup: {i + 1} from a1*");
                Thread.Sleep(100);
            }

            Send("a2", "msg");
        }

        public override void Act(Message message)
        {
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"Act: {i + 1} from a1*");
                Thread.Sleep(100);
            }
        }

        public override void ActDefault()
        {
            Console.WriteLine("a1: No messages");
            Stop();
        }
    }

    public class Agent2 : Agent
    {
        public override void Setup()
        {
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"Setup: {i + 1} from a2");
                Thread.Sleep(100);
            }

            Send("a1", "msg");
        }

        public override void Act(Message message)
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"Act: {i + 1} from a2");
                Thread.Sleep(100);
            }
        }

        public override void ActDefault()
        {
            Console.WriteLine("a2: No messages");
            Stop();
        }
    }
}