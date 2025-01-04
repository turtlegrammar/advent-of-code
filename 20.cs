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

        var cheatsD2 = FindCheats(2);
        var cheatsD20 = FindCheats(20);

        return (cheatsD2.Where(c => c.Item2 >= 100).Count(), cheatsD20.Where(c => c.Item2 >= 100).Count());

        List<(Cheat, long)> FindCheats(int duration)
        {
            var cheats = new List<(Cheat, long)>();
            maze.ForEach((r1, c1, v1) => 
                maze.ForEach((r2, c2, v2) => {
                    if (v1 == EMPTY && v2 == EMPTY) {
                        var dist = (r1, c1).ManhattanDistance((r2, c2));
                        var saved = baselineCost - (fromStart[(r1, c1)] + fromEnd[(r2, c2)] + dist);
                        if (saved > 0 && dist <= duration)
                            cheats.Add((new Cheat((r1, c1), (r2, c2)), saved));
                    }
                }
            ));
            return cheats;
        }
    }
}