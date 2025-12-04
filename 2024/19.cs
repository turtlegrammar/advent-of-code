using Advent; namespace Advent2024;

using System.Data;
using static Advent.Extensions;

public static class Day19
{
    public static (int, long) Run(string file)
    {
        var lines = File.ReadAllLines(file);

        var atoms = lines[0].Split(", ").ToList();
        var patterns = lines.Skip(2).ToList();

        var cache = new Dictionary<string, long>(); 

        var waysToBuild = patterns.Select(WaysToBuild).ToList();

        long WaysToBuild(string sequence)
        {
            if (cache.TryGetValue(sequence, out var p))
                return p;

            if (sequence == "")
                return 1;

            long ways = 0;
            foreach (var atom in atoms)
            {
                if (sequence.StartsWith(atom))
                    ways += WaysToBuild(sequence.Substring(atom.Length));
            }

            return cache[sequence] = ways;
        }

        return (waysToBuild.Where(x => x > 0).Count(), waysToBuild.Sum());
    }
}