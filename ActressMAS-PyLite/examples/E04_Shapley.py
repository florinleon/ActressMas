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
from typing import List, Tuple


class Task:

    def __init__(self, no_attributes, max_level):
        self._no_attributes = no_attributes
        self._max_level = max_level

        self.difficulty_level = [0] * self._no_attributes
        self.price = 0

        # each attribute difficulty is a multiple of 6, between 6 and 6 * max_level
        for i in range(self._no_attributes):
            self.difficulty_level[i] = 6 * (random.randint(1, self._max_level))
            self.price += self.difficulty_level[i]


    def __str__(self):
        # space-separated difficulty levels, without trailing space
        return " ".join(str(self.difficulty_level[i]) for i in range(self._no_attributes))


class WorkerAgent(Agent):

    def __init__(self, s1, s2, s3):
        super().__init__()

        self._skill_levels = [0, 0, 0]
        self._skill_levels[0] = s1
        self._skill_levels[1] = s2
        self._skill_levels[2] = s3

        self._total_reward = 0.0


    def act(self, message):
        try:
            action, parameters = message.parse()

            if action == "task":
                self._handle_task(parameters)
            elif action == "reward":
                self._handle_reward(float(parameters[0]))
            elif action == "report":
                self._handle_report()
            else:
                # ignore
                pass
        except Exception as ex: 
            print(ex)


    def _handle_task(self, task_parameters):
        estimates_parts = []
        no_attributes = int(self.environment.memory["NoAttributes"])
        for i in range(no_attributes):
            # Convert difficulty and divide by skill level
            time_val = int(task_parameters[i])
            time_est = int(time_val / self._skill_levels[i])
            estimates_parts.append(str(time_est))

        estimates = " ".join(estimates_parts)
        self.send("manager", f"time {estimates}")


    def _handle_reward(self, reward):
        self._total_reward += reward


    def _handle_report(self):
        print(f"Agent {self.name} has {self._total_reward:.3f}")


class CalculatorAgent(Agent):

    def act(self, message):
        try:
            print(f"\t{message.format()}")
            action, parameters = message.parse()

            if action != "calculate":
                return

            # utilities are inversely proportional to the completion time
            times = [float(p) for p in parameters]
            if len(times) != 7:
                return

            u1, u2, u3, u12, u13, u23, u123 = [1.0 / t for t in times]

            max_u = max(u1, u2, u3, u12, u13, u23, u123)

            if self._are_equal(max_u, u1):
                self.send("manager", "results 1 0 0")
            elif self._are_equal(max_u, u2):
                self.send("manager", "results 0 1 0")
            elif self._are_equal(max_u, u3):
                self.send("manager", "results 0 0 1")
            elif self._are_equal(max_u, u12):
                # computing the Shapley value with 2 agents (1 and 2)
                value_added1 = (u1 + (u12 - u2)) / 2.0  # AB, BA
                value_added2 = (u2 + (u12 - u1)) / 2.0  # BA, AB
                value_added1, value_added2 = self._normalize2(value_added1, value_added2)
                self.send("manager", f"results {value_added1} {value_added2} 0")
            elif self._are_equal(max_u, u13):
                # computing the Shapley value with 2 agents (1 and 3)
                value_added1 = (u1 + (u13 - u3)) / 2.0  # AC, CA
                value_added3 = (u3 + (u13 - u1)) / 2.0  # CA, AC
                value_added1, value_added3 = self._normalize2(value_added1, value_added3)
                self.send("manager", f"results {value_added1} 0 {value_added3}")
            elif self._are_equal(max_u, u23):
                # computing the Shapley value with 2 agents (2 and 3)
                value_added2 = (u2 + (u23 - u3)) / 2.0  # BC, CB
                value_added3 = (u3 + (u23 - u2)) / 2.0  # CB, BC
                value_added2, value_added3 = self._normalize2(value_added2, value_added3)
                self.send("manager", f"results 0 {value_added2} {value_added3}")
            elif self._are_equal(max_u, u123):
                # computing the Shapley value with 3 agents
                v1, v2, v3 = self._shapley_three(u1, u2, u3, u12, u13, u23, u123)
                v1, v2, v3 = self._normalize3(v1, v2, v3)
                self.send("manager", f"results {v1} {v2} {v3}")
        except Exception as ex:  
            print(ex)


    @staticmethod
    def _are_equal(x, y):
        return abs(x - y) < 1e-10


    @staticmethod
    def _normalize2(a, b):
        s = a + b
        a /= s
        b /= s
        return a, b


    @staticmethod
    def _normalize3(a, b, c):
        s = a + b + c
        a /= s
        b /= s
        c /= s
        return a, b, c


    @staticmethod
    def _shapley_three(u1, u2, u3, u12, u13, u23, u123):
        """Exact Shapley values for three agents given coalition utilities."""
        def v(coalition):
            key = tuple(sorted(coalition))
            if not key:
                return 0.0
            if key == (1,):
                return u1
            if key == (2,):
                return u2
            if key == (3,):
                return u3
            if key == (1, 2):
                return u12
            if key == (1, 3):
                return u13
            if key == (2, 3):
                return u23
            if key == (1, 2, 3):
                return u123
            raise ValueError(f"Unexpected coalition {key}")

        contrib = {1: 0.0, 2: 0.0, 3: 0.0}
        permutations = [
            (1, 2, 3),
            (1, 3, 2),
            (2, 1, 3),
            (2, 3, 1),
            (3, 1, 2),
            (3, 2, 1)
        ]

        for perm in permutations:
            current = []
            for player in perm:
                before = v(tuple(current))
                current.append(player)
                after = v(tuple(current))
                contrib[player] += after - before

        return contrib[1] / 6.0, contrib[2] / 6.0, contrib[3] / 6.0


