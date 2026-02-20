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


class Program:

    @staticmethod
    def main():
        env = EnvironmentMas(no_turns=100)  # environment with a finite number of turns

        manager = ManagerAgent()
        env.add(manager, "Manager")

        no_workers = 5
        for i in range(no_workers):
            worker = WorkerAgent()
            env.add(worker, f"Worker{i}")

        env.start()


class ManagerAgent(Agent):

    def setup(self):
        self.broadcast("start")  # broadcast the initial "start" message to all workers


    def act(self, message):
        print(message.format())

        action, parameters = message.parse()

        if action == "report":
            print(f"\t[{self.name}]: {message.sender} reporting")
        elif action == "reply":
            print(f"\t[{self.name}]: {message.sender} replying")


class WorkerAgent(Agent):
    _rand = random.Random()

    def next_worker(self):
        no_workers = 5
        name = self.name
        
        # choose another worker agent at random
        while name == self.name:
            name = f"Worker{self._rand.randrange(no_workers)}"  

        return name


    def act(self, m):
        print(m.format())

        action, parameters = m.parse()

        if action == "start":
            self.send("Manager", "report")
            self.send(self.next_worker(), "request")
        elif action == "request":
            self.send(m.sender, "reply")
        elif action == "request_reply":
            self.send("Manager", "reply")
            self.send(self.next_worker(), "request")


if __name__ == "__main__":
    Program.main()
