namespace Advent2025;

using System.Data;
using static Advent.Extensions;
using Advent;

public static class Day4
{
    public static (long, long) Run(string file)
    {
        var grid = Matrix<char>.CharacterMatrixFromFile(file, '_');
        var reachable = 0;
        grid.ForEach((row, col, val) =>
        {
            var neighboringPaper = (row, col).Adjacent8().Where(v => grid[v] == '@').Count();
            if (val == '@' && neighboringPaper < 4)
                reachable++;
        });

        var removed = 0;
        var lastRemoved = 0;
        do
        {
            lastRemoved = removed;

            grid.ForEach((row, col, val) =>
            {
                var neighboringPaper = (row, col).Adjacent8().Where(v => grid[v] == '@').Count();
                if (val == '@' && neighboringPaper < 4)
                {
                    removed++;
                    grid[(row, col)] = 'X';
                }
            });
        } while (removed != lastRemoved);

        return (reachable, removed);
    }
}