using Advent; namespace Advent2024;

public static class Day11
{
    public static (long, long) Run(string file)
    {
        var stones = Parse.IntArray(file);

        // (val, iterations) -> count
        var memoized = new Dictionary<(long, int), long>();

        var x = stones.Select(s => BlinksInto(s, 25)).ToList();
        var y = stones.Select(s => BlinksInto(s, 75)).ToList();

        return (x.Sum(), y.Sum());

        long BlinksInto(long value, int iterations)
        {
            if (iterations == 0)
                return 1;
            else if (memoized.TryGetValue((value, iterations), out var v))
                return v;
            else
            {
                var valAsString = value.ToString();

                if (value == 0)
                    return Cache(BlinksInto(1, iterations-1));
                else if (valAsString.Length % 2 == 0)
                {
                    var left = Int64.Parse(valAsString.Substring(0, valAsString.Length/2));
                    var right = Int64.Parse(valAsString.Substring(valAsString.Length/2, valAsString.Length/2));
                    return Cache(BlinksInto(left, iterations-1) + BlinksInto(right, iterations-1));
                }
                else
                    return Cache(BlinksInto(value * 2024, iterations - 1));
            }

            long Cache(long x) =>
                memoized[(value, iterations)] = x;
        }
    }
}