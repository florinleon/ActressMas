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
using System.Collections.Generic;

namespace EnglishAuction
{
    public class AuctioneerAgent : TurnBasedAgent
    {
        private string _highestBidder;
        private int _highestBid;

        public override void Setup()
        {
            _highestBidder = "";
            _highestBid = Utils.ReservePrice;

            Broadcast("start");
        }

        public override void Act(Queue<Message> messages)
        {
            try
            {
                if (messages.Count == 0)
                {
                    HandleFinish();
                    return;
                }

                while (messages.Count > 0)
                {
                    Message message = messages.Dequeue();
                    Console.WriteLine("\r\n\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                    string action; string parameters;
                    Utils.ParseMessage(message.Content, out action, out parameters);

                    switch (action)
                    {
                        case "bid":
                            HandleBid(message.Sender, Convert.ToInt32(parameters));
                            break;

                        default:
                            break;
                    }
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

        private void HandleFinish()
        {
            Console.WriteLine("[auctioneer]: Auction finished");
            Broadcast(Utils.Str("winner", _highestBidder));
            Stop();
        }
    }
}