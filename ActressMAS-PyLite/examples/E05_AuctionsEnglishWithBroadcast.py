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
        self._highest_bidder = ""
        self._highest_bid = 0
        self._turns_to_wait = 0


    def setup(self):
        self._highest_bidder = ""
        self._highest_bid = 0
        self._turns_to_wait = 2
        self.broadcast("start")


    def act(self, message):
        try:
            print(f"\t{message.format()}")
            action, parameters = message.parse_to_string()
            self._turns_to_wait = 2

            if action == "bid":
                self._handle_bid(message.sender, int(parameters) if parameters else 0)
        except Exception as ex:
            print(ex)


    def act_default(self):
        self._turns_to_wait -= 1
        if self._turns_to_wait <= 0:
            self._handle_finish()


    def _handle_bid(self, sender, bid):
        if bid > self._highest_bid and bid >= Settings.ReservePrice:
            self._highest_bid = bid
            self._highest_bidder = sender


    def _handle_finish(self):
        print("[auctioneer]: Auction finished")

        if self._highest_bidder != "":
            self.broadcast(f"winner {self._highest_bidder}")
        else:
            self.broadcast("winner none")

        self.stop()


class BidderAgent(Agent):

    def __init__(self, valuation):
        super().__init__()
        self._valuation = valuation
        self._current_bid = 0

        if self._valuation >= Settings.ReservePrice:
            self._current_bid = Settings.ReservePrice
            self._participating = True
        else:
            self._participating = False


    def setup(self):
        print(f"[{self.name}]: My valuation is {self._valuation}")


    def act(self, message):
        try:
            if not self._participating:
                self.stop()
                return

            action, parameters = message.parse_to_string()

            if action == "start":
                print(f"\n\t{message.format()}")
                self._handle_start()

            elif action == "bid":
                # Each bidder reacts to every bid it receives.
                bid = int(parameters) if parameters else 0
                highest_bid = 0

                if bid > highest_bid:
                    highest_bid = bid

                if highest_bid > 0:
                    self._handle_bid(highest_bid)

            elif action == "winner":
                print(f"\n\t{message.format()}")
                self._handle_winner(parameters)
        except Exception as ex:
            print(ex)


    def _handle_start(self):
        if self._current_bid > 0:
            self.broadcast(f"bid {self._current_bid}")


    def _handle_bid(self, received_bid):
        next_bid = received_bid + Settings.Increment
        if received_bid >= self._current_bid and next_bid <= self._valuation:
            self._current_bid = next_bid
            self.broadcast(f"bid {next_bid}")


    def _handle_winner(self, winner):
        if winner == self.name:
            print(f"[{self.name}]: I have won with {self._current_bid}")

        self.stop()


def main():
    # Inefficient, uses broadcast to simulate public open-cry auction
    env = EnvironmentMas(random_order=False)

    auctioneer_agent = AuctioneerAgent()
    env.add(auctioneer_agent, "auctioneer")

    rand = random.Random()

    for i in range(1, Settings.NoBidders + 1):
        agent_valuation = Settings.MinPrice + rand.randrange(Settings.MaxPrice - Settings.MinPrice)
        bidder_agent = BidderAgent(agent_valuation)
        env.add(bidder_agent, f"bidder{i:02d}")

    env.start()


if __name__ == "__main__":
    main()
