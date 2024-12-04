using System.Linq;
using System.Collections.Generic;

namespace Advent;

public static class Day1
{
    public static int Solve(List<int> first, List<int> second)
    {
        first.Sort();
        second.Sort();

        return first.Zip(second, (x, y) => Math.Abs(x - y)).Sum();
    }

    public static int Solve2(List<int> first, List<int> second)
    {
        var frequencies = second.GroupBy(s => s).ToDictionary(g => g.Key, g => g.Count());

        return first.Select(x => x * frequencies.GetValueOrDefault(x, 0)).Sum();
    }

    public static (List<int>, List<int>) ReadInput()
    {
        var file = "inputs/day-1.txt";
        var lines = File.ReadAllLines(file);
        var first = new List<int>();
        var second = new List<int>();
        foreach (var line in lines)
        {
            var s = line.Split();
            first.Add(Int32.Parse(s[0]));
            second.Add(Int32.Parse(s[s.Length-1]));
        }

        return (first, second);
    }

    public static (int, int) Run()
    {
        var (first, second) = ReadInput();
        return (Solve(first, second), Solve2(first, second));
    } 
}