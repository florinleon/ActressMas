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

namespace MasMobileConsole
{
    public class MobileAgent : ConcurrentAgent
    {
        private string _log;
        private Queue<string> _moves;
        private Timer _timer;
        private bool _firstStart = true;

        public override void Setup()
        {
            if (_firstStart)
            {
                Console.WriteLine("Mobile agent starting at home");

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
                    Console.WriteLine(m);
            }
            else
            {
                Console.WriteLine("I have moved to " + this.Environment.ContainerName);
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
                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                if (message.Content == "wake-up") // from timer
                {
                    if (_moves.Count > 0)
                    {
                        string nextDestination = _moves.Dequeue();

                        if (CanMove(nextDestination))
                        {
                            Console.WriteLine("I'm moving to " + nextDestination);
                            _log += "Moving to " + nextDestination + "\r\n";
                            Move(nextDestination);
                            return;
                        }
                    }

                    _log += "Stopping\r\n";
                    Console.WriteLine(_log);
                    Stop();
                }
                else // info from static agents
                {
                    _log += "Received info: " + message.Content + "\r\n";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

    public class StaticAgent : ConcurrentAgent
    {
        private string _info;

        public override void Setup()
        {
            Console.WriteLine("Static agent {0} starting", Name);
            _info = string.Format("Info from agent {0} in container {1}", this.Name, this.Environment.ContainerName);
        }

        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

            if (message.Content == "request-info")
                Send(message.Sender, _info);
        }
    }
}