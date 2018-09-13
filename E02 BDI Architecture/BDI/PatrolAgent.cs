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
using System.Threading;

namespace Bdi
{
    public class PatrolAgent : Agent
    {
        private Dictionary<string, int> beliefs;
        private HashSet<string> desires;
        private string intention; // only 1 intention active in this model
        private List<string> plan;
        private bool needToReplan;

        public PatrolAgent()
        {
            beliefs = new Dictionary<string, int>();
            desires = new HashSet<string>();
            intention = "";
            plan = new List<string>();
        }

        public override void Setup()
        {
            Console.WriteLine("Starting " + Name);

            beliefs["position"] = 0;
            beliefs["have-water"] = 0;
            beliefs["fire"] = -1; // unknown position
            beliefs["water"] = -1; // unknown position

            Send("terrain", "move right");
        }

        public override void Act(Message message)
        {
            try
            {
                /*StringBuilder sb = new StringBuilder();
                sb.Append("\r\nBeliefs: ");
                foreach (string key in beliefs.Keys)
                    sb.AppendFormat("{0} = {1}  ", key, beliefs[key]);
                sb.Append("\r\nDesires: ");
                foreach (string d in desires)
                    sb.AppendFormat("{0}  ", d);
                sb.AppendFormat("\r\nIntention: {0}", intention);
                sb.Append("\r\nPlan: ");
                foreach (string act in plan)
                    sb.AppendFormat("{0}  ", act);
                Console.WriteLine(sb.ToString() + "\r\n");*/

                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                string action; List<string> parameters;
                Utils.ParseMessage(message.Content, out action, out parameters);

                switch (action)
                {
                    case "percepts":
                        BeliefRevision(parameters);
                        GenerateOptions();
                        FilterDesires();
                        if (needToReplan) // if the environment is very dynamic, one can replan after each perception act
                            MakePlan();
                        ExecuteAction();
                        break;

                    case "got-water":
                        beliefs["have-water"] = 1;
                        break;

                    case "fire-out":
                        beliefs["have-water"] = 0;
                        break;

                    default:
                        break;
                }

                Thread.Sleep(Utils.Delay);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Console.WriteLine(ex.ToString()); // for debugging
            }
        }

        private void BeliefRevision(List<string> parameters)
        {
            beliefs["position"] = Convert.ToInt32(parameters[0]);

            int visualFieldSize = 3;
            TerrainState[] visualField = new TerrainState[visualFieldSize];
            for (int i = 0; i < visualFieldSize; i++)
                visualField[i] = (TerrainState)(Convert.ToInt32(parameters[i + 1]));

            // "see" and update beliefs

            if (intention == "extinguish-fire")
            {
                if (plan.Count > 0) // plan in progress
                    return;
                else // plan finished
                {
                    bool fireOut = true;
                    for (int i = 0; i < visualFieldSize; i++)
                        if (visualField[i] == TerrainState.Fire)
                            fireOut = false;
                    if (fireOut)
                        beliefs["fire"] = -1;
                }
            }

            for (int i = 0; i < visualFieldSize; i++)
            {
                if (visualField[i] == TerrainState.Water)
                    beliefs["water"] = beliefs["position"] + i - 1;

                if (visualField[i] == TerrainState.Fire)
                    beliefs["fire"] = beliefs["position"] + i - 1;
            }
        }

        private void GenerateOptions()
        {
            if (desires.Count == 0)
                desires.Add("patrol-right");

            if (intention == "extinguish-fire" && plan.Count > 0) // plan in progress
                return;

            if (beliefs["position"] == Utils.Size - 1) // at right end, turn left
            {
                desires.Remove("patrol-right");
                desires.Add("patrol-left");
            }
            else if (beliefs["position"] == 0) // at left end, turn right
            {
                desires.Remove("patrol-left");
                desires.Add("patrol-right");
            }

            if (beliefs["fire"] != -1) // fire position is known
            {
                desires.Add("extinguish-fire");
            }
            else // the fire has been extinguished
            {
                desires.Remove("extinguish-fire");
            }
        }

        private void FilterDesires()
        {
            string newIntention = "";

            if (desires.Contains("extinguish-fire"))
                newIntention = "extinguish-fire";
            else if (desires.Contains("patrol-left"))
                newIntention = "patrol-left";
            else if (desires.Contains("patrol-right"))
                newIntention = "patrol-right";

            if (newIntention != intention)
            {
                intention = newIntention;
                needToReplan = true;

                Console.WriteLine("Adopting new intention: " + intention);
            }
        }

        private void MakePlan()
        {
            plan.Clear();
            needToReplan = false;

            switch (intention)
            {
                case "patrol-right":
                    for (int i = beliefs["position"]; i < Utils.Size; i++)
                        plan.Add("move right");
                    break;

                case "patrol-left":
                    for (int i = beliefs["position"]; i >= 0; i--)
                        plan.Add("move left");
                    break;

                case "extinguish-fire":

                    // 4 sub-goals: go to water, get water, go to fire, drop water

                    // the sub-goals can be implemented as a stack, and plans can be made separately for each sub-goal

                    // go to water
                    for (int i = beliefs["position"]; i > beliefs["water"]; i--)
                        plan.Add("move left");

                    // get water
                    plan.Add("get-water");

                    // go to fire
                    for (int i = beliefs["water"]; i < beliefs["fire"]; i++) // this assumes that the position of water < position of fire
                        plan.Add("move right");

                    // drop water
                    plan.Add("drop-water");
                    break;

                default:
                    break;
            }
        }

        private void ExecuteAction()
        {
            if (plan.Count == 0) // plan finished
                intention = "";

            string action = plan[0];
            plan.RemoveAt(0);

            Send("terrain", action);
        }
    }
}