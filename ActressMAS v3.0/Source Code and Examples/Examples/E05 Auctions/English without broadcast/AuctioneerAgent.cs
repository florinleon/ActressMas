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

namespace EnglishAuction
{
    public class AuctioneerAgent : Agent
    {
        private List<string> _bidders;
        private string _highestBidder;
        private int _currentPrice;
        private int _turnsToWait;

        public override void Setup()
        {
            _bidders = new List<string>();

            _currentPrice = Settings.ReservePrice;

            Broadcast($"price {_currentPrice}");
            _turnsToWait = 2;
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");

                message.Parse(out string action, out string parameters);

                switch (action)
                {
                    case "bid":
                        HandleBid(message.Sender);
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

        public override void ActDefault()
        {
            if (--_turnsToWait <= 0)
                HandleNoBids();
        }

        private void HandleBid(string sender)
        {
            _bidders.Add(sender);
        }

        private void HandleNoBids()
        {
            if (_bidders.Count == 0) // no more bids
            {
                _currentPrice -= Settings.Increment;
                if (_currentPrice < Settings.ReservePrice)
                {
                    Console.WriteLine("[auctioneer]: Auction finished. No winner.");
                    Broadcast("winner none");
                }
                else
                {
                    Console.WriteLine($"[auctioneer]: Auction finished. Sold to {_highestBidder} for price {_currentPrice}.");
                    Broadcast($"winner {_highestBidder}");
                }
                Stop();
            }
            else if (_bidders.Count == 1)
            {
                _highestBidder = _bidders[0];
                Console.WriteLine($"[auctioneer]: Auction finished. Sold to {_highestBidder} for price {_currentPrice}");
                Broadcast($"winner {_highestBidder}");
                Stop();
            }
            else
            {
                _highestBidder = _bidders[0]; // first or random from the previous round, breaking ties
                _currentPrice += Settings.Increment;

                foreach (string a in _bidders)
                    Send(a, $"price {_currentPrice}");

                _bidders.Clear();
                _turnsToWait = 2;
            }
        }
    }
}