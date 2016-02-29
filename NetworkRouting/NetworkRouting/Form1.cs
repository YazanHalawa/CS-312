using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace NetworkRouting
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void clearAll()
        {
            startNodeIndex = -1;
            stopNodeIndex = -1;
            sourceNodeBox.Clear();
            sourceNodeBox.Refresh();
            targetNodeBox.Clear();
            targetNodeBox.Refresh();
            arrayTimeBox.Clear();
            arrayTimeBox.Refresh();
            heapTimeBox.Clear();
            heapTimeBox.Refresh();
            differenceBox.Clear();
            differenceBox.Refresh();
            pathCostBox.Clear();
            pathCostBox.Refresh();
            arrayCheckBox.Checked = false;
            arrayCheckBox.Refresh();
            return;
        }

        private void clearSome()
        {
            arrayTimeBox.Clear();
            arrayTimeBox.Refresh();
            heapTimeBox.Clear();
            heapTimeBox.Refresh();
            differenceBox.Clear();
            differenceBox.Refresh();
            pathCostBox.Clear();
            pathCostBox.Refresh();
            return;
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            int randomSeed = int.Parse(randomSeedBox.Text);
            int size = int.Parse(sizeBox.Text);

            Random rand = new Random(randomSeed);
            seedUsedLabel.Text = "Random Seed Used: " + randomSeed.ToString();

            clearAll();
            this.adjacencyList = generateAdjacencyList(size, rand);
            List<PointF> points = generatePoints(size, rand);
            resetImageToPoints(points);
            this.points = points;
        }

        // Generates the distance matrix.  Values of -1 indicate a missing edge.  Loopbacks are at a cost of 0.
        private const int MIN_WEIGHT = 1;
        private const int MAX_WEIGHT = 100;
        private const double PROBABILITY_OF_DELETION = 0.35;

        private const int NUMBER_OF_ADJACENT_POINTS = 3;

        private List<HashSet<int>> generateAdjacencyList(int size, Random rand)
        {
            List<HashSet<int>> adjacencyList = new List<HashSet<int>>();

            for (int i = 0; i < size; i++)
            {
                HashSet<int> adjacentPoints = new HashSet<int>();
                while (adjacentPoints.Count < 3)
                {
                    int point = rand.Next(size);
                    if (point != i) adjacentPoints.Add(point);
                }
                adjacencyList.Add(adjacentPoints);
            }

            return adjacencyList;
        }

        private List<PointF> generatePoints(int size, Random rand)
        {
            List<PointF> points = new List<PointF>();
            for (int i = 0; i < size; i++)
            {
                points.Add(new PointF((float) (rand.NextDouble() * pictureBox.Width), (float) (rand.NextDouble() * pictureBox.Height)));
            }
            return points;
        }

        private void resetImageToPoints(List<PointF> points)
        {
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            Graphics graphics = Graphics.FromImage(pictureBox.Image);
            Pen pen;

            if (points.Count < 100)
                pen = new Pen(Color.Blue);
            else
                pen = new Pen(Color.LightBlue);
            foreach (PointF point in points)
            {
                graphics.DrawEllipse(pen, point.X, point.Y, 2, 2);
            }

            this.graphics = graphics;
            pictureBox.Invalidate();
        }

        // These variables are instantiated after the "Generate" button is clicked
        private List<PointF> points = new List<PointF>();
        private Graphics graphics;
        private List<HashSet<int>> adjacencyList;

        // Use this to generate paths (from start) to every node; then, just return the path of interest from start node to end node
        private void solveButton_Click(object sender, EventArgs e)
        {
            // This was the old entry point, but now it is just some form interface handling
            bool ready = true;

            if(startNodeIndex == -1)
            {
                sourceNodeBox.Focus();
                sourceNodeBox.BackColor = Color.Red;
                ready = false;
            }
            if(stopNodeIndex == -1)
            {
                if(!sourceNodeBox.Focused)
                    targetNodeBox.Focus();
                targetNodeBox.BackColor = Color.Red;
                ready = false;
            }
            if (points.Count > 0)
            {
                resetImageToPoints(points);
                paintStartStopPoints();
            }
            else
            {
                ready = false;
            }
            if(ready)
            {
                clearSome();
                solveButton_Clicked();  // Here is the new entry point
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////// Priority Queue ////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public abstract class PriorityQueue
        {
            public PriorityQueue() { }

            public abstract void makeQueue(int numOfNodes);

            public virtual int deleteMin() { return 0; }

            public virtual int deleteMin(ref List<double> distanceArray) { return 0; }

            public virtual void decreaseKey(int targetIndex, double newKey) { }

            public virtual void decreaseKey(ref List<double> distanceArray, int targetIndex) { }

            public virtual void insert(int elementIndex, double value) { }

            public virtual void insert(ref List<double> distanceArray, int elementIndex) { }

            public abstract void printQueueContents();

            public abstract bool isEmpty();

        }

        public class PriorityQueueArray : PriorityQueue
        {
            private double[] queue;
            private int count;

            public PriorityQueueArray()
            {
            }

            public override bool isEmpty()
            {
                return count == 0;
            }

            public override void printQueueContents()
            {
                Console.Write("The contents of the queue are: ");
                for (int i = 0; i < count; i++)
                {
                    Console.Write(queue[i] + " ");
                }
                Console.WriteLine();
            }

            public override void makeQueue(int numOfNodes)
            {
                queue = new double[numOfNodes];
                for (int i = 0; i < numOfNodes; i++)
                {
                    queue[i] = int.MaxValue;
                }
                count = numOfNodes;
            }

            public override int deleteMin()
            {
                double min = int.MaxValue;
                int minIndex = 0;
                for (int i = 0; i < queue.Count(); i++)
                {
                    if (queue[i] < min)
                    {
                        min = queue[i];
                        minIndex = i;
                    }
                }
                count--;
                queue[minIndex] = int.MaxValue;
                return minIndex;
            }

            public override void decreaseKey(int targetIndex, double newKey)
            {
                queue[targetIndex] = newKey;
            }

            public override void insert(int elementIndex, double value)
            {
                queue[elementIndex] = value;
                count++;
            }
        }

        public class PriorityQueueHeap : PriorityQueue
        {
            private int capacity;
            private int count;
            private int lastElement;
            private int[] distances;
            private int[] pointers;
            public PriorityQueueHeap()
            {
            }

            public override bool isEmpty()
            {
                return count == 0;
            }

            public override void printQueueContents()
            {
                Console.Write("The contents of the queue are: ");
                for (int i = 1; i < capacity; i++)
                {
                    if (distances[i] != -1)
                        Console.Write(distances[i] + " ");
                }
                Console.WriteLine();
            }

            public override void makeQueue(int numOfNodes)
            {
                distances = new int[numOfNodes+1];
                pointers = new int[numOfNodes];
                for (int i = 1; i < numOfNodes + 1; i++)
                {
                    distances[i] = i-1;
                    pointers[i - 1] = i;
                }
                capacity = numOfNodes;
                count = 0;
                lastElement = capacity;
            }

            public override int deleteMin(ref List<double> distanceArray)
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
                    int smallerElementIndex = 2*indexIterator;

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

            public override void decreaseKey(ref List<double> distanceArray, int targetIndex)
            {
                // find the node with the old value
                int indexToHeap = pointers[targetIndex];
                count++;

                // reorder the heap by bubbling up the min to top
                int indexIterator = indexToHeap;
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
 
            public override void insert(ref List<double> distanceArray, int elementIndex)
            {
                // update the count
                count++;

                // as long as its parent has a larger value and have not hit the root
                int indexIterator = pointers[elementIndex];
                while (indexIterator > 1 && distanceArray[distances[indexIterator/2]] > distanceArray[distances[indexIterator]])
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////// Helper Functions /////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /**
        * Helper function to compute distance between two points
        */
        private double computeDistance(PointF point1, PointF point2)
        {
            double deltaX = Math.Pow(point2.X - point1.X, 2);
            double deltaY = Math.Pow(point2.Y - point1.Y, 2);
            return Math.Sqrt(deltaX + deltaY);
        }

        /**
        * Helper function to calculate the midpoint between two points
        */
        private PointF findMidPoint(int firstIndex, int secondIndex)
        {
            PointF midPoint = new PointF();
            midPoint.X = (points[secondIndex].X + points[firstIndex].X) / 2;
            midPoint.Y = (points[secondIndex].Y + points[firstIndex].Y) / 2;
            return midPoint;
        }

        /**
        * Helper function to draw the path between the list of points
        */
        private void drawPath(List<int> path, bool isArray)
        {
            // Create variables to iterate through the path
            int currIndex = stopNodeIndex;
            int prevIndex = currIndex;
            double totalPathCost = 0;
            // Keep looping until the path from start node to end node is drawn
            while (true)
            {
                currIndex = path[currIndex];
                if (currIndex == -1) // if hit start node, exit cause the path is done
                    break;
                // Draw line
                Pen pen;
                if (isArray)
                    pen = new Pen(Color.Black, 2);
                else
                    pen = new Pen(Color.Red, 5);
                graphics.DrawLine(pen, points[currIndex], points[prevIndex]);
                
                // Label it with the distance
                double distance = computeDistance(points[currIndex], points[prevIndex]);
                totalPathCost += distance;
                graphics.DrawString(String.Format("{0}", (int)distance), SystemFonts.DefaultFont, Brushes.Black, findMidPoint(prevIndex, currIndex));

                // Update the iterator
                prevIndex = currIndex;
            }
            pathCostBox.Text = String.Format("{0}", totalPathCost);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////  Dijktra's Algorithm ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /**
        * This function will implement Dijkstra's Algorithm
        */
        private List<int> Dijkstras(ref PriorityQueue queue, bool isArray)
        {
            // Create Queue to track order of points
            queue.makeQueue(points.Count);
            // Set up prev node list
            List<int> prev = new List<int>();
            List<double> dist = new List<double>();
            for (int i = 0; i < points.Count; i++)
            {
                prev.Add(-1);
                dist.Add(double.MaxValue);
            }

            // Initilize the start node distance to 0
            dist[startNodeIndex] = 0;
            
            // Update Priority Queue to reflect change in start point distance
            if (isArray)
                queue.insert(startNodeIndex, 0);
            else
                queue.insert(ref dist, startNodeIndex);

            // Iterate while the queue is not empty
            while (!queue.isEmpty())
            {
                // Grab the next min cost Point
                int indexOfMin;
                if (isArray)
                    indexOfMin = queue.deleteMin();
                else
                    indexOfMin = queue.deleteMin(ref dist);

                PointF u = points[indexOfMin];
                
                // For all edges coming out of u
                foreach (int targetIndex in adjacencyList[indexOfMin])
                {
                    PointF target = points[targetIndex];
                    double newDist = dist[indexOfMin] + computeDistance(u, target);
                    if (dist[targetIndex] > newDist)
                    {
                        prev[targetIndex] = indexOfMin;
                        dist[targetIndex] = newDist;
                        if (isArray)
                            queue.decreaseKey(targetIndex, newDist);
                        else
                            queue.decreaseKey(ref dist, targetIndex);
                    }
                }
            }
            return prev;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void solveButton_Clicked()
        {
            // As default, solve the problem using the heap
            PriorityQueue queue = new PriorityQueueHeap();
            Stopwatch watch = Stopwatch.StartNew();
            List<int> pathHeap = Dijkstras(ref queue, false);
            watch.Stop();

            // Calculate time for heap
            double heapTime = (double)watch.ElapsedMilliseconds / 1000;
            heapTimeBox.Text = String.Format("{0}", heapTime);

            List<int> pathArray = new List<int>();
            // Now If the Array box is checked, solve it again using an array, and compare answers
            if (arrayCheckBox.Checked)
            {
                PriorityQueue queueArray = new PriorityQueueArray();
                watch = Stopwatch.StartNew();
                pathArray = Dijkstras(ref queueArray, true);
                watch.Stop();

                // Calculate time for array
                double arrayTime = (double)watch.ElapsedMilliseconds / 1000;
                arrayTimeBox.Text = String.Format("{0}", arrayTime);
                differenceBox.Text = String.Format("{0}", (arrayTime - heapTime)/heapTime);
                
                // check if the two paths are the same
                for (int i = 0; i < pathArray.Count(); i++)
                {
                    if (pathArray[i] != pathHeap[i])
                        Console.WriteLine("At index " + i + " pathArray was :" + pathArray[i] + " and pathHeap was " + pathHeap[i]);
                }
            }
            
            // Draw the final minimum cost path
            drawPath(pathHeap, false);
            drawPath(pathArray, true);
        }

        private Boolean startStopToggle = true;
        private int startNodeIndex = -1;
        private int stopNodeIndex = -1;
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (points.Count > 0)
            {
                Point mouseDownLocation = new Point(e.X, e.Y);
                int index = ClosestPoint(points, mouseDownLocation);
                if (startStopToggle)
                {
                    startNodeIndex = index;
                    sourceNodeBox.ResetBackColor();
                    sourceNodeBox.Text = "" + index;
                }
                else
                {
                    stopNodeIndex = index;
                    targetNodeBox.ResetBackColor();
                    targetNodeBox.Text = "" + index;
                }
                resetImageToPoints(points);
                paintStartStopPoints();
            }
        }

        private void sourceNodeBox_Changed(object sender, EventArgs e)
        {
            if (points.Count > 0)
            {
                try{ startNodeIndex = int.Parse(sourceNodeBox.Text); }
                catch { startNodeIndex = -1; }
                if (startNodeIndex < 0 | startNodeIndex > points.Count-1)
                    startNodeIndex = -1;
                if(startNodeIndex != -1)
                {
                    sourceNodeBox.ResetBackColor();
                    resetImageToPoints(points);
                    paintStartStopPoints();
                    startStopToggle = !startStopToggle;
                }
            }
        }

        private void targetNodeBox_Changed(object sender, EventArgs e)
        {
            if (points.Count > 0)
            {
                try { stopNodeIndex = int.Parse(targetNodeBox.Text); }
                catch { stopNodeIndex = -1; }
                if (stopNodeIndex < 0 | stopNodeIndex > points.Count-1)
                    stopNodeIndex = -1;
                if(stopNodeIndex != -1)
                {
                    targetNodeBox.ResetBackColor();
                    resetImageToPoints(points);
                    paintStartStopPoints();
                    startStopToggle = !startStopToggle;
                }
            }
        }
        
        private void paintStartStopPoints()
        {
            if (startNodeIndex > -1)
            {
                Graphics graphics = Graphics.FromImage(pictureBox.Image);
                graphics.DrawEllipse(new Pen(Color.Green, 6), points[startNodeIndex].X, points[startNodeIndex].Y, 1, 1);
                this.graphics = graphics;
                pictureBox.Invalidate();
            }

            if (stopNodeIndex > -1)
            {
                Graphics graphics = Graphics.FromImage(pictureBox.Image);
                graphics.DrawEllipse(new Pen(Color.Red, 2), points[stopNodeIndex].X - 3, points[stopNodeIndex].Y - 3, 8, 8);
                this.graphics = graphics;
                pictureBox.Invalidate();
            }
        }

        private int ClosestPoint(List<PointF> points, Point mouseDownLocation)
        {
            double minDist = double.MaxValue;
            int minIndex = 0;

            for (int i = 0; i < points.Count; i++)
            {
                double dist = Math.Sqrt(Math.Pow(points[i].X-mouseDownLocation.X,2) + Math.Pow(points[i].Y - mouseDownLocation.Y,2));
                if (dist < minDist)
                {
                    minIndex = i;
                    minDist = dist;
                }
            }

            return minIndex;
        }
    }
}
