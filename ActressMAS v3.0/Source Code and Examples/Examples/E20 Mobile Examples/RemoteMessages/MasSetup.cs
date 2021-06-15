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

namespace RemoteMessages
{
    public class Global
    {
        public static RichTextBox Rtb;
    }

    public class MasSetup : RunnableMas
    {
        public override void RunMas(EnvironmentMas env)
        {
            string home = env.ContainerName;

            if (home == "")
            {
                Global.Rtb.AppendText("Container not activated.\r\n");
                return;
            }

            Global.Rtb.AppendText($"This is {home}\r\n");

            switch (home)
            {
                case "Container1":
                    var m = new PingAgent();
                    env.Add(m, "ping-agent");
                    break;

                case "Container2":
                    var a = new PongAgent();
                    env.Add(a, "pong-agent");
                    break;

                default:
                    break;
            }

            env.Start();
        }
    }
}