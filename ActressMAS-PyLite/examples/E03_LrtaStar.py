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
import time
from typing import Dict, List


class Program:

    @staticmethod
    def main():
        env = EnvironmentMas()

        map_agent = MapAgent()
        env.add(map_agent, "map")

        search_agent = SearchAgent()
        env.add(search_agent, "agent1")

        env.memory["MapName"] = "Pendulum"
        # env.memory["MapName"] = "StrangeHeuristic"
        env.memory["Delay"] = 100  # milliseconds

        env.start()


class MapAgent(Agent):

    def __init__(self):
        super().__init__()
        self._neighbors= {}
        self._heuristics= []


    def setup(self):
        self._neighbors = {}

        # name of the map (graph) to be loaded
        map_name = self.environment.memory["MapName"]  # type: ignore[index]

        # read graph structure
        with open(map_name + ".grx", "r", encoding="utf-8") as sr:
            graph = sr.read()

        lines = [line for line in graph.splitlines() if line.strip()]
        for line in lines:
            toks = line.split("=")
            self._neighbors[toks[0].strip()] = toks[1].strip()

        # read heuristic values
        with open(map_name + ".grh", "r", encoding="utf-8") as sr:
            all_heuristics = sr.read()

        self._heuristics = [line for line in all_heuristics.splitlines() if line.strip()]


    def expand_state(self, state):
        if state in self._neighbors:
            return self._neighbors[state]
        return ""


    def heuristic_estimate(self, current_state, goal_state):
        for entry in self._heuristics:
            toks = entry.split("|")
            if len(toks) >= 3:
                if current_state == toks[0].strip() and goal_state == toks[1].strip():
                    return int(toks[2])
        return 0


    def act(self, message):
        try:
            print(f"\t{message.format()}")
            action, parameters = message.parse_to_string()

            if action == "expand":
                self.handle_expand(message.sender, parameters)
            elif action == "heuristicsQuery":
                self.handle_heuristics_query(message.sender, parameters)
            elif action == "finished":
                self.stop()
            else:
                pass
        except Exception as ex:
            print(ex)


    def handle_expand(self, sender, state):
        self.send(sender, f"neighbors {self.expand_state(state)}")


    def handle_heuristics_query(self, sender, states):
        toks = states.strip().split()

        # the map agent is not interested in the goal state of a search agent,
        # that is why it receives it within the query
        goal = toks[0]

        reply = ""
        for i in range(1, len(toks)):
            reply += f"{self.heuristic_estimate(toks[i], goal)} "

        self.send(sender, f"heuristicsReply {reply}")


class SearchAgent(Agent):

    class StatePairCost:
        def __init__(self, state1, state2, cost):
            self.from_state= state1
            self.to_state= state2
            self.cost= int(cost)


    def __init__(self):
        super().__init__()
        self._initial_state= ""
        self._goal_state= ""
        self._current_state= ""
        self._heuristics= {}
        self._costs= []
        self._heuristic_queries= []
        self._rand = random.Random()


    def setup(self):
        self._initial_state = "A"
        self._goal_state = "E"
        self._current_state = self._initial_state
        self.send("map", f"expand {self._current_state}")


    def act(self, message):
        try:
            print(f"\t{message.format()}")
            action, parameters = message.parse_to_string()

            if action == "neighbors":
                self.handle_neighbors(parameters)
            elif action == "heuristicsReply":
                self.handle_heuristics_reply(parameters)
            else:
                pass
        except Exception as ex:  # pragma: no cover - defensive, mirrors C#
            print(ex)


    def handle_neighbors(self, neighbors_and_costs):
        # split by space and '|' characters, remove empty entries
        tokens = neighbors_and_costs.replace("|", " ").split()
        self._heuristic_queries = []

        for i in range(len(tokens) // 2):
            neighbor_state = tokens[i * 2]
            cost_to_neighbor = tokens[i * 2 + 1]

            self._costs.append(SearchAgent.StatePairCost(self._current_state, neighbor_state, cost_to_neighbor))

            if neighbor_state not in self._heuristics:
                self._heuristic_queries.append(neighbor_state)

        if self._heuristic_queries:
            hq = f"{self._goal_state} " + " ".join(self._heuristic_queries) + " "
            self.send("map", f"heuristicsQuery {hq}")
        else:
            self.move_to_neighbor_state()


    def handle_heuristics_reply(self, heuristic_values):
        toks = heuristic_values.split()
        for i, tok in enumerate(toks):
            self._heuristics[self._heuristic_queries[i]] = int(tok)
        self.move_to_neighbor_state()


    def move_to_neighbor_state(self):
        fn_min = float("inf")
        states_f= []

        for spc in self._costs:
            if spc.from_state != self._current_state:
                continue

            # only the neighbors are processed here
            fn = spc.cost + self._heuristics[spc.to_state]
            states_f.append(SearchAgent.StatePairCost(self._current_state, spc.to_state, fn))

            if fn < fn_min:
                fn_min = fn

        # when there are more neighboring states with the same f
        min_states= [ spcf.to_state for spcf in states_f if spcf.cost == fn_min ]

        next_state = self._rand.choice(min_states)

        print(f"{self.name} moves to state {next_state}")

        if next_state == self._goal_state:
            print(f"{self.name} reached goal state {next_state}")
            self.send("map", "finished")
            self.stop()
            return

        if self._current_state in self._heuristics:
            self._heuristics[self._current_state] = int(fn_min)
        else:
            self._heuristics[self._current_state] = int(fn_min)

        print(f"{self.name} updates h({self._current_state}) = {self._heuristics[self._current_state]}")

        self._current_state = next_state

        delay_ms = self.environment.memory.get("Delay", 0) if self.environment else 0
        if delay_ms > 0:
            time.sleep(delay_ms / 1000.0)

        self.process_state()


    def process_state(self):
        for spc in self._costs:
            if spc.from_state == self._current_state:
                # it has been in this state before and already knows the neighbors
                self.move_to_neighbor_state()
                return

        self.send("map", f"expand {self._current_state}")


if __name__ == "__main__":
    Program.main()
