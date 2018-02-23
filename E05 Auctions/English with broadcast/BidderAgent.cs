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
        private int valuation;
        private int currentBid;

        public BidderAgent(int val)
        {
            valuation = val;
            currentBid = Utils.MinPrice;
        }

        public override void Setup()
        {
            Console.WriteLine("[{0}]: My valuation is {1}", this.Name, valuation);
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
                string action; string parameters;
                Utils.ParseMessage(message.Content, out action, out parameters);

                switch (action)
                {
                    case "start":
                        Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);
                        HandleStart();
                        break;

                    case "bid":
                        //Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);
                        Console.Write("."); // bids all-to-all
                        HandleBid(Convert.ToInt32(parameters));
                        break;

                    case "winner":
                        Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);
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

        private void HandleStart()
        {
            Broadcast(Utils.Str("bid", currentBid));
        }

        private void HandleBid(int receivedBid)
        {
            int next = receivedBid + Utils.Increment;
            if (receivedBid >= currentBid && next <= valuation)
            {
                currentBid = next;
                Broadcast(Utils.Str("bid", next));
            }
        }

        private void HandleWinner(string winner)
        {
            if (winner == this.Name)
            {
                Console.WriteLine("[{0}]: I won with {1}", this.Name, currentBid);
                Stop();
            }
            else
                Stop();
        }
    }
}