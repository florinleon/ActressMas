using ActressMas;

namespace MultipleMessages
{
    public class Program
    {
        private static void Main(string[] args)
        {
            int noWorkers = 5;
            
            var env = new EnvironmentMas(noTurns:20, parallel:true);

            ManagerAgent manager = new ManagerAgent();
            env.Add(manager, "Manager");

            for (int i = 0; i < noWorkers; i++)
            {
                WorkerAgent worker = new WorkerAgent();
                env.Add(worker, $"Worker{i}");
            }

            env.Start();
        }
    }
}