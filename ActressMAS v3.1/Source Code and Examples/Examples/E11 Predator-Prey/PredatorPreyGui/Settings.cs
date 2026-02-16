/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Predator-prey simulation (ants and doodlebugs) using     *
 *               ActressMas framework                                     *
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
using System.Linq;

namespace PredatorPreyGui
{
    public class Settings
    {
        public static int GridSize = 20;
        public static int NoTurns = 200;
        public static int NoAnts = 100;
        public static int NoDoodlebugs = 2;
        public static bool ShowWorld = false;
        public static bool Verbose = false;
        public static string WorldStateFileName = "world-state.txt";
        public static Random RandNoGen = new Random();

        public static int[] RandomPermutation(int n) =>
            Enumerable.Range(0, n).OrderBy(x => RandNoGen.Next()).ToArray();
    }
}