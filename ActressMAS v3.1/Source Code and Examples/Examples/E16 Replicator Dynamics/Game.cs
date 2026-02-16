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

namespace ReplicatorDynamics
{
    public static class Game
    {
        /// <summary>
        /// Symmetric normal-form game payoff matrix A, where
        /// A[i,j] is the payoff of pure strategy i against pure strategy j.
        /// Must be square and have strictly positive payoffs for the basic
        /// x_i' = x_i * (u_i / u_bar) replicator rule.
        /// </summary>
        public static double[,] PayoffMatrix { get; set; }

        public static int NoStrategies => PayoffMatrix.GetLength(0);

        public static double[] ComputeExpectedPayoffs(double[] x)
        {
            int n = NoStrategies;
            var u = new double[n];

            for (int i = 0; i < n; i++)
            {
                double ui = 0.0;

                for (int j = 0; j < n; j++)
                    ui += PayoffMatrix[i, j] * x[j];

                u[i] = ui;
            }

            return u;
        }

        public static double ComputeAveragePayoff(double[] x, double[] u)
        {
            double uBar = 0.0;

            for (int i = 0; i < x.Length; i++)
                uBar += x[i] * u[i];

            return uBar;
        }
    }
}
