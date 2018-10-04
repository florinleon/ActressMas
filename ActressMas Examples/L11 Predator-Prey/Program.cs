namespace PredatorPrey
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var worldEnv = new World(); // derived from ActressMas.Environment();

            int noAgents = Utils.GridSize * Utils.GridSize;

            worldEnv.InitWorldMap();

            int[] randVect = Utils.RandomPermutation(noAgents);

            for (int i = 0; i < Utils.NoDoodlebugs; i++)
            {
                var a = new DoodlebugAgent();
                worldEnv.Add(a, worldEnv.CreateName(a)); // unique name
                worldEnv.AddAgentToMap(a, randVect[i]);
                a.Start();
            }

            for (int i = Utils.NoDoodlebugs; i < Utils.NoDoodlebugs + Utils.NoAnts; i++)
            {
                var a = new AntAgent();
                worldEnv.Add(a, worldEnv.CreateName(a));
                worldEnv.AddAgentToMap(a, randVect[i]);
                a.Start();
            }

            //for (int i = 0; i < worldEnv.NoAgents; i++)
            //{
            //    worldEnv.Agents[i].Start();
            //}

            var s = new SchedulerAgent();
            worldEnv.Add(s, "scheduler");
            s.Start();

            worldEnv.WaitAll();
        }
    }
}