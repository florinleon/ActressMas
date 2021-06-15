/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Voting using the ActressMas framework                    *
 *  Copyright:   (c) 2021, Florin Leon                                    *
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
using System.Linq;
using System.Text;

namespace Voting
{
    public class VoterAgent : Agent
    {
        private string _preferenceOrder;
        private Random _rand;

        public VoterAgent(Random rand)
        {
            _rand = rand;
        }

        public override void Setup()
        {
            int noCandidates = Environment.Memory["NoCandidates"];

            var pref = RandomOrderPermutation(noCandidates);

            var sb = new StringBuilder();

            if (_rand.NextDouble() < 0.5)
            {
                // complete preference order
                for (int i = 0; i < noCandidates; i++)
                    sb.Append($"{pref[i]} ");
            }
            else
            {
                // incomplete preference order
                int noPreferences = _rand.Next(noCandidates - 1) + 1;
                for (int i = 0; i < noPreferences; i++)
                    sb.Append($"{pref[i]} ");
            }

            _preferenceOrder = sb.ToString().Trim();
        }

        private int[] RandomOrderPermutation(int n) =>
            Enumerable.Range(0, n).OrderBy(x => _rand.Next()).ToArray();

        public override void Act(Message message)
        {
            message.Parse(out string action, out string parameters);  // action == "start"
            HandleStart();
        }

        private void HandleStart()
        {
            Send("teller", $"vote {_preferenceOrder}");

            Stop();
        }
    }
}