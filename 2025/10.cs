namespace Advent2025;

using System.Data;
using static Advent.Extensions;
using System.Linq;
using Advent;
using System.Collections.Generic;
using System.Reflection.Metadata;

public static class Day10
{
    public record Variable(char Name, int Coefficient);
    public record Equation(List<char> Variables, long Sum);
    public record Equation2(List<Variable> Variables, long Sum);
    public record ConstraintSystem(List<Equation2> Equations, Dictionary<char, long> MaxValues);

    public record Machine(string TargetState, List<long[]> Wiring, List<long> Joltage);

    public static (long, long) Run(string file)
    {
        var lines = File.ReadAllLines(file);
        var machines = lines.Select(ParseMachine).ToList();
        var part1 = machines.Select(ShortestPathWiring).ToList();
        // Console.WriteLine(paths.StrJoin(" "));

        var eqs = machines.Select(MachineToConstraints).ToList();

        // var x = ParseMachine("[..#####.##] (1,3,4,6,7,8,9) (4) (0,2) (0,2,3,4,5,7,8,9) (3,6,7,9) (1,5,8) (0,1,2,3,4,6,8) (1,7,8) (0,1,2,3,4,8,9) (2,7,9) (0,1,2,5,6,8) (2,4,7,8) (6,8,9) {73,75,92,46,75,30,45,37,109,39}");
        // var m = MachineToConstraints(x);

        var problemChild = eqs[5];
        // problemChild.Equations.Select(e => $"{e.Variables.StrJoin(" + ")} = {e.Sum}").ForEach(Console.WriteLine);
        // var simped = SimplifySystem(problemChild.Equations.ToList());
        // Console.WriteLine("");
        // simped.Select(e => $"{e.Variables.StrJoin(" + ")} = {e.Sum}").ForEach(Console.WriteLine);
        // var simped2 = SimplifySystem(simped);
        // Console.WriteLine("");
        // simped2.Select(e => $"{e.Variables.StrJoin(" + ")} = {e.Sum}").ForEach(Console.WriteLine);
        // var simped3 = SimplifySystem(simped2);
        Console.WriteLine("");
        // simped3.Select(e => $"{e.Variables.StrJoin(" + ")} = {e.Sum}").ForEach(Console.WriteLine);

        // problemChild = problemChild with { Equations = SimplifySystem(problemChild.Equations).Union(problemChild.Equations).ToList() };
        // problemChild = problemChild with { Equations = SimplifySystem(SimplifySystem(problemChild.Equations)).Union(problemChild.Equations).ToList() };
        // problemChild = problemChild with { Equations = SimplifySystem(problemChild.Equations) };
        // problemChild.Equations.Select(e => $"{e.Variables.StrJoin(" + ")} = {e.Sum}").ForEach(Console.WriteLine);

        // TestConstraintSolver(problemChild);
        // problemChild = problemChild with { Equations = SimplifySystem(SimplifySystem(SimplifySystem(SimplifySystem(problemChild.Equations)))).Union(problemChild.Equations).ToList() };
        problemChild = problemChild with { Equations = SimplifySystem(SimplifySystem(SimplifySystem(SimplifySystem(SimplifySystem(SimplifySystem(problemChild.Equations)))))).Union(problemChild.Equations).ToList() };
        TestConstraintSolver(problemChild);

        // foreach (var eq in eqs) { TestConstraintSolver(eq); }
        // TestConstraintSolver(eqs[5]);
        // TestConstraintSolver(eqs[0]);
        // var startingConstraints = eqs[1].MaxValues.ToDictionary(kvp => kvp.Key, kvp => (0, kvp.Value));
        // Console.WriteLine(startingConstraints.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Value.Item1} <= {kvp.Key} <= {kvp.Value.Item2}").StrJoin(", "));

        // e + f - 3 = 0
        // b + f - 5 = 0
        // e -b + 2 = 0

        // demo: 6,609 -- a=1, b=2, c=0, d=4, e=0, f=3
        // real: 6,683,907 -- a=7, b=16, c=20, d=17, e=7
        // return (paths.Sum(), 0);
        return (part1.Sum(),0);// eqs.Select(MinimalButtonPressesForJoltage).Sum());

        bool ConstraintsValid(Dictionary<char, (long, long)> cs) =>
            cs.All(kvp => kvp.Value.Item1 <= kvp.Value.Item2);

        static Equation2 ToEquation2(Equation eq) =>
            new Equation2(eq.Variables.Select(v => new Variable(v, 1)).ToList(), eq.Sum);

        List<Equation2> SimplifySystem(List<Equation2> equations)
        {
            var mostUsedVariable = equations.SelectMany(e => e.Variables.Select(v => v.Name)).GroupBy(e => e).MaxBy(eg => eg.ToList().Count).Key;
            var smallestEquation = equations.Where(e => e.Variables.Any(v => v.Name == mostUsedVariable)).MinBy(e => e.Variables.Count);
            // var smallestEquation = equations.MinBy(e => e.Variables.Count);
            // var mostUsedVariable = smallestEquation.Variables.First().Name;
            var equivalentExpression = new Equation2(
                smallestEquation.Variables
                    .Where(v => v.Name != mostUsedVariable)
                    .Select(v => new Variable(v.Name, -1*v.Coefficient)).ToList(), 
                smallestEquation.Sum);

            Console.WriteLine($"Substituting {mostUsedVariable}, equivalent exp: {Equation2ToString(equivalentExpression)}");

            return equations.Where(e => e != smallestEquation)
                .Select(e => Substitute(e, mostUsedVariable, equivalentExpression))
                .ToList();

            Equation2 Substitute(Equation2 eq, char varName, Equation2 expression)
            {
                if (!eq.Variables.Any(v => v.Name == varName))
                {
                    return eq;
                }
                var nextSum = eq.Sum - expression.Sum;
                var mag = eq.Variables.Where(v => v.Name == varName).Single().Coefficient;
                // need to handle magnitude
                var newVars = eq.Variables.Concat(expression.Variables.Select(v => new Variable(v.Name, mag*v.Coefficient)))
                    .GroupBy(v => v.Name)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Select(v => v.Coefficient).Sum())
                    .Where(kvp => kvp.Value != 0)
                    .Where(kvp => kvp.Key != varName)
                    .Select(kvp => new Variable(kvp.Key, kvp.Value))
                    .ToList();

                var afterSubstitution = new Equation2(newVars, nextSum);
                Console.WriteLine($"Transformed {Equation2ToString(eq)} into {Equation2ToString(afterSubstitution)}");

                return afterSubstitution;
            }
        }

