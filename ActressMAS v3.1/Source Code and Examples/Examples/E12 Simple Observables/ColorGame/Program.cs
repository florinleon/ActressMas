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

namespace ColorGame
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var env = new EnvironmentMas(noTurns: 10, randomOrder: false, parallel: false);

            var a1 = new Agent1();
            a1.UsingObservables = true;
            env.Add(a1, "Agent1");

            var a2 = new Agent2();
            a2.UsingObservables = true;
            env.Add(a2, "Agent2");

            var a3 = new Agent3();
            a3.UsingObservables = true;
            env.Add(a3, "Agent3");

            env.Start();
        }
    }

    public class MyAgent : Agent
    {
        private List<ObservableAgent> _observableAgents = null;

        public override void See(List<ObservableAgent> observableAgents)
        {
            Console.WriteLine($"I am {Name}. I am looking around...");
            _observableAgents = observableAgents;
        }

        public override void ActDefault()
        {
            if (_observableAgents == null || _observableAgents.Count == 0)
            {
                Console.Write("I did't see anything interesting");
            }
            else
            {
                Console.Write("I saw: ");
                foreach (var oa in _observableAgents)
                    Console.Write($"{oa.Observed["Name"]} ({oa.Observed["Color"]}) ");
            }

            Observables["Color"] = Colors.GenerateColor();

            Console.WriteLine($"\r\nMy color is now {Observables["Color"]}");
            Console.WriteLine("-------------------------------------------");
        }
    }

    public class Agent1 : MyAgent
    {
        public override void Setup()
        {
            Observables["Name"] = Name;
            Observables["Color"] = "red";
        }

        public override bool PerceptionFilter(Dictionary<string, string> observed)
        {
            return observed["Color"] == "green";
        }
    }

    public class Agent2 : MyAgent
    {
        public override void Setup()
        {
            Observables["Name"] = Name;
            Observables["Color"] = "green";
        }

        public override bool PerceptionFilter(Dictionary<string, string> observed)
        {
            return observed["Color"] == "blue";
        }
    }

    public class Agent3 : MyAgent
    {
        public override void Setup()
        {
            Observables["Name"] = Name;
            Observables["Color"] = "blue";
        }

        public override bool PerceptionFilter(Dictionary<string, string> observed)
        {
            return observed["Color"] == "red";
        }
    }

    public class Colors
    {
        private static string[] _colors = new string[] { "red", "green", "blue" };
        private static Random _rand = new Random();

        public static string GenerateColor()
        {
            return _colors[_rand.Next(_colors.Length)];
        }
    }
}