namespace Advent2025;

using System.Data;
using static Advent.Extensions;
using System.Linq;
using Advent;

public static class Day8
{
    public static (long, long) Run(string file)
    {
        var points = Parse.LongArrayLines(file).Select(la => la.ToTuple3()).ToList();
        var pointToClusterId = points.Zip(Enumerable.Range(0, points.Count)).ToDictionary();
        var pointPairsAndDistances = points.OrderedPairs()
            .Select(pt => (pt.Item1.Distance(pt.Item2), pt.Item1, pt.Item2))
            .OrderBy(trip => trip.Item1)
            .ToList();

        var uniqueClusters = pointToClusterId.Values.ToHashSet();
        int mergeIndex = 0;

        for (int i = 0; i < 1000; i++)
            MergeClosestJunctions();

        var part1 = pointToClusterId
            .GroupBy(kvp => kvp.Value)
            .ToDictionary(g => g.Key, g => g.ToList().Count)
            .OrderByDescending(kvp => kvp.Value)
            .Take(3)
            .Select(kvp => kvp.Value)
            .Aggregate((x, y) => x * y);
        
        // part 2
        while (MergeClosestJunctions());
        var (_, lastMergeP1, lastMergeP2) = pointPairsAndDistances[mergeIndex];

        return (part1, lastMergeP1.X() * lastMergeP2.X());

        // true if can continue, false if all unified
        bool MergeClosestJunctions()
        {
            if (mergeIndex >= pointPairsAndDistances.Count)
                return false;

            var (_, p1, p2) = pointPairsAndDistances[mergeIndex];

            var c1 = pointToClusterId[p1]; var c2 = pointToClusterId[p2];

            if (c1 != c2)
            {
                var from = Math.Min(c1, c2); var to = Math.Max(c1, c2);

                pointToClusterId = pointToClusterId.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value == from ? to : kvp.Value
                );

                uniqueClusters.Remove(from);
                if (uniqueClusters.Count == 1)
                    return false;
            }

            mergeIndex++;
            return true;
        }
    }
}