class ManagerAgent(Agent):

    def __init__(self):
        super().__init__()

        self._agent_bidders = []
        self._bids = []
        self._current_task = None
        self._task_count = 0
        self._no_bids = 0
        self._no_tasks = 0
        self._no_attributes = 0
        self._max_level = 0


    def setup(self):
        self._no_bids = int(self.environment.memory["NoBids"])
        self._no_tasks = int(self.environment.memory["NoTasks"])
        self._no_attributes = int(self.environment.memory["NoAttributes"])
        self._max_level = int(self.environment.memory["MaxLevel"])

        self._agent_bidders = ["" for _ in range(self._no_bids)]
        self._generate_new_task()


    def act(self, message):
        try:
            print(f"\t{message.format()}")
            tokens = message.content.split()
            if not tokens:
                return
            action = tokens[0]
            parameters = " ".join(tokens[1:])

            if action == "time":
                self._handle_time(parameters)
            elif action == "results":
                self._handle_results(parameters)
            else:
                # ignore
                pass
        except Exception as ex:
            print(ex)


    def _generate_new_task(self):
        self._current_task = Task(self._no_attributes, self._max_level)
        self._task_count += 1

        self._select_agents()
        print(f"Selected: {self._agent_bidders[0]} {self._agent_bidders[1]} {self._agent_bidders[2]}")

        for i in range(self._no_bids):
            # send the task difficulties as a space-separated list
            self.send(self._agent_bidders[i], f"task {self._current_task}")

        self._bids = []


    def _select_agents(self):
        env = self.environment

        # first worker
        self._agent_bidders[0] = ""
        while self._agent_bidders[0] == "" or not self._agent_bidders[0].startswith("worker"):
            self._agent_bidders[0] = env.random_agent()

        # second worker (different from first)
        self._agent_bidders[1] = ""
        while (self._agent_bidders[1] == "" or self._agent_bidders[1] == self._agent_bidders[0] or \
            not self._agent_bidders[1].startswith("worker")):
            self._agent_bidders[1] = env.random_agent()

        # third worker (different from first two)
        self._agent_bidders[2] = ""
        while (self._agent_bidders[2] == "" or self._agent_bidders[2] == self._agent_bidders[0] or \
            self._agent_bidders[2] == self._agent_bidders[1] or not self._agent_bidders[2].startswith("worker")):
            self._agent_bidders[2] = env.random_agent()


    def _handle_time(self, time_estimate):
        self._bids.append(time_estimate)
        if len(self._bids) == self._no_bids:
            self._analyze_coalitions()


    def _handle_results(self, results):
        toks = results.split()
        for i in range(self._no_bids):
            r = self._current_task.price * float(toks[i])
            self.send(self._agent_bidders[i], f"reward {r}")

        if self._task_count >= self._no_tasks:
            print("Manager has finished")

            for a in self.environment.filtered_agents("worker"):
                self.send(a, "report")
        else:
            self._generate_new_task()


    @staticmethod
    def _min3(a, b, c):
        return min(a, b, c)


    def _analyze_coalitions(self):
        time_to_solve = [ [0 for _ in range(self._no_attributes)] for _ in range(self._no_bids) ]

        for i in range(self._no_bids):
            toks = self._bids[i].split()
            for j in range(self._no_attributes):
                time_to_solve[i][j] = int(toks[j])

        v1 = time_to_solve[0][0] + time_to_solve[0][1] + time_to_solve[0][2]
        v2 = time_to_solve[1][0] + time_to_solve[1][1] + time_to_solve[1][2]
        v3 = time_to_solve[2][0] + time_to_solve[2][1] + time_to_solve[2][2]

        v12 = (min(time_to_solve[0][0], time_to_solve[1][0]) 
               + min(time_to_solve[0][1], time_to_solve[1][1]) 
               + min(time_to_solve[0][2], time_to_solve[1][2]))

        v13 = (min(time_to_solve[0][0], time_to_solve[2][0]) 
               + min(time_to_solve[0][1], time_to_solve[2][1]) 
               + min(time_to_solve[0][2], time_to_solve[2][2]))

        v23 = (min(time_to_solve[1][0], time_to_solve[2][0]) 
               + min(time_to_solve[1][1], time_to_solve[2][1]) 
               + min(time_to_solve[1][2], time_to_solve[2][2]))

        v123 = (self._min3(time_to_solve[0][0], time_to_solve[1][0], time_to_solve[2][0])
            + self._min3(time_to_solve[0][1], time_to_solve[1][1], time_to_solve[2][1])
            + self._min3(time_to_solve[0][2], time_to_solve[1][2], time_to_solve[2][2]))

        self.send("shapley", f"calculate {v1} {v2} {v3} {v12} {v13} {v23} {v123}")


class Program:

    @staticmethod
    def main():
        env = EnvironmentMas(no_turns=1000)

        # create all worker agents with skill levels 1..MaxLevel on each attribute
        for i in range(1, 4):  # MaxLevel
            for j in range(1, 4):
                for k in range(1, 4):
                    worker_agent = WorkerAgent(i, j, k)
                    env.add(worker_agent, f"worker{i}{j}{k}")

        calculator_agent = CalculatorAgent()
        env.add(calculator_agent, "shapley")

        manager_agent = ManagerAgent()
        env.add(manager_agent, "manager")

        env.memory["NoAttributes"] = 3
        env.memory["MaxLevel"] = 3
        env.memory["NoTasks"] = 100
        env.memory["NoBids"] = 3

        env.start()


if __name__ == "__main__":
    Program.main()
