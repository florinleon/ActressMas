/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Contract Net Protocol using the ActressMas framework     *
 *  Copyright:   (c) 2021, Florin Leon                                    *
 *                                                                        *
 *  This code and information is provided "as is" without warranty of     *
 *  any kind, either expressed or implied, including but not limited      *
 *  to the implied warranties of merchantability or fitness for a         *
 *  particular purpose. You are free to use this source code in your      *
 *  applications as long as the original copyright notice is included.    *
 *                                                                        *
 **************************************************************************/

using ActressMas;
using System;
using System.Collections.Generic;
using TheTravelingSalesman;

namespace ContractNetProtocol
{
    public class PostmanAgent : Agent
    {
        private TaskCollection _tasks;
        private List<CNPMessage> _cfps;
        private List<CNPMessage> _proposals;
        private double _money;  // payments from subcontracting

        public PostmanAgent(List<Location> initAssign)
        {
            _tasks = new TaskCollection(initAssign);
            _cfps = new List<CNPMessage>();
            _proposals = new List<CNPMessage>();
            _money = 0;
        }

        public override void Setup()
        {
            DisplayStatus();
            SendCallsForProposals();
        }

        private void DisplayStatus()
        {
            Console.WriteLine($"\n{Name}\nTour: {_tasks.Solution}\nPayoff: {_tasks.Payoff + _money:F2} ({_tasks.Payoff:f2} + {_money:F2})");
        }

        public override void Act(Message message)
        {
            try
            {
                if (message.Content == "finish")  // protocol finished
                {
                    DisplayStatus();
                    Stop();
                    return;
                }

                var msg = message.ContentObj as CNPMessage;
                Console.WriteLine($"\n{message.Sender} -> {message.Receiver}: {msg.Performative} {msg.Task.Name} {msg.InitiatorPayoff:F2} {msg.ParticipantPayoff:F2} {msg.ContractPrice:F2}");

                switch (msg.Performative)
                {
                    case Performatives.CallForProposals:
                        // the agent has the role of participant

                        double payoff = _tasks.AdditionalPayoffByAccepting(msg);
                        if (payoff > 0)
                        {
                            msg.ParticipantPayoff = payoff;

                            // the final payoffs of the initiator and the participant should be equal
                            double averagePrice = (msg.InitiatorPayoff + msg.ParticipantPayoff) / 2.0;

                            // intiator pays ContractPrice to the participant
                            msg.ContractPrice = averagePrice - payoff;

                            _cfps.Add(msg);
                            Console.WriteLine($"{Name} receives cfp {msg.Task.Name} with {msg.InitiatorPayoff:F2}/{msg.ParticipantPayoff:F2}/{msg.ContractPrice:F2}");
                        }
                        break;

                    case Performatives.Propose:
                        // the agent has the role of intiator

                        _proposals.Add(msg);
                        Console.WriteLine($"{Name} receives proposal {msg.Task.Name} with {msg.InitiatorPayoff:F2}/{msg.ParticipantPayoff:F2}/{msg.ContractPrice:F2}");
                        break;

                    case Performatives.Refuse:
                        // not handled explicitly in this version of the protocol
                        // the refused cfps are ignored
                        break;

                    case Performatives.AcceptProposal:
                        // the agent has the role of participant

                        // the participant receives the task and the payment
                        Console.WriteLine($"{Name} is given {msg.Task.Name} with {msg.InitiatorPayoff:F2}/{msg.ParticipantPayoff:F2}/{msg.ContractPrice:F2}");
                        _tasks.AddTask(msg.Task);
                        _money += msg.ContractPrice;
                        break;

                    case Performatives.RejectProposal:
                        // not handled explicitly in this version of the protocol
                        // the refused proposals are ignored
                        break;

                    case Performatives.InformDone:
                        // for this application, there is no confirmation of completing the task
                        // in the end, all postmen will deliver their assigned letters
                        break;

                    case Performatives.InformResult:
                        // for this application, there is no result reported
                        break;

                    case Performatives.Failure:
                        // for this application, there is no notification of failure to complete the task
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

        public override void ActDefault()
        {
            try
            {
                // we rely on turn numbers to handle deadlines

                if (Environment.Memory["Turn"] % 7 == 2)
                    EvaluateCallsForProposals();
                else if (Environment.Memory["Turn"] % 7 == 4)
                    EvaluateProposals();
                else if (Environment.Memory["Turn"] % 7 == 6)
                    SendCallsForProposals();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SendCallsForProposals()
        {
            // the agent has the role of initiator

            bool canInitiate = _tasks.WorstTask(out string taskName, out double dif);

            if (canInitiate)  // difference in payoff > 0
            {
                var msg = new CNPMessage(Name, Performatives.CallForProposals, _tasks[taskName], initiatorPayoff: dif);
                Broadcast(msg);
            }
        }

        private void EvaluateCallsForProposals()
        {
            // the agent has the role of participant
            // it evaluates the best call for proposals from initiators

            if (_cfps.Count == 0)
                return;

            double bestPrice = double.MinValue;
            CNPMessage bestProposal = null;

            for (int i = 0; i < _cfps.Count; i++)
            {
                var p = _cfps[i];
                if (p.ParticipantPayoff + p.ContractPrice > bestPrice)
                {
                    bestPrice = p.ParticipantPayoff + p.ContractPrice;
                    bestProposal = p;
                }
            }

            if (bestProposal != null)
            {
                string receiver = bestProposal.Sender;
                bestProposal.Sender = Name;
                bestProposal.Performative = Performatives.Propose;
                Send(receiver, bestProposal);
            }

            // here the agent can send a Performatives.Refuse message to all the other agents
            // but in this version of the protocol these messages will be ignored anyway

            _cfps.Clear();
        }

        private void EvaluateProposals()
        {
            // the agent has the role of initiator
            // it evaluates the best proposals from participants

            if (_proposals.Count == 0)
                return;

            double bestPrice = double.MinValue;
            CNPMessage bestProposal = null;

            for (int i = 0; i < _proposals.Count; i++)
            {
                var p = _proposals[i];
                if (p.InitiatorPayoff - p.ContractPrice > bestPrice)
                {
                    bestPrice = p.InitiatorPayoff - p.ContractPrice;
                    bestProposal = p;
                }
            }

            if (bestProposal != null)
            {
                // the initiator awards the contract for the task and pays the participant
                string receiver = bestProposal.Sender;
                bestProposal.Sender = Name;
                bestProposal.Performative = Performatives.AcceptProposal;
                Send(receiver, bestProposal);
                _tasks.RemoveTask(bestProposal.Task.Name);
                _money -= bestProposal.ContractPrice;
            }

            // here the agent can send a Performatives.RejectProposal message to all the other agents
            // but in this version of the protocol these messages will be ignored anyway

            _proposals.Clear();
        }
    }
}