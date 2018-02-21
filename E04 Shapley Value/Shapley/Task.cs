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

namespace Shapley
{
    public class Task
    {
        public int[] DifficultyLevel { get; set; }
        public int Price { get; set; }

        public Task()
        {
            DifficultyLevel = new int[Utils.NoAttributes];
            Price = 0;

            for (int i = 0; i < Utils.NoAttributes; i++)
            {
                DifficultyLevel[i] = 6 * (Utils.RandNoGen.Next(Utils.MaxLevel) + 1);
                Price += DifficultyLevel[i];
            }
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < Utils.NoAttributes; i++)
                s += DifficultyLevel[i] + " ";
            return s.Trim();
        }
    }
}