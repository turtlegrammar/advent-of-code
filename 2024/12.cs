using static Advent.Extensions;

using Advent; namespace Advent2024;

public static class Day12
{
    public static char NULL = '_';

    public static (long, long) Run(string file)
    {
        var map = Matrix<char>.CharacterMatrixFromFile(file, _null:NULL);

        var coordinateToRegion = new Dictionary<(int, int), int>();
        var regions = new Dictionary<int, HashSet<(int, int)>>();
        var regionId = -1;

        BuildRegions();

        return (
            regions.Select(kvp => Area(kvp.Value) * Perimeter(kvp.Value)).Sum(),
            regions.Select(kvp => Area(kvp.Value) * Sides(kvp.Value)).Sum()
        );

        long Perimeter(HashSet<(int, int)> region)
        {
            var plant = map.Get(region.First());
            return region.Select(c => c.Adjacent4().Where(a => map.Get(a) != plant).Count()).Sum();
        }

        long Area(HashSet<(int, int)> region) =>
            region.Count;

        long Sides(HashSet<(int, int)> region)
        {
            var plant = map.Get(region.First());
            var rowUps = new Dictionary<int, HashSet<int>>();
            var rowDowns = new Dictionary<int, HashSet<int>>();
            var colLefts = new Dictionary<int, HashSet<int>>();
            var colRights = new Dictionary<int, HashSet<int>>();
            foreach (var coord in region)
            {
                if (map.Get(coord.Add(Direction.Right)) != plant)
                    colRights.AddSet(coord.Item2, coord.Item1);
                if (map.Get(coord.Add(Direction.Left)) != plant)
                    colLefts.AddSet(coord.Item2, coord.Item1);
                if (map.Get(coord.Add(Direction.Up)) != plant)
                    rowUps.AddSet(coord.Item1, coord.Item2);
                if (map.Get(coord.Add(Direction.Down)) != plant)
                    rowDowns.AddSet(coord.Item1, coord.Item2);
            }

            var ups = rowUps.Select(kvp => JoinSides(kvp.Value.ToList()).Count).Sum();
            var downs = rowDowns.Select(kvp => JoinSides(kvp.Value.ToList()).Count).Sum();
            var lefts = colLefts.Select(kvp => JoinSides(kvp.Value.ToList()).Count).Sum();
            var rights = colRights.Select(kvp => JoinSides(kvp.Value.ToList()).Count).Sum();
            return ups + lefts + downs + rights;
        }

        // List(1, 2, 3, 5, 7, 8, 10) -> [3, 1, 2, 1]
        List<long> JoinSides(List<int> xs)
        {
            xs.Sort();
            var result = new List<long>();
            var curSide = 1;
            var cur = xs[0];
            for (int i = 1; i < xs.Count; i++)
            {
                if (xs[i] == cur + 1) {
                    cur++;
                    curSide++;
                } else {
                    cur = xs[i];
                    result.Add(curSide);
                    curSide = 1;
                }
            }
            result.Add(curSide);

            return result;
        }


        void BuildRegions()
        {
            map.ForEach((row, col, val) =>
            {
                if (!coordinateToRegion.ContainsKey((row, col)))
                {
                    regionId++;
                    regions[regionId] = new HashSet<(int, int)>();

                    var coords = new Stack<(int, int)>();
                    var visited = new HashSet<(int, int)>();
                    coords.Push((row, col));

                    while (coords.Count > 0)
                    {
                        var c = coords.Pop();
                        if (!visited.Contains(c))
                        {
                            regions[regionId].Add(c);
                            coordinateToRegion[c] = regionId;
                            visited.Add(c);

                            foreach (var adj in c.Adjacent4().Where(a => map.Get(a) == val))
                                coords.Push(adj);
                        }
                    }
                }
            });
        }
    }
}