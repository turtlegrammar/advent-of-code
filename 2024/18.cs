using Advent; namespace Advent2024;

using System.Data;
using static Advent.Extensions;

public static class Day18
{
    public const char SAFE = '.'; public const char CORRUPTED = '#';
    public static (long, (int, int)) Run(string file)
    {
        var grid = Matrix<char>.Create(SAFE, '_', 71, 71);
        var falls = Parse.IntArrayLines(file).Select(t => (t[1], t[0])).ToList();
        
        var pathLengths = new List<long>();

        foreach (var fall in falls) {
            grid[fall] = CORRUPTED;

            Dictionary<(int, int), Dictionary<(int, int), long>> edges = new ();
            grid.ForEach((row, col, _) => {
                edges[(row, col)] = (row, col).Adjacent4().Where(t => grid.Get(t) == SAFE).ToDictionary(x => x, _ => 1L);
            });

            var (costs, _) = Algorithms.ShortestPathsFrom((0, 0), edges);
            if (costs.TryGetValue((70, 70), out var pathLength))
                pathLengths.Add(pathLength);
            else
                break;
        }

        var fatalFall = falls[pathLengths.Count];
        return (pathLengths[1024], (fatalFall.Item2, fatalFall.Item1));
    }
}