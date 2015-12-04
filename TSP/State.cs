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
            set { LowerBround = value; }
        }

        private double LowerBround;

        // List of edges that are included
        public HashSet<Tuple<int,int>> includedEdges = new HashSet<Tuple<int, int>>();

        public State(double[,] costMatrix)
        {
            this.costMatrix = duplicateMatrix(costMatrix);
            this.lowerBound = reduceCostMatrix(ref this.costMatrix);

            this.currentCityCount = 0;

            this.includedEdges.Clear();
        }

        public State(double[,] costMatrix, double lowerBound, HashSet<Tuple<int, int>> includedEdges)
        {
            this.costMatrix = duplicateMatrix(costMatrix);
            this.lowerBound = lowerBound;

            this.currentCityCount = 0;

            this.includedEdges = new HashSet<Tuple<int, int>>(includedEdges);
        }

        /// <summary>
        /// Reduces the matrix and sets the lower bound for the state.
        /// </summary>
        /// <returns>double[,]</returns>
        private double reduceCostMatrix(ref double[,] matrix)
        {
            double lowerBound = 0;
            //Reduce Rows
            for (int row = 0; row < totalCities; row++)
            {
                double rowMin = double.PositiveInfinity;

                for (int col = 0; col < totalCities; col++)
                    if (matrix[row, col] < rowMin) rowMin = matrix[row, col];

                if (rowMin == double.PositiveInfinity || rowMin == 0)
                    continue;

                for (int col = 0; col < totalCities; col++)
                    matrix[row, col] -= rowMin;

                lowerBound += rowMin;
            }

            //Reduce Columns
            for (int col = 0; col < totalCities; col++)
            {

                double colMin = double.PositiveInfinity;

                for (int row = 0; row < totalCities; row++)
                    if (matrix[row, col] < colMin) colMin = matrix[row, col];

                if (colMin == double.PositiveInfinity || colMin == 0)
                    continue;

                for (int row = 0; row < totalCities; row++)
                    matrix[row, col] -= colMin;

                lowerBound += colMin;
            }

            return lowerBound;
        }

        // Evaluate lowerbound as a combination of the actual lowerbound and number of cities remaining.
        private double heuristic()
        {
            if (totalCities - currentCityCount < 1)
                return LowerBround;
            return LowerBround + (totalCities - currentCityCount);
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
            HashSet<Tuple<int,int>>.Enumerator e = includedEdges.GetEnumerator();
            e.MoveNext();

            int starting_city = e.Current.Item1;
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

        public Tuple<State, State> expandSuccessors()
        {
            Tuple<int, int, double[,], double, double[,], double> successors = getSuccessors();

            if (successors == null)
                return null;

            Tuple<int, int> edge = new Tuple<int, int>(successors.Item1, successors.Item2);

            State include = new State(successors.Item3, successors.Item4, includedEdges);
            State exclude = new State(successors.Item5, successors.Item6, includedEdges);

            include.includedEdges.Add(edge);

            //Remove edges that are part of a premature cycle.
            if (include.includedEdges.Count < include.costMatrix.GetLength(0) - 1)
            {
                int start = edge.Item1, end = edge.Item2, city;

                city = getExited(start);
                while (city != -1)
                {
                    start = city;
                    city = getExited(start);
                }

                city = getEntered(end);
                while (city != -1)
                {
                    end = city;
                    city = getEntered(end);
                }

                while (start != edge.Item2 && start != -1)
                {
                    include.costMatrix[end, start] = double.PositiveInfinity;
                    include.costMatrix[edge.Item2, start] = double.PositiveInfinity;
                    start = getEntered(start);
                }
            }

            include.lowerBound = include.lowerBound + include.reduceCostMatrix(ref include.costMatrix);
            
            return new Tuple<State, State>(include, exclude);
        }

        public Tuple<int, int, double[,], double, double[,], double> getSuccessors()
        {
            Tuple<int, int, double[,], double, double[,], double> bestStatesSoFar = null;
            double difference, maxDifference = -1;
            double includeBound, excludeBound;

            // Loop through all edges
            for (int row = 0; row < totalCities; row++)
            {
                for (int col = 0; col < totalCities; col++)
                {
                    if (costMatrix[row, col] != 0)
                        continue;

                    //Check the lb if included.
                    double[,] tempI = duplicateMatrix(costMatrix);

                    tempI[row, col] = double.PositiveInfinity;
                    tempI[col, row] = double.PositiveInfinity;

                    //Set the row and column to infinity.
                    for (int t = 0; t < tempI.GetLength(0); t++)
                        tempI[t, col] = double.PositiveInfinity;

                    for (int t = 0; t < tempI.GetLength(1); t++)
                        tempI[row, t] = double.PositiveInfinity;

                    includeBound = lowerBound + reduceCostMatrix(ref tempI);

                    //Copy over the matrix and test for the exclude lb.
                    double[,] tempE = duplicateMatrix(costMatrix);

                    tempE[row, col] = double.PositiveInfinity;
                    excludeBound = lowerBound + reduceCostMatrix(ref tempE);

                    // Calculate the difference and check to see if this is the largest difference so far.
                    difference = Math.Abs(excludeBound - includeBound);
                    if (difference > maxDifference)
                    {
                        maxDifference = difference;
                        bestStatesSoFar = new Tuple<int, int, double[,], double, double[,], double>(row, col, tempI, includeBound, tempE, excludeBound);
                    }
                }
            }

            return bestStatesSoFar;
        }

        private double[,] duplicateMatrix(double[,] matrix)
        {
            double[,] newMatrix = new double[matrix.GetLength(0), matrix.GetLength(0)];
            Array.Copy(matrix, newMatrix, matrix.Length);
            return newMatrix;
        }

        private int getEntered(int cityExited)
        {
            foreach (Tuple<int, int> t in includedEdges)
            {
                if (t.Item1 == cityExited)
                    return t.Item2;
            }

            return -1;
        }

        private int getExited(int cityEntered)
        {
            foreach (Tuple<int, int> t in includedEdges)
            {
                if (t.Item2 == cityEntered)
                    return t.Item1;
            }

            return -1;
        }
    }
}
