namespace Advent2025;

using System.Data;
using static Advent.Extensions;
using System.Linq;
using Advent;

public static class Day6
{
    public static (long, long) Run(string file)
    {
        var parsed = Parse.AlphanumLines(file);
        var inverted = parsed.Invert();
        var part1 = inverted.Select(ComputePart1).Sum();

        var transposedReversedGrid = Matrix<char>.CharacterMatrixFromFile(file, ' ').Array
            .Invert().ReverseList()
            .Select(r => r.StrJoin(""))
            .ToList();

        var solutions = new List<long>();
        var intermediate = new List<long>();
        foreach (var row in transposedReversedGrid)
        {
            if (String.IsNullOrWhiteSpace(row))
            {
                intermediate = [];
            }
            else
            {
                intermediate.Add(Parse.MessyLong(row));
                if (row.Contains("*"))
                    solutions.Add(intermediate.Aggregate((x, y) => x * y));
                else if (row.Contains("+"))
                    solutions.Add(intermediate.Aggregate((x, y) => x + y));
            }
        }

        var part2 = solutions.Sum();

        return (part1, part2);

        long ComputePart1(List<string> problem)
        {
            var op = problem.Last();
            var result = op == "*" ? 1L : 0L;
            for (int i = 0; i < problem.Count - 1; i++)
                result = op == "*" ? result * Parse.Long(problem[i]) : result + Parse.Long(problem[i]);
            return result;
        }
    }
}