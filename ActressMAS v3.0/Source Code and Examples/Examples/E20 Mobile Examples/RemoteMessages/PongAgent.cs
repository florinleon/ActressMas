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

namespace RemoteMessages
{
    public class PongAgent : Agent
    {
        public override void Setup()
        {
            Global.Rtb.AppendText($"[{Name}] Waiting for requests...\r\n");
        }

        public override void Act(Message message)
        {
            try
            {
                Global.Rtb.AppendText($"\t{message.Format()}\r\n");

                if (message.Content == "request-info")
                {
                    string info = $"Info 1 from agent {Name} in container {Environment.ContainerName}";
                    Send(message.Sender, info);

                    info = $"Info 2 from agent {Name} in container {Environment.ContainerName}";
                    Send(message.Sender, info);
                }
            }
            catch (Exception ex)
            {
                Global.Rtb.AppendText($"{ex.Message}\r\n");
            }
        }
    }
}