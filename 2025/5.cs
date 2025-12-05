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

        ranges.Sort();
        var merged = new List<(long, long)>();
        var i = 0;
        while (i < ranges.Count)
        {
            System.Console.WriteLine(i);
            var m = ranges[i];
            var j = i + 1;
            while (j < ranges.Count)
            {
                var r = ranges[j];
                if (m.Item2 >= r.Item1)
                {
                    m = (m.Item1, Math.Max(m.Item2, r.Item2));
                    j++;
                }
                else
                    break;
            }
            i = j;
            merged.Add(m);
        }

        return (fresh.Count(), merged.Select(r => r.Item2 - r.Item1 + 1).Sum());
    }
}