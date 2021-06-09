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

namespace YellowPages
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new EnvironmentMas(100);

            var brokerAgent = new BrokerAgent(); env.Add(brokerAgent, "broker");

            ProviderAgent pa1 = new ProviderAgent(ServiceType.Add); env.Add(pa1, "provider1");
            ProviderAgent pa2 = new ProviderAgent(ServiceType.Add); env.Add(pa2, "provider2");
            ProviderAgent pa3 = new ProviderAgent(ServiceType.Sub); env.Add(pa3, "provider3");
            ProviderAgent pa4 = new ProviderAgent(ServiceType.Sub); env.Add(pa4, "provider4");

            ClientAgent ca1 = new ClientAgent(ServiceType.Add); env.Add(ca1, "client1");
            ClientAgent ca2 = new ClientAgent(ServiceType.Add); env.Add(ca2, "client2");
            ClientAgent ca3 = new ClientAgent(ServiceType.Sub); env.Add(ca3, "client3");
            ClientAgent ca4 = new ClientAgent(ServiceType.Sub); env.Add(ca4, "client4");

            env.Start();

            pa1.Send("provider1", "force-unregister");

            ProviderAgent pa5 = new ProviderAgent(ServiceType.Sub); env.Add(pa5, "provider5");

            env.Continue(100);

            ClientAgent ca5 = new ClientAgent(ServiceType.Add); env.Add(ca5, "client5");
            ClientAgent ca6 = new ClientAgent(ServiceType.Sub); env.Add(ca6, "client6");

            env.Continue(100);
        }
    }
}