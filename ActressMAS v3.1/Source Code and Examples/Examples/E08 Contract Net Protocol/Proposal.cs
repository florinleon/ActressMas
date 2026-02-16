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

using System;
using TheTravelingSalesman;

namespace ContractNetProtocol
{
    public enum Performatives { CallForProposals, Propose, Refuse, AcceptProposal, RejectProposal, InformDone, InformResult, Failure };

    public class CNPMessage
    {
        public string Sender { get; set; }
        public Performatives Performative { get; set; }
        public Location Task { get; set; }
        public double InitiatorPayoff { get; set; }
        public double ParticipantPayoff { get; set; }
        public double ContractPrice { get; set; }

        public CNPMessage(string sender, Performatives performative, Location task, double initiatorPayoff = 0, double participantPayoff = 0, double contractPrice = 0)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Performative = performative;
            Task = task ?? throw new ArgumentNullException(nameof(task));
            InitiatorPayoff = initiatorPayoff;
            ParticipantPayoff = participantPayoff;
            ContractPrice = contractPrice;
        }
    }
}