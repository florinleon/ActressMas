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
using System.Collections.Generic;
using System.Timers;

namespace EnglishAuction
{
    public class AuctioneerAgent : ConcurrentAgent
    {
        private List<string> _bidders;
        private string _highestBidder;
        private int _currentPrice;
        private Timer _timer;

        public AuctioneerAgent()
        {
            _bidders = new List<string>();

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
            _currentPrice = Utils.ReservePrice;
            Broadcast(Utils.Str("price", _currentPrice));
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
                        HandleBid(message.Sender);
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

        private void HandleBid(string sender)
        {
            _bidders.Add(sender);
        }

        private void HandleWakeUp()
        {
            if (_bidders.Count == 0) // no more bids
            {
                _currentPrice -= Utils.Increment;
                if (_currentPrice < Utils.ReservePrice)
                {
                    Console.WriteLine("[auctioneer]: Auction finished. No winner.");
                    Broadcast(Utils.Str("winner", "none"));
                }
                else
                {
                    Console.WriteLine("[auctioneer]: Auction finished. Sold to {0} for price {1}.", _highestBidder, _currentPrice);
                    Broadcast(Utils.Str("winner", _highestBidder));
                }
                _timer.Stop();
                Stop();
            }
            else if (_bidders.Count == 1)
            {
                _highestBidder = _bidders[0];
                Console.WriteLine("[auctioneer]: Auction finished. Sold to {0} for price {1}", _highestBidder, _currentPrice);
                Broadcast(Utils.Str("winner", _highestBidder));
                _timer.Stop();
                Stop();
            }
            else
            {
                _highestBidder = _bidders[0]; // first or random from the previous round, breaking ties
                _currentPrice += Utils.Increment;

                foreach (string a in _bidders)
                    Send(a, Utils.Str("price", _currentPrice));

                _bidders.Clear();
            }
        }
    }
}