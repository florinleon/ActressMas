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
    public class PlanetAgent : TurnBasedAgent
    {
        private PlanetForm _formGui;
        public Dictionary<string, string> ExplorerPositions { get; set; }
        public Dictionary<string, string> ResourcePositions { get; set; }
        public Dictionary<string, string> Loads { get; set; }

        public PlanetAgent()
        {
            ExplorerPositions = new Dictionary<string, string>();
            ResourcePositions = new Dictionary<string, string>();
            Loads = new Dictionary<string, string>();

            Thread t = new Thread(new ThreadStart(GUIThread));
            t.Start();
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
            Console.WriteLine("Starting " + Name);

            List<string> resPos = new List<string>();
            string compPos = Utils.Str(Utils.Size / 2, Utils.Size / 2);
            resPos.Add(compPos); // the position of the base

            for (int i = 1; i <= Utils.NoResources; i++)
            {
                while (resPos.Contains(compPos)) // resources do not overlap
                {
                    int x = Utils.RandNoGen.Next(Utils.Size);
                    int y = Utils.RandNoGen.Next(Utils.Size);
                    compPos = Utils.Str(x, y);
                }

                ResourcePositions.Add("res" + i, compPos);
                resPos.Add(compPos);
            }
        }

        public override void Act(Queue<Message> messages)
        {
            try
            {
                while (messages.Count > 0)
                {
                    Message message = messages.Dequeue();
                    Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                    string action; string parameters;
                    Utils.ParseMessage(message.Content, out action, out parameters);

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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                    Send(sender, "rock " + k);
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