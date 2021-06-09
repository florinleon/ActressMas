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
using System.Timers;

namespace MasMobileGuiTB
{
    public class MobileAgent : ConcurrentAgent
    {
        private bool _firstStart = true;
        private string _log;
        private Queue<string> _moves;
        private Timer _timer;

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

            InitTimer(); // Timer is not serializable, must be created and started at each hop
        }

        private void InitTimer()
        {
            _timer = new Timer();
            _timer.Interval = 2000;
            _timer.Elapsed += timer_Elapsed;
            _timer.Start();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Send(this.Name, "wake-up");
        }

        public override void Act(Message message)
        {
            try
            {
                Commons.Rtb.AppendText(string.Format("\t[{1} -> {0}]: {2}\r\n", this.Name, message.Sender, message.Content));

                if (message.Content == "wake-up") // from timer
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
                else // info from static agents
                {
                    _log += "Received info: " + message.Content + "\r\n";
                }
            }
            catch (Exception ex)
            {
                Commons.Rtb.AppendText("Exception: " + ex.Message + "\r\n");
            }
        }

        public override void LoadState(AgentState state)
        {
            MobileAgentState st = (MobileAgentState)state;
            _firstStart = st.FirstStart;
            _log = st.Log;
            _moves = st.Moves;
        }

        public override AgentState SaveState()
        {
            MobileAgentState state = new MobileAgentState();
            state.FirstStart = _firstStart;
            state.Log = _log;
            state.Moves = _moves;
            return state;
        }
    }

    [Serializable]
    public class MobileAgentState : AgentState
    {
        public bool FirstStart;
        public string Log;
        public Queue<string> Moves;
    }

    public class StaticAgent : ConcurrentAgent
    {
        private string _info;

        public override void Setup()
        {
            Commons.Rtb.AppendText(string.Format("Static agent {0} starting\r\n", Name));
            _info = string.Format("Info from agent {0} in container {1}", this.Name, this.Environment.ContainerName);
        }

        public override void Act(Message message)
        {
            Commons.Rtb.AppendText(string.Format("\t[{1} -> {0}]: {2}\r\n", this.Name, message.Sender, message.Content));

            if (message.Content == "request-info")
                Send(message.Sender, _info);
        }
    }
}