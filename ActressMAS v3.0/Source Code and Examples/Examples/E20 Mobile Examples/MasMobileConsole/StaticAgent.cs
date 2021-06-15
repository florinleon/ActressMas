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
    public class StaticAgent : Agent
    {
        private string _info;

        public override void Setup()
        {
            Console.WriteLine($"Static agent {Name} starting");
            _info = $"Info from agent {Name} in container {Environment.ContainerName}";
        }

        public override void Act(Message message)
        {
            Console.WriteLine($"\t{message.Format()}");

            if (message.Content == "request-info")
                Send(message.Sender, _info);
        }
    }
}