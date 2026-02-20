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


class PrisonerAgent(Agent):

    def __init__(self):
        super().__init__()
        self._points = 0
        self._last_outcome = 0


    def act(self, message):
        try:
            print(f"\t{message.format()}")
            action, parameters = message.parse_to_string()

            if action == "outcome":
                outcome = int(parameters) if parameters else 0
                self._handle_outcome(outcome)
            elif action == "end":
                self._handle_end()
        except Exception as ex:
            print(ex)


    def act_default(self):
        self._handle_turn()


    def _choose_action(self, last_outcome):
        raise NotImplementedError("Subclasses must implement _choose_action")


    def _handle_outcome(self, outcome):
        self._last_outcome = outcome
        self._points += self._last_outcome


    def _handle_turn(self):
        action = self._choose_action(self._last_outcome)
        self.send("police", f"play {action}")


    def _handle_end(self):
        print(f"[{self.name}]: I have {self._points} points")
        self.stop()


class ConfessPrisonerAgent(PrisonerAgent):
    def _choose_action(self, last_outcome):
        return "confess"


class RandomPrisonerAgent(PrisonerAgent):
    def __init__(self):
        super().__init__()
        self._rand = random.Random()


    def _choose_action(self, last_outcome):
        if self._rand.random() < 0.5:
            return "confess"
        else:
            return "deny"


class TitForTatPrisonerAgent(PrisonerAgent):
    def _choose_action(self, last_outcome):
        # last_outcome represents the payoff from the previous round
        # 0 or -1  -> both cooperated (deny), so keep cooperating
        # -3 or -5 -> someone defected (confess), so defect next time
        if last_outcome == 0 or last_outcome == -1:
            return "deny"  # cooperate
        if last_outcome == -3 or last_outcome == -5:
            return "confess"  # defect
        return ""


class PoliceAgent(Agent):

    def __init__(self):
        super().__init__()
        self._responses = {}


    def act(self, message):
        try:
            print(f"\t{message.format()}")
            action, chosen_action = message.parse_1p()  # action = "play"
            self._handle_play(message.sender, chosen_action)
        except Exception as ex:
            print(ex)


    def act_default(self):
        pass


    def _handle_play(self, sender, action):
        self._responses[sender] = action

        if len(self._responses) == 2:
            items = list(self._responses.items())
            (p1_name, action1), (p2_name, action2) = items[0], items[1]

            outcome1, outcome2 = self._compute_outcome(action1, action2)

            self.send(p1_name, f"outcome {outcome1}")
            self.send(p2_name, f"outcome {outcome2}")

            self._responses.clear()


    def _compute_outcome(self, action1, action2):
        outcome1 = 0
        outcome2 = 0

        if action1 == "confess" and action2 == "confess":
            outcome1 = outcome2 = -3
        elif action1 == "deny" and action2 == "deny":
            outcome1 = outcome2 = -1
        elif action1 == "confess" and action2 == "deny":
            outcome1 = 0
            outcome2 = -5
        elif action1 == "deny" and action2 == "confess":
            outcome1 = -5
            outcome2 = 0

        return outcome1, outcome2


class IPDEnvironment(EnvironmentMas):

    def __init__(self, no_turns):
        # no_turns is the number of rounds of the Iterated Prisoner's Dilemma
        # The underlying environment runs for no_turns * 2 + 2 turns,
        # as in the original C# implementation.
        super().__init__(no_turns=no_turns * 2 + 2, random_order=False)
        # use a separate field so we do not overwrite EnvironmentMas._no_turns
        self._ipd_turns = no_turns * 2


    def turn_finished(self, turn):
        if turn < self._ipd_turns and turn % 2 == 0:
            print(f"Round {turn // 2 + 1}")

        if turn == self._ipd_turns - 1:
            for agent_name in self.filtered_agents("Prisoner"):
                msg = Message("env", agent_name, "end")
                self.send(msg)


def main():
    env = IPDEnvironment(no_turns=10)

    prisoner_agent1 = TitForTatPrisonerAgent()
    prisoner_agent2 = TitForTatPrisonerAgent()
    # var prisonerAgent1 = new ConfessPrisonerAgent();
    # var prisonerAgent2 = new RandomPrisonerAgent();

    env.add(prisoner_agent1, f"p1-{prisoner_agent1.__class__.__name__}")
    env.add(prisoner_agent2, f"p2-{prisoner_agent2.__class__.__name__}")

    police_agent = PoliceAgent()
    env.add(police_agent, "police")

    env.start()


if __name__ == "__main__":
    main()
