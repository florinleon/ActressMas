using System;
using System.Linq;

namespace PredatorPrey
{
    public class Utils
    {
        public static int GridSize = 50; //20;
        public static int NoTurns = 1000;
        public static int NoAnts = 1000; //100
        public static int NoDoodlebugs = 20; //2
        public static bool ShowWorld = false;
        public static bool Verbose = false;
        public static string WorldStateFileName = "world-state.txt";
        public static Random RandNoGen = new Random();

        public static int[] RandomPermutation(int n)
        {
            int[] numbers = new int[n];
            for (int i = 0; i < n; i++)
                numbers[i] = i;
            int[] randPerm = numbers.OrderBy(x => RandNoGen.Next()).ToArray();
            return randPerm;
        }
    }
}