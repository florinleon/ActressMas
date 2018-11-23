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
        override public void RunTurnBasedMas(TurnBasedEnvironment env)
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
                    env.Add(new MobileAgent(), "mobile");
                    env.Add(new StaticAgent(), "a1");
                    break;

                case "Container2":
                    for (int i = 1; i <= 2; i++)
                        env.Add(new StaticAgent(), "b" + i);
                    break;

                case "Container3":
                    for (int i = 1; i <= 3; i++)
                        env.Add(new StaticAgent(), "c" + i);
                    break;

                default:
                    break;
            }

            env.Start();
        }
    }
}