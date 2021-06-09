/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Computation of the Shapley value for agents using        *
 *               the ActressMas framework                                 *
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

namespace Shapley
{
    public class Task
    {
        public int[] DifficultyLevel { get; set; }
        public int Price { get; set; }

        private Random _rand = new Random();
        private int _noAttributes, _maxLevel;

        public Task(int noAttributes, int maxLevel)
        {
            _noAttributes = noAttributes;
            _maxLevel = maxLevel;

            DifficultyLevel = new int[_noAttributes];
            Price = 0;

            for (int i = 0; i < _noAttributes; i++)
            {
                DifficultyLevel[i] = 6 * (_rand.Next(_maxLevel) + 1);
                Price += DifficultyLevel[i];
            }
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < _noAttributes; i++)
                s += $"{DifficultyLevel[i]} ";
            return s.Trim();
        }
    }
}