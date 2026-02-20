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
        self._bidders = []
        self._highest_bidder = None
        self._current_price = 0
        self._turns_to_wait = 0


    def setup(self):
        self._bidders = []
        self._highest_bidder = None
        self._current_price = Settings.ReservePrice

        self.broadcast(f"price {self._current_price}")
        self._turns_to_wait = 2


    def act(self, message):
        try:
            print(f"\t{message.format()}")

            action, parameters = message.parse_to_string()

            if action == "bid":
                self._handle_bid(message.sender)
        except Exception as ex:
            print(ex)


    def act_default(self):
        self._turns_to_wait -= 1
        if self._turns_to_wait <= 0:
            self._handle_no_bids()


    def _handle_bid(self, sender):
        self._bidders.append(sender)


    def _handle_no_bids(self):
        if len(self._bidders) == 0:
            # no more bids; decrease price to previous level
            self._current_price -= Settings.Increment
            if self._current_price < Settings.ReservePrice:
                print("[auctioneer]: Auction finished. No winner.")
                self.broadcast("winner none")
            else:
                print(f"[auctioneer]: Auction finished. Sold to {self._highest_bidder} for price {self._current_price}.")
                self.broadcast(f"winner {self._highest_bidder}")
            self.stop()
        elif len(self._bidders) == 1:
            self._highest_bidder = self._bidders[0]
            print(f"[auctioneer]: Auction finished. Sold to {self._highest_bidder} for price {self._current_price}")
            self.broadcast(f"winner {self._highest_bidder}")
            self.stop()
        else:
            # more than one bidder; increase price and continue with these bidders only
            self._highest_bidder = self._bidders[0]  # first or random from previous round, breaking ties
            self._current_price += Settings.Increment

            for bidder in self._bidders:
                self.send(bidder, f"price {self._current_price}")

            self._bidders.clear()
            self._turns_to_wait = 2


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

            if action == "price":
                current_price = int(parameters) if parameters else 0
                self._handle_price(current_price)
            elif action == "winner":
                self._handle_winner(parameters)
        except Exception as ex:
            print(ex)


    def _handle_price(self, current_price):
        if current_price <= self._valuation:
            self.send("auctioneer", "bid")
        else:
            self.stop()


    def _handle_winner(self, winner):
        if winner == self.name:
            print(f"[{self.name}]: I have won.")

        self.stop()


def main():
    # More efficient, each bidder only talks to the auctioneer
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
