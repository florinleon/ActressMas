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
using System.Collections.Generic;

namespace Shapley
{
    public class Utils
    {
        public static int NoAttributes = 3;
        public static int MaxLevel = 3;
        public static int NoTasks = 100;
        public static int NoBids = 3;

        public static int Delay = 1;
        public static Random RandNoGen = new Random();

        public static void ParseMessage(string content, out string action, out List<string> parameters)
        {
            string[] t = content.Split();

            action = t[0];

            parameters = new List<string>();
            for (int i = 1; i < t.Length; i++)
                parameters.Add(t[i]);
        }

        public static void ParseMessage(string content, out string action, out string parameters)
        {
            string[] t = content.Split();

            action = t[0];

            parameters = "";

            if (t.Length > 1)
            {
                for (int i = 1; i < t.Length - 1; i++)
                    parameters += t[i] + " ";
                parameters += t[t.Length - 1];
            }
        }


        public static string Str(string p1, double p2)
        {
            return string.Format("{0} {1:F6}", p1, p2);
        }

        public static string Str(string p1, double p2, double p3, double p4)
        {
            return string.Format("{0} {1:F6} {2:F6} {3:F6}", p1, p2, p3, p4);
        }

        public static string Str(object p1, object p2, object p3, object p4, object p5, object p6, object p7, object p8)
        {
            return string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", p1, p2, p3, p4, p5, p6, p7, p8);
        }

        
    }
}