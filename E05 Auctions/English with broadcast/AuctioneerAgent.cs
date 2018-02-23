/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: English auction with broadcast using the ActressMas      *
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
using System.Timers;

namespace EnglishAuction
{
    public class AuctioneerAgent : Agent
    {
        private string highestBidder;
        private int highestBid;
        private Timer timer;

        public AuctioneerAgent()
        {
            timer = new Timer();

            timer.Elapsed += t_Elapsed;
            timer.Interval = 5000;
            timer.Start();
        }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            Send(this.Name, "wake-up");
            timer.Stop();
        }

        public override void Setup()
        {
            highestBidder = "";
            highestBid = Utils.ReservePrice;

            Broadcast("start");
        }

        private void Broadcast(string messageContent)
        {
            foreach (Agent a in this.Environment.Agents)
            {
                if (a.Name != this.Name)
                    Send(a.Name, messageContent);
            }
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                string action; string parameters;
                Utils.ParseMessage(message.Content, out action, out parameters);

                switch (action)
                {
                    case "bid":
                        HandleBid(message.Sender, Convert.ToInt32(parameters));
                        break;

                    case "wake-up":
                        HandleWakeUp();
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void HandleBid(string sender, int bid)
        {
            if (bid > highestBid)
            {
                highestBid = bid;
                highestBidder = sender;
                Console.WriteLine("[auctioneer]: Best bid is {0} by {1}", highestBid, highestBidder);
            }
        }

        private void HandleWakeUp()
        {
            Console.WriteLine("[auctioneer]: Auction finished");
            Broadcast(Utils.Str("winner", highestBidder));
            Stop();
        }
    }
}