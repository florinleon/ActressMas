/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Voting using the ActressMas framework                    *
 *  Copyright:   (c) 2021, Florin Leon                                    *
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

namespace Voting
{
    public class Program
    {
        public static Random _rand = new Random();

        private static void Main(string[] args)
        {
            // select the Condorcet winner if one exists, otherwise use Borda count

            var env = new EnvironmentMas(parallel: false, randomOrder: false);

            var auctioneerAgent = new TellerAgent();
            env.Add(auctioneerAgent, "teller");

            env.Memory["NoCandidates"] = 5;

            int noVoters = 99;
            for (int i = 1; i <= noVoters; i++)
            {
                var voterAgent = new VoterAgent(_rand);
                env.Add(voterAgent, $"voter{i:D2}");
            }

            env.Start();
        }
    }
}