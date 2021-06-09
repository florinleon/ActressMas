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

namespace Agents3
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new TurnBasedEnvironment(5, 100);
            int noAgents = 10;
            for (int i = 1; i <= noAgents; i++)
                env.Add(new MyAgent(), $"agent{i}");
            env.Start();
        }
    }

    public class MyAgent : TurnBasedAgent
    {
        public override void Act(Queue<Message> messages)
        {
            Console.WriteLine(this.Name);
        }
    }
}