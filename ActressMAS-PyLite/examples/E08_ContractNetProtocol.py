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

try:
    from ortools.constraint_solver import routing_enums_pb2
    from ortools.constraint_solver import pywrapcp
except Exception:
    routing_enums_pb2 = None
    pywrapcp = None


class Performatives:
    CallForProposals = 0
    Propose = 1
    Refuse = 2
    AcceptProposal = 3
    RejectProposal = 4
    InformDone = 5
    InformResult = 6
    Failure = 7


class CNPMessage:
    def __init__(self, sender, performative, task, initiator_payoff = 0, participant_payoff = 0, contract_price = 0):
        self.sender = sender
        self.performative = performative
        self.task = task
        self.initiator_payoff = initiator_payoff
        self.participant_payoff = participant_payoff
        self.contract_price = contract_price


class Location:
    def __init__(self, name, latitude, longitude):
        self.name = name
        self.latitude = latitude
        self.longitude = longitude


def haversine_km(lat1, lon1, lat2, lon2):
    # Great-circle distance (km) between two GPS coordinates.
    r = 6371.0
    p1 = math.radians(lat1)
    p2 = math.radians(lat2)
    dp = math.radians(lat2 - lat1)
    dl = math.radians(lon2 - lon1)
    a = math.sin(dp / 2.0) ** 2 + math.cos(p1) * math.cos(p2) * math.sin(dl / 2.0) ** 2
    c = 2.0 * math.atan2(math.sqrt(a), math.sqrt(1.0 - a))
    return r * c


def solve_tsp_ortools(locations):
    # Returns (order_indices, tour_length_km).
    if routing_enums_pb2 is None or pywrapcp is None:
        raise RuntimeError("Google OR-Tools is not available. Install it with: pip install ortools")

    n = len(locations)

    if n == 0:
        return [], 0.0

    if n == 1:
        return [0], 0.0

    scale = 1000  # meters, for integer costs

    dist = [[0 for _ in range(n)] for _ in range(n)]
    for i in range(n):
        li = locations[i]
        for j in range(n):
            if i == j:
                dist[i][j] = 0
            else:
                lj = locations[j]
                dist[i][j] = int(round(haversine_km(li.latitude, li.longitude, lj.latitude, lj.longitude) * scale))

    manager = pywrapcp.RoutingIndexManager(n, 1, 0)
    routing = pywrapcp.RoutingModel(manager)

    def distance_callback(from_index, to_index):
        from_node = manager.IndexToNode(from_index)
        to_node = manager.IndexToNode(to_index)
        return dist[from_node][to_node]

    transit_callback_index = routing.RegisterTransitCallback(distance_callback)
    routing.SetArcCostEvaluatorOfAllVehicles(transit_callback_index)

    search_parameters = pywrapcp.DefaultRoutingSearchParameters()
    search_parameters.first_solution_strategy = routing_enums_pb2.FirstSolutionStrategy.PATH_CHEAPEST_ARC
    search_parameters.local_search_metaheuristic = routing_enums_pb2.LocalSearchMetaheuristic.GUIDED_LOCAL_SEARCH
    try:
        search_parameters.time_limit.FromMilliseconds(50)
    except Exception:
        try:
            search_parameters.time_limit.FromSeconds(0)
            search_parameters.time_limit.nanos = 50000000
        except Exception:
            pass

    solution = routing.SolveWithParameters(search_parameters)

    if solution is None:
        # Fallback: trivial order
        order = list(range(n))
        length_m = 0
        for i in range(n):
            a = order[i]
            b = order[(i + 1) % n]
            length_m += dist[a][b]
        return order, float(length_m) / float(scale)

    order = []
    index = routing.Start(0)
    while not routing.IsEnd(index):
        node = manager.IndexToNode(index)
        order.append(node)
        index = solution.Value(routing.NextVar(index))

    length_km = float(solution.ObjectiveValue()) / float(scale)
    return order, length_km


