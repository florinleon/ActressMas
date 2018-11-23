/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Vickrey auction using the ActressMas framework           *
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
using System.Timers;

namespace VickreyAuction
{
    public class AuctioneerAgent : ConcurrentAgent
    {
        private struct Bid
        {
            public string Bidder { get; set; }
            public int BidValue { get; set; }

            public Bid(string bidder, int bidValue)
                : this()
            {
                Bidder = bidder;
                BidValue = bidValue;
            }
        }

        private List<Bid> _bids;
        private Timer _timer;

        public AuctioneerAgent()
        {
            _bids = new List<Bid>();

            _timer = new Timer();
            _timer.Elapsed += t_Elapsed;
            _timer.Interval = Utils.Delay;
        }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            Send(this.Name, "wake-up");
        }

        public override void Setup()
        {
            Broadcast("start");
            _timer.Start();
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

        private void HandleBid(string sender, int price)
        {
            _bids.Add(new Bid(sender, price));
        }

        private void HandleWakeUp()
        {
            string highestBidder = "";
            int highestBid = int.MinValue;
            int[] bidValues = new int[_bids.Count];

            for (int i = 0; i < _bids.Count; i++)
            {
                int b = _bids[i].BidValue;
                if (b > highestBid && b >= Utils.ReservePrice)
                {
                    highestBid = b;
                    highestBidder = _bids[i].Bidder;
                }
                bidValues[i] = b;
            }

            if (highestBidder == "") // no bids above reserve price
            {
                Console.WriteLine("[auctioneer]: Auction finished. No winner.");
                Broadcast(Utils.Str("winner", "none"));
            }
            else
            {
                Array.Sort(bidValues);
                Array.Reverse(bidValues);
                int winningPrice = bidValues[1]; // second price

                Console.WriteLine("[auctioneer]: Auction finished. Sold to {0} for price {1}.", highestBidder, winningPrice);
                Broadcast(Utils.Str("winner", highestBidder));
            }

            _timer.Stop();
            Stop();
        }
    }
}