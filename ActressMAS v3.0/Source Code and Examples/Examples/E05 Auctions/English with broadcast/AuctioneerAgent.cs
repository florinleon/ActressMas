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

namespace EnglishAuction
{
    public class AuctioneerAgent : Agent
    {
        private string _highestBidder;
        private int _highestBid;
        private int _turnsToWait;

        public override void Setup()
        {
            _highestBidder = "";
            _highestBid = 0;
            _turnsToWait = 2;

            Broadcast("start");
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out string parameters);
                _turnsToWait = 2;

                switch (action)
                {
                    case "bid":
                        HandleBid(message.Sender, Convert.ToInt32(parameters));
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
                HandleFinish();
        }

        private void HandleBid(string sender, int bid)
        {
            if (bid > _highestBid && bid >= Settings.ReservePrice)
            {
                _highestBid = bid;
                _highestBidder = sender;
            }
        }

        private void HandleFinish()
        {
            Console.WriteLine("[auctioneer]: Auction finished");

            if (_highestBidder != "")
                Broadcast($"winner {_highestBidder}");
            else
                Broadcast("winner none");

            Stop();
        }
    }
}