class TaskCollection:

    def __init__(self, assignment):
        self._payment_per_letter = 5
        self._cost_per_km = 1
        self._assignment = list(assignment)
        self._cost_per_km = 1
        self._length = 0.0
        self._path = []
        self.solve()


    def __getitem__(self, task_name):
        for t in self._assignment:
            if t.name == task_name:
                return t
        return None


    def additional_payoff_by_accepting(self, p):
        payoff = self.compute_payoff(self._assignment, self._length)
        p0 = payoff
        ss = self.virtual_add_task(p.task)
        path, length_km = solve_tsp_ortools(ss)
        payoff = self.compute_payoff(ss, length_km)
        p1 = payoff
        return p1 - p0


    def add_task(self, loc):
        self._assignment.append(loc)
        self.solve()


    @property
    def payoff(self):
        return self.compute_payoff(self._assignment, self._length)


    @property
    def solution(self):
        parts = []
        for i in range(len(self._assignment)):
            loc = self._assignment[self._path[i]]
            parts.append(f"{i + 1}.{loc.name} ")
        parts.append("-> ")
        parts.append(f"{self._length} km")
        return "".join(parts)


    def remove_task(self, task_name):
        self._assignment = self.virtual_remove_task(task_name)
        self.solve()


    def worst_task(self):
        p0 = self.compute_payoff(self._assignment, self._length)
        max_payoff = -1e300
        selected = -1

        for i in range(len(self._assignment)):
            ss = self.virtual_remove_task(self._assignment[i].name)

            if len(ss) == 0:
                payoff = 0.0  # no tasks left: payoff is zero (no income, no travel cost)
            else:
                path, length_km = solve_tsp_ortools(ss)
                payoff = self.compute_payoff(ss, length_km)

            if payoff > max_payoff:
                max_payoff = payoff
                selected = i

        if selected != -1:
            p1 = max_payoff
            difference = p1 - p0
            task_name = self._assignment[selected].name

            if difference > 0:
                return True, task_name, difference

        return False, "", 0.0


    def compute_payoff(self, lst, path_length_km):
        income = len(lst) * self._payment_per_letter
        cost = path_length_km * self._cost_per_km
        payoff = income - cost
        return payoff


    def solve(self):
        if self._assignment is None or len(self._assignment) == 0:
            self._path = []
            self._length = 0.0
            return

        path, length_km = solve_tsp_ortools(self._assignment)
        self._path = path  # 0 to n-1
        self._length = length_km


    def virtual_add_task(self, loc):
        new_list = list(self._assignment)
        new_list.append(loc)
        return new_list


    def virtual_remove_task(self, task_name):
        return [t for t in self._assignment if t.name != task_name]


