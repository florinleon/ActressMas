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
using System.Threading;

namespace YellowPages
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new ConcurrentEnvironment();

            var brokerAgent = new BrokerAgent(); env.Add(brokerAgent, "broker"); brokerAgent.Start();

            Thread.Sleep(100);

            ProviderAgent pa1 = new ProviderAgent(ServiceType.Add); env.Add(pa1, "provider1"); pa1.Start();
            ProviderAgent pa2 = new ProviderAgent(ServiceType.Add); env.Add(pa2, "provider2"); pa2.Start();
            ProviderAgent pa3 = new ProviderAgent(ServiceType.Sub); env.Add(pa3, "provider3"); pa3.Start();
            ProviderAgent pa4 = new ProviderAgent(ServiceType.Sub); env.Add(pa4, "provider4"); pa4.Start();

            Thread.Sleep(100);

            ClientAgent ca1 = new ClientAgent(ServiceType.Add); env.Add(ca1, "client1"); ca1.Start();
            ClientAgent ca2 = new ClientAgent(ServiceType.Add); env.Add(ca2, "client2"); ca2.Start();
            ClientAgent ca3 = new ClientAgent(ServiceType.Sub); env.Add(ca3, "client3"); ca3.Start();
            ClientAgent ca4 = new ClientAgent(ServiceType.Sub); env.Add(ca4, "client4"); ca4.Start();

            Thread.Sleep(1000);

            pa1.Send("provider1", "force-unregister");

            ProviderAgent pa5 = new ProviderAgent(ServiceType.Sub); env.Add(pa5, "provider5"); pa5.Start();

            Thread.Sleep(1000);

            ClientAgent ca5 = new ClientAgent(ServiceType.Add); env.Add(ca5, "client5"); ca5.Start();
            ClientAgent ca6 = new ClientAgent(ServiceType.Sub); env.Add(ca6, "client6"); ca6.Start();

            env.WaitAll();
        }
    }
}