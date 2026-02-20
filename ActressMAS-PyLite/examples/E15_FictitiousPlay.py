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
import math
import random


class Game:
    # Prisoner's Dilemma:
    #                 Opponent
    #               C         D
    #        C    (3,3)     (0,5)
    # Me:
    #        D    (5,0)     (1,1)

    ActionA = "C"  # cooperate
    ActionB = "D"  # defect

    Actions = [ActionA, ActionB]

    @staticmethod
    def payoff(my_action, other_action):
        # Mutual cooperation: (C, C) -> 3
        if my_action == Game.ActionA and other_action == Game.ActionA:
            return 3.0

        # Mutual defection: (D, D) -> 1
        if my_action == Game.ActionB and other_action == Game.ActionB:
            return 1.0

        # I cooperate, opponent defects: (C, D) -> 0
        if my_action == Game.ActionA and other_action == Game.ActionB:
            return 0.0

        # I defect, opponent cooperates: (D, C) -> 5
        if my_action == Game.ActionB and other_action == Game.ActionA:
            return 5.0

        raise ValueError("Unknown actions.")


class GameAgent(Agent):

    def __init__(self, player1_name, player2_name, max_rounds):
        super().__init__()
        self._player1Name = player1_name
        self._player2Name = player2_name
        self._maxRounds = max_rounds
        self._currentRound = 0
        self._currentActions = {}
        self._totalPayoff1 = 0.0
        self._totalPayoff2 = 0.0


    def setup(self):
        self._currentRound = 0
        self._currentActions = {}
        self._totalPayoff1 = 0.0
        self._totalPayoff2 = 0.0
        print(f"Starting fictitious play for {self._maxRounds} rounds between {self._player1Name} and {self._player2Name}.")
        self.start_next_round()


    def act(self, message):
        try:
            print(f"\t{message.format()}")
            action, parameters = message.parse_to_string()
            if action == "action":
                self.handle_action(message.sender, parameters)
            else:
                pass
        except Exception as ex:
            print(str(ex))


    def start_next_round(self):
        self._currentRound += 1

        if self._currentRound > self._maxRounds:
            print()
            print("Game finished.")
            print(f"Total payoff for {self._player1Name}: {self._totalPayoff1:.2f}")
            print(f"Total payoff for {self._player2Name}: {self._totalPayoff2:.2f}")
            self.send(self._player1Name, "stop")
            self.send(self._player2Name, "stop")
            self.stop()
            return

        print()
        print(f"--- Round {self._currentRound} ---")

        self._currentActions[self._player1Name] = None
        self._currentActions[self._player2Name] = None

        self.send(self._player1Name, f"play {self._currentRound}")
        self.send(self._player2Name, f"play {self._currentRound}")


    def handle_action(self, sender, parameters):
        action = parameters.strip()
        self._currentActions[sender] = action

        if self._currentActions.get(self._player1Name) is not None and self._currentActions.get(self._player2Name) is not None:
            a1 = self._currentActions[self._player1Name]
            a2 = self._currentActions[self._player2Name]

            payoff1 = Game.payoff(a1, a2)
            payoff2 = Game.payoff(a2, a1)

            self._totalPayoff1 += payoff1
            self._totalPayoff2 += payoff2

            print(f"Joint action: {self._player1Name}={a1}, {self._player2Name}={a2} | payoffs: {payoff1:.2f}, {payoff2:.2f}")

            self.send(self._player1Name, f"result {a2} {payoff1}")
            self.send(self._player2Name, f"result {a1} {payoff2}")

            self.start_next_round()


