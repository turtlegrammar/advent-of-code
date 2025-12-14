namespace Advent2025;

using System.Data;
using static Advent.Extensions;
using System.Linq;
using Advent;

public static class Day12
{
    public enum BoundResult { DefinitelyImpossible, Unclear, DefinitelyPossible }

    public static (long, long) Run(string file)
    {
        var lines = File.ReadAllLines(file);

        var shapeTileOccupancy = lines.Take(30).Chunk(5)
            .Select(chunk => chunk.Select(s => s.Where(c => c == '#').Count()).Sum()).ToList();

        var problems = lines.Skip(30).Select(Parse.Ints);
        var results = problems.Select(VibeOutProblem).ToList();
        // No actual algorithm needed today!
        if (results.Any(r => r == BoundResult.Unclear))
            throw new Exception("Need to actually write an algorithm instead of just do basic bounds checking.");

        return (results.Where(r => r == BoundResult.DefinitelyPossible).Count(), 0);

        BoundResult VibeOutProblem(int[] problem, int i)
        {
            var rows = problem[0]; var cols = problem[1];
            var totalSquares = rows*cols;
            var minRequiredSquares = problem.Skip(2).Zip(shapeTileOccupancy).Select(t => t.Item1*t.Item2).Sum();
            var maxRequiredSquares = problem.Skip(2).Select(n => n*9).Sum();
            var boundResult = 
                totalSquares < minRequiredSquares ? BoundResult.DefinitelyImpossible
                : totalSquares >= maxRequiredSquares ? BoundResult.DefinitelyPossible
                : BoundResult.Unclear;

            return boundResult;
        }
    }
}