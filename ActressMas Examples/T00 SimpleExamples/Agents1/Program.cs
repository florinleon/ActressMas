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

namespace Agents1
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new TurnBasedEnvironment(100);
            var a1 = new MyAgent(); env.Add(a1, "agent1");
            var a2 = new MyAgent(); env.Add(a2, "agent2");
            var m = new MonitorAgent(); env.Add(m, "monitor");
            env.Start();
        }
    }

    public class MyAgent : TurnBasedAgent
    {
        private int _turn = 1;

        public override void Act(Queue<Message> messages)
        {
            if (_turn > 10)
                return;

            Send("monitor", _turn.ToString());
            _turn++;
        }
    }

    public class MonitorAgent : TurnBasedAgent
    {
        public override void Act(Queue<Message> messages)
        {
            while (messages.Count > 0)
            {
                Message message = messages.Dequeue();
                Console.WriteLine("{0}: {1}", message.Sender, message.Content);
            }
        }
    }
}