        string Equation2ToString(Equation2 eq)
        {
            string Coefficient(long c) => 
                c == 1 ? "" : c == -1 ? "-" : c.ToString();
            return $"{eq.Variables.Select(v => $"{Coefficient(v.Coefficient)}{v.Name}").StrJoin(" + ")} = {eq.Sum}";
        }

        Dictionary<char, (long, long)> RefineConstraints(Dictionary<char, (long, long)> starting, ConstraintSystem system)
        {
            // return Go(Go(Go(Go(starting))));
            var last = starting;
            do
            {
                var next = Go(last);
                if (next.All(kvp => last[kvp.Key] == kvp.Value))
                    return next;
                else
                    last = next;
            } while (true);

            Dictionary<char, (long, long)> Go(Dictionary<char, (long, long)> starting)
            {
                var result = starting.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                foreach (var equation in system.Equations)
                {
                    if (equation.Sum < 0) continue;
                    foreach (var v in equation.Variables)
                    {
                        // if (v.Coefficient < 0 || equation.Sum <= 0) continue;
                        var newMin = (equation.Sum - equation.Variables.Where(vv => vv != v)
                            .Select(vv => vv.Coefficient > 0 
                                ? result[vv.Name].Item2*vv.Coefficient
                                : result[vv.Name].Item1*vv.Coefficient
                                ).Sum())/v.Coefficient;
                        if (newMin < 0) newMin = 0;
                        var newMax = (equation.Sum - equation.Variables.Where(vv => vv != v)
                            .Select(vv => 
                                vv.Coefficient > 0
                                ? result[vv.Name].Item1*vv.Coefficient
                                : result[vv.Name].Item2 * vv.Coefficient).Sum())/v.Coefficient;
                        if (newMax > starting[v.Name].Item2) newMax = starting[v.Name].Item2;
                        if (newMin <= newMax)
                            result[v.Name] = ConstrainRange(result[v.Name], (newMin, newMax));
                    }
                }
                return result;
            }

            (long, long) ConstrainRange((long, long) r1, (long, long) r2) =>
                (Math.Max(r1.Item1, r2.Item1), Math.Min(r1.Item2, r2.Item2));
        }

