using System.Text.RegularExpressions;

namespace Advent;

public static class Day7
{
    public static long Run(string file)
    {
        return File.ReadAllLines(file)
            .Select(line => new Regex("[ :]").Split(line).Where(l => l != "").Select(Int64.Parse))
            .Select(l => (l.First(), l.Skip(1).ToList()))
            .Where(e => BuildsUpTo(e.Item2.ReverseList()).Any(x => x == e.Item1))
            .Select(b => b.Item1)
            .Sum();

        IEnumerable<long> BuildsUpTo(IEnumerable<long> nums) =>
            nums.Count() == 1 ? nums
            : BuildsUpTo(nums.Skip(1)).SelectMany(t => new List<long> { 
                    t + nums.First(),
                    t * nums.First(),
                    Concat(t, nums.First())});

        long Concat(long x, long y) =>
            Int64.Parse(x.ToString() + y.ToString());
    }
}