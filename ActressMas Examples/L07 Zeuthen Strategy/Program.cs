/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Zeuthen strategy using the ActressMas framework          *
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

using System.Threading;

namespace Zeuthen
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new ActressMas.Environment();
            var agent1 = new BargainingAgent(); env.Add(agent1, "agent1");
            var agent2 = new BargainingAgent(); env.Add(agent2, "agent2");
            agent2.Start(); Thread.Sleep(100); agent1.Start();

            env.WaitAll();
        }
    }
}