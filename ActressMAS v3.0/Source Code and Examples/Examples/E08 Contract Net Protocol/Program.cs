/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Contract Net Protocol using the ActressMas framework     *
 *  Copyright:   (c) 2021, Florin Leon                                    *
 *                                                                        *
 *  This code and information is provided "as is" without warranty of     *
 *  any kind, either expressed or implied, including but not limited      *
 *  to the implied warranties of merchantability or fitness for a         *
 *  particular purpose. You are free to use this source code in your      *
 *  applications as long as the original copyright notice is included.    *
 *                                                                        *
 **************************************************************************/

using ActressMas;
using System;
using System.Collections.Generic;
using System.Linq;
using TheTravelingSalesman;

namespace ContractNetProtocol
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var env = new MyEnv(noTurns: 100);

            int noPostmen = 3; 
            int noLetters = 20; 

            var locations = new List<Location>();
            var assignment = new int[noLetters];
            var rand = new Random();                

            for (int i = 0; i < noLetters; i++)
            {
                // locations between 47.1 - 47.2 latitude, 27.5 - 27.6 longitude
                locations.Add(new Location($"L{(i + 1):D2}", 47.1 + rand.NextDouble() / 10.0, 27.5 + rand.NextDouble() / 10.0));
                assignment[i] = rand.Next(noPostmen);
            }

            for (int j = 0; j < noPostmen; j++)
            {
                var initAssign = locations.Where((c, i) => assignment[i] == j).ToList();
                var postmanAgent = new PostmanAgent(initAssign);
                env.Add(postmanAgent, $"postman{j + 1}");
            }

            env.Start();

            // end of exchanges

            for (int j = 0; j < noPostmen; j++)
            {
                var m = new Message("env", $"postman{j + 1}", "finish");
                env.Send(m);
            }

            env.Continue(1);
        }
    }

    public class MyEnv : EnvironmentMas
    {
        public MyEnv(int noTurns = 0, int delayAfterTurn = 0, bool randomOrder = true, Random rand = null, bool parallel = true)
            : base(noTurns, delayAfterTurn, randomOrder, rand, parallel)
        {
            Memory["Turn"] = 0;
        }

        public override void TurnFinished(int turn)
        {
            Memory["Turn"] = turn + 1;  // turn is updated after TurnFinished
        }
    }
}