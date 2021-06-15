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
using System.Linq;

namespace GameOfLife
{
    public class CellAgent : Agent
    {
        private int _x, _y;
        private string _state, _newState;
        private int _noLivingNeighbors;

        public CellAgent(string state, int x, int y)
        {
            _x = x;
            _y = y;
            _state = state;
        }

        public override void Setup()
        {
            Observables["Name"] = Name;
            Observables["X"] = $"{_x}";
            Observables["Y"] = $"{_y}";
            Observables["State"] = _state;  // "Living" or "Dead"
        }

        public override bool PerceptionFilter(Dictionary<string, string> observed)
        {
            if (observed["Name"] == "GridAgent")  // the grid is also an agent, but has no cell properties
                return false;

            // see only neighbors (8)
            int obsX = Convert.ToInt32(observed["X"]);
            int obsY = Convert.ToInt32(observed["Y"]);
            return (Math.Abs(obsX - _x) <= 1 && Math.Abs(obsY - _y) <= 1);
        }

        public override void See(List<ObservableAgent> observedAgents)
        {
            try
            {
                _noLivingNeighbors = observedAgents.Count(oa => oa.Observed["State"] == "Living");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override void Act(Message message)
        {
            try
            {
                switch (message.Content)
                {
                    case "evolve":
                        if (_state == "Living" && (_noLivingNeighbors == 2 || _noLivingNeighbors == 3))
                            _newState = "Living";
                        else if (_state == "Dead" && _noLivingNeighbors == 3)
                            _newState = "Living";
                        else
                            _newState = "Dead";
                        break;

                    case "update":
                        _state = _newState;
                        Observables["State"] = _state;
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
    }
}