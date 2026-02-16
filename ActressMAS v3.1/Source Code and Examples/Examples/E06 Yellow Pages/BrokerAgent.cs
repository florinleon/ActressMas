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
    public class BrokerAgent : Agent
    {
        private Dictionary<string, List<string>> _serviceProviders;

        public BrokerAgent()
        {
            _serviceProviders = new Dictionary<string, List<string>>();
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out List<string> parameters);

                switch (action)
                {
                    case "register":
                        HandleRegister(message.Sender, parameters[0]);
                        break;

                    case "unregister":
                        HandleUnregister(message.Sender, parameters[0]);
                        break;

                    case "search":
                        HandleSearch(message.Sender, parameters[0]);
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

        private void HandleRegister(string provider, string service)
        {
            if (_serviceProviders.ContainsKey(service))
            {
                List<string> sp = _serviceProviders[service];
                if (!sp.Contains(provider))
                    sp.Add(provider);
            }
            else
            {
                List<string> sp = new List<string> { provider };
                _serviceProviders.Add(service, sp);
            }
        }

        private void HandleUnregister(string provider, string service)
        {
            if (_serviceProviders.ContainsKey(service))
            {
                List<string> sp = _serviceProviders[service];
                if (sp.Contains(provider))
                    sp.Remove(provider);
            }
        }

        private void HandleSearch(string client, string service)
        {
            if (_serviceProviders.ContainsKey(service))
            {
                List<string> sp = _serviceProviders[service];
                string res = "";
                foreach (string p in sp)
                    res += $"{p} ";
                Send(client, $"providers {res.Trim()}");
            }
        }
    }
}