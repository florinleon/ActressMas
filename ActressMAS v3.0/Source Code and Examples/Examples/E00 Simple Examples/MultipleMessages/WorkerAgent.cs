using ActressMas;
using System;
using System.Collections.Generic;

namespace MultipleMessages
{
    public class WorkerAgent : Agent
    {
        private static Random rand = new Random();

        private string NextWorker()
        {
            int noWorkers = 5;
            string name = Name;
            while (name == Name)
                name = $"Worker{rand.Next(noWorkers)}";
            return name;
        }

        public override void Act(Message m)
        {
            Console.WriteLine(m.Format());
            m.Parse(out string action, out List<string> parameters);

            switch (action)
            {
                case "start":
                    Send("Manager", "report");
                    Send(NextWorker(), "request");
                    break;

                case "request":
                    Send(m.Sender, "reply");
                    break;

                case "request_reply":
                    Send("Manager", "reply");
                    Send(NextWorker(), "request");
                    break;
            };
        }
    }
}