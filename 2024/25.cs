namespace Advent;

using System.Data;
using static Advent.Extensions;
using System.Linq;

public static class Day25
{
    public static (long, long) Run(string file)
    {
        var blocks = Parse.Blocks(file);

        List<List<int>> keys = [];
        List<List<int>> locks = [];

        foreach (var block in blocks)
        {
            var isKey = block[0] == ".....";
            var content = block.Select(s => s.ToCharArray().ToList())
                .ToList()
                .Slice(1, 5)
                .Invert();
            var parsed = content.Select(line => line.Where(c => c == '#').Count()).ToList();
            (isKey ? keys : locks).Add(parsed);
        }

        var part1 = locks.AllPairsWith(keys).Where(kl => Fits(kl.Item1, kl.Item2)).Count();

        // There is no part 2
        return (part1, 0);

        bool Fits(List<int> Lock, List<int> Key) =>
            Lock.Zip(Key, (x, y) => x + y).All(i => i <= 5);
    }
}