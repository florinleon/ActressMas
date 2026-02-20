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


class BargainingAgent(Agent):

    def __init__(self):
        super().__init__()
        self.my_utility_function = None
        self.others_utility_function = None
        self._my_proposal = 0.0
        self._others_proposal = 0.0
        self._others_name = ""
        self._rand = None


    def setup(self):
        if self.name == "agent1":
            self.my_utility_function = self.environment.memory["Utility1"]
            self.others_utility_function = self.environment.memory["Utility2"]

            self._others_name = "agent2"
            self._my_proposal = 0.1
            self.send(self._others_name, f"propose {self._my_proposal:.1f}")
        else:
            self.my_utility_function = self.environment.memory["Utility2"]
            self.others_utility_function = self.environment.memory["Utility1"]

            self._others_name = "agent1"
            self._my_proposal = 4.9

        self._rand = random.Random()


    def act(self, message):
        try:
            print(f"\t{message.format()}")
            action, parameters = message.parse()

            if action == "propose":
                proposal = float(parameters[0]) if parameters else 0.0
                self._handle_propose(proposal)

            elif action == "continue":
                self._handle_continue()

            elif action == "accept":
                self._handle_accept()

        except Exception as ex:
            print(ex)


    def _new_proposal(self, d1, d2):
        # The new proposal is computed iteratively so that the method is general
        # and can be applied for any pair of utility functions.
        # For the linear utility functions used in this example, the new proposal
        # could also be computed analytically.

        eps = self.environment.memory["Eps"]

        if self.name == "agent1":
            d = d1
            while d <= d2:
                if (self.my_utility_function(d) * self.others_utility_function(d) > self.my_utility_function(d2) * self.others_utility_function(d2)):
                    # This inequality is equivalent to: risk1(d) > risk2(d2)
                    return d
                d += eps
            return d2
        else:
            d = d1
            while d >= d2:
                if (self.my_utility_function(d) * self.others_utility_function(d) > self.my_utility_function(d2) * self.others_utility_function(d2)):
                    return d
                d -= eps
            return d1


    def _risk(self, utility, d1, d2):
        return 1.0 - utility(d2) / utility(d1)


    def _handle_propose(self, proposal):
        self._others_proposal = proposal

        if self.my_utility_function(self._others_proposal) >= self.my_utility_function(self._my_proposal):
            self.send(self._others_name, "accept")
            return

        if self.my_utility_function(self._others_proposal) < 0:
            self.send(self._others_name, "continue")
            return

        my_risk = self._risk(self.my_utility_function, self._my_proposal, self._others_proposal)
        others_risk = self._risk(self.others_utility_function, self._others_proposal, self._my_proposal)

        print(f"[{self.name}]: My risk is {my_risk:.4f} and the other's risk is {others_risk:.4f}")

        if (my_risk < others_risk) or (self._are_equal(my_risk, others_risk) and self._rand.random() < 0.5):
            # On equality, concede randomly.
            self._my_proposal = self._new_proposal(self._my_proposal, self._others_proposal)
            print(f"[{self.name}]: I will concede with new proposal {self._my_proposal:.1f}")
            self.send(self._others_name, f"propose {self._my_proposal:.1f}")
        else:
            print(f"[{self.name}]: I will not concede")

            eps = self.environment.memory["Eps"]
            # To avoid a blockage when finishing with equal risks.
            self.environment.memory["Eps"] = eps / 10.0

            self.send(self._others_name, "continue")


    def _handle_continue(self):
        self._my_proposal = self._new_proposal(self._my_proposal, self._others_proposal)
        self.send(self._others_name, f"propose {self._my_proposal:.1f}")


    def _handle_accept(self):
        print(f"[{self.name}]: I accept {self._my_proposal:.1f}")
        self.send(self._others_name, "accept")
        self.stop()


    def _are_equal(self, x, y):
        return abs(x - y) < 1e-10


def main():
    env = EnvironmentMas(random_order=False)

    agent1 = BargainingAgent()
    env.add(agent1, "agent1")

    agent2 = BargainingAgent()
    env.add(agent2, "agent2")

    env.memory["Eps"] = 0.1
    env.memory["Utility1"] = lambda deal: 5.0 - deal
    env.memory["Utility2"] = lambda deal: (2.0 / 3.0) * deal

    env.start()


if __name__ == "__main__":
    main()
