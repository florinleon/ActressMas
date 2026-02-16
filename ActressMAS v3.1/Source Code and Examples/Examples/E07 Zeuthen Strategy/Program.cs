/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Zeuthen strategy using the ActressMas framework          *
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

using ActressMas;
using System;

namespace Zeuthen
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new EnvironmentMas(randomOrder: false, parallel: false);
            var agent1 = new BargainingAgent(); env.Add(agent1, "agent1");
            var agent2 = new BargainingAgent(); env.Add(agent2, "agent2");

            env.Memory["Eps"] = 0.1;
            env.Memory["Utility1"] = (Func<double, double>)((double deal) => 5.0 - deal);
            env.Memory["Utility2"] = (Func<double, double>)((double deal) => 2.0 / 3.0 * deal);

            env.Start();
        }
    }
}