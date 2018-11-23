/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: The BDI architecture using the ActressMas framework      *
 *  Copyright:   (c) 2018, Florin Leon                                    *
 *                                                                        *
 *  Acknowledgement:                                                      *
 *  The idea of this example is inspired by this project:                 *
 *  https://github.com/gama-platform/gama/wiki/UsingBDI                   *
 *  The actual implementation is completely original.                     *
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
using System.Threading;
using System.Windows.Forms;

namespace Bdi
{
    public enum TerrainState { Normal, Fire, Water, GettingWater, None }

    public class TerrainAgent : ConcurrentAgent
    {
        private TerrainForm _formGui;
        public int PatrolPosition { get; set; }

        public TerrainState[] States { get; set; }

        private int _turns = 0;

        public TerrainAgent()
        {
            States = new TerrainState[Utils.Size];

            Thread t = new Thread(new ThreadStart(GUIThread));
            t.Start();
        }

        private void GUIThread()
        {
            _formGui = new TerrainForm();
            _formGui.SetOwner(this);
            _formGui.ShowDialog();
            Application.Run();
        }

        public override void Setup()
        {
            Console.WriteLine("Starting " + Name);

            States[0] = TerrainState.Water;

            for (int i = 1; i < Utils.Size; i++)
                States[i] = TerrainState.Normal;

            PatrolPosition = 0;
        }

        public override void Act(ActressMas.Message message)
        {
            try
            {
                if (_turns == 0)
                    Thread.Sleep(Utils.Delay);

                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                string action; string parameters;
                Utils.ParseMessage(message.Content, out action, out parameters);

                for (int i = 0; i < States.Length; i++)
                    if (States[i] == TerrainState.GettingWater)
                        States[i] = TerrainState.Water;

                switch (action)
                {
                    case "move":
                        if (_turns == 4) States[4] = TerrainState.Fire; // at some point, start a fire
                        if (_turns == 20) States[7] = TerrainState.Fire;
                        if (_turns == 50) States[9] = TerrainState.Fire;

                        if (parameters == "right")
                            PatrolPosition++;
                        else if (parameters == "left")
                            PatrolPosition--;

                        UpdateVisualField(message.Sender);
                        _turns++;
                        break;

                    case "get-water":
                        if (States[PatrolPosition] == TerrainState.Water)
                        {
                            States[PatrolPosition] = TerrainState.GettingWater;
                            Send(message.Sender, "got-water");
                        }
                        UpdateVisualField(message.Sender);
                        break;

                    case "drop-water":
                        if (States[PatrolPosition] == TerrainState.Fire)
                        {
                            States[PatrolPosition] = TerrainState.Normal;
                            Send(message.Sender, "fire-out");
                            UpdateVisualField(message.Sender);
                        }
                        break;

                    default:
                        break;
                }

                _formGui.UpdateTerrainGUI();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Console.WriteLine(ex.ToString()); // for debugging
            }
        }

        private void UpdateVisualField(string sender)
        {
            TerrainState left, current, right;

            if (PatrolPosition == 0) // left end
            {
                left = TerrainState.None;
                current = States[PatrolPosition];
                right = States[PatrolPosition + 1];
            }
            else if (PatrolPosition == Utils.Size - 1) // right end
            {
                left = States[PatrolPosition - 1];
                current = States[PatrolPosition];
                right = TerrainState.None;
            }
            else
            {
                left = States[PatrolPosition - 1];
                current = States[PatrolPosition];
                right = States[PatrolPosition + 1];
            }

            Send(sender, Utils.Str("percepts", PatrolPosition, (int)left, (int)current, (int)right));
        }
    }
}