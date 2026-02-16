using Sharpen;
using Sharpen.Function;
using System;
using System.Collections.Generic;
using TheTravelingSalesman.Impl;

namespace TheTravelingSalesman
{
    public class ProblemBuilder<T>
    {
        private IList<T> locations__;

        private BiFunction<T, T, double?> distanceCalc;

        private int intMultiplicator = 1;

        private int fixedFirstLocationIndex = -1;

        private T fixedFirstLocation;

        private Type clazz;

        public ProblemBuilder(Type clazz)
        {
            this.clazz = typeof(T);
        }

        public static ProblemBuilder<V> Create<V>()
        {
            return new ProblemBuilder<V>(typeof(V));
        }

        public virtual ProblemBuilder<T> Locations(IList<T> locations)
        {
            this.locations__ = locations;
            return this;
        }

        public virtual ProblemBuilder<T> Distances(BiFunction<T, T, double?> distanceCalc)
        {
            this.distanceCalc = distanceCalc;
            return this;
        }

        public virtual ProblemBuilder<T> FixedFirstLocation(int index)
        {
            this.fixedFirstLocationIndex = index;
            return this;
        }

        public virtual ProblemBuilder<T> FixedFirstLocation(T start)
        {
            this.fixedFirstLocation = start;
            return this;
        }

        public virtual IProblem<T> BuildDoubleArray()
        {
            return BuildDoubleProblem((IList<T> locations, double[][] dist) => new DoubleArrayProblem<T>(locations, dist));
        }

        public virtual IProblem<T> BuildDouble2DArray()
        {
            return BuildDoubleProblem((IList<T> locations, double[][] dist) => new Double2DArrayProblem<T>(locations, dist));
        }

        public virtual IProblem<T> BuildDoubleProblem(BiFunction<IList<T>, double[][], IProblem<T>> creator)
        {
            double[][] dist = new double[locations__.Count][];
            for (int i = 0; i < locations__.Count; i++)
            {
                dist[i] = new double[locations__.Count];
                for (int j = 0; j < locations__.Count; j++)
                {
                    dist[i][j] = distanceCalc.Invoke(locations__[i], locations__[j]).Value;
                }
            }
            IProblem<T> result = creator.Invoke(locations__, dist);
            result.SetFixedFirstLocation(fixedFirstLocationIndex);
            return result;
        }

        public virtual IProblem<T> BuildIntegerArray()
        {
            return BuildIntegerArray((IList<T> locations, int[][] dist) => new IntegerArrayProblem<T>(locations, dist));
        }

        public virtual IProblem<T> BuildInteger2DArray()
        {
            return BuildIntegerArray((IList<T> locations, int[][] dist) => new Integer2DArrayProblem<T>(locations, dist));
        }

        public virtual IProblem<T> BuildIntegerArray(BiFunction<IList<T>, int[][], IProblem<T>> creator)
        {
            if (fixedFirstLocation != null && !locations__.Contains(fixedFirstLocation))
            {
                locations__.Add(0, fixedFirstLocation);
                fixedFirstLocationIndex = 0;
            }
            else if (fixedFirstLocation != null && locations__.Contains(fixedFirstLocation))
            {
                fixedFirstLocationIndex = locations__.IndexOf(fixedFirstLocation);
            }

            int[][] dist = new int[locations__.Count][];
            for (int i = 0; i < locations__.Count; i++)
            {
                dist[i] = new int[locations__.Count];
                for (int j = 0; j < locations__.Count; j++)
                {
                    dist[i][j] = MapToInt(distanceCalc.Invoke(locations__[i], locations__[j]).Value);
                }
            }
            IProblem<T> result = creator.Invoke(locations__, dist);
            result.SetFixedFirstLocation(fixedFirstLocationIndex);
            return result;
        }

        protected internal virtual int MapToInt(double value)
        {
            return (int)(value * intMultiplicator);
        }
    }
}