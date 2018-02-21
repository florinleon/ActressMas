/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Learning Real-Time A* (LRTA*) search algorithm using     *
 *               the ActressMas framework                                 *
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

namespace LrtaStar
{
    public class SearchAgent : Agent
    {
        public class StatePairCost
        {
            public string FromState { get; set; }
            public string ToState { get; set; }
            public int Cost { get; set; }

            public StatePairCost(string state1, string state2, string cost)
            {
                FromState = state1;
                ToState = state2;
                Cost = Convert.ToInt32(cost);
            }

            public StatePairCost(string state1, string state2, int cost)
            {
                FromState = state1;
                ToState = state2;
                Cost = cost;
            }
        }

        private string initialState, goalState;
        private string currentState;

        private Dictionary<string, int> heuristics;
        private List<StatePairCost> costs;
        private List<string> heuristicQueries;

        public SearchAgent()
        {
            heuristics = new Dictionary<string, int>();
            costs = new List<StatePairCost>();
        }

        public override void Setup()
        {
            initialState = "A";
            goalState = "E";
            currentState = initialState;
            Send("map", "expand " + currentState);
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                string action; string parameters;
                Utils.ParseMessage(message.Content, out action, out parameters);

                switch (action)
                {
                    case "neighbors":
                        HandleNeighbors(parameters);
                        break;

                    case "heuristicsReply":
                        HandleHeuristicsReply(parameters);
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

        private void HandleNeighbors(string neighborsAndCosts)
        {
            string[] toks = neighborsAndCosts.Split(" |".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            heuristicQueries = new List<string>();

            for (int i = 0; i < toks.Length / 2; i++)
            {
                string neighborState = toks[i * 2];
                string costToNeighbor = toks[i * 2 + 1];

                costs.Add(new StatePairCost(currentState, neighborState, costToNeighbor));

                if (!heuristics.ContainsKey(neighborState))
                    heuristicQueries.Add(neighborState);
            }

            if (heuristicQueries.Count > 0)
            {
                string hq = goalState + " ";
                foreach (string s in heuristicQueries)
                    hq += s + " ";
                Send("map", "heuristicsQuery " + hq);
            }
            else
                MoveToNeighborState();
        }

        private void HandleHeuristicsReply(string heuristicValues)
        {
            string[] toks = heuristicValues.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < toks.Length; i++)
                heuristics.Add(heuristicQueries[i], Convert.ToInt32(toks[i]));
            MoveToNeighborState();
        }

        private void MoveToNeighborState()
        {
            int fnMin = int.MaxValue;
            List<StatePairCost> statesF = new List<StatePairCost>();

            foreach (StatePairCost spc in costs)
            {
                if (spc.FromState != currentState)
                    continue;

                // only the neighbors are processed here

                int fn = spc.Cost + heuristics[spc.ToState];

                statesF.Add(new StatePairCost(currentState, spc.ToState, fn));

                if (fn < fnMin)
                    fnMin = fn;
            }

            List<string> minStates = new List<string>(); // when there are more neighboring states with the same f

            foreach (StatePairCost spcf in statesF)
            {
                if (spcf.Cost == fnMin)
                    minStates.Add(spcf.ToState);
            }

            string nextState = minStates[Utils.RandNoGen.Next(minStates.Count)];

            Console.WriteLine("{0} moves to state {1}", this.Name, nextState);

            if (nextState == goalState)
            {
                Console.WriteLine("{0} reached goal state {1}", this.Name, nextState);
                Send("map", "finished");
                Stop();
                return;
            }

            if (heuristics.ContainsKey(currentState))
                heuristics[currentState] = fnMin;
            else
                heuristics.Add(currentState, fnMin);

            Console.WriteLine("{0} updates h({1}) = {2}", this.Name, currentState, heuristics[currentState]);

            currentState = nextState;

            Thread.Sleep(Utils.Delay);

            ProcessState();
        }

        private void ProcessState()
        {
            foreach (StatePairCost spc in costs)
            {
                if (spc.FromState == currentState)
                {
                    // it has been in this state before and already knows the neighbors
                    MoveToNeighborState();
                    return;
                }
            }

            Send("map", "expand " + currentState);
        }
    }
}