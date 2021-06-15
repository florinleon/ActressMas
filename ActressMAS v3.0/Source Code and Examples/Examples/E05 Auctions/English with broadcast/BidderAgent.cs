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
    public class BidderAgent : Agent
    {
        private int _valuation;
        private int _currentBid;
        private bool _participating;

        public BidderAgent(int val)
        {
            _valuation = val;

            if (_valuation >= Settings.ReservePrice)
            {
                _currentBid = Settings.ReservePrice;
                _participating = true;
            }
            else
                _participating = false;
        }

        public override void Setup()
        {
            Console.WriteLine($"[{Name}]: My valuation is {_valuation}");
        }

        public override void Act(Message message)
        {
            try
            {
                if (!_participating)
                    Stop();

                int highestBid = 0;

                message.Parse(out string action, out string parameters);

                switch (action)
                {
                    case "start":
                        Console.WriteLine($"\r\n\t{message.Format()}");
                        HandleStart();
                        break;

                    case "bid":
                        Console.Write("."); // bids all-to-all
                        int bid = Convert.ToInt32(parameters);
                        if (bid > highestBid)
                            highestBid = bid;
                        if (highestBid > 0)
                            HandleBid(highestBid);
                        break;

                    case "winner":
                        Console.WriteLine($"\r\n\t{message.Format()}");
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

        public override void ActDefault()
        {
            if (!_participating)
                Stop();
        }

        private void HandleStart()
        {
            if (_currentBid > 0)
                Broadcast($"bid {_currentBid}");
        }

        private void HandleBid(int receivedBid)
        {
            int next = receivedBid + Settings.Increment;
            if (receivedBid >= _currentBid && next <= _valuation)
            {
                _currentBid = next;
                Broadcast($"bid {next}");
            }
        }

        private void HandleWinner(string winner)
        {
            if (winner == Name)
                Console.WriteLine($"[{Name}]: I have won with {_currentBid}");

            Stop();
        }
    }
}