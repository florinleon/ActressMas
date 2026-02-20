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


class Game:
    """Symmetric normal-form game payoff matrix A, where
    A[i,j] is the payoff of pure strategy i against pure strategy j.
    Must be square and have strictly positive payoffs for the basic
    x_i' = x_i * (u_i / u_bar) replicator rule.
    """
    payoff_matrix = None


    @staticmethod
    def no_strategies():
        return len(Game.payoff_matrix)


    @staticmethod
    def compute_expected_payoffs(x):
        n = Game.no_strategies()
        u = [0.0 for _ in range(n)]

        for i in range(n):
            ui = 0.0
            for j in range(n):
                ui += Game.payoff_matrix[i][j] * x[j]
            u[i] = ui

        return u


    @staticmethod
    def compute_average_payoff(x, u):
        u_bar = 0.0
        for i in range(len(x)):
            u_bar += x[i] * u[i]
        return u_bar


class GameAgent(Agent):

    def __init__(self, strategy_names, max_rounds):
        super().__init__()
        self._strategy_names = list(strategy_names)
        self._no_strategies = len(self._strategy_names)
        self._max_rounds = max_rounds
        self._name_to_index = {}
        self._current_proportions = {}
        self._current_round = 0

        for i in range(self._no_strategies):
            self._name_to_index[self._strategy_names[i]] = i


    def setup(self):
        self._current_round = 0
        self._current_proportions = {}
        for name in self._strategy_names:
            self._current_proportions[name] = float("nan")

        print(f"Replicator dynamics with {self._no_strategies} strategies for {self._max_rounds} rounds.")
        self.start_next_round()


    def act(self, message):
        try:
            action, parameters = message.parse_to_string()

            if action == "state":
                self.handle_state(message.sender, parameters)
            else:
                pass
        except Exception as ex:
            print(f"GameAgent error: {ex}")


    def start_next_round(self):
        self._current_round += 1

        if self._current_round > self._max_rounds:
            print()
            print("Simulation finished.")
            for name in self._strategy_names:
                self.send(name, "stop")
            self.stop()
            return

        print()
        print(f"--- Round {self._current_round} ---")

        for name in self._strategy_names:
            self._current_proportions[name] = float("nan")

        for name in self._strategy_names:
            self.send(name, f"tick {self._current_round}")


    def handle_state(self, sender, parameters):
        xi = float(parameters.strip())
        self._current_proportions[sender] = xi

        if not self.all_proportions_received():
            return

        x = [0.0 for _ in range(self._no_strategies)]
        for i in range(self._no_strategies):
            name = self._strategy_names[i]
            x[i] = self._current_proportions[name]

        s = 0.0
        for v in x:
            s += v

        if s <= 0.0:
            print("Population proportions sum to non-positive value.")
            for name in self._strategy_names:
                self.send(name, "stop")
            self.stop()
            return

        if abs(s - 1.0) > 1e-6:
            for i in range(self._no_strategies):
                x[i] /= s
            print(f"State normalized: sum before normalization = {s:.2f}")

        u = Game.compute_expected_payoffs(x)
        u_bar = Game.compute_average_payoff(x, u)

        print("x = [" + ", ".join([f"{x[i]:.2f}" for i in range(self._no_strategies)]) + "]")
        print("u = [" + ", ".join([f"{u[i]:.2f}" for i in range(self._no_strategies)]) + "]")
        print(f"ubar = {u_bar:.2f}")

        for i in range(self._no_strategies):
            name = self._strategy_names[i]
            msg = f"payoff {u[i]} {u_bar}"
            self.send(name, msg)

        self.start_next_round()


    def all_proportions_received(self):
        for name in self._strategy_names:
            if math.isnan(self._current_proportions[name]):
                return False
        return True


class StrategyAgent(Agent):

    def __init__(self, coordinator_name, index, initial_proportion):
        super().__init__()
        self._coordinator_name = coordinator_name
        self._index = index
        self._proportion = initial_proportion


    def setup(self):
        print(f"{self.name} (strategy {self._index}) initial proportion = {self._proportion:.4f}")


    def act(self, message):
        try:
            action, parameters = message.parse_to_string()

            if action == "tick":
                self.handle_tick(parameters)
            elif action == "payoff":
                self.handle_payoff(parameters)
            elif action == "stop":
                self.handle_stop()
            else:
                pass
        except Exception as ex:
            print(f"{self.name} error: {ex}")


    def handle_tick(self, parameters):
        # parameters: time step (optional, not used here)
        self.send(self._coordinator_name, f"state {self._proportion:.4f}")


    def handle_payoff(self, parameters):
        # parameters: "<ui> <ubar>"
        toks = parameters.strip().split()

        if len(toks) < 2:
            print(f"{self.name}: payoff message malformed: '{parameters}'")
            return

        ui = float(toks[0])
        u_bar = float(toks[1])

        if u_bar <= 0.0:
            print(f"{self.name}: average payoff non-positive ({u_bar}), proportion not updated.")
            return

        old_proportion = self._proportion

        # Discrete-time replicator update: x_i' = x_i * (u_i / u_bar)
        self._proportion = self._proportion * (ui / u_bar)

        print(f"{self.name}: ui = {ui:.2f}, ubar = {u_bar:.2f}, x(old) = {old_proportion:.4f}, x(new) = {self._proportion:.2f}")


    def handle_stop(self):
        print(f"Final proportion for {self.name}: {self._proportion:.2f}")
        self.stop()


def main():
    # 2x2 coordination game
    #          A     B
    #   A    3.0   0.0
    #   B    0.0   3.0
    #
    # Payoffs are strictly positive, which matches the simple replicator rule.

    Game.payoff_matrix = [[3.0, 0.0], [0.0, 3.0]]

    n = Game.no_strategies()

    # initial distribution over strategies; must sum to 1.0
    initial_proportions = [0.5, 0.5]

    s = 0.0
    for v in initial_proportions:
        s += v

    env = EnvironmentMas()

    strategy_names = ["" for _ in range(n)]
    for i in range(n):
        strategy_names[i] = f"S{i}"

    max_rounds = 50

    game_agent = GameAgent(strategy_names, max_rounds)
    env.add(game_agent, "game")

    for i in range(n):
        agent = StrategyAgent("game", i, initial_proportions[i])
        env.add(agent, strategy_names[i])

    env.start()


if __name__ == "__main__":
    main()
