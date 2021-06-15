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
using System.Collections.Generic;

namespace Bdi
{
    public class PatrolAgent : Agent
    {
        private Dictionary<string, int> _beliefs;
        private HashSet<string> _desires;
        private string _intention; // only 1 intention active in this model
        private List<string> _plan;
        private bool _needToReplan;
        private int _size;

        public PatrolAgent()
        {
            _beliefs = new Dictionary<string, int>();
            _desires = new HashSet<string>();
            _intention = "";
            _plan = new List<string>();
        }

        public override void Setup()
        {
            Console.WriteLine($"Starting {Name}");

            _size = Environment.Memory["Size"];

            _beliefs["position"] = 0;
            _beliefs["have-water"] = 0;
            _beliefs["fire"] = -1; // unknown position
            _beliefs["water"] = -1; // unknown position

            Send("terrain", "move right");
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out List<string> parameters);

                switch (action)
                {
                    case "percepts":
                        BeliefRevision(parameters);
                        GenerateOptions();
                        FilterDesires();
                        if (_needToReplan) // if the environment is very dynamic, one can replan after each perception act
                            MakePlan();
                        ExecuteAction();
                        break;

                    case "got-water":
                        _beliefs["have-water"] = 1;
                        break;

                    case "fire-out":
                        _beliefs["have-water"] = 0;
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Console.WriteLine(ex.ToString()); // for debugging
            }
        }

        private void BeliefRevision(List<string> parameters)
        {
            _beliefs["position"] = Convert.ToInt32(parameters[0]);

            int visualFieldSize = 3;
            var visualField = new TerrainState[visualFieldSize];
            for (int i = 0; i < visualFieldSize; i++)
                visualField[i] = (TerrainState)(Convert.ToInt32(parameters[i + 1]));

            // "see" and update beliefs

            if (_intention == "extinguish-fire")
            {
                if (_plan.Count > 0) // plan in progress
                    return;
                else // plan finished
                {
                    bool fireOut = true;
                    for (int i = 0; i < visualFieldSize; i++)
                        if (visualField[i] == TerrainState.Fire)
                            fireOut = false;
                    if (fireOut)
                        _beliefs["fire"] = -1;
                }
            }

            for (int i = 0; i < visualFieldSize; i++)
            {
                if (visualField[i] == TerrainState.Water)
                    _beliefs["water"] = _beliefs["position"] + i - 1;

                if (visualField[i] == TerrainState.Fire)
                    _beliefs["fire"] = _beliefs["position"] + i - 1;
            }
        }

        private void GenerateOptions()
        {
            if (_desires.Count == 0)
                _desires.Add("patrol-right");

            if (_intention == "extinguish-fire" && _plan.Count > 0) // plan in progress
                return;

            if (_beliefs["position"] == _size - 1) // at right end, turn left
            {
                _desires.Remove("patrol-right");
                _desires.Add("patrol-left");
            }
            else if (_beliefs["position"] == 0) // at left end, turn right
            {
                _desires.Remove("patrol-left");
                _desires.Add("patrol-right");
            }

            if (_beliefs["fire"] != -1) // fire position is known
            {
                _desires.Add("extinguish-fire");
            }
            else // the fire has been extinguished
            {
                _desires.Remove("extinguish-fire");
            }
        }

        private void FilterDesires()
        {
            string newIntention = "";

            if (_desires.Contains("extinguish-fire"))
                newIntention = "extinguish-fire";
            else if (_desires.Contains("patrol-left"))
                newIntention = "patrol-left";
            else if (_desires.Contains("patrol-right"))
                newIntention = "patrol-right";

            if (newIntention != _intention)
            {
                _intention = newIntention;
                _needToReplan = true;

                Console.WriteLine($"Adopting new intention: {_intention}");
            }
        }

        private void MakePlan()
        {
            _plan.Clear();
            _needToReplan = false;

            switch (_intention)
            {
                case "patrol-right":
                    for (int i = _beliefs["position"]; i < _size; i++)
                        _plan.Add("move right");
                    break;

                case "patrol-left":
                    for (int i = _beliefs["position"]; i >= 0; i--)
                        _plan.Add("move left");
                    break;

                case "extinguish-fire":

                    // 4 sub-goals: go to water, get water, go to fire, drop water

                    // the sub-goals can be implemented as a stack, and plans can be made separately for each sub-goal

                    // go to water
                    for (int i = _beliefs["position"]; i > _beliefs["water"]; i--)
                        _plan.Add("move left");

                    // get water
                    _plan.Add("get-water");

                    // go to fire
                    for (int i = _beliefs["water"]; i < _beliefs["fire"]; i++) // this assumes that the position of water < position of fire
                        _plan.Add("move right");

                    // drop water
                    _plan.Add("drop-water");
                    break;

                default:
                    break;
            }
        }

        private void ExecuteAction()
        {
            if (_plan.Count == 0) // plan finished
                _intention = "";

            string action = _plan[0];
            _plan.RemoveAt(0);

            Send("terrain", action);
        }
    }
}