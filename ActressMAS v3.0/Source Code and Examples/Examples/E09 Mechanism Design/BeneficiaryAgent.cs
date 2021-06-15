/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Mechanism design using the ActressMas framework          *
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

namespace MechanismDesign
{
    public class BeneficiaryAgent : Agent
    {
        private int[] _netBenefits;
        private string _netBenefitsStr;

        public override void Setup()
        {
            int[] totalBenefits = null;

            switch (Name)
            {
                case "a":
                    totalBenefits = new int[] { 0, 60, 90, 155 };
                    break;

                case "b":
                    totalBenefits = new int[] { 0, 80, 120, 140 };
                    break;

                case "c":
                    totalBenefits = new int[] { 0, 120, 200, 220 };
                    break;

                default:
                    break;
            }

            _netBenefits = new int[Settings.NoOptions];
            _netBenefitsStr = "";

            for (int i = 0; i < Settings.NoOptions; i++)
            {
                _netBenefits[i] = totalBenefits[i] - i * Settings.UnitCost / Settings.NoBeneficiaries;
                _netBenefitsStr += $"{_netBenefits[i]} ";
            }

            _netBenefitsStr = _netBenefitsStr.TrimEnd();

            Console.WriteLine($"[{Name}]: My net benefits are ({_netBenefitsStr})");
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out List<string> parameters);

                switch (action)
                {
                    case "report":
                        HandleReport();
                        break;

                    case "result":
                        HandleResult(parameters);
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

        private void HandleReport()
        {
            if (Settings.AisLying && Name == "a")
            {
                Console.WriteLine("[a]: My *reported* net benefits are (0 20 10 70)");
                Send("dm", "benefits 0 20 10 70");
            }
            else
                Send("dm", $"benefits {_netBenefitsStr}");
        }

        private void HandleResult(List<string> parameters)
        {
            int noLights = Convert.ToInt32(parameters[0]);
            int tax = Convert.ToInt32(parameters[1]);

            Console.WriteLine($"[{Name}]: My net benefit after tax is {_netBenefits[noLights] - tax}");

            Stop();
        }
    }
}