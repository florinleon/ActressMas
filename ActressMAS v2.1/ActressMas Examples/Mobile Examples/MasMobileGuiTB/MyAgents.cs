/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Developing mobile agents using the ActressMas framework  *
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

namespace MasMobileGuiTB
{
    public class MobileAgent : TurnBasedAgent
    {
        private string _log;
        private Queue<string> _moves;
        private bool _firstStart = true;

        public override void Setup()
        {
            if (_firstStart)
            {
                Commons.Rtb.AppendText("Mobile agent starting at home\r\n");

                _firstStart = false;

                _log = "\r\nReporting:\r\n";

                _moves = new Queue<string>();

                foreach (string cn in this.Environment.AllContainers())
                {
                    if (cn != this.Environment.ContainerName) // home
                        _moves.Enqueue(cn);
                }

                _moves.Enqueue(this.Environment.ContainerName); // return home, get local info and report

                foreach (string m in _moves)
                    Commons.Rtb.AppendText(m + "\r\n");
            }
            else
            {
                Commons.Rtb.AppendText("I have moved to " + this.Environment.ContainerName + "\r\n");
                _log += "Arrived to " + this.Environment.ContainerName + "\r\n";
                Broadcast("request-info");
            }
        }

        public override void Act(Queue<Message> messages)
        {
            try
            {
                if (messages.Count == 0)
                {
                    if (_moves.Count > 0)
                    {
                        string nextDestination = _moves.Dequeue();

                        if (CanMove(nextDestination))
                        {
                            Commons.Rtb.AppendText("I'm moving to " + nextDestination + "\r\n");
                            _log += "Moving to " + nextDestination + "\r\n";
                            Move(nextDestination);
                            return;
                        }
                    }

                    _log += "Stopping\r\n";
                    Commons.Rtb.AppendText(_log + "\r\n");
                    Stop();
                }
                else
                {
                    while(messages.Count > 0)
                    {
                        Message message = messages.Dequeue();
                        Commons.Rtb.AppendText(string.Format("\t[{1} -> {0}]: {2}\r\n", this.Name, message.Sender, message.Content));
                        _log += "Received info: " + message.Content + "\r\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Commons.Rtb.AppendText("Exception: " + ex.Message + "\r\n");
            }
        }

        public override AgentState SaveState()
        {
            MobileAgentState state = new MobileAgentState();
            state.FirstStart = _firstStart;
            state.Log = _log;
            state.Moves = _moves;
            return state;
        }

        public override void LoadState(AgentState state)
        {
            MobileAgentState st = (MobileAgentState)state;
            _firstStart = st.FirstStart;
            _log = st.Log;
            _moves = st.Moves;
        }
    }

    [Serializable]
    public class MobileAgentState : AgentState
    {
        public bool FirstStart;
        public string Log;
        public Queue<string> Moves;
    }

    public class StaticAgent : TurnBasedAgent
    {
        private string _info;

        public override void Setup()
        {
            Commons.Rtb.AppendText(string.Format("Static agent {0} starting\r\n", Name));
            _info = string.Format("Info from agent {0} in container {1}", this.Name, this.Environment.ContainerName);
        }

        public override void Act(Queue<Message> messages)
        {
            while (messages.Count > 0)
            {
                Message message = messages.Dequeue();
                Commons.Rtb.AppendText(string.Format("\t[{1} -> {0}]: {2}\r\n", this.Name, message.Sender, message.Content));

                if (message.Content == "request-info")
                    Send(message.Sender, _info);
            }
        }
    }
}