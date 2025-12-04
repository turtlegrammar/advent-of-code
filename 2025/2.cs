namespace Advent2025;

using System.Data;
using static Advent.Extensions;
using Advent;

public static class Day2
{
    public static (long, long) Run(string file)
    {
        var ranges = File.ReadAllText(file).Split(",").Select(range => 
            range.Split("-").Pipe(r => (Parse.Long(r[0]), Parse.Long(r[1])))).ToList();

        var simpleInvalids = List(2, 4, 6, 8, 10).SelectMany(n => SimpleInvalidsOfLength(n, 2)).ToList();

        var simpleInvalidsInARange = simpleInvalids.Where(i => 
            ranges.Any(r => r.Item1 <= i && i <= r.Item2)
        ).ToList();

        var complexInvalids = new HashSet<long>();
        for (var n = 1; n <= 10; n++)
            for (var i = 2; i <= n; i++)
                if (n % i == 0)
                    complexInvalids.UnionWith(SimpleInvalidsOfLength(n, i).ToHashSet());

        var complexInvalidsInARange = complexInvalids.Where(i => 
            ranges.Any(r => r.Item1 <= i && i <= r.Item2)
        ).ToList();


        return (simpleInvalidsInARange.Sum(), complexInvalidsInARange.Sum());

        static List<long> SimpleInvalidsOfLength(int length, int partitions)
        {
            var x = length / partitions;
            var max = (int)Math.Pow(10, x);
            var results = new List<string>();
            for (var i = (int)Math.Pow(10, x - 1); i < max; i++)
                results.Add(Enumerable.Range(0, partitions).Select(_ => i.ToString()).StrJoin());
            return results.Select(Parse.Long).ToList();
        }
    }
}