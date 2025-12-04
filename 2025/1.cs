namespace Advent2025;

using System.Data;
using static Advent.Extensions;
using Advent;

public static class Day1
{
    public static (long, long) Run(string file)
    {
        var dial = 50;
        var transformations = File.ReadLines(file).Select(s => 
            s[0] == 'L' ? (-1 * Parse.Int(s.Substring(1))) : Parse.Int(s.Substring(1))
        ).ToList();

        var zeroCount = 0;
        var allZeroCount = 0;

        foreach (var t in transformations)
        {
            if (dial + t >= 100)
                allZeroCount += (dial + t) / 100;
            if (dial + t < 0)
                allZeroCount +=  (dial == 0 ? 0 : 1) + Math.Abs(dial + t) / 100;
            if (dial + t == 0)
                allZeroCount += 1 + Math.Abs(t) / 100;

            dial = (dial + t) % 100;
            if (dial < 0) dial = 100 + dial;

            System.Console.WriteLine(dial);
            if (dial == 0)
                zeroCount++;
        }

        return (zeroCount, allZeroCount);

    }
}