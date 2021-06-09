/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: English auction without broadcast using the ActressMas   *
 *               framework                                                *
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

namespace EnglishAuction
{
    public class Program
    {
        private static void Main(string[] args)
        {
            // more efficient, each bidder only talks to the auctioneer

            var env = new EnvironmentMas();

            var rand = new Random();

            for (int i = 1; i <= Settings.NoBidders; i++)
            {
                int agentValuation = Settings.MinPrice + rand.Next(Settings.MaxPrice - Settings.MinPrice);
                var bidderAgent = new BidderAgent(agentValuation);
                env.Add(bidderAgent, $"bidder{i:D2}");
            }

            var auctioneerAgent = new AuctioneerAgent();
            env.Add(auctioneerAgent, "auctioneer");

            env.Start();
        }
    }
}