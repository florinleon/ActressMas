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
    public class BidderAgent : Agent
    {
        private int _valuation;

        public BidderAgent(int val)
        {
            _valuation = val;
        }

        public override void Setup()
        {
            Console.WriteLine($"[{Name}]: My valuation is {_valuation}");
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out string parameters);

                switch (action)
                {
                    case "price":
                        HandlePrice(Convert.ToInt32(parameters));
                        break;

                    case "winner":
                        HandleWinner(parameters);
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

        private void HandlePrice(int currentPrice)
        {
            if (currentPrice <= _valuation)
                Send("auctioneer", "bid");
            else
                Stop();
        }

        private void HandleWinner(string winner)
        {
            if (winner == Name)
                Console.WriteLine($"[{Name}]: I have won.");

            Stop();
        }
    }
}