namespace Advent;

using System.Data;
using static Advent.Extensions;

public static class Day22
{
    public const char EMPTY = '.'; public const char WALL = '#';

    public static (long, long) Run(string file)
    {
        var input = Parse.LongArray(file);

        var x = IterativeSeq(123L, Next).Take(12).ToList();
        var xs = ChangeSequences(x);

        var nums = input.Select(n => IterativeSeq(n, Next).Take(2001).ToList());
        var answer1 = nums.Select(x => x[2000]).Sum();

        var changeSeqs = nums.Select(ChangeSequences).ToList();
        var possibleChangeSeqs = AllPossibleChangeSequences();

        var seq = possibleChangeSeqs.MaxBy(s => changeSeqs.Select(cs => cs.GetValueOrDefault(s, 0)).Sum());

        return (answer1, changeSeqs.Select(cs => cs.GetValueOrDefault(seq, 0)).Sum());

        IEnumerable<(long, long, long, long)> AllPossibleChangeSequences()
        {
            for (long a = -9; a < 10; a++)
                for (long b = -9; b < 10; b++)
                    for (long c = -9; c < 10; c++)
                        for (long d = -9; d < 10; d++)
                            yield return (a, b, c, d);
        }

        Dictionary<(long, long, long, long), long> ChangeSequences(List<long> ns)
        {
            var result = new Dictionary<(long, long, long, long), long>();
            for (int i = 0; i < ns.Count - 4; i++)
            {
                var a = ns[i] % 10; var b = ns[i + 1] % 10; var c = ns[i + 2] % 10; var d = ns[i + 3] % 10; var e = ns[i + 4] % 10;
                var k = (b - a, c - b, d - c, e - d);
                if (!result.ContainsKey(k))
                    result[k] = e;
            }
            return result;
        }

        long Next(long n)
        {
            var x = Prune(Mix(n, n * 64)); // 2^6
            var y = Prune(Mix(x, x / 32)); // 2^5
            var z = Prune(Mix(y, y * 2048)); // 2^10
            return z;
        }

        long Mix(long s, long n) => s ^ n;
        long Prune(long s) => s % 16777216; // 2^24
    }
}