/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Iterated Prisoner's Dilemma using ActressMas framework   *
 *  Copyright:   (c) 2018, Florin Leon                                    *
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

namespace IteratedPrisonersDilemma
{
    public class RandomPrisonerAgent : PrisonerAgent
    {
        private static Random _rand = new Random();

        protected override string ChooseAction(int lastOutcome)
        {
            if (_rand.NextDouble() < 0.5)
                return "confess";
            else
                return "deny";
        }
    }
}