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
    public class DecisionMakerAgent : TurnBasedAgent
    {
        private int[,] _netBenefits;
        private int[] _taxes;
        private int _answersReceived;

        public override void Setup()
        {
            _netBenefits = new int[Utils.NoBeneficiaries, Utils.NoOptions];
            _taxes = new int[Utils.NoBeneficiaries];
            _answersReceived = 0;
            Broadcast("report");
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
                        case "benefits":
                            HandleBenefits(message.Sender, parameters);
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

        private void HandleBenefits(string sender, List<string> benefits)
        {
            int agentIndex = (int)(sender[0] - 'a');

            for (int i = 0; i < Utils.NoOptions; i++)
                _netBenefits[agentIndex, i] = Convert.ToInt32(benefits[i]);

            _answersReceived++;

            if (_answersReceived == Utils.NoBeneficiaries)
                ComputeTaxes();
        }

        private void ComputeTaxes()
        {
            int bestOption = -1;
            int largestNetSocialBenefit = -1;

            for (int i = 0; i < Utils.NoOptions; i++)
            {
                int sum = 0;
                for (int j = 0; j < Utils.NoBeneficiaries; j++)
                    sum += _netBenefits[j, i];

                if (sum > largestNetSocialBenefit)
                {
                    largestNetSocialBenefit = sum;
                    bestOption = i;
                }
            }

            Console.WriteLine("[{0}]: We will install {1} lights", this.Name, bestOption);

            for (int k = 0; k < Utils.NoBeneficiaries; k++) // without k
            {
                int bestOptionWithoutK = -1;
                int largestNSBWithoutK = -1;
                int previousLargestNSBWithoutK = 0;

                for (int i = 0; i < Utils.NoOptions; i++)
                {
                    int sum = 0;
                    for (int j = 0; j < Utils.NoBeneficiaries; j++)
                        if (j != k)
                            sum += _netBenefits[j, i];

                    if (i == bestOption)
                        previousLargestNSBWithoutK = sum;

                    if (sum > largestNSBWithoutK)
                    {
                        largestNSBWithoutK = sum;
                        bestOptionWithoutK = i;
                    }
                }

                if (bestOption != bestOptionWithoutK)
                {
                    _taxes[k] = largestNSBWithoutK - previousLargestNSBWithoutK;
                    Console.WriteLine("[{0}]: {1} is pivotal - from {2} - and its tax is {3}", this.Name, (char)('a' + k), bestOptionWithoutK, _taxes[k]);
                }
                else
                    Console.WriteLine("[{0}]: {1} is not pivotal and its tax is 0", this.Name, (char)('a' + k));
            }

            for (int i = 0; i < Utils.NoBeneficiaries; i++)
            {
                string name = ((char)('a' + i)).ToString();
                Send(name, Utils.Str("result", bestOption, _taxes[i]));
            }
        }
    }
}