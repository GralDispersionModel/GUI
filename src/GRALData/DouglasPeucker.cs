using System;
using System.Collections;
using System.Collections.Generic;

namespace GralData
{
    /// <summary>
    /// Douglas Peucker point reduction of a List of PointD values based on the value epsilon
    /// </summary>
    public class DouglasPeucker
    {
        public void DouglasPeuckerRun(List<GralDomain.PointD> points, double epsilon)
        {
            if (epsilon < 0.05)
            {
                return;
            }

            var bitArray = DouglasPeuckerDo(points, epsilon);

            int points_removed = 0;


            // Remove all not needed points 
            int count = points.Count;
            for (int i = 0; i < count - 1; i++)
            {
                if (bitArray[i] == false && i > 0)
                {
                    points.RemoveAt(i - points_removed);
                    points_removed++;
                }
            }
        }

        private BitArray DouglasPeuckerDo(List<GralDomain.PointD> points, double epsilon)
        {
            // non iterative Douglas-Peucker algorithm using a stack & a bitarray instead

            int startindex = 0;
            int lastindex = points.Count - 1;

            var stk = new Stack<KeyValuePair<int, int>>();
            stk.Push(new KeyValuePair<int, int>(startindex, lastindex));

            var bitArray = new BitArray(lastindex + 1, true);

            while (stk.Count > 0)
            {
                startindex = stk.Peek().Key;
                lastindex = stk.Peek().Value;
                stk.Pop();

                double dmax = 0;
                int index = startindex;

                for (int i = index + 1; i < lastindex; i++)
                {
                    if (bitArray[i])
                    {
                        double d = PointLineDistance(points[i], points[startindex], points[lastindex]);
                        if (d > dmax)
                        {
                            index = i;
                            dmax = d;
                        }
                    }
                }

                if (dmax > epsilon)
                {
                    stk.Push(new KeyValuePair<int, int>(startindex, index));
                    stk.Push(new KeyValuePair<int, int>(index, lastindex));
                }
                else
                {
                    for (int i = startindex + 1; i < lastindex; ++i)
                    {
                        bitArray[i] = false;
                    }
                }
            }
            return bitArray;
        }

        /// <summary>
        /// Shortest distance between a point and the line start - end 
        /// </summary>
        private double PointLineDistance(GralDomain.PointD point, GralDomain.PointD start, GralDomain.PointD end)
        {
            if (start == end)
            {
                return Math.Sqrt(Math.Pow(point.X - start.X, 2) + Math.Pow(point.Y - start.Y, 2));
            }

            double dx = end.X - start.X;
            double dy = end.Y - start.Y;

            //normalize
            double mag = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            if (mag > 0)
            {
                dx /= mag;
                dy /= mag;
            }

            double pvx = point.X - start.X;
            double pvy = point.Y - start.Y;

            // dot product
            double pvdot = dx * pvx + dy * pvy;

            //scale line dir vector
            double dsx = pvdot * dx;
            double dsy = pvdot * dy;

            // subtract from pv
            double ax = pvx - dsx;
            double ay = pvy - dsy;

            return Math.Sqrt(Math.Pow(ax, 2) + Math.Pow(ay, 2));
        }

    }
}
