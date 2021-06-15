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

namespace SendingObjects
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new EnvironmentMas(10);
            var a1 = new Agent1(); env.Add(a1, "a1");
            var a2 = new Agent2(); env.Add(a2, "a2");
            env.Start();
        }
    }

    public class ContentInfo
    {
        public int Number { get; }
        public string Text { get; }

        public ContentInfo(int number, string text)
        {
            Number = number;
            Text = text;
        }
    }

    public class Agent1 : Agent
    {
        public override void Setup()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"a1* sending: setup {i + 1} from a1*");
                var c = new ContentInfo(i + 1, "setup a1*");
                Send("a2", c);
                Thread.Sleep(100);
            }
        }

        public override void Act(Message message)
        {
            var c = message.ContentObj as ContentInfo;
            Console.WriteLine($"a1* receiving: {c.Text} - {c.Number}");
            Thread.Sleep(100);
        }
    }

    public class Agent2 : Agent
    {
        public override void Setup()
        {
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"a2 sending: setup: {i + 1} from a2");
                var c = new ContentInfo(i + 1, "setup a2");
                Send("a1", c);
                Thread.Sleep(100);
            }
        }

        public override void Act(Message message)
        {
            var c = message.ContentObj as ContentInfo;
            Console.WriteLine($"a2 receiving: {c.Text} - {c.Number}");
            Thread.Sleep(100);
        }
    }
}