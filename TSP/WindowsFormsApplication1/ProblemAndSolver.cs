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
        #region StateClass
        /////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////// State Class //////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////
        /**
        * This class represents the state at each node in the branch and bound algorithm. It contains
        * 
        */
        class State
        {
            private ArrayList path;
            private double lowerBound;
            private double[,] costMatrix;

            /**
            * Constructor
            */
            public State(ArrayList newPath, double newLowerBound, double[,] newCostMatrix)
            {
                path = newPath;
                lowerBound = newLowerBound;
                costMatrix = newCostMatrix;
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
            public void setCostMatrix(double[,] newMatrix)
            {
                costMatrix = newMatrix;
            }

        }
        #endregion
        #region HelperFunctions
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////// Helper Functions //////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /**
        * Helper Function to create an initial greedy solution to assign BSSF to in the beginning
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
        * Helper function to reduce a cost matrix and calculate the lower bound of the corresponding state
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
                lowerBound += minVal;
                // Subtract the min value from each cell
                for (int column = 0; column < Cities.Length; column++)
                {
                    costMatrix[row, column] -= minVal;
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
                lowerBound += minVal;
                // Subtract the min value from each cell
                for (int row = 0; row < Cities.Length; row++)
                {
                    costMatrix[row, column] -= minVal;
                }
            }

            return lowerBound;
        }
        /**
        * Helper function that will create the initial State starting at the first City in the list.
        */
        State createInitialState()
        {
            /* First Create the initial cost matrix based on the costs to get from each city to the other */
            /* Loop through all the matrix cells, if we are at the diagonal then set the cost to infinity, else set it to the 
               cost to get from city at rows(i) to city at columns(j) */
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
            ArrayList path = new ArrayList();
            path.Add(Cities[0]);

            /* Third Calculate the lower Bound */
            double lowerBound = reduceMatrix(ref initialCostMatrix);
            return new State(path, lowerBound, initialCostMatrix);
        }
        #endregion
        #region MainBBAlgorithm
        /// <summary>
        /// performs a Branch and Bound search of the state space of partial tours
        /// stops when time limit expires and uses BSSF as solution
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] bBSolveProblem()
        {
            string[] results = new string[3];

            // Create the initial root State
            State initialState = createInitialState();

            // Create the initial BSSF Greedily
            double BSSFCOST = createGreedyInitialBSSF();
            
            // Now that we have all the initial data that we need, start the recursive algorithm 

            results[COST] = "not implemented";    // load results into array here, replacing these dummy values
            results[TIME] = "-1";
            results[COUNT] = "-1";

            return results;
        }
        #endregion
        #region PriorityQueue
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////// Priority Queue ////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public class PriorityQueueHeap
        {
            private int capacity;
            private int count;
            private int lastElement;
            private int[] distances;
            private int[] pointers;
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
            * Helper method to print the contents of the queue. Time Complexity: O(|V|) where |V| is the number of nodes.
            * Space Complexity: O(1) as it does not create any extra variables that vary with the size of the input.
            */
            public void printQueueContents()
            {
                Console.Write("The contents of the queue are: ");
                for (int i = 1; i < capacity; i++)
                {
                    if (distances[i] != -1)
                        Console.Write(distances[i] + " ");
                }
                Console.WriteLine();
            }

            /**
            * This method creates an array to implement the queue. Time and Space Complexities are both O(|V|) where |V| is
            * the number of nodes. This is because you create an array of the same size and specify the value for each item by 
            * iterating over the entire array.
            */
            public void makeQueue(int numOfNodes)
            {
                distances = new int[numOfNodes + 1];
                pointers = new int[numOfNodes];
                for (int i = 1; i < numOfNodes + 1; i++)
                {
                    distances[i] = i - 1;
                    pointers[i - 1] = i;
                }
                capacity = numOfNodes;
                count = 0;
                lastElement = capacity;
            }

            /**
            * This method returns the index of the element with the minimum value and removes it from the queue. 
            * Time Complexity: O(log(|V|)) because removing a node is constant time as we have its position in
            * the queue, then to readjust the heap we just bubble up the min value which takes as long as 
            * the depth of the tree which is log(|V|), where |V| is the number of nodes
            * Space Complexity: O(1) because we don't create any extra variables that vary with the size of the input.
            */
            public int deleteMin(ref List<double> distanceArray)
            {
                // grab the node with min value which will be at the root
                int minValue = distances[1];
                count--;
                //Console.WriteLine("last element is " + lastElement);
                if (lastElement == -1)
                    return minValue;
                distances[1] = distances[lastElement];
                pointers[distances[1]] = 1;
                lastElement--;


                // fix the heap
                int indexIterator = 1;
                while (indexIterator <= lastElement)
                {
                    // grab left child
                    int smallerElementIndex = 2 * indexIterator;

                    // if child does not exist, break
                    if (smallerElementIndex > lastElement)
                        break;

                    // if right child exists and is of smaller value, pick it
                    if (smallerElementIndex + 1 <= lastElement && distanceArray[distances[smallerElementIndex + 1]] < distanceArray[distances[smallerElementIndex]])
                    {
                        smallerElementIndex++;
                    }

                    if (distanceArray[distances[indexIterator]] > distanceArray[distances[smallerElementIndex]])
                    {
                        // set the node's value to that of its smaller child and update the iterator
                        int temp = distances[smallerElementIndex];
                        distances[smallerElementIndex] = distances[indexIterator];
                        distances[indexIterator] = temp;

                        pointers[distances[indexIterator]] = indexIterator;
                        pointers[distances[smallerElementIndex]] = smallerElementIndex;
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
            public void insert(ref List<double> distanceArray, int elementIndex)
            {
                // update the count
                count++;

                // as long as its parent has a larger value and have not hit the root
                int indexIterator = pointers[elementIndex];
                while (indexIterator > 1 && distanceArray[distances[indexIterator / 2]] > distanceArray[distances[indexIterator]])
                {
                    // swap the two nodes
                    int temp = distances[indexIterator / 2];
                    distances[indexIterator / 2] = distances[indexIterator];
                    distances[indexIterator] = temp;

                    // update the pointers array
                    pointers[distances[indexIterator / 2]] = indexIterator / 2;
                    pointers[distances[indexIterator]] = indexIterator;

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
