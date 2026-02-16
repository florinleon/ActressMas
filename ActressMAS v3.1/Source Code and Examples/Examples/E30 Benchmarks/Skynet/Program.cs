using ActressMas;
using System;
using System.Diagnostics;

namespace Skynet
{
    public static class Global
    {
        public static int MaxAgents = 10000;
        public static int NoChildren = 10;
    }

    public class Program
    {
        private static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();

            var env = new EnvironmentMas();
            //var env = new EnvironmentMas(parallel: false);

            var a = new MyAgent(0, -1);
            env.Add(a, "a0");  // the root agent

            env.Start();

            sw.Stop();
            Console.WriteLine($"{sw.ElapsedMilliseconds:F3} ms");
        }

        public class MyAgent : Agent
        {
            private long _id, _parentId, _sum;
            private int _messagesLeft, _noChildren, _maxAgents;

            public MyAgent(long id, long parentId)
            {
                _id = id;
                _parentId = parentId;
                _sum = _id;
                _noChildren = Global.NoChildren;
                _maxAgents = Global.MaxAgents;
                _messagesLeft = 0;
            }

            public override void Setup()
            {
                for (int i = 1; i <= _noChildren; i++)
                {
                    long newId = _id * 10 + i;

                    if (newId < _maxAgents)
                    {
                        var a = new MyAgent(newId, _id);
                        //Console.WriteLine($"Creating a{newId}");
                        Environment.Add(a, $"a{newId}");
                        _messagesLeft++;
                    }
                }

                if (_messagesLeft == 0)
                {
                    Send($"a{_parentId}", $"{_id}");  // send id to parent
                    Stop();
                }
            }

            public override void Act(Message m)
            {
                _sum += Convert.ToInt64(m.Content);
                _messagesLeft--;

                if (_messagesLeft == 0)
                {
                    if (Name == "a0")
                        Console.WriteLine($"Sum = {_sum}");  // root reporting
                    else
                        Send($"a{_parentId}", $"{_sum}");  // send sum to parent
                    Stop();
                }
            }
        }
    }
}