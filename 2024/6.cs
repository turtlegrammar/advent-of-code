using static Advent.Extensions;
using System.Linq;
using System.Collections.Generic;
using Advent;

using Advent; namespace Advent2024;

public static class Day6
{
    public static char GUARD = '^';
    public static char OBSTACLE = '#';
    public static char EMPTY = '.';

    public static (int, bool) Walk((int, int) start, Matrix<char> map)
    {
        var guardPoint = start;

        var direction = Direction.Up;

        var visited = new Dictionary<(int, int), HashSet<(int, int)>> { { guardPoint, new HashSet<(int, int)> { Direction.Up }}};
        var loop = false;

        while (map[guardPoint.Add(direction)] != '_' && !loop)
        {
            while(map[guardPoint.Add(direction)] == EMPTY && !loop)
            {
                guardPoint = guardPoint.Add(direction);
                if (visited.TryGetValue(guardPoint, out var hereBefore) && hereBefore.Contains(direction))
                    loop = true;
                Visit(guardPoint, direction);
            }
            while (map[guardPoint.Add(direction)] == OBSTACLE && !loop)
            {
                direction = Direction.TurnRight(direction);
                Visit(guardPoint, direction);
            }
        }

        return (visited.Count, loop);

        void Visit((int, int) guardPoint, (int, int) direction)
        {
            if (visited.TryGetValue(guardPoint, out var set))
                set.Add(direction);
            else
                visited[guardPoint] = new HashSet<(int, int)> { direction };
        }
    }

    public static (int, int) Run(string file)
    {
         var map = Matrix<char>.CharacterMatrixFromFile(file, _null:'_');
         var guardPoint = map.IndexOf(GUARD);
         map[guardPoint] = EMPTY;

         var obstacles = 0;
         var (visitCount, _) = Walk(guardPoint, map);

         map.ForEach((row, col, square) => {
            if (square != OBSTACLE && (row, col) != guardPoint)
            {
                map[(row, col)] = OBSTACLE;
                var (_, loop) = Walk(guardPoint, map);

                if (loop)
                    obstacles++;

                map[(row, col)] = EMPTY;
            }
         });

         return (visitCount, obstacles);
    }

    // public static (int, int) Run(string file)
    // {
    //     var map = Matrix<char>.CharacterMatrixFromFile(file, _null:'_');

    //     var guardPoint = map.IndexOf(GUARD);
    //     var start = guardPoint;
    //     map[guardPoint] = EMPTY;

    //     var direction = Direction.Up;

    //     var visited = new Dictionary<(int, int), HashSet<(int, int)>> { { guardPoint, new HashSet<(int, int)> { Direction.Up }}};
    //     var obstacles = new HashSet<(int, int)> {};

    //     while (map[guardPoint.Add(direction)] != '_')
    //     {
    //         while(map[guardPoint.Add(direction)] == EMPTY)
    //         {
    //             {
    //                 var rightDir = Direction.TurnRight(direction);
    //                 var hypotheticalNextPoints = new HashSet<(int, int)> { guardPoint };
    //                 var next = guardPoint.Add(rightDir);
    //                 while (map[next] == EMPTY)
    //                 {
    //                     hypotheticalNextPoints.Add(next);
    //                     next = next.Add(rightDir);
    //                 }
    //                 if (hypotheticalNextPoints.Any(n => visited.TryGetValue(n, out var prevVisits) && prevVisits.Contains(rightDir)))
    //                 {
    //                     obstacles.Add(guardPoint.Add(direction));
    //                 }
    //             }

    //             guardPoint = guardPoint.Add(direction);
    //             Visit(guardPoint, direction);
    //         }
    //         while (map[guardPoint.Add(direction)] == OBSTACLE)
    //         {
    //             direction = Direction.TurnRight(direction);
    //             Visit(guardPoint, direction);
    //         }
    //     }

    //     return (visited.Count, obstacles.Count);

    //     void Visit((int, int) guardPoint, (int, int) direction)
    //     {
    //         if (visited.TryGetValue(guardPoint, out var set))
    //             set.Add(direction);
    //         else
    //             visited[guardPoint] = new HashSet<(int, int)> { direction };
    //     }
    // }
}