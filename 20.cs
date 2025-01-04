namespace Advent;

using System.Data;
using static Advent.Extensions;

public static class Day20
{
    public const char EMPTY = '.'; public const char WALL = '#';
    record Cheat((int, int) Start, (int, int) End);

    public static (long, long) Run(string file)
    {
        var maze = Matrix<char>.CharacterMatrixFromFile(file, _null: '_');

        var startPoint = maze.IndexOf('S');
        var endPoint = maze.IndexOf('E');

        maze[startPoint] = EMPTY; maze[endPoint] = EMPTY;

        var neighbors = new Dictionary<(int, int), Dictionary<(int, int), long>>();
        maze.ForEach((row, col, val) => {
            if (val != WALL)
            {
                neighbors[(row, col)] = (row, col).Adjacent4().Where(t => maze.Get(t) != WALL).ToDictionary(x => x, _ => 1L);
            }
        });

        var (fromStart, _) = Algorithms.ShortestPathsFrom(startPoint, neighbors);
        fromStart[startPoint] = 0;
        var (fromEnd, _) = Algorithms.ShortestPathsFrom(endPoint, neighbors);
        fromEnd[endPoint] = 0;

        var baselineCost = fromStart[endPoint];

        var cheats = new List<(Cheat, long)>();
        maze.ForEach((row, col, val) => {
            if (val == EMPTY) {
                var dirs = Direction.CardinalDirections;
                var costToHere = fromStart[(row, col)];
                var cs = dirs
                    .Select(d => new Cheat((row, col).Add(d), (row, col).Add(d).Add(d)))
                    .Where(c => maze.Get(c.Start) == WALL && maze.Get(c.End) == EMPTY);
                foreach (var c in cs)
                {
                    var saved = baselineCost - (costToHere + fromEnd[c.End] + 2);
                    if (saved > 0)
                        cheats.Add((c, saved));
                }
            }
        });

        var grouped = cheats.GroupBy(t => t.Item2).ToDictionary(k => k.Key, k => k.Count());

        // var possibleCheats = maze.Select((row, col, val) => 
        //     val == WALL && row != 0 && row != maze.Array.Length - 1 && col != 0 && col != maze.Array[0].Length
        //         ? (row, col).Adjacent4().Select(t => new Cheat((row, col), t)) 
        //         : new List<Cheat>())
        //     .SelectMany(x => x)
        //     .Select(c => (c, baselineCost - (fromStart.GetValueOrDefault(c.One, 999999999) + fromEnd.GetValueOrDefault(c.Two, 999999999))))
        //     // .Where(c => c.Item2 > 0)
        //     .ToList();

        return (cheats.Where(c => c.Item2 >= 100).Count(), 0);
    }
}