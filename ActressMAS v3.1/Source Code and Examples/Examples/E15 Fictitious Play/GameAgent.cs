/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Fictitious Play using the ActressMas framework           *
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

using ActressMas;
using System;
using System.Collections.Generic;

namespace FictitiousPlay
{
    public class GameAgent : Agent
    {
        private readonly string _player1Name;
        private readonly string _player2Name;
        private readonly int _maxRounds;

        private int _currentRound;
        private Dictionary<string, string> _currentActions;

        private double _totalPayoff1;
        private double _totalPayoff2;

        public GameAgent(string player1Name, string player2Name, int maxRounds)
        {
            _player1Name = player1Name;
            _player2Name = player2Name;
            _maxRounds = maxRounds;
        }

        public override void Setup()
        {
            _currentRound = 0;
            _currentActions = new Dictionary<string, string>();

            Console.WriteLine($"Starting fictitious play for {_maxRounds} rounds between {_player1Name} and {_player2Name}.");

            StartNextRound();
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out string parameters);

                switch (action)
                {
                    case "action":
                        HandleAction(message.Sender, parameters);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void StartNextRound()
        {
            _currentRound++;

            if (_currentRound > _maxRounds)
            {
                Console.WriteLine();
                Console.WriteLine("Game finished.");
                Console.WriteLine($"Total payoff for {_player1Name}: {_totalPayoff1:F2}");
                Console.WriteLine($"Total payoff for {_player2Name}: {_totalPayoff2:F2}");

                Send(_player1Name, "stop");
                Send(_player2Name, "stop");

                Stop();
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"--- Round {_currentRound} ---");

            _currentActions[_player1Name] = null;
            _currentActions[_player2Name] = null;

            Send(_player1Name, $"play {_currentRound}");
            Send(_player2Name, $"play {_currentRound}");
        }

        private void HandleAction(string sender, string parameters)
        {
            string action = parameters.Trim();
            _currentActions[sender] = action;

            if (_currentActions[_player1Name] != null && _currentActions[_player2Name] != null)
            {
                string a1 = _currentActions[_player1Name];
                string a2 = _currentActions[_player2Name];

                double payoff1 = Game.Payoff(a1, a2);
                double payoff2 = Game.Payoff(a2, a1);

                _totalPayoff1 += payoff1;
                _totalPayoff2 += payoff2;

                Console.WriteLine($"Joint action: {_player1Name}={a1}, {_player2Name}={a2} | payoffs: {payoff1:F2}, {payoff2:F2}");

                Send(_player1Name, $"result {a2} {payoff1}");
                Send(_player2Name, $"result {a1} {payoff2}");

                StartNextRound();
            }
        }
    }
}