        long MinimalButtonPressesForJoltage(ConstraintSystem system)
        {
            var (solutions, iterations) = SolveConstraint(system);
            if (!solutions.Any())
            {
                Console.WriteLine($"F'd ");
                return 0;
            }
            var bestSolution = solutions.MinBy(s => s.Values.Sum());
            return bestSolution.Values.Sum();
        }

        void TestConstraintSolver(ConstraintSystem system)
        {
            system.Equations.Select(e => $"{e.Variables.Select(v => $"{v.Coefficient}{v.Name}").StrJoin(" + ")} = {e.Sum}").ForEach(Console.WriteLine);
            Console.WriteLine(system.MaxValues.Select(kvp => $"{kvp.Key} <= {kvp.Value}").StrJoin($"; "));
            var (solutions, iterations) = SolveConstraint(system);
            var bestSolution = solutions.MinBy(s => s.Values.Sum());
            Console.WriteLine($"Iterations: {iterations}");
            Console.WriteLine(bestSolution.Select(kvp => $"{kvp.Key}={kvp.Value}").StrJoin(", "));
        }

        (List<Dictionary<char, long>> Solutions, long Iterations) SolveConstraint(ConstraintSystem system)
        {
            var variables = system.MaxValues.Keys.Order().ToList();
            var state = new Dictionary<char, long>();
            var cursor = 0;
            var iterations = 0;
            var solutions = new List<Dictionary<char, long>>();
            var bestSolutionThusFar = Int64.MaxValue;
            var constraints = system.MaxValues.ToDictionary(kvp => kvp.Key, kvp => (0L, kvp.Value));

            while (cursor >= 0)
            {
                var stateCopy = state.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                if (cursor < variables.Count && stateCopy.ContainsKey(variables[cursor]))
                    stateCopy.Remove(variables[cursor]);
                var redoneConstraints = system.MaxValues.ToDictionary(kvp => kvp.Key, kvp => (0L, kvp.Value))
                        .MergeWith(stateCopy.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value, kvp.Value)), (left, right) => right);
                constraints = RefineConstraints(redoneConstraints, system);
                // constraints = RefineConstraints(system.MaxValues.ToDictionary(kvp => kvp.Key, kvp => (0L, kvp.Value))
                //         .MergeWith(state.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value, kvp.Value)), (left, right) => right), system);
                if (iterations % 10000 == 0)
                    Console.WriteLine(constraints.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Value.Item1} <= {kvp.Key} <= {kvp.Value.Item2}").StrJoin(", "));
                // Console.WriteLine(state.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key} = {kvp.Value}").StrJoin(", "));

