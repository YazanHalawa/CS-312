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
            /// for your data structure(s) and search algorithm. 
            /// </summary>
            public ArrayList
                Route;

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="iroute">a (hopefully) valid tour</param>
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

        /// <summary>
        /// Default time limit (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Time text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int TIME_LIMIT = 60;        //in seconds

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

        /// <summary>
        /// time limit in milliseconds for state space search
        /// can be used by any solver method to truncate the search and return the BSSF
        /// </summary>
        private int time_limit;
        #endregion

        #region Public members

        /// <summary>
        /// These three constants are used for convenience/clarity in populating and accessing the results array that is passed back to the calling Form
        /// </summary>
        public const int COST = 0;           
        public const int TIME = 1;
        public const int COUNT = 2;
        
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
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed)
        {
            this._seed = seed;
            rnd = new Random(seed);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = TIME_LIMIT * 1000;                        // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        public ProblemAndSolver(int seed, int size, int time)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = time*1000;                        // time is entered in the GUI in seconds, but timer wants it in milliseconds

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
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            this._size = size;
            this._mode = mode;
            resetData();
        }

        /// <summary>
        /// make a new problem with the given size, now including timelimit paremeter that was added to form.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode, int timelimit)
        {
            this._size = size;
            this._mode = mode;
            this.time_limit = timelimit*1000;                                   //convert seconds to milliseconds
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
        /// This is the entry point for the default solver
        /// which just finds a valid random tour 
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] defaultSolveProblem()
        {
            int i, swap, temp, count=0;
            string[] results = new string[3];
            int[] perm = new int[Cities.Length];
            Route = new ArrayList();
            Random rnd = new Random();
            Stopwatch timer = new Stopwatch();

            timer.Start();

            do
            {
                for (i = 0; i < perm.Length; i++)                                 // create a random permutation template
                    perm[i] = i;
                for (i = 0; i < perm.Length; i++)
                {
                    swap = i;
                    while (swap == i)
                        swap = rnd.Next(0, Cities.Length);
                    temp = perm[i];
                    perm[i] = perm[swap];
                    perm[swap] = temp;
                }
                Route.Clear();
                for (i = 0; i < Cities.Length; i++)                            // Now build the route using the random permutation 
                {
                    Route.Add(Cities[perm[i]]);
                }
                bssf = new TSPSolution(Route);
                count++;
            } while (costOfBssf() == double.PositiveInfinity);                // until a valid route is found
            timer.Stop();

            results[COST] = costOfBssf().ToString();                          // load results array
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = count.ToString();

            return results;
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////// State Class //////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////
        #region StateClass
        /**
        * This class represents the state at each node in the branch and bound algorithm.
        * 
        */
        public class State
        {
            private ArrayList path;
            private double lowerBound;
            private double priority;
            private double[,] costMatrix;

            /**
            * Constructor
            */
            public State(ref ArrayList newPath, ref double newLowerBound, ref double[,] newCostMatrix, int length)
            {
                path = newPath;
                lowerBound = newLowerBound;
                costMatrix = newCostMatrix;
                priority = double.MaxValue;
            }

            /**
            * Functions to Manipulate and return the path
            */
            public ArrayList getPath()
            {
                return path;
            }
            public void addCityToPath(City newCity)
            {
                path.Add(newCity);
            }

            /**
            * Functions to Manipalte and return the priority
            */
            public double getPriority()
            {
                return priority;
            }
            public void setPriority(double newPriority)
            {
                priority = newPriority;
            }

            /**
            * Functions to set and return the lower bound
            */
            public double getLowerBound()
            {
                return lowerBound;
            }
            public void setLowerBound(double newLowerBound)
            {
                lowerBound = newLowerBound;
            }

            /**
            * Functions to set and return the cost matrix
            */
            public double[,] getCostMatrix()
            {
                return costMatrix;
            }

        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////// Helper Functions //////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////
        #region HelperFunctions
        /**
        * Helper Function to create a key value for a state for use in priority queue
        * Time Complexity: O(1) as it only perform a mathematical addition and multiplication, and if we 
        * assume n is the size of the input then those would be constant operations
        * Space Complexity: O(1) as it does not create any extra data structures that depend on the size of the input.
        */
        double calculateKey(int numofCitiesLeft, double lowerBound)
        {
            // If there are no cities left, just use the lower bound
            if (numofCitiesLeft < 1)
                return lowerBound;
            else
                return lowerBound + numofCitiesLeft * 31; // The number 31 was picked because it is a prime number
        }
        /**
        * Helper Function to create an initial greedy solution to assign BSSF to in the beginning
        * Time Complexity: O(n^2) because for each city it is iterating over all the cities in the list. so n is the 
        * number of cities, or rather |V|
        * Space Complexity: O(n) as it creates an Array list(Route) of size equal to the number of cities in the graph where
        * n is the number of cities, or rather |V|
        */
        double createGreedyInitialBSSF()
        {
            // Create variables to track progress
            Route = new ArrayList();
            Route.Add(Cities[0]);
            int currCityIndex = 0;
            
            // While we haven't added |V| edges to our route
            while (Route.Count < Cities.Length)
            {
                double minValue = double.MaxValue;
                int minIndex = 0;

                // Loop over all the cities and find the one with min cost to get to
                for (int i = 0; i < Cities.Length; i++)
                {
                    // We don't want to be checking ourselves because that will be the minimum and it won't be a tour
                    if (currCityIndex != i)
                    {
                        // We don't want to add a city that we already added because that forms a cycle and it won't be a tour
                        if (!Route.Contains(Cities[i]))
                        {
                            double tempValue = Cities[currCityIndex].costToGetTo(Cities[i]);
                            if (tempValue < minValue)
                            {
                                if (Route.Count == Cities.Length - 1 && Cities[i].costToGetTo(Cities[0]) == double.MaxValue)
                                {
                                    continue;
                                }
                                minValue = tempValue;
                                minIndex = i;
                            }
                        }
                    }
                }

                // Add the min edge to the Route by adding the destination city
                currCityIndex = minIndex;
                Route.Add(Cities[currCityIndex]);
            }

            // Once we have a complete tour, we set out BSSF to it as an upper bound for all solutions to follow
            bssf = new TSPSolution(Route);
            return bssf.costOfRoute();
        }
        /**
        * Helper Function to initially set up a cost matrix at a current state
        * Time Complexity: O(n) as it iterates over one row and one column in the matrix and n would be the length of 
        * the row/column which is the number of cities in the graph, or rather |V|
        * Space Complexity: O(1) as all the data is passed by reference and the function does not create extra
        * data structures that depend on the size of the input.
        */ 
        void setUpMatrix(ref double[,] costMatrix, int indexOfParent, int indexOfChild, ref double lowerBound)
        {
            if (costMatrix[indexOfParent, indexOfChild] != double.MaxValue)
                lowerBound += costMatrix[indexOfParent, indexOfChild];
            // Make sure to set all costs coming from the currState to infinity
            for (int column = 0; column < Cities.Length; column++)
            {
                costMatrix[indexOfParent, column] = double.MaxValue;
            }
            // Make sure to set all costs coming into the child State to infinity
            for (int row = 0; row < Cities.Length; row++)
            {
                costMatrix[row, indexOfChild] = double.MaxValue;
            }
            // Make sure to set the cost of going from child state back to parent to infinity as we don't want cycles
            costMatrix[indexOfChild, indexOfParent] = double.MaxValue;
        }
        /**
        * Helper function to reduce a cost matrix and calculate the lower bound of the corresponding state
        * Time Complexity: O(n^2) because it iterates over all the cells in an nxn matrix 2 times (so really it is
        * O(4n^2) but the constant is ommitted.
        * Space Complexity: O(1) as the matrix is passed by reference and so the function does not create 
        * any data structures that depend on the size of the input
        */
        double reduceMatrix(ref double[,] costMatrix)
        {
            double lowerBound = 0;
            // Loop through the rows, find the min value for each, and subtract it from every other cell value
            for (int row = 0; row < Cities.Length; row++)
            {
                // Find The Minimum value in the row
                double minVal = double.MaxValue;
                for (int column = 0; column < Cities.Length; column++)
                {
                    if (costMatrix[row, column] < minVal)
                    {
                        minVal = costMatrix[row, column];
                    }
                }

                // Subtract the min value from each cell if the min value is not infinity
                if (minVal != 0 && minVal != double.MaxValue)
                {
                    lowerBound += minVal;
                    for (int column = 0; column < Cities.Length; column++)
                    {
                        if (costMatrix[row, column] != double.MaxValue)
                            costMatrix[row, column] -= minVal;
                    }
                }            
            }

            // Loop through the columns, find the minvalue for each and subtract it from every other cell value
            for (int column = 0; column < Cities.Length; column++)
            {
                // Find The Minimum value in the row
                double minVal = double.MaxValue;
                for (int row = 0; row < Cities.Length; row++)
                {
                    if (costMatrix[row, column] < minVal)
                    {
                        minVal = costMatrix[row, column];
                    }
                }
                // Subtract the min value from each cell if the min value is not infinity
                if (minVal != 0 && minVal != double.MaxValue)
                {
                    lowerBound += minVal;
                    for (int row = 0; row < Cities.Length; row++)
                    {
                        if (costMatrix[row, column] != double.MaxValue)
                            costMatrix[row, column] -= minVal;
                    }
                }            
            }

            return lowerBound;
        }
        /**
        * Helper function that will create the initial State starting at the first City in the list.
        * Time Complexity: summing up the time complexities of the parts of this function as explained in the code:
        * n^2 + 1 + n^2 = O(n^2) because constants are ommitted
        * Space Complexity: summing up the space complexities of the parts of this function as explained in the code:
        * n^2 + 1 + 1 = O(n^2) because constants are ommitted.
        */
        State createInitialState()
        {
            /* First Create the initial cost matrix based on the costs to get from each city to the other */
            /* Loop through all the matrix cells, if we are at the diagonal then set the cost to infinity, else set it to the 
               cost to get from city at rows(i) to city at columns(j) */
            /* This part takes O(n^2) time and O(n^2) space*/
            double[,] initialCostMatrix = new double[Cities.Length, Cities.Length];
            for (int i = 0; i < Cities.Length; i++)
            {
                for (int j = 0; j < Cities.Length; j++)
                {
                    if (i == j)
                        initialCostMatrix[i, j] = double.MaxValue;
                    else
                        initialCostMatrix[i, j] = Cities[i].costToGetTo(Cities[j]);
                }
            }
            /* Second the path will be simply the starting node */
            /* This part takes O(1) time and space */
            ArrayList path = new ArrayList();
            path.Add(Cities[0]);

            /* Third Calculate the lower Bound */
            /* This part takes O(n^2) time and O(1) space */
            double lowerBound = reduceMatrix(ref initialCostMatrix);
            return new State(ref path, ref lowerBound, ref initialCostMatrix, Cities.Length);
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////// BB Algorithm ////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////
        #region MainBBAlgorithm
        /// <summary>
        /// performs a Branch and Bound search of the state space of partial tours
        /// stops when time limit expires and uses BSSF as solution
        /// Time Complexity: O((n^2)*(2^n) as that is the most dominant factor in the code, and it is a result
        /// of the loop, for more details scroll to the comment above the loop in the function.
        /// Space Complexity: O((n^2)*(2^n) as that is the most dominant factor in the code, and it is a result
        /// of the loop, for more details scroll to the comment above the loop in the function.
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>

        public string[] bBSolveProblem()
        {
            string[] results = new string[3];
            
            // Helper variables
            /* This part of the code takes O(1) space and time as we are just initializing some data */
            int numOfCitiesLeft = Cities.Length;
            int numOfSolutions = 0;
            int numOfStatesCreated = 0;
            int numOfStatesNotExpanded = 0;

            // Initialize the time variable to stop after the time limit, which is defaulted to 60 seconds
            /* This part of the code takes O(1) space and time as we are just initializing some data */
            DateTime start = DateTime.Now;
            DateTime end = start.AddSeconds(time_limit/1000);

            // Create the initial root State and set its priority to its lower bound as we don't have any extra info at this point
            /* This part of the code takes O(n^2) space and time as explained above */
            State initialState = createInitialState();
            numOfStatesCreated++;
            initialState.setPriority(calculateKey(numOfCitiesLeft - 1, initialState.getLowerBound()));

            // Create the initial BSSF Greedily
            /* This part of the code takes O(n^2) time and O(n) space as explained above */
            double BSSFBOUND = createGreedyInitialBSSF();

            // Create the queue and add the initial state to it, then subtract the number of cities left
            /* This part of the code takes O(1) time since we are just creating a data structure and 
            O(1,000,000) space which is just a constant so O(1) space as well*/
            PriorityQueueHeap queue = new PriorityQueueHeap();
            queue.makeQueue(Cities.Length);
            queue.insert(initialState);

            // Branch and Bound until the queue is empty, we have exceeded the time limit, or we found the optimal solution
            /* This loop will have a iterate 2^n times approximately with expanding and pruning for each state, then for each state it
            does O(n^2) work by reducing the matrix, so over all O((n^2)*(2^n)) time and space as well as it creates a nxn 
            matrix for each state*/
            while (!queue.isEmpty() && DateTime.Now < end && queue.getMinLB() != BSSFBOUND)
            {
                // Grab the next state in the queue
                State currState = queue.deleteMin();

                // check if lower bound is less than the BSSF, else prune it
                if (currState.getLowerBound() < BSSFBOUND)
                {
                    // Branch and create the child states
                    for (int i = 0; i < Cities.Length; i++)
                    {
                        // First check that we haven't exceeded the time limit
                        if (DateTime.Now >= end)
                            break;

                        // Make sure we are only checking cities that we haven't checked already
                        if (currState.getPath().Contains(Cities[i]))
                            continue;

                        // Create the State
                        double[,] oldCostMatrix = currState.getCostMatrix();
                        double[,] newCostMatrix = new double[Cities.Length, Cities.Length];
                        // Copy the old array in the new one to modify the new without affecting the old
                        for (int k = 0; k < Cities.Length; k++)
                        {
                            for (int l = 0; l < Cities.Length; l++)
                            {
                                newCostMatrix[k, l] = oldCostMatrix[k, l];
                            }
                        } 
                        City lastCityinCurrState = (City)currState.getPath()[currState.getPath().Count-1];
                        double oldLB = currState.getLowerBound();
                        setUpMatrix(ref newCostMatrix, Array.IndexOf(Cities, lastCityinCurrState), i, ref oldLB);
                        double newLB = oldLB + reduceMatrix(ref newCostMatrix);
                        ArrayList oldPath = currState.getPath();
                        ArrayList newPath = new ArrayList();
                        foreach (City c in oldPath)
                        {
                            newPath.Add(c);
                        }
                        newPath.Add(Cities[i]);
                        State childState = new State(ref newPath, ref newLB, ref newCostMatrix, Cities.Length);
                        numOfStatesCreated++;

                        // Prune States larger than the BSSF
                        if (childState.getLowerBound() < BSSFBOUND)
                        {
                            City firstCity = (City)childState.getPath()[0];
                            City lastCity = (City)childState.getPath()[childState.getPath().Count-1];
                            double costToLoopBack = lastCity.costToGetTo(firstCity);

                            // If we found a solution and it goes back from last city to first city
                            if (childState.getPath().Count == Cities.Length && costToLoopBack != double.MaxValue)
                            {
                                childState.setLowerBound(childState.getLowerBound() + costToLoopBack);
                                bssf = new TSPSolution(childState.getPath());
                                //Console.WriteLine("lower bound is " + childState.getLowerBound() +
                                //                    "and BSSF is " + BSSFBOUND);
                                BSSFBOUND = bssf.costOfRoute();
                                numOfSolutions++;
                                numOfStatesNotExpanded++; // this state is not expanded because it is not put on the queue
                            }
                            else
                            {
                                // Set the priority for the state and add the new state to the queue
                                numOfCitiesLeft = Cities.Length - childState.getPath().Count;
                                childState.setPriority(calculateKey(numOfCitiesLeft, childState.getLowerBound()));
                                queue.insert(childState);
                            }
                        }
                        else
                        {
                            numOfStatesNotExpanded++; // States that are pruned are not expanded
                        }             
                    }           
                }
                currState = null;
            }
            numOfStatesNotExpanded += queue.getSize(); // if the code terminated before queue is empty, then those states never got expanded
            Console.WriteLine("Number of states generated: " + numOfStatesCreated);
            Console.WriteLine("Number of states not Expanded: " + numOfStatesNotExpanded);
            end = DateTime.Now;
            TimeSpan diff = end - start;
            double seconds = diff.TotalSeconds;
            results[COST] = System.Convert.ToString(bssf.costOfRoute());    // load results into array here, replacing these dummy values
            results[TIME] = System.Convert.ToString(seconds);
            results[COUNT] = System.Convert.ToString(numOfSolutions);

            return results;
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////// Priority Queue ///////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////
        #region PriorityQueue
        public sealed class PriorityQueueHeap
        {
            private int capacity;
            private int count;
            private State[] states;
            public PriorityQueueHeap()
            {
            }

            /**
            * This functions returns whether the queue is empty or not. Time and Space = O(1) as it only involves an int comparison
            */
            public bool isEmpty()
            {
                return count == 0;
            }

            /**
            * This function returns the number of items in the queue
            */
            public int getSize()
            {
                return count;
            }
            /**
            * This function returns the lower bound of the first item in the queue
            */
            public double getMinLB()
            {
                return states[1].getLowerBound();
            }

            /**
            * This method creates an array to implement the queue. Time and Space Complexities are both O(|V|) where |V| is
            * the number of nodes. This is because you create an array of the same size and specify the value for each item by 
            * iterating over the entire array.
            */
            public void makeQueue(int numOfNodes)
            {
                states = new State[1000000];
                capacity = numOfNodes;
                count = 0;
            }

            /**
            * This method returns the index of the element with the minimum value and removes it from the queue. 
            * Time Complexity: O(log(|V|)) because removing a node is constant time as we have its position in
            * the queue, then to readjust the heap we just bubble up the min value which takes as long as 
            * the depth of the tree which is log(|V|), where |V| is the number of nodes
            * Space Complexity: O(1) because we don't create any extra variables that vary with the size of the input.
            */
            public State deleteMin()
            {
                // grab the node with min value which will be at the root
                State minValue = states[1];
                //states[1].setPriority(double.MaxValue);
                states[1] = states[count];
                count--;
                // fix the heap
                int indexIterator = 1;
                while (indexIterator <= count)
                {
                    // grab left child
                    int smallerElementIndex = 2 * indexIterator;

                    // if child does not exist, break
                    if (smallerElementIndex > count)
                        break;

                    // if right child exists and is of smaller value, pick it
                    if (smallerElementIndex + 1 <= count && states[smallerElementIndex + 1].getPriority()
                                                                  < states[smallerElementIndex].getPriority())
                    {
                        smallerElementIndex++;
                    }

                    if (states[indexIterator].getPriority() > states[smallerElementIndex].getPriority())
                    {
                        // set the node's value to that of its smaller child
                        State temp = states[smallerElementIndex];
                        states[smallerElementIndex] = states[indexIterator];
                        states[indexIterator] = temp;
                    }

                    indexIterator = smallerElementIndex;
                }
                // return the min value
                return minValue;
            }

            /**
            * This method updates the nodes in the queue after inserting a new node
            * Time Complexity: O(log(|V|)) as reording the heap works by bubbling up the min value to the top
            * which takes as long as the depth of the tree which is log|V|.
            * Space Complexity: O(1) as it does not create any extra variables that vary with the size of the input.
            */
            public void insert(State newState)
            {
                // update the count
                count++;
                states[count] = newState;

                // as long as its parent has a larger value and have not hit the root
                int indexIterator = count;
                while (indexIterator > 1 && states[indexIterator / 2].getPriority() > states[indexIterator].getPriority())
                {
                    // swap the two nodes
                    State temp = states[indexIterator / 2];
                    states[indexIterator / 2] = states[indexIterator];
                    states[indexIterator] = temp;

                    indexIterator /= 2;
                }
            }
        }
        #endregion


        /////////////////////////////////////////////////////////////////////////////////////////////
        // These additional solver methods will be implemented as part of the group project.
        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// finds the greedy tour starting from each city and keeps the best (valid) one
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] greedySolveProblem()
        {
            string[] results = new string[3];

            // TODO: Add your implementation for a greedy solver here.

            results[COST] = "not implemented";    // load results into array here, replacing these dummy values
            results[TIME] = "-1";
            results[COUNT] = "-1";

            return results;
        }

        public string[] fancySolveProblem()
        {
            string[] results = new string[3];

            // TODO: Add your implementation for your advanced solver here.
           
            results[COST] = "not implemented";    // load results into array here, replacing these dummy values
            results[TIME] = "-1";
            results[COUNT] = "-1";

            return results;
        }
        #endregion
    }

}
