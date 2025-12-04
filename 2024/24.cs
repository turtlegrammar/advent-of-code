namespace Advent;

using System.Data;
using static Advent.Extensions;

public static class Day24
{
    public static (long, long) Run(string file)
    {
        var lines = Parse.AlphanumLines(file);
        var solvedValues = lines.TakeWhile(l => l.Length == 2).ToDictionary(t => t[0], t => Int32.Parse(t[1]));
        var relations = lines.SkipWhile(l => l.Length != 4).ToDictionary(t => t[3], t => (t[1], t[0], t[2]));

        relations.Keys.ForEach(Solve);

        var binary = relations.Keys
            .Where(wire => wire.StartsWith("z"))
            .OrderByDescending(s => Convert.ToInt32(s.Substring(1)))
            .Select(wire => solvedValues[wire].ToString())
            .StrJoin();
        var asDecimal = Convert.ToInt64(binary, 2);

        return (asDecimal, 0);

        int Solve(string wire)
        {
            if (solvedValues.TryGetValue(wire, out var v))
                return v;

            var (relation, wire1, wire2) = relations[wire];

            var s1 = Solve(wire1); var s2 = Solve(wire2);

            return solvedValues[wire] = relation switch
            {
                "AND" => s1 & s2,
                "XOR" => s1 ^ s2,
                "OR" => s1 | s2
            };
        }
    }
}