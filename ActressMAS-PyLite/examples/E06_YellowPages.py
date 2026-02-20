"""
/**************************************************************************
 *                                                                        *
 *  Description: Example of using the ActressMas framework                *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Copyright:   (c) 2026, Florin Leon                                    *
 *                                                                        *
 *  This program is free software; you can redistribute it and/or modify  *
 *  it under the terms of the GNU General Public License as published by  *
 *  the Free Software Foundation. This program is distributed in the      *
 *  hope that it will be useful, but WITHOUT ANY WARRANTY; without even   *
 *  the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR   *
 *  PURPOSE. See the GNU General Public License for more details.         *
 *                                                                        *
 **************************************************************************/
"""

from ActressMas import *
import random


class ServiceType:
    Add = "Add"
    Sub = "Sub"


class BrokerAgent(Agent):

    def __init__(self):
        super().__init__()
        self._service_providers = {}


    def act(self, message):
        try:
            print(f"\t{message.format()}")
            action, parameters = message.parse()

            if action == "register" and parameters:
                self._handle_register(message.sender, parameters[0])
            elif action == "unregister" and parameters:
                self._handle_unregister(message.sender, parameters[0])
            elif action == "search" and parameters:
                self._handle_search(message.sender, parameters[0])
        except Exception as ex:
            print(ex)


    def _handle_register(self, provider, service):
        if service in self._service_providers:
            providers = self._service_providers[service]
            if provider not in providers:
                providers.append(provider)
        else:
            self._service_providers[service] = [provider]


    def _handle_unregister(self, provider, service):
        if service in self._service_providers:
            providers = self._service_providers[service]
            if provider in providers:
                providers.remove(provider)


    def _handle_search(self, client, service):
        if service in self._service_providers:
            providers = self._service_providers[service]
            if providers:
                res = " ".join(providers)
                self.send(client, f"providers {res}")


class ClientAgent(Agent):

    def __init__(self, service_type):
        super().__init__()
        self._type = service_type
        self._operation_parameter1 = 0
        self._operation_parameter2 = 0
        self._rand = random.Random()


    def setup(self):
        self.send("broker", f"search {self._type}")


    def act(self, message):
        try:
            print(f"\t{message.format()}")
            action, parameters = message.parse()

            if action == "providers":
                self._handle_providers(parameters)
            elif action == "response" and parameters:
                self._handle_response(parameters[0])
        except Exception as ex:
            print(ex)


    def _handle_providers(self, providers):
        if not providers:
            return

        selected = providers[self._rand.randrange(len(providers))]
        self._operation_parameter1 = self._rand.randrange(100)
        self._operation_parameter2 = self._rand.randrange(100)
        self.send(selected, f"request {self._operation_parameter1} {self._operation_parameter2}")


    def _handle_response(self, result):
        print(f"[{self.name}]: {self._type}({self._operation_parameter1}, {self._operation_parameter2}) = {result}")


class ProviderAgent(Agent):

    def __init__(self, service_type):
        super().__init__()
        self._type = service_type


    def setup(self):
        self.send("broker", f"register {self._type}")


    def act(self, message):
        try:
            print(f"\t{message.format()}")
            action, parameters = message.parse()

            if action == "force-unregister":
                self._handle_force_unregister()
            elif action == "request":
                self._handle_request(message, parameters)
        except Exception as ex:
            print(ex)


    def _handle_force_unregister(self):
        self.send("broker", f"unregister {self._type}")


    def _handle_request(self, message, parameters):
        if len(parameters) < 2:
            return

        p1 = int(parameters[0])
        p2 = int(parameters[1])
        if self._type == ServiceType.Add:
            result = p1 + p2
        else:
            result = p1 - p2

        self.send(message.sender, f"response {result}")


def main():
    env = EnvironmentMas(100)

    broker_agent = BrokerAgent()
    env.add(broker_agent, "broker")

    pa1 = ProviderAgent(ServiceType.Add)
    env.add(pa1, "provider1")
    pa2 = ProviderAgent(ServiceType.Add)
    env.add(pa2, "provider2")
    pa3 = ProviderAgent(ServiceType.Sub)
    env.add(pa3, "provider3")
    pa4 = ProviderAgent(ServiceType.Sub)
    env.add(pa4, "provider4")

    ca1 = ClientAgent(ServiceType.Add)
    env.add(ca1, "client1")
    ca2 = ClientAgent(ServiceType.Add)
    env.add(ca2, "client2")
    ca3 = ClientAgent(ServiceType.Sub)
    env.add(ca3, "client3")
    ca4 = ClientAgent(ServiceType.Sub)
    env.add(ca4, "client4")

    env.start()

    pa1.send("provider1", "force-unregister")

    pa5 = ProviderAgent(ServiceType.Sub)
    env.add(pa5, "provider5")

    env.continue_simulation(100)

    ca5 = ClientAgent(ServiceType.Add)
    env.add(ca5, "client5")
    ca6 = ClientAgent(ServiceType.Sub)
    env.add(ca6, "client6")

    env.continue_simulation(100)


if __name__ == "__main__":
    main()
