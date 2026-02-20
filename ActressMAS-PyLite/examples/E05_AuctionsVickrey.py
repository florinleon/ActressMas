"""
/**************************************************************************
 *                                                                        *
 *  Description: Example of using the ActressMas framework                *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Copyright:   (c) 2026, Florin Leon                                    *
 *                                                                        *
 *  This program is free software; you can redistribute it and/or modify  *
 *  it under the terms of the GNU General Public License as published by  *
 *  the Free Software Foundation. This program is distributed in the      *
 *  hope that it will be useful, but WITHOUT ANY WARRANTY; without even   *
 *  the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR   *
 *  PURPOSE. See the GNU General Public License for more details.         *
 *                                                                        *
 **************************************************************************/
"""

from ActressMas import *
import random


class Settings:
    NoBidders = 5
    ReservePrice = 100
    MinPrice = 50
    MaxPrice = 500
    Increment = 10


class AuctioneerAgent(Agent):

    def __init__(self):
        super().__init__()
        self._bids = []
        self._turns_to_wait = 0


    def setup(self):
        self.broadcast("start")
        self._turns_to_wait = 2


    def act(self, message):
        try:
            print(f"\t{message.format()}")
            action, parameters = message.parse_to_string()

            if action == "bid":
                price = int(parameters) if parameters else 0
                self._handle_bid(message.sender, price)
        except Exception as ex:
            print(ex)


    def act_default(self):
        self._turns_to_wait -= 1
        if self._turns_to_wait <= 0:
            self._handle_finish()


    def _handle_bid(self, sender, price):
        self._bids.append((sender, price))


    def _handle_finish(self):
        highest_bidder = ""
        highest_bid = float("-inf")
        bid_values = []

        for bidder, bid in self._bids:
            if bid > highest_bid and bid >= Settings.ReservePrice:
                highest_bid = bid
                highest_bidder = bidder

            bid_values.append(bid)

        if highest_bidder == "":
            print("[auctioneer]: Auction finished. No winner.")
            self.broadcast("winner none")
        else:
            bid_values.sort(reverse=True)
            winning_price = bid_values[1]  # second price

            print(f"[auctioneer]: Auction finished. Sold to {highest_bidder} for price {winning_price}.")
            self.broadcast(f"winner {highest_bidder}")

        self.stop()


class BidderAgent(Agent):

    def __init__(self, valuation):
        super().__init__()
        self._valuation = valuation


    def setup(self):
        print(f"[{self.name}]: My valuation is {self._valuation}")


    def act(self, message):
        try:
            print(f"\t{message.format()}")
            action, parameters = message.parse_to_string()

            if action == "start":
                self._handle_start()
            elif action == "winner":
                self._handle_winner(parameters)
        except Exception as ex:
            print(ex)


    def _handle_start(self):
        self.send("auctioneer", f"bid {self._valuation}")


    def _handle_winner(self, winner):
        if winner == self.name:
            print(f"[{self.name}]: I have won.")

        self.stop()


def main():
    env = EnvironmentMas()

    rand = random.Random()

    for i in range(1, Settings.NoBidders + 1):
        agent_valuation = Settings.MinPrice + rand.randrange(Settings.MaxPrice - Settings.MinPrice)
        bidder_agent = BidderAgent(agent_valuation)
        env.add(bidder_agent, f"bidder{i:02d}")

    auctioneer_agent = AuctioneerAgent()
    env.add(auctioneer_agent, "auctioneer")

    env.start()


if __name__ == "__main__":
    main()
