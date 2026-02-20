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


class Settings:
    NoBeneficiaries = 3
    NoOptions = 4
    UnitCost = 120
    AisLying = False


class BeneficiaryAgent(Agent):

    def __init__(self):
        super().__init__()
        self._net_benefits = None
        self._net_benefits_str = ""


    def setup(self):
        if self.name == "a":
            total_benefits = [0, 60, 90, 155]
        elif self.name == "b":
            total_benefits = [0, 80, 120, 140]
        elif self.name == "c":
            total_benefits = [0, 120, 200, 220]
        else:
            raise ValueError("Unknown beneficiary name")

        self._net_benefits = [0] * Settings.NoOptions
        parts = []
        for i in range(Settings.NoOptions):
            self._net_benefits[i] = total_benefits[i] - i * Settings.UnitCost // Settings.NoBeneficiaries
            parts.append(str(self._net_benefits[i]))
        self._net_benefits_str = " ".join(parts)

        print(f"[{self.name}]: My net benefits are ({self._net_benefits_str})")

        self.send("dm", "report")


    def act(self, message):
        try:
            print(f"\t{message.format()}")
            action, parameters = message.parse()

            if action == "report":
                self._handle_report()
            elif action == "result":
                self._handle_result(parameters)
        except Exception as ex:
            print(ex)


    def act_default(self):
        # No default behavior when no messages are received
        pass


    def _handle_report(self):
        print(f"[{self.name}]: I report that my net benefits are ({self._net_benefits_str})")

        if Settings.AisLying and self.name == "a":
            lying_net_benefits_str = "0 20 10 70"
            print(f"[{self.name}]: Actually I'm lying and my reported net benefits are ({lying_net_benefits_str})")
            self.send("dm", f"benefits {lying_net_benefits_str}")
        else:
            self.send("dm", f"benefits {self._net_benefits_str}")


    def _handle_result(self, parameters):
        no_lights = int(parameters[0])
        tax = int(parameters[1])

        print(f"[{self.name}]: My net benefit after tax is {self._net_benefits[no_lights] - tax}")

        self.stop()


class DecisionMakerAgent(Agent):

    def __init__(self):
        super().__init__()
        self._net_benefits = None
        self._taxes = None
        self._answers_received = 0


    def setup(self):
        self._net_benefits = [ [0] * Settings.NoOptions for _ in range(Settings.NoBeneficiaries) ]
        self._taxes = [0] * Settings.NoBeneficiaries
        self._answers_received = 0

        self.broadcast("report")


    def act(self, message):
        try:
            print(f"\t{message.format()}")
            action, parameters = message.parse()

            if action == "benefits":
                self._handle_benefits(message.sender, parameters)
        except Exception as ex:
            print(ex)


    def act_default(self):
        pass


    def _handle_benefits(self, sender, benefits):
        agent_index = ord(sender[0]) - ord("a")

        for i in range(Settings.NoOptions):
            self._net_benefits[agent_index][i] = int(benefits[i])

        self._answers_received += 1

        if self._answers_received == Settings.NoBeneficiaries:
            self._compute_taxes()


    def _compute_taxes(self):
        best_option = -1
        largest_net_social_benefit = -1

        for i in range(Settings.NoOptions):
            s = 0
            for j in range(Settings.NoBeneficiaries):
                s += self._net_benefits[j][i]

            if s > largest_net_social_benefit:
                largest_net_social_benefit = s
                best_option = i

        print(f"[{self.name}]: The net social benefits are largest for option {best_option}")

        for k in range(Settings.NoBeneficiaries):
            best_option_without_k = -1
            largest_nsb_without_k = -1
            previous_largest_nsb_without_k = 0

            for i in range(Settings.NoOptions):
                s = 0
                for j in range(Settings.NoBeneficiaries):
                    if j != k:
                        s += self._net_benefits[j][i]

                if i == best_option:
                    previous_largest_nsb_without_k = s

                if s > largest_nsb_without_k:
                    largest_nsb_without_k = s
                    best_option_without_k = i

            if best_option != best_option_without_k:
                self._taxes[k] = largest_nsb_without_k - previous_largest_nsb_without_k
                print(f"[{self.name}]: {chr(ord('a') + k)} is pivotal - from {best_option_without_k}"
                      f" - and its tax is {self._taxes[k]}")
            else:
                print(f"[{self.name}]: {chr(ord('a') + k)} is not pivotal and its tax is 0")

        for i in range(Settings.NoBeneficiaries):
            name = chr(ord("a") + i)
            self.send(name, f"result {best_option} {self._taxes[i]}")


def run_simulation(is_lying):
    Settings.AisLying = is_lying
    print()
    print(f"=== A is {'lying' if is_lying else 'truthful'} ===")

    env = EnvironmentMas(no_turns=10, random_order=False)

    dm = DecisionMakerAgent()
    env.add(dm, "dm")

    for i in range(Settings.NoBeneficiaries):
        beneficiary = BeneficiaryAgent()
        name = chr(ord("a") + i)
        env.add(beneficiary, name)

    env.start()


def main():
    run_simulation(False)
    run_simulation(True)


if __name__ == "__main__":
    main()
