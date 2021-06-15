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
    public class WorkerAgent : Agent
    {
        private int[] _skillLevels;
        private double _totalReward;

        public WorkerAgent(int s1, int s2, int s3)
        {
            _skillLevels = new int[3];  // in the rest of the program, NoAttributes and MaxLevel are used
            _skillLevels[0] = s1;
            _skillLevels[1] = s2;
            _skillLevels[2] = s3;
            _totalReward = 0;
        }

        public override void Act(Message message)
        {
            try
            {
                message.Parse(out string action, out List<string> parameters);

                switch (action)
                {
                    case "task":
                        HandleTask(parameters);
                        break;

                    case "reward":
                        HandleReward(Convert.ToDouble(parameters[0]));
                        break;

                    case "report":
                        HandleReport();
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

        private void HandleTask(List<string> taskParameters)
        {
            string estimates = "";
            for (int i = 0; i < Environment.Memory["NoAttributes"]; i++)
            {
                int time = Convert.ToInt32(taskParameters[i]) / _skillLevels[i];
                estimates += $"{time} ";
            }
            Send("manager", $"time {estimates.Trim()}");
        }

        private void HandleReward(double reward)
        {
            _totalReward += reward;
        }

        private void HandleReport()
        {
            Console.WriteLine($"Agent {Name} has {_totalReward:F3}");
        }
    }
}