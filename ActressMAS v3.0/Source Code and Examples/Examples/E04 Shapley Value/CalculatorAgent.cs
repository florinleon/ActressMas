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
    public class CalculatorAgent : Agent
    {
        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out List<string> parameters);

                // utility = 1 / time

                double u1 = 1.0 / Convert.ToDouble(parameters[0]);
                double u2 = 1.0 / Convert.ToDouble(parameters[1]);
                double u3 = 1.0 / Convert.ToDouble(parameters[2]);
                double u12 = 1.0 / Convert.ToDouble(parameters[3]);
                double u13 = 1.0 / Convert.ToDouble(parameters[4]);
                double u23 = 1.0 / Convert.ToDouble(parameters[5]);
                double u123 = 1.0 / Convert.ToDouble(parameters[6]);

                double max = MaxGen(u1, u2, u3, u12, u13, u23, u123);

                if (AreEqual(max, u1))
                    Send("manager", "results 1 0 0");
                else if (AreEqual(max, u2))
                    Send("manager", "results 0 1 0");
                else if (AreEqual(max, u3))
                    Send("manager", "results 0 0 1");
                else if (AreEqual(max, u12))
                {
                    // computing the Shapley value with 2 agents
                    double valueAdded1 = (u1 + (u12 - u2)) / 2.0; // AB, BA
                    double valueAdded2 = (u2 + (u12 - u1)) / 2.0; // BA, AB
                    Normalize(ref valueAdded1, ref valueAdded2);
                    Send("manager", $"results {valueAdded1} {valueAdded2} 0");
                }
                else if (AreEqual(max, u13))
                {
                    // computing the Shapley value with 2 agents
                    double valueAdded1 = (u1 + (u13 - u3)) / 2.0; // AC, CA
                    double valueAdded3 = (u3 + (u13 - u1)) / 2.0; // CA, AC
                    Normalize(ref valueAdded1, ref valueAdded3);
                    Send("manager", $"results {valueAdded1} 0 {valueAdded3}");
                }
                else if (AreEqual(max, u23))
                {
                    // computing the Shapley value with 2 agents
                    double valueAdded2 = (u2 + (u23 - u3)) / 2.0; // BC, CB
                    double valueAdded3 = (u3 + (u23 - u2)) / 2.0; // CB, BC
                    Normalize(ref valueAdded2, ref valueAdded3);
                    Send("manager", $"results 0 {valueAdded2} {valueAdded3}");
                }
                else if (AreEqual(max, u123))
                {
                    // computing the Shapley value with 3 agents
                    double valueAdded1 = (u1 + u1 + (u12 - u2) + (u123 - u23) + (u13 - u3) + (u123 - u23)) / 6.0; // ABC, ACB, BAC, BCA, CAB, CBA
                    double valueAdded2 = (u2 + u2 + (u12 - u1) + (u123 - u13) + (u123 - u13) + (u23 - u3)) / 6.0; // BAC, BCA, ABC, ACB, CAB, CBA
                    double valueAdded3 = (u3 + u3 + (u123 - u12) + (u13 - u1) + (u123 - u12) + (u23 - u2)) / 6.0; // CAB, CBA, ABC, ACB, BAC, BCA
                    Normalize(ref valueAdded1, ref valueAdded2, ref valueAdded3);
                    Send("manager", $"results {valueAdded1} {valueAdded2} {valueAdded3}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private double MaxGen(params double[] x)
        {
            double max = x[0];
            for (int i = 1; i < x.Length; i++)
                max = Math.Max(max, x[i]);
            return max;
        }

        private void Normalize(ref double a, ref double b)
        {
            double sum = a + b;
            a /= sum;
            b /= sum;
        }

        private void Normalize(ref double a, ref double b, ref double c)
        {
            double sum = a + b + c;
            a /= sum;
            b /= sum;
            c /= sum;
        }

        private bool AreEqual(double x, double y)
        {
            return (Math.Abs(x - y) < 1e-10);
        }
    }
}