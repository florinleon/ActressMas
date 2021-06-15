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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheTravelingSalesman;

namespace ContractNetProtocol
{
    public class TaskCollection
    {
        private List<Location> _assignment;  // letters currently assigned
        private double _costPerKm;
        private double _length;
        private int[] _path;
        private double _paymentPerLetter;

        public TaskCollection(List<Location> assignment)
        {
            _paymentPerLetter = 5;
            _costPerKm = 1;

            _assignment = assignment;
            Solve();
        }

        public Location this[string taskName] =>
            _assignment.Where(t => t.Name == taskName).FirstOrDefault();

        public double AdditionalPayoffByAccepting(CNPMessage p)
        {
            double payoff = ComputePayoff(_assignment, _length);
            double p0 = payoff;
            var ss = VirtualAddTask(p.Task);
            var path = TSP.Solve(ss);
            double len = path.GetPathLength();
            payoff = ComputePayoff(ss, len);
            double p1 = payoff;
            return p1 - p0;
        }

        public void AddTask(Location loc)
        {
            _assignment.Add(loc);
            Solve();
        }

        public double Payoff =>
            ComputePayoff(_assignment, _length);

        public string Solution
        {
            get
            {
                var sb = new StringBuilder();
                for (int i = 0; i < _assignment.Count; i++)
                {
                    Location loc = _assignment[_path[i]];
                    sb.Append($"{i + 1}.{loc.Name} ");
                }
                sb.Append("-> ");
                sb.Append($"{ _length} km");
                return sb.ToString();
            }
        }

        public void RemoveTask(string taskName)
        {
            _assignment = VirtualRemoveTask(taskName);
            Solve();
        }

        public bool WorstTask(out string taskName, out double difference)
        {
            double payoff = ComputePayoff(_assignment, _length);
            double p0 = payoff;
            double maxPayoff = double.MinValue;
            int selected = -1;

            for (int i = 0; i < _assignment.Count; i++)
            {
                var ss = VirtualRemoveTask(_assignment[i].Name);
                var path = TSP.Solve(ss);
                double len = path.GetPathLength();
                payoff = ComputePayoff(ss, len);

                if (payoff > maxPayoff)
                {
                    maxPayoff = payoff;
                    selected = i;
                }
            }

            if (selected != -1)
            {
                double p1 = maxPayoff;
                difference = p1 - p0;
                taskName = _assignment[selected].Name;

                if (difference > 0)
                    return true;
            }

            taskName = "";
            difference = 0;
            return false;
        }

        private double ComputePayoff(List<Location> list, double pathLength)
        {
            double income = list.Count * _paymentPerLetter;
            double cost = pathLength * _costPerKm;
            double payoff = income - cost;
            return payoff;
        }

        private void Solve()
        {
            var path = TSP.Solve(_assignment);
            _path = path.GetLocations();  // 0 to n-1
            _length = path.GetPathLength();
        }

        private List<Location> VirtualAddTask(Location loc)
        {
            var newList = new List<Location>(_assignment);
            newList.Add(loc);
            return newList;
        }

        private List<Location> VirtualRemoveTask(string taskName) =>
            _assignment.Where(t => t.Name != taskName).ToList();
    }
}