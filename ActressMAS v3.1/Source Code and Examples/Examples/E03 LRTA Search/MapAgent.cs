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
using System.IO;

namespace LrtaStar
{
    public class MapAgent : Agent
    {
        private Dictionary<string, string> _neighbors;
        private string[] _heuristics;

        public override void Setup()
        {
            _neighbors = new Dictionary<string, string>();

            string mapName = Environment.Memory["MapName"];

            var sr = new StreamReader(mapName + ".grx");
            string graph = sr.ReadToEnd();
            sr.Close();

            string[] lines = graph.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string[] toks = lines[i].Split('=');
                _neighbors.Add(toks[0].Trim(), toks[1].Trim());
            }

            sr = new StreamReader(mapName + ".grh");
            string all = sr.ReadToEnd();
            sr.Close();

            _heuristics = all.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }

        private string ExpandState(string state)
        {
            if (_neighbors.ContainsKey(state))
                return _neighbors[state];
            else
                return "";
        }

        private double HeuristicEstimate(string currentState, string goalState)
        {
            for (int i = 0; i < _heuristics.Length; i++)
            {
                string[] toks = _heuristics[i].Split('|');
                if (currentState == toks[0].Trim() && goalState == toks[1].Trim())
                    return Convert.ToInt32(toks[2]);
            }
            return 0;
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out string parameters);

                switch (action)
                {
                    case "expand":
                        HandleExpand(message.Sender, parameters);
                        break;

                    case "heuristicsQuery":
                        HandleHeuristicsQuery(message.Sender, parameters);
                        break;

                    case "finished":
                        Stop();
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

        private void HandleExpand(string sender, string state)
        {
            Send(sender, $"neighbors {ExpandState(state)}");
        }

        private void HandleHeuristicsQuery(string sender, string states)
        {
            string[] toks = states.Trim().Split();

            // the map agent is not interested in the goal state of a search agent, that is why it receives it within the query
            string goal = toks[0];

            string reply = "";
            for (int i = 1; i < toks.Length; i++)
                reply += $"{HeuristicEstimate(toks[i], goal)} ";
            Send(sender, $"heuristicsReply {reply}");
        }
    }
}