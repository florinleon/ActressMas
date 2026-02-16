/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Replicator Dynamics using the ActressMas framework       *
 *  Copyright:   (c) 2026, Florin Leon                                    *
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

namespace ReplicatorDynamics
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // 2x2 coordination game
            //          A     B
            //   A    3.0   0.0
            //   B    0.0   3.0
            //
            // Payoffs are strictly positive, which matches the simple replicator rule.

            Game.PayoffMatrix = new double[,]
            {
                { 3.0, 0.0 },
                { 0.0, 3.0 }
            };

            int n = Game.NoStrategies;

            // Initial distribution over strategies; must sum to 1.0
            var initialProportions = new double[] { 0.5, 0.5 };

            double sum = 0.0;
            foreach (double v in initialProportions)
                sum += v;

            var env = new EnvironmentMas();

            string[] strategyNames = new string[n];
            for (int i = 0; i < n; i++)
                strategyNames[i] = $"S{i}";

            int maxRounds = 50;

            var gameAgent = new GameAgent(strategyNames, maxRounds);
            env.Add(gameAgent, "game");

            for (int i = 0; i < n; i++)
            {
                var agent = new StrategyAgent("game", i, initialProportions[i]);
                env.Add(agent, strategyNames[i]);
            }

            env.Start();
        }
    }
}
