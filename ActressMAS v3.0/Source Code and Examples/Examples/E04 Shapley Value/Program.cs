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

namespace Shapley
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new EnvironmentMas(1000);

            for (int i = 1; i <= 3; i++) // MaxLevel
                for (int j = 1; j <= 3; j++)
                    for (int k = 1; k <= 3; k++)
                    {
                        var workerAgent = new WorkerAgent(i, j, k);
                        env.Add(workerAgent, $"worker{i}{j}{k}");
                    }

            var calculatorAgent = new CalculatorAgent();
            env.Add(calculatorAgent, "shapley");

            var managerAgent = new ManagerAgent();
            env.Add(managerAgent, "manager");

            env.Memory.Add("NoAttributes", 3);
            env.Memory.Add("MaxLevel", 3);
            env.Memory.Add("NoTasks", 100);
            env.Memory.Add("NoBids", 3);

            env.Start();
        }
    }
}