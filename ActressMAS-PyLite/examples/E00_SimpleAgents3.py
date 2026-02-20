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


def main():
    env = MyEnvironment(no_turns=10, delay_after_turn=100)
    # env = MyEnvironment(no_turns=10, delay_after_turn=100, random_order=False)

    no_agents = 4

    for i in range(1, no_agents + 1):
        a = MyAgent()
        env.add(a, f"agent{i}")

    m = MonitorAgent()
    env.add(m, "monitor")

    env.start()


class MyEnvironment(EnvironmentMas):
    def __init__(self, no_turns = 0, delay_after_turn = 0, random_order = True, rand = None):
        super().__init__(no_turns=no_turns, delay_after_turn_ms=delay_after_turn, random_order=random_order, rand=rand)


    def turn_finished(self, turn):
        print(f"\nTurn {turn + 1} finished\n")


    def simulation_finished(self):
        print("\nSimulation finished\n")


class MyAgent(Agent):
    def act(self, message):
        # print(f"{self.name} acting")
        print(f"\t{message.format()}")

        if message.sender == "monitor" and (message.content == "start" or message.content == "continue"):
            self.send("monitor", "done")


class MonitorAgent(Agent):
    _rand = random.Random()

    def __init__(self):
        super().__init__()
        self._round = 0
        self._max_rounds = 0
        self._finished = {}
        self._agent_names = []


    def setup(self):
        self._finished = {}
        self._agent_names = []
        self._max_rounds = 3
        self._round = 0

        print("monitor: start round 1 in setup")

        if self.environment is None:
            raise RuntimeError("MonitorAgent has no environment associated")

        for a in self.environment.filtered_agents("agent"):
            self._agent_names.append(a)
            self._finished[a] = False

        for name in self._agent_names:
            print(f"-> sending to {name}")
            self.send(name, "start")


    def act(self, message):
        print(f"\t{message.format()}")

        if message.content == "done":
            self._finished[message.sender] = True

        if self._all_finished():
            self._round += 1
            if self._round >= self._max_rounds:
                return

            # print(f"{self.name} acting scenario")

            for name in self._agent_names:
                self._finished[name] = False

            print(f"\r\nmonitor: start round {self._round + 1} in act")

            for name in self._agent_names:
                print(f"-> sending to {name}")
                self.send(name, "continue")


    def _all_finished(self):
        return all(self._finished[a] for a in self._finished)


if __name__ == "__main__":
    main()
