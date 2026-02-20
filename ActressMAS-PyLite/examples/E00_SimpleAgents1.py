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
import time
import random


def main():
    env = EnvironmentMas(no_turns=100)

    a1 = MyAgent()
    env.add(a1, "agent1")

    a2 = MyAgent()
    env.add(a2, "agent2")

    monitor = MonitorAgent()
    env.add(monitor, "monitor")

    env.start()


class MyAgent(Agent):
    _rand = random.Random()

    def setup(self):
        print(f"{self.name} starting")

        for i in range(1, 11):
            print(f"{self.name} sending {i}")
            self.send("monitor", f"{i}")

            dt = 10 + self._rand.randrange(90)
            time.sleep(dt / 1000.0)  # miliseconds


class MonitorAgent(Agent):
    def act(self, message):
        print(f"{self.name} has received {message.format()}")


if __name__ == "__main__":
    main()
