/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: The reactive architecture using the ActressMas framework *
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
using System.Threading;

namespace Reactive
{
    public class PlanetAgent : Agent
    {
        private PlanetForm _formGui;
        public Dictionary<string, string> ExplorerPositions { get; set; }
        public Dictionary<string, string> ResourcePositions { get; set; }
        public Dictionary<string, string> Loads { get; set; }
        private static Random _rand = new Random();

        public PlanetAgent()
        {
            ExplorerPositions = new Dictionary<string, string>();
            ResourcePositions = new Dictionary<string, string>();
            Loads = new Dictionary<string, string>();

            Thread t = new Thread(new ThreadStart(GUIThread));
            t.Start();
            Thread.Sleep(100);
        }

        private void GUIThread()
        {
            _formGui = new PlanetForm();
            _formGui.SetOwner(this);
            _formGui.ShowDialog();
            System.Windows.Forms.Application.Run();
        }

        public override void Setup()
        {
            Console.WriteLine($"Starting {Name}");

            int size = Environment.Memory["Size"];
            int noResources = Environment.Memory["NoResources"];

            List<string> resPos = new List<string>();
            string compPos = $"{size / 2} {size / 2}";
            resPos.Add(compPos); // the position of the base

            for (int i = 1; i <= noResources; i++)
            {
                while (resPos.Contains(compPos)) // resources do not overlap
                {
                    int x = _rand.Next(size);
                    int y = _rand.Next(size);
                    compPos = $"{x} {y}";
                }

                ResourcePositions.Add($"res{i}", compPos);
                resPos.Add(compPos);
            }
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out string parameters);

                switch (action)
                {
                    case "position":
                        HandlePosition(message.Sender, parameters);
                        break;

                    case "change":
                        HandleChange(message.Sender, parameters);
                        break;

                    case "pick-up":
                        HandlePickUp(message.Sender, parameters);
                        break;

                    case "carry":
                        HandleCarry(message.Sender, parameters);
                        break;

                    case "unload":
                        HandleUnload(message.Sender);
                        break;

                    default:
                        break;
                }

                _formGui.UpdatePlanetGUI();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Console.WriteLine(ex.ToString()); // for debugging
            }
        }

        private void HandlePosition(string sender, string position)
        {
            ExplorerPositions.Add(sender, position);
            Send(sender, "move");
        }

        private void HandleChange(string sender, string position)
        {
            ExplorerPositions[sender] = position;

            foreach (string k in ExplorerPositions.Keys)
            {
                if (k == sender)
                    continue;
                if (ExplorerPositions[k] == position)
                {
                    Send(sender, "block");
                    return;
                }
            }

            foreach (string k in ResourcePositions.Keys)
            {
                if (ResourcePositions[k] == position)
                {
                    Send(sender, $"rock {k}");
                    return;
                }
            }

            Send(sender, "move");
        }

        private void HandlePickUp(string sender, string position)
        {
            Loads[sender] = position;
            Send(sender, "move");
        }

        private void HandleCarry(string sender, string position)
        {
            ExplorerPositions[sender] = position;
            string res = Loads[sender];
            ResourcePositions[res] = position;
            Send(sender, "move");
        }

        private void HandleUnload(string sender)
        {
            Loads.Remove(sender);
            Send(sender, "move");
        }
    }
}