                if (FoundSolution())
                {
                    solutions.Add(state.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                    bestSolutionThusFar = state.Values.Sum();
                }

                iterations++;
                if (NeedToBackTrack(out var reason))
                {
                    // Can we increase the current cursor?
                    // If so, do it, and move on.
                    // Otherwise, remove this, increment the last cursor.
                    // Console.WriteLine($"Backtracking: {reason} ");
                    if (cursor < variables.Count)
                    {
                        if (state.ContainsKey(variables[cursor]))
                        {
                            var stateCopy2 = state.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                            stateCopy2.Remove(variables[cursor]);
                            var redoneConstraints2 = system.MaxValues.ToDictionary(kvp => kvp.Key, kvp => (0L, kvp.Value))
                                    .MergeWith(stateCopy.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value, kvp.Value)), (left, right) => right);
                            constraints = RefineConstraints(redoneConstraints2, system);
                        }
                        if (state.ContainsKey(variables[cursor]) && state[variables[cursor]] <= constraints[variables[cursor]].Item2)
                        {
                            state[variables[cursor]] = Math.Max(state[variables[cursor]] + 1, constraints[variables[cursor]].Item1);
                            // Console.WriteLine($"Set {variables[cursor]} = {state[variables[cursor]]}");
                        }
                        else
                        {
                            // Console.WriteLine($"Removing value of {variables[cursor]}");
                            state.Remove(variables[cursor]); //[variables[cursor]] = 0;
                            cursor--;
                            if (cursor >=0)
                            {
                                state[variables[cursor]]++;
                                // cursor++;
                                // Console.WriteLine($"Set {variables[cursor]} = {state[variables[cursor]]}");
                            }
                        }
                    }
                    else if (cursor >= variables.Count)
                    {
                        cursor--;
                    }
                    // else if (cursor >= 0 && state.ContainsKey(variables[cursor]))
                    // {
                    //     state[variables[cursor]]++;
                    //     // Console.WriteLine($"Set {variables[cursor]} = {state[variables[cursor]]}");
                    // }
                }
                else
                {
                    // state[variables[cursor]]++;
                    if (!state.ContainsKey(variables[cursor]))
                        state[variables[cursor]] = constraints[variables[cursor]].Item1;
                    // Console.WriteLine($"Set {variables[cursor]} = {state[variables[cursor]]}");
                    cursor++;
                    // if (cursor < variables.Count)
                }
            }

            return (solutions, iterations);

            bool NeedToBackTrack(out string reason)
            {
                if (cursor >= variables.Count)
                {
                    reason = "Cursor exceeds variable count";
                    return true;
                }
                else if (!ConstraintsValid(constraints))
                {
                    reason = "Constraints invalid";
                    return true;
                }
                else if (state.Any(cv => cv.Value < constraints[cv.Key].Item1 || cv.Value > constraints[cv.Key].Item2))
                {
                    reason = "A value in the state exceeds that which is specified by a constraint.";
                    return true;
                }
                else if (state.Values.Sum() >= bestSolutionThusFar)
                {
                    reason = "This solution sucks compared to a previous one.";
                    return true;
                } 
                // else if(system.Equations.Any(eq => EvalEq(eq) > eq.Sum))
                // {
                //     var badEq = system.Equations.First(eq => EvalEq(eq) > eq.Sum);
                //     reason = $"An equation evaluates to more than its possible sum - can only get worse from here: {badEq.Variables.StrJoin(",")} = {badEq.Sum}";
                //     return true;
                // }
                reason = "";
                return false;
            }

            bool FoundSolution() =>
                system.Equations.All(eq => EvalEq(eq) == eq.Sum);

            long EvalEq(Equation2 eq) => 
                eq.Variables.Select(v => state.TryGetValue(v.Name, out var value) ? value*v.Coefficient : 0).Sum();
        }

        ConstraintSystem MachineToConstraints(Machine m)
        {
            var wiresToEqs = m.Wiring.Select(GenVector).ToList();
            var equations = new List<Equation2>();
            for (var column = 0; column < m.Joltage.Count; column++)
            {
                var variables = new List<Variable>();
                for (var weq = 0; weq < wiresToEqs.Count; weq++)
                    if (wiresToEqs[weq][column] > 0)
                        variables.Add(new Variable((char)('a' + (char)weq), 1));
                equations.Add(new Equation2(variables, m.Joltage[column]));
            }

            var maxes = equations.Select(e => e.Variables.ToDictionary(v => v.Name, _ => e.Sum)).MergeWith((x, y) => Math.Min(x, y));

            return new (equations, maxes);

            long[] GenVector(long[] wiring)
            {
                var result = m.Joltage.Select(_ => 0L).ToArray();
                foreach (var w in wiring)
                    result[w] = 1;
                return result;
            }
        }

        long ShortestPathWiring(Machine m)
        {
            var visisted = new HashSet<string>();
            var queue = new Queue<(string, long)>();
            queue.Enqueue((m.TargetState.Select(_ => ".").StrJoin(""), 0));
            while (queue.TryDequeue(out var here))
            {
                var (currentState, i) = here;
                if (currentState == m.TargetState)
                    return i;
                if (!visisted.Contains(currentState))
                {
                    visisted.Add(currentState);
                    var next = m.Wiring.Select(w => ApplyWiringToIndicator(currentState, w)).ToList();
                    // Console.WriteLine($"{here}  =>  {next.StrJoin(", ")}");
                    foreach (var nextState in next)
                        if (!visisted.Contains(nextState))
                            queue.Enqueue((nextState, i + 1));
                }
            }
            return Int64.MaxValue;
        }

        (string, string) ApplyMultipleWirings((string, string) indicatorAndJoltage, List<long[]> wiring)
        {
            var next = indicatorAndJoltage;
            foreach (var w in wiring)
            {
                var prev = next;
                next = ApplyWiring(next, w);
                Console.WriteLine($"{prev} + ({w.StrJoin(",")}) => {next}");
            }
            return next;
        }

        (string, string) ApplyWiring((string, string) indicatorAndJoltage,  long[] wiring)
        {
            var (i, j) = indicatorAndJoltage;
            return (ApplyWiringToIndicator(i, wiring), ApplyWiringToJoltage(i, wiring));
        }

        string ApplyWiringToJoltage(string joltage, long[] wiring)
        {
            var parsedJoltage = joltage.Split(",").Select(Parse.Long).ToList();
            foreach (var i in wiring)
                parsedJoltage[(int)i]++;
            return parsedJoltage.StrJoin(",");
        }

        string ApplyWiringToIndicator(string indicatorDiagram, long[] wiring)
        {
            var arr = indicatorDiagram.ToCharArray();
            foreach (var i in wiring)
                arr[i] = arr[i] == '.' ? '#' : '.';
            return new string(arr);
        }

        Machine ParseMachine(string s)
        {
            var bySpace = s.Split(" ").ToList();
            var targetState = bySpace[0].Substring(1, bySpace[0].Length-2); 
            var joltage = Parse.Longs(bySpace.Last()).ToList();
            var wiring = bySpace.Slice(1, bySpace.Count-2).Select(Parse.Longs).ToList();
            return new(targetState, wiring, joltage);
        }

        // ApplyMultipleWirings(("....", "0,0,0,0"), new List<long[]>
        // {
        //     new long[] { 3 },
        //     new long[] { 1, 3 },
        //     new long[] { 1, 3 },
        //     new long[] { 1, 3 },
        //     new long[] { 2, 3 },
        //     new long[] { 2, 3 },
        //     new long[] { 2, 3 },
        //     new long[] { 0, 2 },
        //     new long[] { 0, 1 }
        // });

        // Console.WriteLine("");

        // ApplyMultipleWirings((".....", "0,0,0,0,0"), new List<long[]>
        // {
        //     new long[] { 0,2,3,4 },
        //     new long[] { 0,2,3,4 },
        //     new long[] { 2, 3 },
        //     new long[] { 2, 3 },
        //     new long[] { 2, 3 },
        //     new long[] { 2, 3 },
        //     new long[] { 2, 3 },
        //     new long[] { 0, 1, 2 },
        //     new long[] { 0, 1, 2 },
        //     new long[] { 0, 1, 2 },
        //     new long[] { 0, 1, 2 },
        //     new long[] { 0, 1, 2 },
        // });
    }
}