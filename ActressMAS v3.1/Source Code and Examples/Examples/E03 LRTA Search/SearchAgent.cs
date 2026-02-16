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

        private string _initialState, _goalState;
        private string _currentState;
        private Dictionary<string, int> _heuristics;
        private List<StatePairCost> _costs;
        private List<string> _heuristicQueries;
        private Random _rand = new Random();

        public SearchAgent()
        {
            _heuristics = new Dictionary<string, int>();
            _costs = new List<StatePairCost>();
        }

        public override void Setup()
        {
            _initialState = "A";
            _goalState = "E";
            _currentState = _initialState;
            Send("map", $"expand {_currentState}");
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out string parameters);

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
            _heuristicQueries = new List<string>();

            for (int i = 0; i < toks.Length / 2; i++)
            {
                string neighborState = toks[i * 2];
                string costToNeighbor = toks[i * 2 + 1];

                _costs.Add(new StatePairCost(_currentState, neighborState, costToNeighbor));

                if (!_heuristics.ContainsKey(neighborState))
                    _heuristicQueries.Add(neighborState);
            }

            if (_heuristicQueries.Count > 0)
            {
                string hq = $"{_goalState} ";
                foreach (string s in _heuristicQueries)
                    hq += $"{s} ";
                Send("map", $"heuristicsQuery {hq}");
            }
            else
                MoveToNeighborState();
        }

        private void HandleHeuristicsReply(string heuristicValues)
        {
            string[] toks = heuristicValues.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < toks.Length; i++)
                _heuristics.Add(_heuristicQueries[i], Convert.ToInt32(toks[i]));
            MoveToNeighborState();
        }

        private void MoveToNeighborState()
        {
            int fnMin = int.MaxValue;
            List<StatePairCost> statesF = new List<StatePairCost>();

            foreach (StatePairCost spc in _costs)
            {
                if (spc.FromState != _currentState)
                    continue;

                // only the neighbors are processed here

                int fn = spc.Cost + _heuristics[spc.ToState];

                statesF.Add(new StatePairCost(_currentState, spc.ToState, fn));

                if (fn < fnMin)
                    fnMin = fn;
            }

            var minStates = new List<string>();  // when there are more neighboring states with the same f

            foreach (StatePairCost spcf in statesF)
            {
                if (spcf.Cost == fnMin)
                    minStates.Add(spcf.ToState);
            }

            string nextState = minStates[_rand.Next(minStates.Count)];

            Console.WriteLine($"{Name} moves to state {nextState}");

            if (nextState == _goalState)
            {
                Console.WriteLine($"{Name} reached goal state {nextState}");
                Send("map", "finished");
                Stop();
                return;
            }

            if (_heuristics.ContainsKey(_currentState))
                _heuristics[_currentState] = fnMin;
            else
                _heuristics.Add(_currentState, fnMin);

            Console.WriteLine($"{Name} updates h({_currentState}) = {_heuristics[_currentState]}");

            _currentState = nextState;

            Thread.Sleep(Environment.Memory["Delay"]);

            ProcessState();
        }

        private void ProcessState()
        {
            foreach (StatePairCost spc in _costs)
            {
                if (spc.FromState == _currentState)
                {
                    // it has been in this state before and already knows the neighbors
                    MoveToNeighborState();
                    return;
                }
            }

            Send("map", $"expand {_currentState}");
        }
    }
}