/******************************************************************************
 *
 * Copyright 2019 Tim Brogden
 * All rights reserved. This program and the accompanying materials   
 * are made available under the terms of the Eclipse Public License v1.0  
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *           
 * Contributors: 
 * Tim Brogden - Keysticks application and installer
 *
 *****************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
//using System.Diagnostics;

namespace Keysticks.Core
{
    /// <summary>
    /// Geometry utility methods
    /// </summary>
    public class GeometryUtils
    {
        public GeometryUtils()        
        {            
        }        

        /// <summary>
        /// Calculate a simple bounding box from the max and min X and Y coords of the points.
        /// </summary>
        /// <param name="pointsList"></param>
        /// <returns></returns>
        public Rect CalcSimpleBoundingRect(Point[] pointsList)
        {
            double minX = 0.0;
            double maxX = 0.0;
            double minY = 0.0;
            double maxY = 0.0;
            if (pointsList.Length > 0)
            {
                minX = maxX = pointsList[0].X;
                minY = maxY = pointsList[0].Y;
                foreach (Point point in pointsList)
                {
                    minX = Math.Min(point.X, minX);
                    maxX = Math.Max(point.X, maxX);
                    minY = Math.Min(point.Y, minY);
                    maxY = Math.Max(point.Y, maxY);
                }
            }

            return new Rect(new Point(minX, minY), new Point(maxX, maxY));
        }

        /// <summary>
        /// Calculate a convex hull for a set of points
        /// </summary>
        /// <param name="pointsList"></param>
        /// <returns></returns>
        public List<Point> CalculateConvexHull(Point[] pointsList)
        {
            List<Point> vertices = new List<Point>();
            if (pointsList.Length > 0)
            {
                // Get the left-most vertex
                double minX = pointsList[0].X;
                int currentIndex = 0;
                for (int i=1; i<pointsList.Length; i++)
                {
                    if (pointsList[i].X < minX)
                    {
                        minX = pointsList[i].X;
                        currentIndex = i;
                    }
                }

                bool[] isHull = new bool[pointsList.Length];
                Segment seg = new Segment();
                seg.P = pointsList[currentIndex];
                vertices.Add(seg.P);
                isHull[currentIndex] = true;

                // Repeatedly find the next vertex until we're back to the start
                bool matched;
                do
                {
                    // Test all segments from the current vertex, except the one leading back the way we came
                    matched = false;
                    for (int i = 0; i < pointsList.Length; i++)
                    {
                        if (!isHull[i])
                        {
                            seg.Q = pointsList[i];
                            if (IsEdge(pointsList, seg))
                            {
                                currentIndex = i;
                                seg.P = pointsList[currentIndex];
                                vertices.Add(seg.P);
                                isHull[currentIndex] = true;
                                matched = true;
                                break;
                            }
                        }
                    }                    
                }
                while (matched);

                // For testing
                /*
                if (pointsList.Length > 100)
                {
                    Rect simpleBox = CalcSimpleBoundingRect(pointsList);
                    char[,] visual = new char[(int)simpleBox.Width + 1, (int)simpleBox.Height + 1];
                    for (int k = 0; k < pointsList.Length; k++)
                    {
                        Point point = pointsList[k];
                        int x = (int)(point.X - simpleBox.X);
                        int y = (int)(point.Y - simpleBox.Y);
                        visual[x, y] = 'x';
                    }
                    for (int k = 0; k < vertices.Count; k++)
                    {
                        Point point = vertices[k];
                        int x = (int)(point.X - simpleBox.X);
                        int y = (int)(point.Y - simpleBox.Y);
                        visual[x, y] = 'o';
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int j = 0; j < visual.GetLength(1); j++)
                    {
                        // j-th row
                        for (int i = 0; i < visual.GetLength(0); i++)
                        {
                            // i-th column
                            if (visual[i, j] == 0)
                            {
                                sb.Append(' ');
                            }
                            else
                            {
                                sb.Append(visual[i, j]);
                            }
                        }
                        sb.AppendLine();
                    }
                    Trace.WriteLine(sb.ToString());
                }*/
            }

            return vertices;
        }

        #region Functions
        
        /// <summary>
        /// Determines whether a segment (two points) form an edge of the convex hull
        /// </summary>
        /// <param name="pointsList"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        private bool IsEdge(Point[] pointsList, Segment segment)
        {
            for (int k = 0; k < pointsList.Length; k++)
            {
                if (!segment.Contains(pointsList[k]) && IsLeft(segment, pointsList[k]))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsLeft(Segment segment, Point r)
        {
            double D;
            double px, py, qx, qy, rx, ry;
            // If the determinant
            // | 1 px py |
            // | 1 qx qy |
            // | 1 rx ry |
            // is positive then the point is left of the segment
            px = segment.P.X;
            py = segment.P.Y;
            qx = segment.Q.X;
            qy = segment.Q.Y;
            rx = r.X;
            ry = r.Y;

            D = ((qx * ry) - (qy * rx)) - (px * (ry - qy)) + (py * (rx - qx));

            return (D > 0);
        }        

        #endregion
    }

    #region Structures

    public struct Segment
    {
        public Point P;
        public Point Q;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        public Segment(Point p, Point q)
        {
            P = p;
            Q = q;
        }

        public bool Contains(Point point)
        {
            if (P.Equals(point) || Q.Equals(point))
                return true;
            return false;
        }

        public override string ToString()
        {
            return string.Format("Segment P: {0}, Q {1}", P, Q);
        }
    }
    
    #endregion    
}
