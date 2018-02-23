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
        private int[] netBenefits;
        private string netBenefitsStr;

        public override void Setup()
        {
            int[] totalBenefits = null;

            switch (this.Name)
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

            netBenefits = new int[Utils.NoOptions];
            netBenefitsStr = "";

            for (int i = 0; i < Utils.NoOptions; i++)
            {
                netBenefits[i] = totalBenefits[i] - i * Utils.UnitCost / Utils.NoBeneficiaries;
                netBenefitsStr += string.Format("{0} ", netBenefits[i]);
            }

            netBenefitsStr = netBenefitsStr.TrimEnd();

            Console.WriteLine("[{0}]: My net benefits are ({1})", this.Name, netBenefitsStr);
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                string action; List<string> parameters;
                Utils.ParseMessage(message.Content, out action, out parameters);

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
            if (Utils.AIsLying && this.Name == "a")
            {
                Console.WriteLine("[a]: My *reported* net benefits are (0 20 10 70)");
                Send("dm", "benefits 0 20 10 70");
            }
            else
                Send("dm", Utils.Str("benefits", netBenefitsStr));
        }

        private void HandleResult(List<string> parameters)
        {
            int noLights = Convert.ToInt32(parameters[0]);
            int tax = Convert.ToInt32(parameters[1]);

            Console.WriteLine("[{0}]: My net benefit after tax is {1}", this.Name, netBenefits[noLights] - tax);

            Stop();
        }
    }
}