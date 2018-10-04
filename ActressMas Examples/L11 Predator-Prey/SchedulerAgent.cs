using ActressMas;
using System;
using System.IO;

namespace PredatorPrey
{
    public class SchedulerAgent : Agent
    {
        private int _turn;
        private StreamWriter _sw;
        private World _worldEnv;

        public override void Setup()
        {
            _sw = new StreamWriter(Utils.WorldStateFileName);
            _sw.WriteLine("Doodlebugs\tAnts");

            _turn = 1;
            _worldEnv = (World)this.Environment;

            _worldEnv.StartNewTurn();

            string name = _worldEnv.GetNextInsect();

            if (Utils.Verbose)
                Console.WriteLine("\t\tRunning " + name);

            Send(name, "act");
        }

        public override void Act(Message message)
        {
            try
            {
                if (Utils.Verbose)
                    Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                string next = _worldEnv.GetNextInsect();

                if (Utils.Verbose)
                    Console.WriteLine("\t\tNext is: " + next);

                if (next == "") // all have moved
                {
                    int noDoodlebugs, noAnts;
                    _worldEnv.CountInsects(out noDoodlebugs, out noAnts);

                    Console.WriteLine("Turn {0}: {1} doodlebugs, {2} ants", _turn, noDoodlebugs, noAnts);

                    _sw.WriteLine("{0}\t{1}", noDoodlebugs, noAnts);
                    _sw.Flush();

                    if (Utils.ShowWorld)
                        ShowWorld();

                    if (_turn == Utils.NoTurns || noDoodlebugs == 0)
                    {
                        Console.WriteLine("\r\nSimulation finished");
                        _sw.Close();
                        this.Environment.StopAll();
                        return;
                    }

                    _turn++; // next turn

                    _worldEnv.StartNewTurn();

                    string name = _worldEnv.GetNextInsect();

                    if (Utils.Verbose)
                        Console.WriteLine("\t\tNext turn: Running " + name);

                    Send(name, "act");
                }
                else
                    Send(next, "act");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void ShowWorld()
        {
            Console.WriteLine(_worldEnv.PrintMap());
            Console.WriteLine("\r\nPress ENTER to continue ");
            Console.ReadLine();
        }
    }
}