namespace Advent2025;

using System.Data;
using static Advent.Extensions;
using Advent;

public static class Day5
{
    public static (long, long) Run(string file)
    {
        var (ranges, ingredients) = Parse.Multi(file, Parse.Range, Parse.Long);

        var fresh = ingredients.Where(i => ranges.Any(r => r.Item1 <= i && i <= r.Item2)).ToList();

        var lastRangeCount = 0;
        do
        {
            lastRangeCount = ranges.Count;

            var nextRanges = new List<(long, long)>();
            var merged = new HashSet<int>();
            for (int i = 0; i < ranges.Count; i++)
            {
                for (int j = i + 1; j < ranges.Count; j++)
                {
                    if (merged.Contains(i) || merged.Contains(j))
                        break;
                    var ((low1, high1), (low2, high2)) = (ranges[i], ranges[j]).Order();
                    if (high1 >= low2) {
                        nextRanges.Add((low1, Math.Max(high1, high2)));
                        merged.Add(i); merged.Add(j);
                    }
                }
            }
            for (int i = 0; i < ranges.Count; i++)
                if (!merged.Contains(i))
                    nextRanges.Add(ranges[i]);
            
            ranges = nextRanges;
        } while (ranges.Count != lastRangeCount);

        return (fresh.Count(), ranges.Select(r => r.Item2 - r.Item1 + 1).Sum());
    }
}