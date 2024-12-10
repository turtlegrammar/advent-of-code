using static Advent.Extensions;

namespace Advent;

public static class Day10
{
    public static int NULL = -1;

    public static (int, int) Run(string file)
    {
        var map = Matrix<int>.IntegerMatrixFromFile(file, _null:NULL);

        var trailheads = map.SelectWhere((row, col, x) => x == 0 ? Some((row, col)) : None<(int, int)>());

        var scoresAndRatings = trailheads.Select(ScoreAndRating).ToList();

        return (scoresAndRatings.Select(t => t.Item1).Sum(), scoresAndRatings.Select(t => t.Item2).Sum());

        List<(int, int)> ReachableNines((int, int) coord, int val) =>
            val == 9
            ? List(coord)
            : coord.Adjacent4()
                .Where(c => map.Get(c) == val + 1)
                .SelectMany(c => ReachableNines(c, val + 1))
                .ToList();

        (int, int) ScoreAndRating((int, int) trailhead)
        {
            var reachableNines = ReachableNines(trailhead, 0);
            return (reachableNines.ToHashSet().Count, reachableNines.Count);
        }
    }
}