class PlayerAgent(Agent):

    def __init__(self, game_agent_name, opponent_name):
        super().__init__()
        self._gameAgentName = game_agent_name
        self._opponentName = opponent_name
        self._opponentActionCounts = None
        self._myActionCounts = None
        self._rand = random.Random()
        self._round = 0


    def setup(self):
        self._opponentActionCounts = {}
        for a in Game.Actions:
            self._opponentActionCounts[a] = 1  # Laplace smoothing

        self._myActionCounts = {}
        for a in Game.Actions:
            self._myActionCounts[a] = 0

        self._round = 0


    def act(self, message):
        try:
            print(f"\t{message.format()}")
            action, parameters = message.parse_to_string()

            if action == "play":
                self.handle_play(parameters)
            elif action == "result":
                self.handle_result(parameters)
            elif action == "stop":
                print(f"{self.name}: stopping.")
                self.print_strategy_probabilities()
                self.stop()
            else:
                pass
        except Exception as ex:
            print(str(ex))


    def handle_play(self, parameters):
        self._round += 1

        chosen_action = self.choose_action()

        if chosen_action in self._myActionCounts:
            self._myActionCounts[chosen_action] += 1
        else:
            self._myActionCounts[chosen_action] = 1

        print(f"{self.name} (round {self._round}) plays {chosen_action}")

        self.send(self._gameAgentName, f"action {chosen_action}")


    def print_strategy_probabilities(self):
        # Belief about opponent strategy (based on counts, includes Laplace smoothing)
        opp_total = 0
        for v in self._opponentActionCounts.values():
            opp_total += v

        oppPA = float(self._opponentActionCounts[Game.ActionA]) / opp_total
        oppPB = float(self._opponentActionCounts[Game.ActionB]) / opp_total

        # Empirical strategy based on my own play history (does not include smoothing)
        my_total = 0
        for v in self._myActionCounts.values():
            my_total += v

        myPA = float(self._myActionCounts[Game.ActionA]) / my_total if my_total > 0 else 0.0
        myPB = float(self._myActionCounts[Game.ActionB]) / my_total if my_total > 0 else 0.0

        print(f"{self.name}: belief about {self._opponentName}: P(A)={oppPA:.2f}, P(B)={oppPB:.2f} "
              f"[counts A={self._opponentActionCounts[Game.ActionA]}, B={self._opponentActionCounts[Game.ActionB]}]")

        print(f"{self.name}: empirical strategy: P(A)={myPA:.2f}, P(B)={myPB:.2f} "
              f"[counts A={self._myActionCounts[Game.ActionA]}, B={self._myActionCounts[Game.ActionB]}, rounds={my_total}]")


    def handle_result(self, parameters):
        # parameters: "<opponentAction> <myPayoff>"
        toks = parameters.strip().split()

        if len(toks) >= 1:
            opponent_action = toks[0]

            if opponent_action in self._opponentActionCounts:
                self._opponentActionCounts[opponent_action] += 1
            else:
                self._opponentActionCounts[opponent_action] = 1

            payoff = 0.0
            if len(toks) >= 2:
                payoff = float(toks[1])

            print(f"{self.name} observes opponent played {opponent_action}, payoff = {payoff:.2f}")


    def choose_action(self):
        total = 0
        for v in self._opponentActionCounts.values():
            total += v

        pA = float(self._opponentActionCounts[Game.ActionA]) / total
        pB = float(self._opponentActionCounts[Game.ActionB]) / total

        expectedA = pA * Game.payoff(Game.ActionA, Game.ActionA) + pB * Game.payoff(Game.ActionA, Game.ActionB)
        expectedB = pA * Game.payoff(Game.ActionB, Game.ActionA) + pB * Game.payoff(Game.ActionB, Game.ActionB)

        if math.fabs(expectedA - expectedB) < 1e-6:  # tie: randomize
            return Game.ActionA if self._rand.random() < 0.5 else Game.ActionB

        return Game.ActionA if expectedA > expectedB else Game.ActionB


def main():
    env = EnvironmentMas(random_order = False)

    game_agent_name = "game"
    player1_name = "player1"
    player2_name = "player2"
    max_rounds = 50

    game_agent = GameAgent(player1_name, player2_name, max_rounds)
    env.add(game_agent, game_agent_name)

    player1 = PlayerAgent(game_agent_name, player2_name)
    env.add(player1, player1_name)

    player2 = PlayerAgent(game_agent_name, player1_name)
    env.add(player2, player2_name)

    env.start()


if __name__ == "__main__":
    main()
    