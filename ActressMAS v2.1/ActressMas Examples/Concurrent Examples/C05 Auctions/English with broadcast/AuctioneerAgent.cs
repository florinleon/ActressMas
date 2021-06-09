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
    public class AuctioneerAgent : ConcurrentAgent
    {
        private string _highestBidder;
        private int _highestBid;
        private Timer _timer;

        public AuctioneerAgent()
        {
            _timer = new Timer();

            _timer.Elapsed += t_Elapsed;
            _timer.Interval = 5000;
            _timer.Start();
        }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            Send(this.Name, "wake-up");
            _timer.Stop();
        }

        public override void Setup()
        {
            _highestBidder = "";
            _highestBid = Utils.ReservePrice;

            Broadcast("start");
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
            if (bid > _highestBid)
            {
                _highestBid = bid;
                _highestBidder = sender;
                Console.WriteLine("[auctioneer]: Best bid is {0} by {1}", _highestBid, _highestBidder);
            }
        }

        private void HandleWakeUp()
        {
            Console.WriteLine("[auctioneer]: Auction finished");
            Broadcast(Utils.Str("winner", _highestBidder));
            Stop();
        }
    }
}