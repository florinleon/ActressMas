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
    public class ClientAgent : TurnBasedAgent
    {
        private ServiceType _type;
        private int _operationParameter1, _operationParameter2;

        public ClientAgent(ServiceType serviceType)
        {
            _type = serviceType;
        }

        public override void Setup()
        {
            Send("broker", Utils.Str("search", _type.ToString()));
        }

        public override void Act(Queue<Message> messages)
        {
            try
            {
                while (messages.Count > 0)
                {
                    Message message = messages.Dequeue();
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void HandleProviders(List<string> providers)
        {
            string selected = providers[Utils.RandNoGen.Next(providers.Count)];
            _operationParameter1 = Utils.RandNoGen.Next(100);
            _operationParameter2 = Utils.RandNoGen.Next(100);
            Send(selected, Utils.Str("request", _operationParameter1, _operationParameter2));
        }

        private void HandleResponse(string result)
        {
            Console.WriteLine("[{0}]: {1}({2}, {3}) = {4}", this.Name, _type, _operationParameter1, _operationParameter2, result);
        }
    }
}