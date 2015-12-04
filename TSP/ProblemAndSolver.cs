using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace TSP
{

    class ProblemAndSolver
    {

        private class TSPSolution
        {
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// You are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your node data structure and search algorithm. 
            /// </summary>
            public ArrayList
                Route;

            public TSPSolution(ArrayList iroute)
            {
                Route = new ArrayList(iroute);
            }


            /// <summary>
            /// Compute the cost of the current route.  
            /// Note: This does not check that the route is complete.
            /// It assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                double cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }

                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);
                return cost;
            }
        }

        #region Private members 

        /// <summary>
        /// Default number of cities (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Problem Size text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int DEFAULT_SIZE = 25;

        private const int CITY_ICON_SIZE = 5;

        // For normal and hard modes:
        // hard mode only
        private const double FRACTION_OF_PATHS_TO_REMOVE = 0.20;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf; 

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// Difficulty level
        /// </summary>
        private HardMode.Modes _mode;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;
        #endregion

        #region Public members
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        #region Constructors
        public ProblemAndSolver()
        {
            this._seed = 1; 
            rnd = new Random(1);
            this._size = DEFAULT_SIZE;

            this.resetData();
        }

        public ProblemAndSolver(int seed)
        {
            this._seed = seed;
            rnd = new Random(seed);
            this._size = DEFAULT_SIZE;

            this.resetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed); 
            this.resetData();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Reset the problem instance.
        /// </summary>
        private void resetData()
        {

            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null;

            if (_mode == HardMode.Modes.Easy)
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());
            }
            else // Medium and hard
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() * City.MAX_ELEVATION);
            }

            HardMode mm = new HardMode(this._mode, this.rnd, Cities);
            if (_mode == HardMode.Modes.Hard)
            {
                int edgesToRemove = (int)(_size * FRACTION_OF_PATHS_TO_REMOVE);
                mm.removePaths(edgesToRemove);
            }
            City.setModeManager(mm);

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.Blue,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        //public void GenerateProblem(int size) // unused
        //{
        //   this.GenerateProblem(size, Modes.Normal);
        //}

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            this._size = size;
            this._mode = mode;
            resetData();
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-45F;
            Font labelFont = new Font("Arial", 10);

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[index+1]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else 
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[0]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D; 
        }

        /// <summary>
        ///  Helper method that updates the appropriate text fields on the form. 
        /// </summary>
        /// <returns></returns>
        private void updateForm(Stopwatch timer = null)
        {
            Program.MainForm.tbCostOfTour.Text = " " + this.costOfBssf();

            if (timer != null)
                Program.MainForm.tbElapsedTime.Text = timer.Elapsed.TotalSeconds.ToString();

            Program.MainForm.Invalidate();
        }

        /// <summary>
        /// Creates a distance matrix for the current list of cities.
        /// All of the last indexes in each row and column contain the index of min value
        /// in that row and column.
        /// </summary>
        /// <returns>double[,]</returns>
        private double[,] generateCostMatrix()
        {
            double[,] matrix = new double[Cities.Length, Cities.Length];

            for (int src = 0; src < Cities.Length; src++)
                for (int dst = 0; dst < Cities.Length; dst++)
                    matrix[src, dst] = Cities[src].costToGetTo(Cities[dst]);

            return matrix;
        }

        /// <summary>
        /// A solution is created by picking cities at random. 
        /// If a city is unreachable, start over.  
        /// </summary>
        /// <returns></returns>
        public void branchAndBoundSolution()
        {
            int totalStatesPruned = 0, totalStatesCreated = 0, bssfUpdates = 0, maxStoredStates = 0;
            State.totalCities = Cities.Length;
            bool bssfUpdated = false;

            greedySolution(true);

            double[,] initialMatrix = generateCostMatrix();

            PriorityQueue<State> states = new PriorityQueue<State>();

            State initial = new State(initialMatrix);
            states.Enqueue(initial, initial.lowerBound);
            totalStatesCreated++;
            maxStoredStates++;

            Stopwatch timer = new Stopwatch();
            timer.Start();

            State current = null;
            while (states.Count > 0)
            {
                if (timer.ElapsedMilliseconds > 30000)
                    break;

                current = states.Dequeue();

                if (current.lowerBound > bssf.costOfRoute())
                {
                    totalStatesPruned++;
                    continue;
                }

                if (current.isValidSolution())
                {
                    bssf = new TSPSolution(current.getRoute(ref Cities));
                    bssfUpdated = true;
                    continue;
                }

                Tuple<State, State> successors = current.expandSuccessors();
                if (successors == null)
                    continue;

                State include = successors.Item1;
                State exclude = successors.Item2;
                totalStatesCreated += 2;

                if (include.lowerBound > costOfBssf())
                    totalStatesPruned++;
                else
                {
                    states.Enqueue(include, include.lowerBound);
                    if (states.Count > maxStoredStates)
                        maxStoredStates = states.Count;
                }

                if (exclude.lowerBound > costOfBssf())
                    totalStatesPruned++;
                else
                {
                    states.Enqueue(exclude, exclude.lowerBound);
                    if (states.Count > maxStoredStates)
                        maxStoredStates = states.Count;
                }
            }

            timer.Stop();
            bssf = new TSPSolution(Route);
            string msg = "STATS:";
            msg += " Created: " + totalStatesCreated;
            msg += " Pruned: " + totalStatesPruned;
            msg += " Stored: " + maxStoredStates;
            Program.MainForm.tourInfo.Text = msg;
            updateForm(timer);
            return;
        }

        /// <summary>
        /// A solution is created by picking cities at random. 
        /// If a city is unreachable, start over.  
        /// </summary>
        /// <returns></returns>
        public void randomSolution()
        {
            Route = new ArrayList();
            Random generator = new Random();
            Route.Add(Cities[generator.Next(Cities.Length)]);

            int unreachableCities = 0;

            Stopwatch timer = new Stopwatch();
            timer.Start();

            while (Route.Count < Cities.Length)
            {
                City newCity = Cities[generator.Next(Cities.Length)];
                if (Route.Contains(newCity))
                    continue;

                //Make sure that the node is reachable.
                double cost = ((City)Route[Route.Count - 1]).costToGetTo(newCity);
                if (cost != double.PositiveInfinity)
                {
                    Route.Add(newCity);
                }
                else
                {
                    //If not, increment the unreachable cities counter.
                    //If this exceeds the number of cities, start over again.
                    unreachableCities++;
                    if (unreachableCities > Cities.Length)
                    {
                        unreachableCities = 0;
                        Route.Clear();
                        Route.Add(Cities[generator.Next(Cities.Length)]);
                    }
                }
            }

            timer.Stop();

            bssf = new TSPSolution(Route);
            updateForm(timer);
            return;

        }

        /// <summary>
        /// Sorts all the edges according to length.
        /// </summary>
        /// <returns></returns>
        public SortedList getSortedEdges()
        {
            SortedList sortedEdges = new SortedList();

            for (int src = 0; src < Cities.Length; src++)
                for (int dst = 0; dst < Cities.Length; dst++)
                    sortedEdges.Add(new Tuple<int, double, int>(src, Cities[src].costToGetTo(Cities[dst]), dst), new Tuple<int, int>(src, dst));

            return sortedEdges;
        }

        public void greedySolution(bool usingForBssf = false)
        {
            bool valid = false;
            Random r = new Random();
            Stopwatch timer = new Stopwatch();
            timer.Start();

            while (!valid)
            {
                Route.Clear();
                int currentIndex = r.Next(Cities.Length);
                Route.Add(Cities[currentIndex]);

                while (Route.Count != Cities.Length)
                {
                    double minDist = double.MaxValue;

                    for (int i = 0; i < Cities.Length; i++)
                    {
                        if (Route.Contains(Cities[i]))
                            continue;

                        double dist = Cities[currentIndex].costToGetTo(Cities[i]);

                        if (dist < minDist)
                        {
                            minDist = dist;
                            currentIndex = i;
                        }
                    }
                    Route.Add(Cities[currentIndex]);
                }

                bssf = new TSPSolution(Route);
                valid = bssf.costOfRoute() != double.PositiveInfinity;
            }
            timer.Stop();
            
            if(!usingForBssf)
                updateForm(timer);
            return;
        }

        /// <summary>
        ///  solve the problem.  This is the entry point for the solver when the run button is clicked
        /// right now it just picks a simple solution. 
        /// </summary>
        public void solveProblem()
        {
            int x;
            Route = new ArrayList(); 
            // this is the trivial solution. 
            for (x = 0; x < Cities.Length; x++)
            {
                Route.Add( Cities[Cities.Length - x -1]);
            }
            // call this the best solution so far.  bssf is the route that will be drawn by the Draw method. 
            bssf = new TSPSolution(Route);
            updateForm();

        }
        #endregion
    }

}
