using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace _2_convex_hull
{
    class ConvexHullSolver
    {
        System.Drawing.Graphics g;
        System.Windows.Forms.PictureBox pictureBoxView;

        public ConvexHullSolver(System.Drawing.Graphics g, System.Windows.Forms.PictureBox pictureBoxView)
        {
            this.g = g;
            this.pictureBoxView = pictureBoxView;
        }

        public void Refresh()
        {
            // Use this especially for debugging and whenever you want to see what you have drawn so far
            pictureBoxView.Refresh();
        }

        public void Pause(int milliseconds)
        {
            // Use this especially for debugging and to animate your algorithm slowly
            pictureBoxView.Refresh();
            System.Threading.Thread.Sleep(milliseconds);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////// Helper Functions ///////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        /**
        * Helper function to create a list of 2 or 3 points in counter clock wise order
        */
        public void connectPoints(ref List<System.Drawing.PointF> pointList)
        {
            // find the right most point
            PointF rightMostIter = pointList[locateRightMost(pointList)];

            // sort the list in counter clock wise order
            pointList.Sort(delegate (PointF first, PointF second)
            {
                if (first == rightMostIter) return -1;
                else if (second == rightMostIter) return 1;
                else
                {
                    double firstSlope = computeSlope(rightMostIter, first);
                    double secondSlope = computeSlope(rightMostIter, second);
                    return secondSlope.CompareTo(firstSlope);
                }
            });
        }
        
        /**
        * Helper function to locate the right most point in a list of points and return its index
        */
        int locateRightMost(List<System.Drawing.PointF> pointList)
        {
            int indexOfMax = 0;
            int counter = 0;
            float rightMostVal = -1000;
            foreach (PointF point in pointList)
            {
                if (point.X > rightMostVal)
                {
                    indexOfMax = counter;
                    rightMostVal = point.X;
                }
                counter++;
            }
            return indexOfMax;
        }

        /**
        * Helper function to locate the left most node in a list of points and return it
        */
        int locateLeftMost(List<System.Drawing.PointF> pointList)
        {
            int indexOfMin = 0;
            int counter = 0;
            float leftMostVal = 1000;
            foreach (PointF point in pointList)
            {
                if (point.X < leftMostVal)
                {
                    indexOfMin = counter;
                    leftMostVal = point.X;
                }
                counter++;
            }
            return indexOfMin;
        }

        /**
        * Helper function to computer slope between two points
        */
        double computeSlope(PointF first, PointF second)
        {
            double YDiff = second.Y - first.Y;
            double XDiff = second.X - first.X;
            return YDiff / XDiff;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////// Functions Responsible for Finding Tangents //////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /**
        * This function is to find the upper common tangent
        */
        void findUpperCommonTangent(ref PointF upperLeft, ref PointF upperRight, List<System.Drawing.PointF> list_first_half,
                                    List<System.Drawing.PointF> list_second_half)
        {
            // Define Flags and other variables
            bool upperTangentToBoth = false;
            bool upperTangentToLeft;
            bool upperTangentToRight;
            bool iteratedOnLeft;
            bool iteratedOnRight;
            double oldSlope;
            int indexOnLeftHullList = list_first_half.IndexOf(upperLeft);
            int indexOnRightHullList = list_second_half.IndexOf(upperRight);

            // Keep Going until you find the upper tangent
            while (!upperTangentToBoth)
            {
                // Setup the inital state of each iteration
                indexOnLeftHullList = list_first_half.IndexOf(upperLeft);
                indexOnRightHullList = list_second_half.IndexOf(upperRight);
                iteratedOnLeft = false;
                iteratedOnRight = false;
                upperTangentToLeft = false;
                upperTangentToRight = false;

                // Move counter clock wise on left hull
                oldSlope = computeSlope(upperLeft, upperRight);
                while (!upperTangentToLeft)
                {
                    indexOnLeftHullList++;
                    if (indexOnLeftHullList > list_first_half.Count - 1) // Avoid out of range
                        indexOnLeftHullList = 0;
                    double newSlope = computeSlope(list_first_half[indexOnLeftHullList], upperRight);
                    if (newSlope - oldSlope > 0) // slope is increasing
                    {
                        iteratedOnLeft = true;
                        // Move the iterator to the new point
                        upperLeft = list_first_half[indexOnLeftHullList];
                        oldSlope = newSlope;
                    }
                    else // we found the top point on left hull
                    {
                        break;
                    }
                }
                // Set the status bit of the left hull tangent to true
                upperTangentToLeft = true;

                // Move Clock wise on right hull
                oldSlope = computeSlope(upperRight, upperLeft);
                while (!upperTangentToRight)
                {

                    indexOnRightHullList--;
                    if (indexOnRightHullList < 0) // Avoid out of range
                        indexOnRightHullList = list_second_half.Count - 1;
                    double newSlope = computeSlope(list_second_half[indexOnRightHullList], upperLeft);
                    if (newSlope - oldSlope < 0) // slope is decreasing
                    {
                        iteratedOnRight = true;
                        // Move the iterator to the new point
                        upperRight = list_second_half[indexOnRightHullList];
                        oldSlope = newSlope;
                    }
                    else // we found the top point on right hull
                    {
                        break;
                    }
                }
                // Set the status bit for the right hull upper tangent to true
                upperTangentToRight = true;

                // Break when both tangents are found
                if (upperTangentToLeft && upperTangentToRight && !iteratedOnLeft && !iteratedOnRight) // We have found the upper tangent
                {
                    upperTangentToBoth = true;
                }
            }
        }

        /**
       * This function is to find the lower common tangent
       */
        void findLowerCommonTangent(ref PointF lowerLeft, ref PointF lowerRight, List<System.Drawing.PointF> list_first_half,
                                    List<System.Drawing.PointF> list_second_half)
        {
            // Create Flags to monitor the loop
            bool lowerTangentToBoth = false;
            bool lowerTangentToLeft;
            bool lowerTangentToRight;
            bool iteratedOnLeft;
            bool iteratedOnRight;
            double oldSlope;
            int indexOnLeftHullList = list_first_half.IndexOf(lowerLeft);
            int indexOnRightHullList = list_second_half.IndexOf(lowerRight);

            // Keep going until you find the lower tangent
            while (!lowerTangentToBoth)
            {
                // Setup the initial state of each iteration
                indexOnLeftHullList = list_first_half.IndexOf(lowerLeft);
                indexOnRightHullList = list_second_half.IndexOf(lowerRight);
                iteratedOnRight = false;
                iteratedOnLeft = false;
                lowerTangentToLeft = false;
                lowerTangentToRight = false;

                // Move clock wise on left hull
                oldSlope = computeSlope(lowerLeft, lowerRight);
                while (!lowerTangentToLeft)
                {
                    indexOnLeftHullList--;
                    if (indexOnLeftHullList < 0) // Avoid out of range
                        indexOnLeftHullList = list_first_half.Count - 1;
                    double newSlope = computeSlope(list_first_half[indexOnLeftHullList], lowerRight);
                    if (newSlope - oldSlope < 0) // slope is decreasing
                    {
                        iteratedOnLeft = true;

                        // Move the iterator to the new point
                        lowerLeft = list_first_half[indexOnLeftHullList];
                        oldSlope = newSlope;
                    }
                    else // we found the top point on left hull
                    {
                        break;
                    }
                }
                // Setup the state bit for the left hull lower tangent to true
                lowerTangentToLeft = true;

                // Move Clock wise on right hull
                oldSlope = computeSlope(lowerRight, lowerLeft);
                while (!lowerTangentToRight)
                {
                    indexOnRightHullList++;
                    if (indexOnRightHullList > list_second_half.Count - 1) // Avoid out of range
                        indexOnRightHullList = 0;
                    double newSlope = computeSlope(list_second_half[indexOnRightHullList], lowerLeft);
                    if (newSlope - oldSlope > 0) // slope is increasing
                    {
                        iteratedOnRight = true;
                        // Move the iterator to the new point
                        lowerRight = list_second_half[indexOnRightHullList];
                        oldSlope = newSlope;
                    }
                    else // we found the top point on right hull
                    {
                        break;
                    }
                }
                // Setup the state bit for the right hull lower tangent to true
                lowerTangentToRight = true;

                // Break when both tangents are found
                if (lowerTangentToLeft && lowerTangentToRight && !iteratedOnLeft && !iteratedOnRight) // We have found the upper tangent
                {
                    lowerTangentToBoth = true;
                }
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////// Main Divide and Conquer Algorithm ///////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /**
        * Recursive function that uses divide and conquer approach to draw a convex hull around a specific list of points
        */
        public List<System.Drawing.PointF> drawPoly(List<System.Drawing.PointF> pointList)
        {
            //////////////////////////////// Base Case ///////////////////////////////////////////////////////
            if (pointList.Count < 4)
            {
                // Connect all the points to form the simplest convex hull
                connectPoints(ref pointList);
                return pointList; 
            } 
            else
            {
                //////////////////////////////// Recursive Steps ////////////////////////////////////////////

                List<System.Drawing.PointF> list_first_half, list_second_half;
                // find the middle index of the list
                int halfOfElements = pointList.Count / 2;
                int middleIndex = halfOfElements;

                list_first_half = drawPoly(pointList.GetRange(0, halfOfElements)); // left half of points
                list_second_half = drawPoly(pointList.GetRange(middleIndex, pointList.Count - halfOfElements)); // right half of points
                
                ///////////////////////////////// Connecting the two Shapes ////////////////////////////////
                
                // first locate the right most point in the left list
                int indexOfRightMost = locateRightMost(list_first_half);

                // Next locate the left most point in the right list
                int indexOfLeftMost = locateLeftMost(list_second_half);


                // Set up some Variables
                PointF rightMostIter = list_first_half[indexOfRightMost];
                PointF leftMostIter = list_second_half[indexOfLeftMost];
                PointF upperLeft = rightMostIter;
                PointF upperRight = leftMostIter;
                PointF lowerLeft = rightMostIter;
                PointF lowerRight = leftMostIter;

                // find the upper common tangent and draw that line
                findUpperCommonTangent(ref upperLeft, ref upperRight, list_first_half, list_second_half);

                // find the lower common tangent and draw that line
                findLowerCommonTangent(ref lowerLeft, ref lowerRight, list_first_half, list_second_half);

                // Construct the new list
                List<System.Drawing.PointF> newList = new List<System.Drawing.PointF>();

                // Move Counter clock wise from upper left to lower left 
                int i = list_first_half.IndexOf(upperLeft);
                while (i != list_first_half.IndexOf(lowerLeft))
                {
                    newList.Add(list_first_half[i]);
                    i++;
                    if (i > list_first_half.Count - 1)
                        i = 0;
                }
                newList.Add(list_first_half[i]);

                // Move Counter clock wise from lower right to upper right
                i = list_second_half.IndexOf(lowerRight);
                while (i != list_second_half.IndexOf(upperRight))
                {
                    newList.Add(list_second_half[i]);
                    i++;
                    if (i > list_second_half.Count - 1)
                        i = 0;
                }
                newList.Add(list_second_half[i]);
               
                return newList;
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Solve(List<System.Drawing.PointF> pointList)
        {
            // First we sort the list of points in increasing x-values
            // Time Complexity: O(n log n)
            pointList.Sort(delegate (PointF first, PointF second)
            {
                return first.X.CompareTo(second.X);
            });

            // Acquire the list of points that form the convex hull
            List<System.Drawing.PointF> finalList = new List<System.Drawing.PointF>();
            finalList = drawPoly(pointList);

            // Draw the lines connecting the points of the convex hull
            Pen redPen = new Pen(Color.Red, 3);
            for (int i = 0; i < finalList.Count; i++)
            {
                if (i != finalList.Count - 1)
                    g.DrawLine(redPen, finalList[i], finalList[i + 1]);
                else
                    g.DrawLine(redPen, finalList[i], finalList[0]);
            }
        }
    }
}
