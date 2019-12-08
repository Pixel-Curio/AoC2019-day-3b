using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Day_3b
{
    class Program
    {
        private const string InputPath = @"day3a-input.txt";

        private static Point[] _wire1Positions;
        private static Point[] _wire2Positions;

        static void Main(string[] args)
        {
            string[] lines = File.ReadAllLines(InputPath);
            Dictionary<(int x, int y), (int wire1Id, int wire2Id)> intersections =
                new Dictionary<(int, int), (int, int)>();

            _wire1Positions = ConvertToPositions(lines[0].Split(","));
            _wire2Positions = ConvertToPositions(lines[1].Split(","));

            for (int x = 0; x < _wire1Positions.Length - 1; x++)
            {
                for (int y = 0; y < _wire2Positions.Length - 1; y++)
                {
                    if (!DoIntersect(_wire1Positions[x], _wire1Positions[x + 1],
                        _wire2Positions[y], _wire2Positions[y + 1])) continue;

                    Point intersection = FindIntersection(_wire1Positions[x], _wire1Positions[x + 1],
                        _wire2Positions[y], _wire2Positions[y + 1]);

                    if (intersection != null && intersection.TotalDistance != 0 &&
                        !intersections.ContainsKey((intersection.x, intersection.y)))
                    {
                        intersections.Add((intersection.x, intersection.y), (x, y));
                        Console.WriteLine($"Intersection at x:{intersection.x} y:{intersection.y} distance:{intersection.TotalDistance}");
                    }
                }
            }

            int bestDistance = 0;
            foreach (var intersection in intersections)
            {
                int distance = GetLength(intersection.Key, intersection.Value);
                if (bestDistance == 0 || distance < bestDistance) bestDistance = distance;
                Console.WriteLine($"Intersection at x:{intersection.Key.x} y:{intersection.Key.y} WireDistance:{distance}");
            }

            Console.WriteLine($"Best distance: {bestDistance}");
        }

        private static int GetLength((int x, int y) intersection, (int wire1Id, int wire2Id) wireIds)
        {
            Point GetPoint((int x, int y) tuple) => new Point { x = tuple.x, y = tuple.y };

            int sum = _wire1Positions[wireIds.wire1Id].GetDistanceTo(GetPoint(intersection));
            for (int i = wireIds.wire1Id; i > 0; i--) sum += _wire1Positions[i].GetDistanceTo(_wire1Positions[i - 1]);

            sum += _wire2Positions[wireIds.wire2Id].GetDistanceTo(GetPoint(intersection));
            for (int i = wireIds.wire2Id; i > 0; i--) sum += _wire2Positions[i].GetDistanceTo(_wire2Positions[i - 1]);

            return sum;
        }

        private static Point[] ConvertToPositions(string[] commands)
        {
            Point pos = new Point();
            return commands.Select(com =>
            {
                int amt = int.Parse(com.Substring(1));
                return new Point
                {
                    x = com[0] == 'R' ? pos.x += amt : com[0] == 'L' ? pos.x -= amt : pos.x,
                    y = com[0] == 'U' ? pos.y += amt : com[0] == 'D' ? pos.y -= amt : pos.y
                };
            }).Prepend(new Point { x = 0, y = 0 }).ToArray();
        }

        private static bool OnSegment(Point p, Point q, Point r)
        {
            if (q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) &&
                q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y))
                return true;

            return false;
        }

        private static int Orientation(Point p, Point q, Point r)
        {
            // See https://www.geeksforgeeks.org/orientation-3-ordered-points/ 
            // for details of below formula. 
            int val = (q.y - p.y) * (r.x - q.x) -
                      (q.x - p.x) * (r.y - q.y);

            if (val == 0) return 0; // colinear 

            return (val > 0) ? 1 : 2; // clock or counterclock wise 
        }

        private static bool DoIntersect(Point p1, Point q1, Point p2, Point q2)
        {
            // Find the four orientations needed for general and 
            // special cases 
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases 
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1 
            if (o1 == 0 && OnSegment(p1, p2, q1)) return true;

            // p1, q1 and q2 are colinear and q2 lies on segment p1q1 
            if (o2 == 0 && OnSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2 
            if (o3 == 0 && OnSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2 
            if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases 
        }

        private static Point FindIntersection(Point s1, Point e1, Point s2, Point e2)
        {
            int a1 = e1.y - s1.y;
            int b1 = s1.x - e1.x;
            int c1 = a1 * s1.x + b1 * s1.y;

            int a2 = e2.y - s2.y;
            int b2 = s2.x - e2.x;
            int c2 = a2 * s2.x + b2 * s2.y;

            int delta = a1 * b2 - a2 * b1;
            //If lines are parallel, the result will be (NaN, NaN).
            return delta == 0 ? new Point()
                : new Point { x = (b2 * c1 - b1 * c2) / delta, y = (a1 * c2 - a2 * c1) / delta };
        }

        public class Point
        {
            public int x;
            public int y;
            public int TotalDistance => Math.Abs(x) + Math.Abs(y);

            public int GetDistanceTo(Point target) => Math.Abs(x - target.x) + Math.Abs(y - target.y);
        }
    }
}
