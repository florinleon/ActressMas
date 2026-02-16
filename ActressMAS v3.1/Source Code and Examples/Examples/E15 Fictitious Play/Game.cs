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

using System;

namespace FictitiousPlay
{
    public static class Game
    {
        // Prisoner's Dilemma:
        //                 Opponent
        //               C         D
        //        C    (3,3)     (0,5)
        // Me:
        //        D    (5,0)     (1,1)

        public const string ActionA = "C";  // cooperate
        public const string ActionB = "D";  // defect

        public static readonly string[] Actions = { ActionA, ActionB };

        public static double Payoff(string myAction, string otherAction)
        {
            // Mutual cooperation: (C, C) -> 3
            if (myAction == ActionA && otherAction == ActionA)
                return 3.0;

            // Mutual defection: (D, D) -> 1
            if (myAction == ActionB && otherAction == ActionB)
                return 1.0;

            // I cooperate, opponent defects: (C, D) -> 0
            if (myAction == ActionA && otherAction == ActionB)
                return 0.0;

            // I defect, opponent cooperates: (D, C) -> 5
            if (myAction == ActionB && otherAction == ActionA)
                return 5.0;

            throw new ArgumentException("Unknown actions.");
        }
    }
}
