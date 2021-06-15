/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Voting using the ActressMas framework                    *
 *  Copyright:   (c) 2021, Florin Leon                                    *
 *                                                                        *
 *  This program is free software; you can redistribute it and/or modify  *
 *  it under the terms of the GNU General Public License as published by  *
 *  the Free Software Foundation. This program is distributed in the      *
 *  hope that it will be useful, but WITHOUT ANY WARRANTY; without even   *
 *  the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR   *
 *  PURPOSE. See the GNU General Public License for more details.         *
 *                                                                        *
 **************************************************************************/

using ActressMas;
using System;
using System.Collections.Generic;

namespace Voting
{
    public class TellerAgent : Agent
    {
        private int _turnsToWait, _noCandidates;
        private List<string> _votes;

        public TellerAgent()
        {
            _votes = new List<string>();
        }

        public override void Setup()
        {
            _noCandidates = Environment.Memory["NoCandidates"];
            _turnsToWait = 2;
            Broadcast("start");
        }

        public override void Act(Message message)
        {
            message.Parse(out string action, out string parameters);  // action == "vote"
            HandleVote(parameters);
        }

        public override void ActDefault()
        {
            if (--_turnsToWait <= 0)
                HandleResults();
        }

        private void HandleVote(string preferenceOrder)
        {
            Console.WriteLine(preferenceOrder);
            _votes.Add(preferenceOrder);  // optimization: use int[][] instead of string[] to store the votes
        }

        private void HandleResults()
        {
            Copeland(out bool isCondorcet, out int winner);

            if (isCondorcet)
                Console.WriteLine($"Candidate {winner} is the Condorcet winner");
            else
                Console.WriteLine("There is no Condorcet winner");

            Console.WriteLine($"Using Copeland's method, candidate {winner} is the winner");
            winner = Borda();
            Console.WriteLine($"Using Borda count, candidate {winner} is the winner");

            Stop();
        }

        private void Copeland(out bool isCondorcet, out int winner)
        {
            isCondorcet = false;
            winner = -1;

            int[,] pairWiseWins = new int[_noCandidates, _noCandidates];

            for (int i = 0; i < _noCandidates - 1; i++)
                for (int j = i + 1; j < _noCandidates; j++)
                {
                    int iWins = 0, jWins = 0;

                    foreach (string v in _votes)
                    {
                        CheckWinner(i, j, v, out bool iw, out bool jw);

                        if (iw)
                            iWins++;
                        if (jw)
                            jWins++;
                    }

                    if (iWins > jWins)
                        pairWiseWins[i, j]++;
                    else if (iWins < jWins)
                        pairWiseWins[j, i]++;
                }

            int[] score = new int[_noCandidates];
            int maxScore = 0;

            for (int i = 0; i < _noCandidates; i++)
            {
                NoWinsLosses(i, pairWiseWins, out int noWins, out int noLosses);
                score[i] = noWins - noLosses;

                if (score[i] > maxScore)
                {
                    maxScore = score[i];
                    winner = i;
                }
            }

            if (maxScore == _noCandidates - 1)
                isCondorcet = true;
        }

        private void NoWinsLosses(int i, int[,] pairWiseWins, out int noWins, out int noLosses)
        {
            noWins = 0;
            noLosses = 0;

            for (int a = 0; a < _noCandidates; a++)
            {
                if (a == i)
                    continue;

                if (pairWiseWins[i, a] > 0)
                    noWins++;
                else if (pairWiseWins[a, i] > 0)
                    noLosses++;
            }
        }

        private void CheckWinner(int i, int j, string v, out bool iWins, out bool jWins)
        {
            int posI = GetPosition(i, v);
            int posJ = GetPosition(j, v);

            if (posI < posJ)
            {
                iWins = true;
                jWins = false;
            }
            else if (posI > posJ)
            {
                iWins = false;
                jWins = true;
            }
            else // posI == posJ
            {
                iWins = false;
                jWins = false;
            }
        }

        private int GetPosition(int i, string v)
        {
            string[] toks = v.Split();
            for (int a = 0; a < toks.Length; a++)
                if (i == Convert.ToInt32(toks[a]))
                    return a;

            return _noCandidates; // i is not in the preference order v
        }

        private int Borda()
        {
            int[] score = new int[_noCandidates];

            foreach (string v in _votes)
            {
                for (int i = 0; i < _noCandidates; i++)
                {
                    int pos = GetPosition(i, v);
                    score[i] += _noCandidates - pos;
                }
            }

            int winner = -1;
            int maxScore = 0;

            for (int i = 0; i < _noCandidates; i++)
            {
                if (score[i] > maxScore)
                {
                    maxScore = score[i];
                    winner = i;
                }
            }

            return winner;
        }
    }
}