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

namespace Agents4
{
    public class Program
    {
        private static void Main(string[] args)
        {
            int noAgents = 5;

            var env = new TurnBasedEnvironment(5, 100);
            env.Add(new WriterAgent(), "writer");
            for (int i = 1; i <= noAgents; i++)
                env.Add(new MyAgent(), "a" + i);
            env.Start();
        }
    }

    public class MyAgent : TurnBasedAgent
    {
        private int _turn = 1;

        public override void Act(Queue<Message> messages)
        {
            if (_turn > 3)
                return;

            for (int i = 1; i <= 3; i++)
                Send("writer", i.ToString());

            _turn++;
        }
    }

    public class WriterAgent : TurnBasedAgent
    {
        public override void Act(Queue<Message> messages)
        {
            while (messages.Count > 0)
            {
                Message message = messages.Dequeue();
                Console.WriteLine("Agent \'{0}\' says \'{1}\'", message.Sender, message.Content);
            }

            Console.WriteLine();
        }
    }
}