using ActressMas;
using System;

namespace PredatorPrey
{
    public class AntAgent : InsectAgent
    {
        public override void Setup()
        {
            _turnsSurvived = 0;

            _worldEnv = (World)this.Environment;

            if (Utils.Verbose)
                Console.WriteLine("AntAgent {0} started in ({1},{2})", this.Name, Line, Column);
        }

        public override void Act(Message message)
        {
            try
            {
                if (Utils.Verbose)
                    Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                AntAction();
                Send("scheduler", "done");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void AntAction()
        {
            /*
              • Move: For every time step, the ants randomly try to move up, down, left, or right. If the neighboring
                cell in the selected direction is occupied or would move the ant off the grid, then the ant stays in the
                current cell.
                • Breed: If an ant survives for three time steps, at the end of the time step (i.e., after moving) the ant will
                breed. This is simulated by creating a new ant in an adjacent (up, down, left, or right) cell that is
                empty. If there is no empty cell available, no breeding occurs. Once an offspring is produced, an ant
                cannot produce an offspring again until it has survived three more time steps.
             */

            _turnsSurvived++;

            // move
            TryToMove(); // implemented in base class InsectAgent

            // breed
            if (_turnsSurvived >= 3)
            {
                if (TryToBreed()) // implemented in base class InsectAgent
                    _turnsSurvived = 0;
            }
        }
    }
}