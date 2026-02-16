/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Game of Life using the ActressMas framework              *
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
using System.Threading;

namespace GameOfLife
{
    public enum TurnType { Evolving, Updating };

    public class GridAgent : Agent
    {
        private int _size;
        public bool[,] CellStates { get; set; }
        private TurnType _turnType;
        private GridForm _formGui;

        public override void Setup()
        {
            Observables["Name"] = Name;

            _size = Environment.Memory["Size"];
            CellStates = new bool[_size, _size];
            _turnType = TurnType.Evolving;

            var t = new Thread(new ThreadStart(GUIThread));
            Thread.Sleep(100);
            t.Start();
        }

        private void GUIThread()
        {
            _formGui = new GridForm();
            _formGui.SetOwner(this);
            _formGui.ShowDialog();
        }

        public override bool PerceptionFilter(Dictionary<string, string> observed) =>
            true; // see all properties (position and state) of other cell agents

        public override void See(List<ObservableAgent> observableAgents)
        {
            try
            {
                if (_turnType == TurnType.Evolving)
                    return;

                foreach (var oa in observableAgents)
                {
                    int x = Convert.ToInt32(oa.Observed["X"]);
                    int y = Convert.ToInt32(oa.Observed["Y"]);
                    CellStates[x, y] = (oa.Observed["State"] == "Living");  // => true or false
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override void ActDefault()
        {
            try
            {
                if (_turnType == TurnType.Evolving)
                {
                    Console.WriteLine("evolve");
                    Broadcast("evolve");
                    _turnType = TurnType.Updating;
                }
                else
                {
                    Console.WriteLine("update");
                    Broadcast("update");
                    _turnType = TurnType.Evolving;
                }

                _formGui.UpdatePlanetGUI();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}