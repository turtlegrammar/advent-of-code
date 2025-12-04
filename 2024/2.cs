using System.Linq;
using System.Collections.Generic;

using Advent; namespace Advent2024;

public static class Day2
{
    public static List<List<int>> Removals(List<int> report)
    {
        var coll = new List<List<int>>();
        for (var skip = 0; skip < report.Count; skip++)
        {
            var x = new List<int>();
            for (var i = 0; i < report.Count; i++)
            {
                if (i != skip)
                {
                    x.Add(report[i]);
                }
            }
            coll.Add(x);
        }
        return coll;
    }

    public static bool Safe(List<int> report)
    {
        var asc = report[0] < report[1];

        for (var i = 0; i < report.Count-1; i++)
        {
            if (asc != report[i] < report[i + 1])
                return false;

            var dist = Math.Abs(report[i] - report[i + 1]);

            if (dist < 1 || dist > 3)
                return false;
        }

        return true;
    }

    public static bool Safe2(List<int> report) =>
        Removals(report).Any(Safe);

    public static int Solve(List<List<int>> reports) =>
        reports.Where(Safe).Count();

    public static int Solve2(List<List<int>> reports) =>
        reports.Where(Safe2).Count();

    public static List<List<int>> Read(string file) =>
        File.ReadAllLines(file).Select(line => line.Split().Select(Int32.Parse).ToList()).ToList();

    public static int Run(string file) =>
        Solve(Read(file));

    public static int Run2(string file) =>
        Solve2(Read(file));
}