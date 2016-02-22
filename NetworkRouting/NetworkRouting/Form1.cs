using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////// Priority Queue ////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public abstract class PriorityQueue
        {
            private int count;

            public PriorityQueue(){}

            public abstract void makeQueue(int numOfNodes);

            public abstract int deleteMin();

            public abstract void decreaseKey(int targetIndex, double newKey);

            public abstract void insert(int elementIndex, double value);

            public abstract double getDistance(int index);

            public int getCount()
            {
                return count;
            }

        }

        public class PriorityQueueArray : PriorityQueue
        {
            private double[] queue;
            private int count;

            public PriorityQueueArray()
            {
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
                for (int i = 0; i < count; i++)
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
                if (queue[elementIndex] == int.MaxValue)
                {
                    queue[elementIndex] = value;
                    count++;
                }
            }

            public override double getDistance(int index)
            {
                return queue[index];
            }

        }

        //public class PriorityQueueHeap : PriorityQueue
        //{
        //    public PriorityQueueHeap()
        //    {
        //    }

        //    public override void makeQueue(int numOfNodes)
        //    {

        //    }

        //    public override int deleteMin()
        //    {

        //    }

        //    public override void decreaseKey(PointF target, double newKey)
        //    {

        //    }

        //    public override void insert(int elementIndex, double value)
        //    {

        //    }
        //}

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
        * Helper function to draw the path between the list of points
        */
        private void drawPath(List<int> path)
        {
            // Create variables to iterate through the path
            int currIndex = stopNodeIndex;
            int prevIndex = currIndex;
            // Keep looping until the path from start node to end node is drawn
            while (true)
            {
                currIndex = path[currIndex];
                if (currIndex == -1) // if hit start node, exit cause the path is done
                    break;
                // Draw line and label it with the distance
                Pen black = new Pen(Color.Black, 3);
                graphics.DrawLine(black, points[currIndex], points[prevIndex]);
                string drawString = System.Convert.ToString(computeDistance(points[currIndex], points[prevIndex]));
                System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 16);
                System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                float x = points[currIndex].X - points[prevIndex].X;
                float y = points[currIndex].Y - points[prevIndex].Y;
                graphics.DrawString(drawString, drawFont, drawBrush, x, y);

                // Update the iterator
                prevIndex = currIndex;
            }
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
        private List<int> Dijkstras()
        {
            // Create Queue to track order of points
            PriorityQueue queue = new PriorityQueueArray();
            queue.makeQueue(points.Count);

            // Set up prev node list
            List<int> prev = new List<int>();
            for (int i = 0; i < points.Count; i++)
            {
                prev.Add(-1);
            }

            // Initilize the start node distance to 0
            queue.insert(startNodeIndex, 0);

            // Iterate while the queue is not empty
            while (queue.getCount() != 0)
            {
                // Grab the next min cost Point
                int indexOfMin = queue.deleteMin();
                PointF u = points[indexOfMin];

                // For all edges coming out of u
                foreach (int targetIndex in adjacencyList[indexOfMin])
                {
                    PointF target = points[targetIndex];
                    double newDist = queue.getDistance(indexOfMin) + computeDistance(u, target);
                    if (queue.getDistance(targetIndex) > newDist)
                    {
                        prev[targetIndex] = indexOfMin;
                        queue.decreaseKey(targetIndex, newDist);
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
            // *** Implement this method, use the variables "startNodeIndex" and "stopNodeIndex" as the indices for your start and stop points, respectively ***
            List<int> path = Dijkstras();

            drawPath(path);
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
