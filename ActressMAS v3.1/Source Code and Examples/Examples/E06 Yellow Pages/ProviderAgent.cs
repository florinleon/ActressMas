/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Yellow pages using the ActressMas framework              *
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
using System.Collections.Generic;

namespace YellowPages
{
    public enum ServiceType { Add, Sub };

    public class ProviderAgent : Agent
    {
        private ServiceType _type;

        public ProviderAgent(ServiceType serviceType)
        {
            _type = serviceType;
        }

        public override void Setup()
        {
            Send("broker", $"register {_type}");
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out List<string> parameters);

                switch (action)
                {
                    case "force-unregister":
                        HandleForceUnregister();
                        break;

                    case "request":
                        HandleRequest(message, parameters);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void HandleForceUnregister()
        {
            Send("broker", $"unregister {_type}");
        }

        private void HandleRequest(Message message, List<string> parameters)
        {
            int p1 = Convert.ToInt32(parameters[0]);
            int p2 = Convert.ToInt32(parameters[1]);
            int result = (_type == ServiceType.Add) ? (p1 + p2) : (p1 - p2);
            Send(message.Sender, $"response {result}");
        }
    }
}