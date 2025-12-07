namespace Advent;

using System.Data;
using static Advent.Extensions;

public static class Day24
{
    public record Wire(string Op, string Gate1, string Gate2);
    public static (long, string) Run(string file)
    {
        var lines = Parse.AlphanumLines(file);
        var solvedValues = lines.TakeWhile(l => l.Length == 2).ToDictionary(t => t[0], t => Int32.Parse(t[1]));
        var relations = lines.SkipWhile(l => l.Length != 4).ToDictionary(
            t => t[3], 
            t => { 
                var (g1, g2) = (t[0], t[2]).Order(); 
                return new Wire(t[1], g1, g2);
            });

        // Part 1
        relations.Keys.ForEach(Solve);

        var binary = relations.Keys
            .Where(wire => wire.StartsWith("z"))
            .OrderByDescending(s => Convert.ToInt32(s.Substring(1)))
            .Select(wire => solvedValues[wire].ToString())
            .StrJoin();
        var asDecimal = Convert.ToInt64(binary, 2);

        // Part 2
        var swaps = Validate(relations);

        return (asDecimal, swaps.SelectMany(t => t.ToList()).Order().StrJoin(","));

        // For exploration, to discover the recurrence relationship
        string UnNest(string gate, bool allowZ)
        {
            if (gate.StartsWith("x") || gate.StartsWith("y") || (gate.StartsWith("z") && allowZ))
                return gate;
            else
            {
                var (op, g1, g2) = relations[gate];
                return $"({op} {UnNest(g1, true)} {UnNest(g2, true)})";
            }
        }

        List<(string, string)> Validate(Dictionary<string, Wire> relations)
        {

            /*
            The recurrence relation that underpins validation:

             z(n-1) = (XOR p q)
             z(n) = (XOR 
                       (OR 
                           (AND y(n-1) x(n-1)) 
                           (AND p q)) 
                       (XOR xn yn))
            */

            var swaps = new List<(string, string)>();
            var inverseRelations = relations.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

            for (int zNum = 3; zNum < 45; zNum++)
            {
                // (AND y(n-1) x(n-1)) 
                var andPreviousXY = InverseRelation("AND", Gate("x", zNum-1), Gate("y", zNum-1));
                // z(n-1) = (XOR p q)
                var (_, p, q) = relations[Gate("z", zNum-1)]; (p, q) = (p, q).Order();
                // (AND p q)
                var andLastZ = InverseRelation("AND", p, q);
                // (OR (AND p q) (AND y(n-1) x(n-1)))
                var orAtThisLevel = (andPreviousXY, andLastZ).Order().Pipe(t => InverseRelation("OR", t.Item1, t.Item2));
                var xOrAtThisLevel = InverseRelation("XOR", Gate("x", zNum), Gate("y", zNum));

                var (e1, e2) = (orAtThisLevel, xOrAtThisLevel).Order();
                var expectedHere = InverseRelation("XOR", e1, e2);

                var (op, g1, g2) = relations[Gate("z", zNum)]; (g1, g2) = (g1, g2).Order();

                if ((op, g1, g2) != ("XOR", e1, e2))
                {
                    Swap(expectedHere, Gate("z", zNum));
                }
            }

            return swaps;

            string InverseRelation(string op, string g1, string g2)
            {
                if (inverseRelations.TryGetValue(new(op, g1, g2), out var wire))
                    return wire;
                
                var possibleSwaps = relations.Where(kvp => kvp.Value.Op == op 
                    && (kvp.Value.Gate1 == g1 || kvp.Value.Gate2 == g1 || kvp.Value.Gate1 == g2 || kvp.Value.Gate2 == g2))
                    .ToList();

                var letsSwap = possibleSwaps.First().Value;
                var lgs = new HashSet<string> { g1, g2}; var rgs = new HashSet<string> { letsSwap.Gate1, letsSwap.Gate2};
                var leftSwap = lgs.Except(rgs).First(); var rightSwap = rgs.Except(lgs).First();
                var same = lgs.Intersect(rgs).First();

                Swap(leftSwap, rightSwap);

                var (g1New, g2New) = (same, rightSwap).Order();

                // todo: while this solves the problem, I should probably propagate this swap upwards
                return inverseRelations[new("XOR", g1New, g2New)];
            }

            void Swap(string leftWire, string rightWire)
            {
                var (lop, lg1, lg2) = relations[leftWire]; 
                var (rop, rg1, rg2) = relations[rightWire];
                relations[leftWire] = new(rop, rg1, rg2);
                relations[rightWire] = new(lop, lg1, lg2);
                swaps.Add((leftWire, rightWire));
                inverseRelations = relations.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            }

            string Gate(string letter, int i) =>
                letter + (i < 10 ? "0" + i : i);
        }

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