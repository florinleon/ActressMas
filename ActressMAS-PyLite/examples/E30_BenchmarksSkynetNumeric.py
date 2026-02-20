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
    MaxAgents = 20000
    NoChildren = 10


class MyAgent(Agent):
    def __init__(self, id, parent_id):
        super().__init__()
        self._id = id
        self._parent_id = parent_id
        self._sum = self._id
        self._no_children = Global.NoChildren
        self._max_agents = Global.MaxAgents
        self._messages_left = 0


    def setup(self):
        for i in range(1, self._no_children + 1):
            new_id = self._id * 10 + i

            if new_id < self._max_agents:
                a = MyAgent(new_id, self._id)
                self.environment.add(a, f"a{new_id}")
                self._messages_left += 1

        if self._messages_left == 0:
            self.send_obj(f"a{self._parent_id}", self._id)  # send id to parent
            self.stop()


    def act(self, m):
        self._sum += m.content_obj
        self._messages_left -= 1

        if self._messages_left == 0:
            if self.name == "a0":
                print(f"Long Sum = {self._sum}")  # root reporting
            else:
                self.send_obj(f"a{self._parent_id}", self._sum)  # send sum to parent
            self.stop()


def main():
    start = time.perf_counter()

    env = EnvironmentMas()

    a = MyAgent(0, -1)
    env.add(a, "a0")  # the root agent

    env.start()

    elapsed_ms = (time.perf_counter() - start) * 1000.0
    print(f"{elapsed_ms:.3f} ms")


if __name__ == "__main__":
    main()
