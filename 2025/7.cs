namespace Advent2025;

using System.Data;
using static Advent.Extensions;
using System.Linq;
using Advent;

public static class Day7
{
    public static (long, long) Run(string file)
    {
        var grid = Matrix<char>.CharacterMatrixFromFile(file, ' ');

        var starting = grid.IndexOf('S');

        var quantumCache = new Dictionary<(int, int), long>();

        return (PropagateBeam(starting), PropagateQuantum(starting));

        long PropagateQuantum((int, int) pos)
        {
            if (quantumCache.TryGetValue(pos, out var c))
                return c;
            else if (pos.Item1 == grid.Array.Length -1)
                return 1;
            else
            {
                var down = pos.Add(Direction.Down);
                if (grid[down] == '^')
                    return quantumCache[pos] =
                        PropagateQuantum(down.Add(Direction.Left))
                        + PropagateQuantum(down.Add(Direction.Right));
                else
                    return quantumCache[pos] = PropagateQuantum(down);

            }
        }

        int PropagateBeam((int, int) start)
        {
            var row = start.Item1;
            var frontier = new HashSet<(int, int)> { start };
            var splits = 0;

            while (row < grid.Array.Length)
            {
                var nextFrontier = new HashSet<(int, int)>();

                foreach (var coord in frontier)
                {
                    var down = coord.Add(Direction.Down);
                    if (grid[down] == '^')
                    {
                        splits++;
                        nextFrontier.UnionWith([down.Add(Direction.Left), down.Add(Direction.Right)]);
                    }
                    else
                        nextFrontier.Add(down);
                }

                frontier = nextFrontier;
                row++;
            }

            return splits;
        }
    }
}