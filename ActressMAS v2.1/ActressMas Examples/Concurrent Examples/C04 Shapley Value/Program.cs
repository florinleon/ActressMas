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
using System.Threading;

namespace Shapley
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new ConcurrentEnvironment();

            for (int i = 1; i <= 3; i++) // Utils.MaxLevel
                for (int j = 1; j <= 3; j++)
                    for (int k = 1; k <= 3; k++)
                    {
                        var workerAgent = new WorkerAgent(i, j, k);
                        env.Add(workerAgent, string.Format("worker{0}{1}{2}", i, j, k));
                        workerAgent.Start();
                    }

            var calculatorAgent = new CalculatorAgent();
            env.Add(calculatorAgent, "shapley");
            calculatorAgent.Start();

            Thread.Sleep(100);

            var managerAgent = new ManagerAgent();
            env.Add(managerAgent, "manager");
            managerAgent.Start();

            env.WaitAll();
        }
    }
}