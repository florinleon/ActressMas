/**************************************************************************
 *                                                                        *
 *  Description: Simple example of using observables in ActressMas        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Copyright:   (c) 2020, Florin Leon                                    *
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

namespace NumberGame
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var env = new EnvironmentMas(noTurns: 10, randomOrder: false, parallel: false);

            var a1 = new MyAgent();
            a1.UsingObservables = true;
            env.Add(a1, "Agent1");

            var a2 = new MyAgent();
            a2.UsingObservables = true;
            env.Add(a2, "Agent2");

            var a3 = new MyAgent();
            a3.UsingObservables = true;
            env.Add(a3, "Agent3");

            env.Start();
        }
    }

    public class MyAgent : Agent
    {
        private List<ObservableAgent> _observableAgents = null;

        public override void Setup()
        {
            Observables["Name"] = Name;
            Observables["Number"] = $"{Numbers.GenerateNumber():F2}";
        }

        public override bool PerceptionFilter(Dictionary<string, string> observed)
        {
            double myNumber = Convert.ToDouble(Observables["Number"]);
            double obsNumber = Convert.ToDouble(observed["Number"]);
            return (Math.Abs(myNumber - obsNumber) < 10);
        }

        public override void See(List<ObservableAgent> observableAgents)
        {
            _observableAgents = observableAgents;
        }

        public override void ActDefault()
        {
            Console.Write($"I am {Name}. ");

            if (_observableAgents == null || _observableAgents.Count == 0)
            {
                Console.WriteLine("I did't see anything interesting.");
            }
            else
            {
                Console.WriteLine($"My number is {Observables["Number"]} and I saw:");
                foreach (var oa in _observableAgents)
                    Console.WriteLine($"{oa.Observed["Name"]} with number = {oa.Observed["Number"]}");
            }

            Observables["Number"] = $"{Numbers.GenerateNumber():F2}";

            Console.WriteLine($"My number is now {Observables["Number"]}");
            Console.WriteLine("----------------------------------------------");
        }
    }

    public class Numbers
    {
        private static Random _rand = new Random();

        public static double GenerateNumber()
        {
            return _rand.NextDouble() * 30;
        }
    }
}