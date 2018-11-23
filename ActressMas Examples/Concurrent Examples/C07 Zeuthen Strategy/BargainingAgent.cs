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
    public class BargainingAgent : ConcurrentAgent
    {
        private delegate double UtilityFunctionDelegate(double d);
        private delegate double RiskFunctionDelegate(double d1, double d2);
        private delegate double NewProposalFunctionDelegate(double d1, double d2);
        private UtilityFunctionDelegate MyUtilityFunction, OthersUtilityFunction;
        private RiskFunctionDelegate MyRiskFunction, OthersRiskFunction;
        private NewProposalFunctionDelegate MyNewProposalFunction;

        private double _myProposal, _othersProposal;
        private string _othersName;

        public override void Setup()
        {
            if (this.Name == "agent1")
            {
                MyUtilityFunction = SharedKnowledge.Utility1;
                OthersUtilityFunction = SharedKnowledge.Utility2;
                MyRiskFunction = SharedKnowledge.Risk1;
                OthersRiskFunction = SharedKnowledge.Risk2;
                MyNewProposalFunction = SharedKnowledge.NewProposal1;
                _othersName = "agent2";

                _myProposal = 0.1;
                Send(_othersName, Utils.Str("propose", _myProposal));
            }
            else
            {
                MyUtilityFunction = SharedKnowledge.Utility2;
                OthersUtilityFunction = SharedKnowledge.Utility1;
                MyRiskFunction = SharedKnowledge.Risk2;
                OthersRiskFunction = SharedKnowledge.Risk1;
                MyNewProposalFunction = SharedKnowledge.NewProposal2;
                _othersName = "agent1";

                _myProposal = 5;
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

            double myRisk = MyRiskFunction(_myProposal, _othersProposal);
            double otherRisk = OthersRiskFunction(_othersProposal, _myProposal);

            Console.WriteLine("[{0}]: My risk is {1:F2} and the other's risk is {2:F2}", this.Name, myRisk, otherRisk);

            if ((myRisk < otherRisk) || (AreEqual(myRisk, otherRisk) && Utils.RandNoGen.NextDouble() < 0.5)) // on equality, concede randomly
            {
                _myProposal = MyNewProposalFunction(_myProposal, _othersProposal);
                Console.WriteLine("[{0}]: I will concede with new proposal {1:F1}", this.Name, _myProposal);
                Send(_othersName, Utils.Str("propose", _myProposal));
            }
            else
            {
                Console.WriteLine("[{0}]: I will not concede", this.Name);
                Send(_othersName, "continue");
            }
        }

        private void HandleContinue()
        {
            _myProposal = MyNewProposalFunction(_myProposal, _othersProposal);
            Send(_othersName, Utils.Str("propose", _myProposal));
        }

        private void HandleAccept()
        {
            Console.WriteLine("[{0}]: I accept {1:F1}", this.Name, _myProposal);
            Send(_othersName, "accept");
            Stop();
        }

        protected bool AreEqual(double x, double y)
        {
            return (Math.Abs(x - y) < 1e-10);
        }
    }
}