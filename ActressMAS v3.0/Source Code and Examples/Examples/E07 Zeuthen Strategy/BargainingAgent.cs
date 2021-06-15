/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Zeuthen strategy using the ActressMas framework          *
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

namespace Zeuthen
{
    public class BargainingAgent : Agent
    {
        private Func<double, double> MyUtilityFunction, OthersUtilityFunction;
        private double _myProposal, _othersProposal;
        private string _othersName;
        private Random _rand;

        public override void Setup()
        {
            if (Name == "agent1")
            {
                MyUtilityFunction = Environment.Memory["Utility1"];
                OthersUtilityFunction = Environment.Memory["Utility2"];

                _othersName = "agent2";
                _myProposal = 0.1;
                Send(_othersName, $"propose {_myProposal}");
            }
            else
            {
                MyUtilityFunction = Environment.Memory["Utility2"];
                OthersUtilityFunction = Environment.Memory["Utility1"];

                _othersName = "agent1";
                _myProposal = 4.9;
            }

            _rand = new Random();
        }

        private double NewProposal(double d1, double d2)
        {
            // the new proposal is computed iteratively, so that the procedure is general and can be applied for any pair of utility functions
            // for our example, with linear utility functions, the new proposal can be computed analytically

            if (Name == "agent1")
            {
                for (double d = d1; d <= d2; d += Environment.Memory["Eps"])
                    if (MyUtilityFunction(d) * OthersUtilityFunction(d) > MyUtilityFunction(d2) * OthersUtilityFunction(d2))
                        // this inequality is equivalent to: risk1(d) > risk2(d2)
                        return d;
                return d2;
            }
            else
            {
                for (double d = d1; d >= d2; d -= Environment.Memory["Eps"])
                    if (MyUtilityFunction(d) * OthersUtilityFunction(d) > MyUtilityFunction(d2) * OthersUtilityFunction(d2))
                        return d;
                return d1;
            }
        }

        private double Risk(Func<double, double> Utility, double d1, double d2)
        {
            return 1.0 - Utility(d2) / Utility(d1);
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out string parameters);

                switch (action)
                {
                    case "propose":
                        HandlePropose(Convert.ToDouble(parameters));
                        break;

                    case "continue":
                        HandleContinue();
                        break;

                    case "accept":
                        HandleAccept();
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

        private void HandlePropose(double proposal)
        {
            _othersProposal = proposal;

            if (MyUtilityFunction(_othersProposal) >= MyUtilityFunction(_myProposal))
            {
                Send(_othersName, "accept");
                return;
            }

            if (MyUtilityFunction(_othersProposal) < 0)
            {
                Send(_othersName, "continue");
                return;
            }

            double myRisk = Risk(MyUtilityFunction, _myProposal, _othersProposal);
            double othersRisk = Risk(OthersUtilityFunction, _othersProposal, _myProposal);

            Console.WriteLine($"[{Name}]: My risk is {myRisk:F4} and the other's risk is {othersRisk:F4}");

            if ((myRisk < othersRisk) || (AreEqual(myRisk, othersRisk) && _rand.NextDouble() < 0.5)) // on equality, concede randomly
            {
                _myProposal = NewProposal(_myProposal, _othersProposal);
                Console.WriteLine($"[{Name}]: I will concede with new proposal {_myProposal:F1}");
                Send(_othersName, $"propose {_myProposal}");
            }
            else
            {
                Console.WriteLine($"[{Name}]: I will not concede");

                double eps = Environment.Memory["Eps"];
                Environment.Memory["Eps"] = eps / 10.0;  // to avoid a blockage when finishing with equal risks

                Send(_othersName, "continue");
            }
        }

        private void HandleContinue()
        {
            _myProposal = NewProposal(_myProposal, _othersProposal);
            Send(_othersName, $"propose {_myProposal}");
        }

        private void HandleAccept()
        {
            Console.WriteLine($"[{Name}]: I accept {_myProposal:F1}");
            Send(_othersName, "accept");
            Stop();
        }

        private bool AreEqual(double x, double y)
        {
            return (Math.Abs(x - y) < 1e-10);
        }
    }
}