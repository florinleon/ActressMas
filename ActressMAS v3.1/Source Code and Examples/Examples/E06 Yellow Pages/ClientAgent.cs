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
    public class ClientAgent : Agent
    {
        private ServiceType _type;
        private int _operationParameter1, _operationParameter2;
        private static Random _rand = new Random();

        public ClientAgent(ServiceType serviceType)
        {
            _type = serviceType;
        }

        public override void Setup()
        {
            Send("broker", $"search {_type}");
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out List<string> parameters);

                switch (action)
                {
                    case "providers":
                        HandleProviders(parameters);
                        break;

                    case "response":
                        HandleResponse(parameters[0]);
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

        private void HandleProviders(List<string> providers)
        {
            string selected = providers[_rand.Next(providers.Count)];
            _operationParameter1 = _rand.Next(100);
            _operationParameter2 = _rand.Next(100);
            Send(selected, $"request {_operationParameter1} {_operationParameter2}");
        }

        private void HandleResponse(string result)
        {
            Console.WriteLine($"[{Name}]: {_type}({_operationParameter1}, {_operationParameter2}) = {result}");
        }
    }
}