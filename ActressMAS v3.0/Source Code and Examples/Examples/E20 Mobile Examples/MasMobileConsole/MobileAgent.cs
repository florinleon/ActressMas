using ActressMas;
using System;
using System.Collections.Generic;

namespace MasMobileConsole
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
            if (_firstStart)
            {
                Console.WriteLine("Mobile agent starting at home");

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
                    Console.WriteLine(m);
            }
            else
            {
                Console.WriteLine($"I have moved to {Environment.ContainerName}");
                _log += $"Arrived to {Environment.ContainerName}\r\n";
                Broadcast("request-info");
                _turnsToWaitForInfo = 3;
            }
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                _log += $"Received info: {message.Content}\r\n";  // info from static agents
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override void ActDefault()
        {
            if (_turnsToWaitForInfo-- > 0)
                return;

            if (_moves.Count > 0)
            {
                string nextDestination = _moves.Dequeue();

                if (CanMove(nextDestination))
                {
                    Console.WriteLine($"I'm moving to {nextDestination}");
                    _log += $"Moving to {nextDestination}\r\n";
                    Move(nextDestination);
                    return;
                }
            }

            _log += "Stopping\r\n";
            Console.WriteLine(_log);
            Stop();
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