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
using System.Windows.Forms;

namespace MasMobileGuiTB
{
    public class Commons
    {
        public static RichTextBox Rtb;
    }

    public class MasSetup : RunnableMas
    {
        public override void RunConcurrentMas(ConcurrentEnvironment env)
        {
            string home = env.ContainerName;

            if (home == "")
            {
                Commons.Rtb.AppendText("Container not activated.\r\n");
                return;
            }

            Commons.Rtb.AppendText("This is " + home + "\r\n");

            switch (home)
            {
                case "Container1":
                    var m = new MobileAgent();
                    env.Add(m, "Container1.mobile");

                    for (int i = 1; i <= 1; i++)
                    {
                        var a = new StaticAgent();
                        env.Add(a, "Container1.a" + i);
                        a.Start();
                    }

                    m.Start();
                    break;

                case "Container2":
                    for (int i = 1; i <= 2; i++)
                    {
                        var a = new StaticAgent();
                        env.Add(a, "Container2.b" + i);
                        a.Start();
                    }
                    break;

                case "Container3":
                    for (int i = 1; i <= 3; i++)
                    {
                        var a = new StaticAgent();
                        env.Add(a, "Container3.c" + i);
                        a.Start();
                    }
                    break;

                default:
                    break;
            }

            env.WaitAll();
        }
    }
}