class PostmanAgent(Agent):

    def __init__(self, init_assign):
        super().__init__()
        self._tasks = TaskCollection(init_assign)
        self._cfps = []
        self._proposals = []
        self._money = 0


    def setup(self):
        self.display_status()
        self.send_calls_for_proposals()


    def display_status(self):
        print(f"\n{self.name}\nTour: {self._tasks.solution}\nPayoff: {self._tasks.payoff + self._money:.2f} "
              f"({self._tasks.payoff:.2f} + {self._money:.2f})")


    def act(self, message):
        try:
            if message.content == "finish":  # protocol finished
                self.display_status()
                self.stop()
                return

            msg = message.content_obj
            print(f"\n{message.sender} -> {message.receiver}: {self.performative_to_string(msg.performative)} "
                  f"{msg.task.name} {msg.initiator_payoff:.2f} {msg.participant_payoff:.2f} {msg.contract_price:.2f}")

            if msg.performative == Performatives.CallForProposals:
                # the agent has the role of participant

                payoff = self._tasks.additional_payoff_by_accepting(msg)
                if payoff > 0:
                    msg.participant_payoff = payoff

                    # the final payoffs of the initiator and the participant should be equal
                    average_price = (msg.initiator_payoff + msg.participant_payoff) / 2.0

                    # intiator pays ContractPrice to the participant
                    msg.contract_price = average_price - payoff

                    self._cfps.append(msg)
                    print(f"{self.name} receives cfp {msg.task.name} with {msg.initiator_payoff:.2f}/"
                          f"{msg.participant_payoff:.2f}/{msg.contract_price:.2f}")

            elif msg.performative == Performatives.Propose:
                # the agent has the role of intiator

                self._proposals.append(msg)
                print(f"{self.name} receives proposal {msg.task.name} with {msg.initiator_payoff:.2f}/"
                      f"{msg.participant_payoff:.2f}/{msg.contract_price:.2f}")

            elif msg.performative == Performatives.Refuse:
                # not handled explicitly in this version of the protocol
                # the refused cfps are ignored
                pass

            elif msg.performative == Performatives.AcceptProposal:
                # the agent has the role of participant

                # the participant receives the task and the payment
                print(f"{self.name} is given {msg.task.name} with {msg.initiator_payoff:.2f}/"
                      f"{msg.participant_payoff:.2f}/{msg.contract_price:.2f}")
                self._tasks.add_task(msg.task)
                self._money += msg.contract_price

            elif msg.performative == Performatives.RejectProposal:
                # not handled explicitly in this version of the protocol
                # the refused proposals are ignored
                pass

            elif msg.performative == Performatives.InformDone:
                # for this application, there is no confirmation of completing the task
                # in the end, all postmen will deliver their assigned letters
                pass

            elif msg.performative == Performatives.InformResult:
                # for this application, there is no result reported
                pass

            elif msg.performative == Performatives.Failure:
                # for this application, there is no notification of failure to complete the task
                pass
            else:
                pass
        except Exception as ex:
            print(str(ex))


    def act_default(self):
        try:
            # we rely on turn numbers to handle deadlines

            if self.environment.memory["Turn"] % 7 == 2:
                self.evaluate_calls_for_proposals()
            elif self.environment.memory["Turn"] % 7 == 4:
                self.evaluate_proposals()
            elif self.environment.memory["Turn"] % 7 == 6:
                self.send_calls_for_proposals()
        except Exception as ex:
            print(str(ex))


    def send_calls_for_proposals(self):
        # the agent has the role of initiator

        can_initiate, task_name, dif = self._tasks.worst_task()

        if can_initiate:  # difference in payoff > 0
            msg = CNPMessage(self.name, Performatives.CallForProposals, self._tasks[task_name], initiator_payoff=dif)
            self.broadcast_obj(msg)


    def evaluate_calls_for_proposals(self):
        # the agent has the role of participant
        # it evaluates the best call for proposals from initiators

        if len(self._cfps) == 0:
            return

        best_price = -1e300
        best_proposal = None

        for p in self._cfps:
            if p.participant_payoff + p.contract_price > best_price:
                best_price = p.participant_payoff + p.contract_price
                best_proposal = p

        if best_proposal is not None:
            receiver = best_proposal.sender
            best_proposal.sender = self.name
            best_proposal.performative = Performatives.Propose
            self.send_obj(receiver, best_proposal)

        # here the agent can send a Performatives.Refuse message to all the other agents
        # but in this version of the protocol these messages will be ignored anyway

        self._cfps.clear()


    def evaluate_proposals(self):
        # the agent has the role of initiator
        # it evaluates the best proposals from participants

        if len(self._proposals) == 0:
            return

        best_price = -1e300
        best_proposal = None

        for p in self._proposals:
            if p.initiator_payoff - p.contract_price > best_price:
                best_price = p.initiator_payoff - p.contract_price
                best_proposal = p

        if best_proposal is not None:
            # the initiator awards the contract for the task and pays the participant
            receiver = best_proposal.sender
            best_proposal.sender = self.name
            best_proposal.performative = Performatives.AcceptProposal
            self.send_obj(receiver, best_proposal)
            self._tasks.remove_task(best_proposal.task.name)
            self._money -= best_proposal.contract_price

        # here the agent can send a Performatives.RejectProposal message to all the other agents
        # but in this version of the protocol these messages will be ignored anyway

        self._proposals.clear()


    def performative_to_string(self, perf):
        if perf == Performatives.CallForProposals:
            return "CallForProposals"
        if perf == Performatives.Propose:
            return "Propose"
        if perf == Performatives.Refuse:
            return "Refuse"
        if perf == Performatives.AcceptProposal:
            return "AcceptProposal"
        if perf == Performatives.RejectProposal:
            return "RejectProposal"
        if perf == Performatives.InformDone:
            return "InformDone"
        if perf == Performatives.InformResult:
            return "InformResult"
        if perf == Performatives.Failure:
            return "Failure"
        return str(perf)


class MyEnv(EnvironmentMas):

    def __init__(self, no_turns = 0, delay_after_turn_ms = 0, random_order = True, rand = None):
        super().__init__(no_turns, delay_after_turn_ms, random_order, rand)
        self.memory["Turn"] = 0


    def turn_finished(self, turn):
        self.memory["Turn"] = turn + 1  # turn is updated after TurnFinished


def main():
    env = MyEnv(no_turns=100)

    no_postmen = 3
    no_letters = 20

    locations = []
    assignment = []
    rand = random.Random()

    for i in range(no_letters):
        # locations between 47.1 - 47.2 latitude, 27.5 - 27.6 longitude
        locations.append(Location(f"L{(i + 1):02d}", 47.1 + rand.random() / 10.0, 27.5 + rand.random() / 10.0))
        assignment.append(rand.randrange(no_postmen))

    for j in range(no_postmen):
        init_assign = [c for i, c in enumerate(locations) if assignment[i] == j]
        postman_agent = PostmanAgent(init_assign)
        env.add(postman_agent, f"postman{j + 1}")

    env.start()

    # end of exchanges

    for j in range(no_postmen):
        m = Message("env", f"postman{j + 1}", "finish", None)
        env.send(m)

    env.continue_simulation(1)


if __name__ == "__main__":
    main()
