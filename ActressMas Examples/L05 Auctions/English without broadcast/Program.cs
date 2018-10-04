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

using System.Threading;

namespace EnglishAuction
{
    public class Program
    {
        private static void Main(string[] args)
        {
            // more efficient, each bidder only talks to the auctioneer

            var env = new ActressMas.Environment();

            for (int i = 1; i <= Utils.NoBidders; i++)
            {
                int agentValuation = Utils.MinPrice + Utils.RandNoGen.Next(Utils.MaxPrice - Utils.MinPrice);
                var bidderAgent = new BidderAgent(agentValuation);
                env.Add(bidderAgent, string.Format("bidder{0:D2}", i));
                bidderAgent.Start();
            }

            Thread.Sleep(100);

            var auctioneerAgent = new AuctioneerAgent();
            env.Add(auctioneerAgent, "auctioneer");
            auctioneerAgent.Start();

            env.WaitAll();
        }
    }
}