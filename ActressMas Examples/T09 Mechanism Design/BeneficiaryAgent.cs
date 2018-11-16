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
    public class BeneficiaryAgent : TurnBasedAgent
    {
        private int[] _netBenefits;
        private string _netBenefitsStr;

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

            _netBenefits = new int[Utils.NoOptions];
            _netBenefitsStr = "";

            for (int i = 0; i < Utils.NoOptions; i++)
            {
                _netBenefits[i] = totalBenefits[i] - i * Utils.UnitCost / Utils.NoBeneficiaries;
                _netBenefitsStr += string.Format("{0} ", _netBenefits[i]);
            }

            _netBenefitsStr = _netBenefitsStr.TrimEnd();

            Console.WriteLine("[{0}]: My net benefits are ({1})", this.Name, _netBenefitsStr);
        }

        public override void Act(Queue<Message> messages)
        {
            try
            {
                while (messages.Count > 0)
                {
                    Message message = messages.Dequeue();
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void HandleReport()
        {
            if (Utils.AisLying && this.Name == "a")
            {
                Console.WriteLine("[a]: My *reported* net benefits are (0 20 10 70)");
                Send("dm", "benefits 0 20 10 70");
            }
            else
                Send("dm", Utils.Str("benefits", _netBenefitsStr));
        }

        private void HandleResult(List<string> parameters)
        {
            int noLights = Convert.ToInt32(parameters[0]);
            int tax = Convert.ToInt32(parameters[1]);

            Console.WriteLine("[{0}]: My net benefit after tax is {1}", this.Name, _netBenefits[noLights] - tax);

            Stop();
        }
    }
}