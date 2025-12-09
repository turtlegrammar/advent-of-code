namespace Advent2025;

using System.Data;
using static Advent.Extensions;
using System.Linq;
using Advent;

public static class Day9
{
    public static (long, long) Run(string file)
    {
        var redTiles = Parse.LongArrayLines(file).Select(l => l.ToTuple2()).ToList();
        var pairs = redTiles.OrderedPairs();
        var areas = pairs.Zip(pairs.Select(Area)).OrderByDescending(t => t.Item2);

        var (perimeter, atX, atY) = BuildPerimeter();

        var part2 = areas.First(trip => CanMakeRectangleFrom(trip.Item1.Item1, trip.Item1.Item2)).Item2;

        return (areas.First().Item2, part2);

        long Area(((long, long), (long, long)) tup)
        {
            var (a, b) = tup;
            return (1 + Math.Abs(a.X() - b.X())) * (1 + Math.Abs(a.Y() - b.Y()));
        }

        bool CanMakeRectangleFrom((long, long) p, (long, long) q)
        {
            var topLeft = (Math.Min(p.X(), q.X()), Math.Min(p.Y(), q.Y()));
            var bottomRight = (Math.Max(p.X(), q.X()), Math.Max(p.Y(), q.Y()));

            return XSegmentWithinPerimeter(topLeft.Y(), topLeft.X(), bottomRight.X())
                && XSegmentWithinPerimeter(bottomRight.Y(), topLeft.X(), bottomRight.X())
                && YSegmentWithinPerimeter(topLeft.X(), topLeft.Y(), bottomRight.Y())
                && YSegmentWithinPerimeter(bottomRight.X(), topLeft.Y(), bottomRight.Y());
        }

        bool XSegmentWithinPerimeter(long y, long minX, long maxX)
        {
            if (!atY.TryGetValue(y, out var perimeterPointsAtSameY))
                return false;

            var checks = new List<(long, long)> { (minX, y), (maxX, y), ((maxX-minX)/2+minX, y) };
            // for each crossing, verify that the points both left and right are within bounds.
            // we already know that the crossing point itself is in, since it's part of the perimeter.
            foreach (var q in perimeterPointsAtSameY)
                if (minX < q.X() && q.X() < maxX)
                {
                    var left = ( q.X() - 1, y);
                    var right = (q.X() + 1, y);
                    checks.AddRange(List(left, right));
                }

            return checks.All(WithinPerimeter);
        }

        bool YSegmentWithinPerimeter(long x, long minY, long maxY)
        {
            if (!atX.TryGetValue(x, out var perimeterPointsAtSameX))
                return false;

            var checks = new List<(long, long)> { (x, minY), (x, maxY), (x, minY + (maxY - minY)/2) };
            // for each crossing, verify that the points both above and below are within bounds
            foreach (var q in perimeterPointsAtSameX)
                if (minY < q.Y() && q.Y() < maxY)
                {
                    // check left of crossing and right of crossing
                    var above = (x, q.Y() - 1); 
                    var below = (x, q.Y() + 1);
                    checks.AddRange(List(above, below));
                }

            return checks.All(WithinPerimeter);
        }


        bool WithinPerimeter((long, long) p)
        {
            if (perimeter.Contains(p))
                return true;

            return WithinX(p) && WithinY(p);

            bool WithinX((long, long) p)
            {
                if (!atX.TryGetValue(p.X(), out var perimeterPointsAtSameX))
                    return false;

                if (perimeterPointsAtSameX.Count == 0)
                    return false;
                
                if (p.Y() < perimeterPointsAtSameX.First().Y() || p.Y() > perimeterPointsAtSameX.Last().Y())
                    return false;

                /*
                Walk across from left to right. At first, nothing above or below is in bounds.
                When we encounter a perimeter point with neighbors above and below like
                     #
                  -> #
                     #
                Then we know that anything above or below is in range. 
                We manage inUp and inDown separately to handle segments like the following,
                where we're riding along the perimeter and only values above or below are in, but not both.

                #   #   #    #    #   #
                #   #   #    #    #   #
            ->  #   ######   ######   #
                #                     #
                #######################

                */
                var inUp = false;
                var inDown = false;
                for (int i = 0; i < perimeterPointsAtSameX.Count-1; i++)
                {
                    if (perimeter.Contains(perimeterPointsAtSameX[i].Add(Direction.Up)))
                        inUp = !inUp;
                    if (perimeter.Contains(perimeterPointsAtSameX[i].Add(Direction.Down)))
                        inDown = !inDown;
                    if (perimeterPointsAtSameX[i].Y() < p.Y() && p.Y() < perimeterPointsAtSameX[i + 1].Y())
                        return inUp && inDown;
                }
                return false;
            }

            bool WithinY((long, long) p)
            {
                if (!atY.TryGetValue(p.Y(), out var perimeterPointsAtSameY))
                    return false;
                if (perimeterPointsAtSameY.Count == 0)
                    return false;
                
                if (p.X() < perimeterPointsAtSameY.First().X() || p.X() > perimeterPointsAtSameY.Last().X())
                    return false;

                var inLeft = false;
                var inRight = false;
                for (int i = 0; i < perimeterPointsAtSameY.Count-1; i++)
                {
                    if (perimeter.Contains(perimeterPointsAtSameY[i].Add(Direction.Left)))
                        inLeft = !inLeft;
                    if (perimeter.Contains(perimeterPointsAtSameY[i].Add(Direction.Right)))
                        inRight = !inRight;
                    if (perimeterPointsAtSameY[i].X() < p.X() && p.X() < perimeterPointsAtSameY[i + 1].X())
                        return inLeft && inRight;
                }
                return false;
            }
        }

        // (perimeter set, x coordinate -> points at x coordinate, y coordinate -> points at y coordinate)
        (HashSet<(long, long)>, Dictionary<long, List<(long, long)>>, Dictionary<long, List<(long, long)>>) BuildPerimeter()
        {
            var perimeter = new HashSet<(long, long)>();
            for (int i = 0; i < redTiles.Count; i++)
            {
                var j = (i + 1) % redTiles.Count;
                var p1 = redTiles[i]; var p2 = redTiles[j];
                if (p1.X() == p2.X())
                {
                    var startY = Math.Min(p1.Y(), p2.Y());
                    var endY = Math.Max(p1.Y(), p2.Y());
                    for (long y = startY; y <= endY; y++)
                        perimeter.Add((p1.X(), y));
                }
                if (p1.Y() == p2.Y())
                {
                    var startX = Math.Min(p1.X(), p2.X());
                    var endX = Math.Max(p1.X(), p2.X());
                    for (long x = startX; x <= endX; x++)
                        perimeter.Add((x, p1.Y()));
                }
            }
            var atX = perimeter.GroupBy(p => p.X()).ToDictionary(
                kvp => kvp.Key,
                // chunkifying (compressing long segments into start and end points) speeds it up by about 10x
                // but is not necessary for correctness
                kvp => Algorithms.Chunkify(kvp.ToList().OrderBy(p => p.Y()).ToList(), p => p.Y())
            );
            var atY = perimeter.GroupBy(p => p.Y()).ToDictionary(
                kvp => kvp.Key,
                // chunkifying (compressing long segments into start and end points) speeds it up by about 10x
                // but is not necessary for correctness
                kvp => Algorithms.Chunkify(kvp.ToList().OrderBy(p => p.X()).ToList(), p => p.X())
            );
            return (perimeter, atX, atY);
        }
    }
}