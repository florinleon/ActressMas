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

namespace FictitiousPlay
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var env = new EnvironmentMas();

            string player1Name = "player1";
            string player2Name = "player2";
            string gameName = "game";

            int rounds = 50;

            var gameAgent = new GameAgent(player1Name, player2Name, rounds);
            var player1 = new PlayerAgent(gameName, player2Name);
            var player2 = new PlayerAgent(gameName, player1Name);

            env.Add(gameAgent, gameName);
            env.Add(player1, player1Name);
            env.Add(player2, player2Name);

            env.Start();
        }
    }
}
