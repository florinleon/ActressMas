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


_rand = random.Random()


class TellerAgent(Agent):

    def __init__(self):
        super().__init__()
        self._turns_to_wait = 0
        self._no_candidates = 0
        self._votes = []


    def setup(self):
        self._no_candidates = self.environment.memory["NoCandidates"]
        self._turns_to_wait = 2
        self.broadcast("start")


    def act(self, message):
        action, parameters = message.parse_to_string()  # action == "vote"
        self.handle_vote(parameters)


    def act_default(self):
        self._turns_to_wait -= 1
        if self._turns_to_wait <= 0:
            self.handle_results()


    def handle_vote(self, preference_order):
        print(preference_order)
        self._votes.append(preference_order)


    def handle_results(self):
        is_condorcet, winner = self.copeland()

        if is_condorcet:
            print(f"Candidate {winner} is the Condorcet winner")
        else:
            print("There is no Condorcet winner")

        print(f"Using Copeland's method, candidate {winner} is the winner")
        winner = self.borda()
        print(f"Using Borda count, candidate {winner} is the winner")

        self.stop()


    def copeland(self):
        is_condorcet = False
        winner = -1

        pair_wise_wins = [[0 for _ in range(self._no_candidates)] for _ in range(self._no_candidates)]

        for i in range(self._no_candidates - 1):
            for j in range(i + 1, self._no_candidates):
                i_wins = 0
                j_wins = 0

                for v in self._votes:
                    iw, jw = self.check_winner(i, j, v)

                    if iw:
                        i_wins += 1
                    if jw:
                        j_wins += 1

                if i_wins > j_wins:
                    pair_wise_wins[i][j] += 1
                elif i_wins < j_wins:
                    pair_wise_wins[j][i] += 1

        score = [0 for _ in range(self._no_candidates)]
        max_score = 0

        for i in range(self._no_candidates):
            no_wins, no_losses = self.no_wins_losses(i, pair_wise_wins)
            score[i] = no_wins - no_losses

            if score[i] > max_score:
                max_score = score[i]
                winner = i

        if max_score == self._no_candidates - 1:
            is_condorcet = True

        return is_condorcet, winner


    def no_wins_losses(self, i, pair_wise_wins):
        no_wins = 0
        no_losses = 0

        for a in range(self._no_candidates):
            if a == i:
                continue

            if pair_wise_wins[i][a] > 0:
                no_wins += 1
            elif pair_wise_wins[a][i] > 0:
                no_losses += 1

        return no_wins, no_losses


    def check_winner(self, i, j, v):
        pos_i = self.get_position(i, v)
        pos_j = self.get_position(j, v)

        if pos_i < pos_j:
            i_wins = True
            j_wins = False
        elif pos_i > pos_j:
            i_wins = False
            j_wins = True
        else:  # posI == posJ
            i_wins = False
            j_wins = False

        return i_wins, j_wins


    def get_position(self, i, v):
        toks = v.split()
        for a in range(len(toks)):
            if i == int(toks[a]):
                return a

        return self._no_candidates  # i is not in the preference order v


    def borda(self):
        score = [0 for _ in range(self._no_candidates)]

        for v in self._votes:
            for i in range(self._no_candidates):
                pos = self.get_position(i, v)
                score[i] += self._no_candidates - pos  # last ranked: 1 point, unranked: 0 points

        winner = -1
        max_score = 0

        for i in range(self._no_candidates):
            if score[i] > max_score:
                max_score = score[i]
                winner = i

        return winner


class VoterAgent(Agent):

    def __init__(self, rand):
        super().__init__()
        self._preference_order = ""
        self._rand = rand


    def setup(self):
        no_candidates = self.environment.memory["NoCandidates"]

        pref = self.random_order_permutation(no_candidates)

        if self._rand.random() < 0.5:
            # complete preference order
            parts = []
            for i in range(no_candidates):
                parts.append(f"{pref[i]}")
            self._preference_order = " ".join(parts)
        else:
            # incomplete preference order
            no_preferences = self._rand.randrange(no_candidates - 1) + 1
            parts = []
            for i in range(no_preferences):
                parts.append(f"{pref[i]}")
            self._preference_order = " ".join(parts)


    def random_order_permutation(self, n):
        return sorted(range(n), key=lambda x: self._rand.random())


    def act(self, message):
        action, parameters = message.parse_to_string()  # action == "start"
        self.handle_start()


    def handle_start(self):
        self.send("teller", f"vote {self._preference_order}")

        self.stop()


def main():
    # select the Condorcet winner if one exists, otherwise use Borda count

    env = EnvironmentMas(random_order=False)

    auctioneer_agent = TellerAgent()
    env.add(auctioneer_agent, "teller")

    env.memory["NoCandidates"] = 5

    no_voters = 99
    for i in range(1, no_voters + 1):
        voter_agent = VoterAgent(_rand)
        env.add(voter_agent, f"voter{i:02d}")

    env.start()


if __name__ == "__main__":
    main()
