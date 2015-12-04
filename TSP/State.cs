using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    //Represents a state in the include/exlude branch and bound algorithm.
    class State
    {
        public static int totalCities;
        public int currentCityCount;
        public double[,] costMatrix;

        public double lowerBound {
            get { return heuristic(); }
            set { }
        }

        // List of edges that are included
        public HashSet<Tuple<int,int>> includedEdges = new HashSet<Tuple<int, int>>();

        public State(double[,] costMatrix, double lowerBound, HashSet<Tuple<int, int>> includedEdges=null)
        {
            this.costMatrix = duplicateMatrix(costMatrix);
            this.lowerBound = lowerBound;
            this.currentCityCount = 0;

            if (includedEdges == null)
                this.includedEdges.Clear();
            else
                this.includedEdges = new HashSet<Tuple<int, int>>(includedEdges);
        }

        private double[,] duplicateMatrix(double[,] matrix)
        {
            double[,] newMatrix = new double[matrix.GetLength(0), matrix.GetLength(0)];
            Array.Copy(matrix, newMatrix, matrix.Length);
            return newMatrix;
        }

        // Evaluate lowerbound as a combination of the actual lowerbound and number of cities remaining.
        private double heuristic()
        {
            if (totalCities - currentCityCount < 1)
                return lowerBound;
            return lowerBound + (totalCities - currentCityCount);
        }

        public bool isValidSolution()
        {
            if (includedEdges.Count == totalCities)
                return true;
            return false;
        }

        //Returns the route that corresponds with the set of included edges in this state.
        public ArrayList getRoute(ref City[] Cities)
        {
            ArrayList route = new ArrayList();
            int starting_city = (int)includedEdges.GetEnumerator().Current.Item1;
            int current_city = starting_city;

            do
            {
                route.Add(Cities[current_city]);
                foreach (Tuple<int, int> edge in includedEdges)
                {
                    if (edge.Item1 == current_city)
                    {
                        current_city = edge.Item2;
                        break;
                    }
                }
            } while (current_city != starting_city);

            return route;
        }

        public Tuple<int, int> getNextEdge()
        {
            Tuple<int, int> bestEdgeSoFar = null;
            double difference, maxDifference = -1;
            double includeBound, excludeBound;

            // Loop through all edges
            for (int i = 0; i < totalCities; i++)
            {
                for (int j = 0; j < totalCities; j++)
                {
                    if (costMatrix[i, j] != 0)
                        continue;

                    double[,] tempCm = duplicateMatrix(costMatrix);

                    tempCm[i, j] = double.PositiveInfinity;
                    tempCm[j, i] = double.PositiveInfinity;

                    for (int t = 0; t < tempCm.GetLength(0); t++)
                        tempCm[t, j] = double.PositiveInfinity;

                    for (int t = 0; t < tempCm.GetLength(1); t++)
                        tempCm[i, t] = double.PositiveInfinity;

                    includeBound = bound + ProblemAndSolver.reduceCM(ref tempCm);

                    // For exclusion, make the cost of [i, j] infinite, then
                    // b(Se) = b(Sparent) + min(rowi) + min(colj)
                    tempCm = duplicateCM(cm);

                    tempCm[i, j] = double.PositiveInfinity;
                    excludeBound = bound + ProblemAndSolver.reduceCM(ref tempCm);

                    // Calculate the differnce, check to see if this is lower than the lowest so far
                    difference = Math.Abs(excludeBound - includeBound);
                    if (difference > maxDifference)
                    {
                        maxDifference = difference;
                        bestEdgeSoFar = new Tuple<int, int>(i, j);
                    }
                }
            }

            return bestEdgeSoFar;
        }
    }
}
