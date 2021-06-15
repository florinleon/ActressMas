/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Computation of the Shapley value for agents using        *
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

namespace Shapley
{
    public class ManagerAgent : Agent
    {
        private string[] _agentBidders;
        private List<string> _bids;
        private Task _currentTask;
        private int _taskCount = 0;
        private int _noBids, _noTasks, _noAttributes, _maxLevel;

        public override void Setup()
        {
            _noBids = Environment.Memory["NoBids"];
            _noTasks = Environment.Memory["NoTasks"];
            _noAttributes = Environment.Memory["NoAttributes"];
            _maxLevel = Environment.Memory["MaxLevel"];

            _agentBidders = new string[_noBids];
            GenerateNewTask();
        }

        private void GenerateNewTask()
        {
            _currentTask = new Task(_noAttributes, _maxLevel);
            _taskCount++;

            SelectAgents();
            Console.WriteLine($"Selected: {_agentBidders[0]} {_agentBidders[1]} {_agentBidders[2]}");

            for (int i = 0; i < _noBids; i++)
                Send(_agentBidders[i], $"task {_currentTask}");

            _bids = new List<string>();
        }

        private void SelectAgents()
        {
            int noAgents = Environment.NoAgents;

            _agentBidders[0] = "";
            while ((_agentBidders[0] == "") || !_agentBidders[0].StartsWith("worker"))
            {
                _agentBidders[0] = Environment.RandomAgent();
            }

            _agentBidders[1] = "";
            while (((_agentBidders[1] == "") || (_agentBidders[1] == _agentBidders[0])) || !_agentBidders[1].StartsWith("worker"))
            {
                _agentBidders[1] = Environment.RandomAgent();
            }

            _agentBidders[2] = "";
            while ((((_agentBidders[2] == "") || (_agentBidders[2] == _agentBidders[0])) || (_agentBidders[2] == _agentBidders[1])) || !_agentBidders[2].StartsWith("worker"))
            {
                _agentBidders[2] = Environment.RandomAgent();
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
                    case "time":
                        HandleTime(parameters);
                        break;

                    case "results":
                        HandleResults(parameters);
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

        private void HandleTime(string timeEstimate)
        {
            _bids.Add(timeEstimate);
            if (_bids.Count == _noBids)
                AnalyzeCoalitions();
        }

        private void HandleResults(string results)
        {
            string[] toks = results.Split();
            for (int i = 0; i < _noBids; i++)
            {
                double r = _currentTask.Price * Convert.ToDouble(toks[i]);
                Send(_agentBidders[i], $"reward {r}");
            }
            if (_taskCount >= _noTasks)
            {
                Console.WriteLine("Manager has finished");

                foreach (var a in Environment.FilteredAgents("worker"))
                    Send(a, "report");
            }
            else
            {
                GenerateNewTask();
            }
        }

        private void AnalyzeCoalitions()
        {
            int[,] timeToSolve = new int[_noBids, _noAttributes];

            for (int i = 0; i < _noBids; i++)
            {
                string[] toks = _bids[i].Split(new char[0]);
                for (int j = 0; j < _noAttributes; j++)
                    timeToSolve[i, j] = Convert.ToInt32(toks[j]);
            }

            int v1 = (timeToSolve[0, 0] + timeToSolve[0, 1]) + timeToSolve[0, 2];
            int v2 = (timeToSolve[1, 0] + timeToSolve[1, 1]) + timeToSolve[1, 2];
            int v3 = (timeToSolve[2, 0] + timeToSolve[2, 1]) + timeToSolve[2, 2];
            int v12 = (Math.Min(timeToSolve[0, 0], timeToSolve[1, 0]) + Math.Min(timeToSolve[0, 1], timeToSolve[1, 1])) + Math.Min(timeToSolve[0, 2], timeToSolve[1, 2]);
            int v13 = (Math.Min(timeToSolve[0, 0], timeToSolve[2, 0]) + Math.Min(timeToSolve[0, 1], timeToSolve[2, 1])) + Math.Min(timeToSolve[0, 2], timeToSolve[2, 2]);
            int v23 = (Math.Min(timeToSolve[1, 0], timeToSolve[2, 0]) + Math.Min(timeToSolve[1, 1], timeToSolve[2, 1])) + Math.Min(timeToSolve[1, 2], timeToSolve[2, 2]);
            int v123 = (Min3(timeToSolve[0, 0], timeToSolve[1, 0], timeToSolve[2, 0]) + Min3(timeToSolve[0, 1], timeToSolve[1, 1], timeToSolve[2, 1])) + Min3(timeToSolve[0, 2], timeToSolve[1, 2], timeToSolve[2, 2]);

            Send("shapley", $"calculate {v1} {v2} {v3} {v12} {v13} {v23} {v123}");
        }

        private int Min3(int x1, int x2, int x3) => Math.Min(x1, Math.Min(x2, x3));
    }
}