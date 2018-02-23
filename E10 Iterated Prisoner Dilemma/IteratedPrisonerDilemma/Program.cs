/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Iterated Prisoner's Dilemma using ActressMas framework   *
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

namespace IteratedPrisonersDilemma
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new ActressMas.Environment();

            //var prisonerAgent1 = new ConfessPrisonerAgent();
            //var prisonerAgent2 = new RandomPrisonerAgent();
            var prisonerAgent1 = new TitForTatPrisonerAgent();
            var prisonerAgent2 = new TitForTatPrisonerAgent();

            env.Add(prisonerAgent1, "p1-" + prisonerAgent1.GetType().Name);
            prisonerAgent1.Start();

            env.Add(prisonerAgent2, "p2-" + prisonerAgent2.GetType().Name);
            prisonerAgent2.Start();

            Thread.Sleep(100);

            var policeAgent = new PoliceAgent();
            env.Add(policeAgent, "police");
            policeAgent.Start();

            while (true) { };
        }
    }
}