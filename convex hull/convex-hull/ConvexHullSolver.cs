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
        * Helper function to connect a group of 2 or three points using lines
        */
        public void connectPoints(List<System.Drawing.PointF> pointList)
        {
            Pen redPen = new Pen(Color.Red, 3);
            foreach (PointF firstIter in pointList)
            {
                foreach (PointF secondIter in pointList)
                {
                    if (firstIter != secondIter)
                    {
                        g.DrawLine(redPen, firstIter, secondIter);
                    }
                }
            }
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
        void findUpperCommonTangent(List<System.Drawing.PointF> list_first_half,
                                    List<System.Drawing.PointF> list_second_half)
        {
            bool upperTangentToBoth = false;
            bool upperTangentToLeft = false;
            bool upperTangentToRight = false;
            bool iteratedOnLeft = false;
            bool iteratedOnRight = false;
            PointF rightMostIter = list_first_half[0];
            PointF leftMostIter = list_second_half[0];
            PointF upperLeft = rightMostIter;
            PointF upperRight = leftMostIter;
            int indexOnLeftHullList = 0; // where 0 is the right most point
            int indexOnRightHullList = 0; // where 0 is the left most point
            while (!upperTangentToBoth)
            {
                iteratedOnLeft = false;
                iteratedOnRight = false;
                double oldSlope = computeSlope(rightMostIter, leftMostIter);
                while (!upperTangentToLeft && indexOnLeftHullList < list_first_half.Count-1)
                {
                    // Move counter clock wise on left hull
                    indexOnLeftHullList++;
                    double newSlope = computeSlope(list_first_half[indexOnLeftHullList], leftMostIter);
                    if (newSlope - oldSlope < 0) // slope is decreasing
                    {
                        iteratedOnLeft = true;
                        if (list_first_half.Count > 2)
                        {
                            // erase the line between the two points
                            Pen whitePen = new Pen(Color.White, 3);
                            g.DrawLine(whitePen, upperLeft, list_first_half[indexOnLeftHullList]);
                        }

                        // Move the iterator to the new point
                        upperLeft = list_first_half[indexOnLeftHullList];
                        oldSlope = newSlope;
                    }
                    else // we found the top point on left hull
                    {
                        break;
                    }
                }
                upperTangentToLeft = true;
                oldSlope = computeSlope(upperRight, rightMostIter);
                while (!upperTangentToRight && indexOnRightHullList < list_second_half.Count-1)
                {
                    // Move Clock wise on right hull
                    indexOnRightHullList++;
                    double newSlope = computeSlope(list_second_half[indexOnRightHullList], rightMostIter);
                    if (newSlope - oldSlope > 0) // slope is increasing
                    {
                        iteratedOnRight = true;
                        if (list_second_half.Count > 2)
                        {
                            // erase the line between the two points
                            Pen whitePen = new Pen(Color.White, 3);
                            g.DrawLine(whitePen, upperRight, list_second_half[indexOnRightHullList]);
                        }
                        // Move the iterator to the new point
                        upperRight = list_second_half[indexOnRightHullList];
                        oldSlope = newSlope;
                    }
                    else // we found the top point on right hull
                    {
                        break;
                    }
                }
                upperTangentToRight = true;
                if (upperTangentToLeft && upperTangentToRight && !iteratedOnLeft && !iteratedOnRight) // We have found the upper tangent
                {
                    Console.WriteLine("left upper is: " + indexOnLeftHullList + " and right upper is: " + indexOnRightHullList);
                    upperTangentToBoth = true;
                    Pen redPen = new Pen(Color.Red, 3);
                    g.DrawLine(redPen, upperRight, upperLeft);
                }
            }
        }

        /**
       * This function is to find the lower common tangent
       */
        void findLowerCommonTangent(List<System.Drawing.PointF> list_first_half,
                                    List<System.Drawing.PointF> list_second_half)
        {
            bool lowerTangentToBoth = false;
            bool lowerTangentToLeft = false;
            bool lowerTangentToRight = false;
            bool iteratedOnLeft = false;
            bool iteratedOnRight = false;
            PointF rightMostIter = list_first_half[0];
            PointF leftMostIter = list_second_half[0];
            PointF lowerLeft = rightMostIter;
            PointF lowerRight = leftMostIter;
            int indexOnLeftHullList = 0; // where 0 is the right most point
            int indexOnRightHullList = 0; // where 0 is the left most point
            while (!lowerTangentToBoth)
            {
                iteratedOnRight = false;
                iteratedOnLeft = false;
                double oldSlope = computeSlope(lowerLeft, leftMostIter);
                while (!lowerTangentToLeft && indexOnLeftHullList < list_first_half.Count-1)
                {
                    // Move clock wise on left hull
                    indexOnLeftHullList++;
                    double newSlope = computeSlope(list_first_half[indexOnLeftHullList], leftMostIter);
                    if (newSlope - oldSlope > 0) // slope is increasing
                    {
                        iteratedOnLeft = true;
                        if (list_first_half.Count > 2)
                        {
                            // erase the line between the two points
                            Pen whitePen = new Pen(Color.White, 3);
                            g.DrawLine(whitePen, lowerLeft, list_first_half[indexOnLeftHullList]);
                        }

                        // Move the iterator to the new point
                        lowerLeft = list_first_half[indexOnLeftHullList];
                        oldSlope = newSlope;
                    }
                    else // we found the top point on left hull
                    {
                        break;
                    }
                }
                lowerTangentToLeft = true;
                oldSlope = computeSlope(lowerRight, rightMostIter);
                while (!lowerTangentToRight && indexOnRightHullList < list_second_half.Count-1)
                {
                    // Move Clock wise on right hull
                    indexOnRightHullList++;
                    double newSlope = computeSlope(list_second_half[indexOnRightHullList], rightMostIter);
                    if (newSlope - oldSlope < 0) // slope is decreasing
                    {
                        iteratedOnRight = true;
                        if (list_second_half.Count > 2)
                        {
                            // erase the line between the two points
                            Pen whitePen = new Pen(Color.White, 3);
                            g.DrawLine(whitePen, lowerRight, list_second_half[indexOnRightHullList]);
                        }

                        // Move the iterator to the new point
                        lowerRight = list_second_half[indexOnRightHullList];
                        oldSlope = newSlope;
                    }
                    else // we found the top point on right hull
                    {
                        break;
                    }
                }
                lowerTangentToRight = true;
                if (lowerTangentToLeft && lowerTangentToRight && !iteratedOnLeft && !iteratedOnRight) // We have found the upper tangent
                {
                    lowerTangentToBoth = true;
                    Pen redPen = new Pen(Color.Red, 3);
                    g.DrawLine(redPen, lowerLeft, lowerRight);
                }
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////// Functions Responsible For Sorting //////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /**
        * This function is to order the lists in a way to navigate up
        */
        void orderListsToNavigateUp(PointF leftMostIter, PointF rightMostIter, 
                                   ref List<System.Drawing.PointF> firstList, ref List<System.Drawing.PointF> secondList)
        {
            //order the first half of the list in counter clock wise order --> increasing slope from rightmost
            firstList.Sort(delegate (PointF first, PointF second)
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

            // order the second half of the list in clock wise order --> decreasing slope from leftmost
            secondList.Sort(delegate (PointF first, PointF second)
            {
                if (first == leftMostIter) return -1;
                else if (second == leftMostIter) return 1;
                else
                {
                    double firstSlope = computeSlope(leftMostIter, first);
                    double secondSlope = computeSlope(leftMostIter, second);
                    return firstSlope.CompareTo(secondSlope);
                }
            });
        }

        /**
        * This function is to order the lists in a way to navigate down
        */
        void orderListsToNavigateDown(PointF leftMostIter, PointF rightMostIter,
                                   ref List<System.Drawing.PointF> firstList, ref List<System.Drawing.PointF> secondList)
        {
            // order the first half of the list in clock wise order --> decreasing slope from rightmost
            firstList.Sort(delegate (PointF first, PointF second)
            {
                if (first == rightMostIter) return -1;
                else if (second == rightMostIter) return 1;
                else
                {
                    double firstSlope = computeSlope(rightMostIter, first);
                    double secondSlope = computeSlope(rightMostIter, second);
                    return firstSlope.CompareTo(secondSlope);
                }
            });

            // order the second half of the list in clock wise order --> increasing slope from leftmost
            secondList.Sort(delegate (PointF first, PointF second)
            {
                if (first == leftMostIter) return -1;
                else if (second == leftMostIter) return 1;
                else
                {
                    double firstSlope = computeSlope(leftMostIter, first);
                    double secondSlope = computeSlope(leftMostIter, second);
                    return secondSlope.CompareTo(firstSlope);
                }
            });
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////// Main Divide and Conquer Algorithm //////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /**
        * Recursive function that uses divide and conquer approach to draw a convex hull around a specific list of points
        */
        public List<System.Drawing.PointF> drawPoly(List<System.Drawing.PointF> pointList)
        {
            //////////////////////////////// Base Case ///////////////////////////////////////////////////////
            if (pointList.Count < 4)
            {
                // Connect all the points to form the simplest convex hull
                connectPoints(pointList);
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


                // Find the upper common tangent
                PointF rightMostIter = list_first_half[indexOfRightMost];
                PointF leftMostIter = list_second_half[indexOfLeftMost];

                // find the upper common tangent and draw that line
                orderListsToNavigateUp(leftMostIter, rightMostIter, ref list_first_half, ref list_second_half);
                findUpperCommonTangent(list_first_half, list_second_half);

                // find the lower common tangent and draw that line
                orderListsToNavigateDown(leftMostIter, rightMostIter, ref list_first_half, ref list_second_half);
                findLowerCommonTangent(list_first_half, list_second_half);

                return pointList;


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

            drawPoly(pointList);
        }
    }
}
