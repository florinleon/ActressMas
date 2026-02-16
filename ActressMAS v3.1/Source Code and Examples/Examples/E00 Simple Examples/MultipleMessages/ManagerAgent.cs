using ActressMas;
using System;
using System.Collections.Generic;

namespace MultipleMessages
{
    internal class ManagerAgent : Agent
    {
        public override void Setup()
        {
            Broadcast("start");
        }

        public override void Act(Message message)
        {
            Console.WriteLine(message.Format());
            message.Parse(out string action, out List<string> parameters);

            switch (action)
            {
                case "report":
                    Console.WriteLine($"\t[{Name}]: {message.Sender} reporting");
                    break;

                case "reply":
                    Console.WriteLine($"\t[{Name}]: {message.Sender} replying");
                    break;
            }
        }
    }
}