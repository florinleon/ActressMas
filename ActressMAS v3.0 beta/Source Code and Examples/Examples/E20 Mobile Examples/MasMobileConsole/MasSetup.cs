/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Developing mobile agents using the ActressMas framework  *
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

namespace MasMobileConsole
{
    public class MasSetup : RunnableMas
    {
        public override void RunMas(EnvironmentMas env)
        {
            string home = env.ContainerName;

            if (home == "")
            {
                Console.WriteLine("Container not activated.");
                return;
            }

            Console.WriteLine($"This is {home}");

            switch (home)
            {
                case "Container1":
                    var m = new MobileAgent();
                    env.Add(m, "mobile-k1");

                    for (int i = 1; i <= 1; i++)
                    {
                        var a = new StaticAgent();
                        env.Add(a, $"a{i}-k1");
                    }
                    break;

                case "Container2":
                    for (int i = 1; i <= 2; i++)
                    {
                        var a = new StaticAgent();
                        env.Add(a, $"b{i}-k1");
                    }
                    break;

                case "Container3":
                    for (int i = 1; i <= 3; i++)
                    {
                        var a = new StaticAgent();
                        env.Add(a, $"c{i}-k3");
                    }
                    break;

                default:
                    break;
            }

            env.Start();
        }
    }
}