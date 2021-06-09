/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Mechanism design using the ActressMas framework          *
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

namespace MechanismDesign
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new ConcurrentEnvironment();

            Utils.AisLying = false;
            var agentA = new BeneficiaryAgent(); env.Add(agentA, "a"); agentA.Start();
            var agentB = new BeneficiaryAgent(); env.Add(agentB, "b"); agentB.Start();
            var agentC = new BeneficiaryAgent(); env.Add(agentC, "c"); agentC.Start();

            Thread.Sleep(100);

            var dmAgent = new DecisionMakerAgent(); env.Add(dmAgent, "dm"); dmAgent.Start();

            env.WaitAll();

            Thread.Sleep(1000);
            Console.WriteLine("\r\n");

            Utils.AisLying = true;
            env.Add(agentA, "a"); agentA.Start();
            env.Add(agentB, "b"); agentB.Start();
            env.Add(agentC, "c"); agentC.Start();

            Thread.Sleep(100);

            env.Add(dmAgent, "dm"); dmAgent.Start();

            env.WaitAll();
        }
    }
}