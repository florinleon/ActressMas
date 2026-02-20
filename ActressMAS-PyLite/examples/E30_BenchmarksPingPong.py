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


class Global:
    TimerStart = None
    NoMessages = 0
    MaxMessages = 10 * 1000 * 1000
    NoAgents = 10


class MyAgent(Agent):

    def __init__(self, id):
        super().__init__()
        self._id = id


    def setup(self):
        for i in range(Global.NoAgents):
            if i != self._id:
                self.send(f"a{i}", "msg")
                Global.NoMessages += 1


    def act(self, m):
        if Global.NoMessages < Global.MaxMessages:
            self.send(m.sender, "msg")
            Global.NoMessages += 1


def main():
    n = 5
    sum = 0.0

    for t in range(n):
        print(f"Trial {t + 1}")
        Global.NoMessages = 0

        Global.TimerStart = time.perf_counter()

        env = EnvironmentMas(no_turns=10000, random_order=True)

        for i in range(Global.NoAgents):
            a = MyAgent(i)
            env.add(a, f"a{i}")

        env.start()

        elapsed_ms_int = int((time.perf_counter() - Global.TimerStart) * 1000.0)
        if elapsed_ms_int <= 0:
            elapsed_ms_int = 1

        print(f"{Global.NoMessages} msg")
        print(f"{elapsed_ms_int} ms\n")

        sum += float(Global.NoMessages) / float(elapsed_ms_int)

    print(f"{sum / n:.3f} msg/ms")


if __name__ == "__main__":
    main()
