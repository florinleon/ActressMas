using ActressMas;
using System;
using System.Collections.Generic;

namespace MasMobileGui
{
    [Serializable]
    public class MobileAgentState : AgentState
    {
        public bool FirstStart;
        public string Log;
        public Queue<string> Moves;
    }

    public class MobileAgent : Agent
    {
        private string _log;
        private Queue<string> _moves;
        private bool _firstStart = true;
        private int _turnsToWaitForInfo;

        public override void Setup()
        {
            try
            {
                if (_firstStart)
                {
                    Global.Rtb.AppendText("Mobile agent starting at home\r\n");

                    _firstStart = false;

                    _log = "\r\nReporting:\r\n";

                    _moves = new Queue<string>();

                    foreach (string cn in Environment.AllContainers())
                    {
                        if (cn != Environment.ContainerName) // home
                            _moves.Enqueue(cn);
                    }

                    _moves.Enqueue(Environment.ContainerName); // return home, get local info and report

                    foreach (string m in _moves)
                        Global.Rtb.AppendText($"{m}\r\n");
                }
                else
                {
                    Console.WriteLine($"I have moved to {Environment.ContainerName}");
                    _log += $"Arrived to {Environment.ContainerName}\r\n";
                    Broadcast("request-info");
                    _turnsToWaitForInfo = 3;
                }
            }
            catch (Exception ex)
            {
                Global.Rtb.AppendText($"{ex.Message}\r\n");
            }
        }

        public override void Act(Message message)
        {
            try
            {
                Global.Rtb.AppendText($"\t{message.Format()}\r\n");
                _log += $"Received info: {message.Content}\r\n";  // info from static agents
            }
            catch (Exception ex)
            {
                Global.Rtb.AppendText($"{ex.Message}\r\n");
            }
        }

        public override void ActDefault()
        {
            try
            {
                if (_turnsToWaitForInfo-- > 0)
                    return;

                if (_moves.Count > 0)
                {
                    string nextDestination = _moves.Dequeue();

                    if (CanMove(nextDestination))
                    {
                        Global.Rtb.AppendText($"I'm moving to {nextDestination}\r\n");
                        _log += $"Moving to {nextDestination}\r\n";
                        Move(nextDestination);
                        return;
                    }
                }

                _log += "Stopping\r\n";
                Global.Rtb.AppendText(_log);
                Stop();
            }
            catch (Exception ex)
            {
                Global.Rtb.AppendText($"{ex.Message}\r\n");
            }
        }

        public override AgentState SaveState()
        {
            var state = new MobileAgentState
            {
                FirstStart = _firstStart,
                Log = _log,
                Moves = _moves
            };
            return state;
        }

        public override void LoadState(AgentState state)
        {
            var st = (MobileAgentState)state;
            _firstStart = st.FirstStart;
            _log = st.Log;
            _moves = st.Moves;
        }
    }
}