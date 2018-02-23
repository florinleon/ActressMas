﻿/**************************************************************************
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
        private ServiceType type;
        private int operationParameter1, operationParameter2;

        public ClientAgent(ServiceType serviceType)
        {
            type = serviceType;
        }

        public override void Setup()
        {
            Send("broker", Utils.Str("search", type.ToString()));
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                string action; List<string> parameters;
                Utils.ParseMessage(message.Content, out action, out parameters);

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
            string selected = providers[Utils.RandNoGen.Next(providers.Count)];
            operationParameter1 = Utils.RandNoGen.Next(100);
            operationParameter2 = Utils.RandNoGen.Next(100);
            Send(selected, Utils.Str("request", operationParameter1, operationParameter2));
        }

        private void HandleResponse(string result)
        {
            Console.WriteLine("[{0}]: {1}({2}, {3}) = {4}", this.Name, type, operationParameter1, operationParameter2, result);
        }
    }
}