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
using System.Threading;

namespace Zeuthen
{
    public class BargainingAgent : Agent
    {
        private delegate double UtilityFunction(double d);
        private delegate double RiskFunction(double d1, double d2);
        private delegate double NewProposalFunction(double d1, double d2);
        private UtilityFunction myUtilityFunction, otherUtilityFunction;
        private RiskFunction myRiskFunction, otherRiskFunction;
        private NewProposalFunction myNewProposalFunction;

        private double myProposal, otherProposal;
        private string otherName;

        public override void Setup()
        {
            if (this.Name == "agent1")
            {
                myUtilityFunction = SharedKnowledge.Utility1;
                otherUtilityFunction = SharedKnowledge.Utility2;
                myRiskFunction = SharedKnowledge.Risk1;
                otherRiskFunction = SharedKnowledge.Risk2;
                myNewProposalFunction = SharedKnowledge.NewProposal1;
                otherName = "agent2";

                myProposal = 0.1;
                Send(otherName, Utils.Str("propose", myProposal));
            }
            else
            {
                myUtilityFunction = SharedKnowledge.Utility2;
                otherUtilityFunction = SharedKnowledge.Utility1;
                myRiskFunction = SharedKnowledge.Risk2;
                otherRiskFunction = SharedKnowledge.Risk1;
                myNewProposalFunction = SharedKnowledge.NewProposal2;
                otherName = "agent1";

                myProposal = 5;
            }
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                string action; string parameters;
                Utils.ParseMessage(message.Content, out action, out parameters);

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

                Thread.Sleep(Utils.Delay);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void HandlePropose(double proposal)
        {
            otherProposal = proposal;

            if (myUtilityFunction(otherProposal) >= myUtilityFunction(myProposal))
            {
                Send(otherName, "accept");
                return;
            }

            if (myUtilityFunction(otherProposal) < 0)
            {
                Send(otherName, "continue");
                return;
            }

            double myRisk = myRiskFunction(myProposal, otherProposal);
            double otherRisk = otherRiskFunction(otherProposal, myProposal);

            Console.WriteLine("[{0}]: My risk is {1:F2} and the other's risk is {2:F2}", this.Name, myRisk, otherRisk);

            if ((myRisk < otherRisk) || (AreEqual(myRisk, otherRisk) && Utils.RandNoGen.NextDouble() < 0.5)) // on equality, concede randomly
            {
                myProposal = myNewProposalFunction(myProposal, otherProposal);
                Console.WriteLine("[{0}]: I will concede with new proposal {1:F1}", this.Name, myProposal);
                Send(otherName, Utils.Str("propose", myProposal));
            }
            else
            {
                Console.WriteLine("[{0}]: I will not concede", this.Name);
                Send(otherName, "continue");
            }
        }

        private void HandleContinue()
        {
            myProposal = myNewProposalFunction(myProposal, otherProposal);
            Send(otherName, Utils.Str("propose", myProposal));
        }

        private void HandleAccept()
        {
            Console.WriteLine("[{0}]: I accept {1:F1}", this.Name, myProposal);
            Send(otherName, "accept");
            Stop();
        }

        protected bool AreEqual(double x, double y)
        {
            return (Math.Abs(x - y) < 1e-10);
        }
    }
}