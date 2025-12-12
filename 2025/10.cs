namespace Advent2025;

using System.Data;
using static Advent.Extensions;
using System.Linq;
using Advent;
using System.Collections.Generic;
using System.Reflection.Metadata;


/*
c + d + g + i + k = 73
c + d + g + i + j + k + l = 92
a + f + g + h + i + k = 75
a + d + e + g + i = 46
a + b + d + g + i + l = 75
d + f + k = 30
a + e + g + k + m = 45
a + d + e + h + j + l = 37
a + d + f + g + h + i + k + l + m = 109
a + d + e + i + j + m = 39

c + d + g + i + k = 73
c + d + g + i + j + k + l = 92
=>
j + l = 19

a + f + g + h + i + k = 75
a + d + f + g + h + i + k + l + m = 109
=> d + l + m => 24

d + f + k = 30
a + d + f + g + h + i + k + l + m = 109
=>
a + g + h + i + l + m => 79

a + g + h + i + l + m => 79
d + l + m => 24
j + l = 19
substitutions:
c + d + g + i + k = 73
c + d + g + i + j + k + l = 92 -> c + d + g + i + k = 73
a + f + g + h + i + k = 75
a + d + e + g + i = 46
a + b + d + g + i + l = 75
d + f + k = 30
a + e + g + k + m = 45
a + d + e + h + j + l = 37
a + d + f + g + h + i + k + l + m = 109   -> a + d + f + k = 30 AND a + f + g + h + i + k = 
a + d + e + i + j + m = 39


*/

public static class Day10
{
    public record Variable(char Name, int Coefficient);
    public record Equation(List<char> Variables, long Sum);
    public class Equation2
    {
        public Equation2(List<Variable> variables, long sum)
        {
            Variables = variables; Sum = sum;
        }

        public long Sum { get; set; }
        public List<Variable> Variables { get; set; }
    }
    public record ConstraintSystem(List<Equation2> Equations, Dictionary<char, long> MaxValues, List<char> OrderedVariables, long MinimumSolution);

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

        // var (m, e) = (machines[5], eqs[5]);

        // Console.WriteLine("old method: ");
        // TestConstraintSolver(e);

        var part2 = eqs.Select(TestConstraintSolver).Sum();

        return (part1.Sum(),part2);// eqs.Select(MinimalButtonPressesForJoltage).Sum());

        bool ConstraintsValid(Dictionary<char, (long, long)> cs) =>
            cs.All(kvp => kvp.Value.Item1 <= kvp.Value.Item2);

        static Equation2 ToEquation2(Equation eq) =>
            new Equation2(eq.Variables.Select(v => new Variable(v, 1)).ToList(), eq.Sum);

        bool SimplifySystem2(List<Equation2> equations)
        {
            var didSomething = true;
            var haveMerged = new HashSet<(string, string)>();
            var equationExists = equations.Select(Equation2ToString).ToHashSet();
            while (didSomething)
            {
                var toAdd = new List<Equation2>();
                didSomething = false;
                foreach (var e in equations)
                    foreach (var f in equations)
                    {
                        // if all of e is contained within f
                        if (e.Variables.Any() && f.Variables.Any() 
                            && e != f 
                            && e.Variables.All(v => f.Variables.Any(fv => fv.Name == v.Name))
                            && f.Variables.Count > e.Variables.Count
                            && !haveMerged.Contains((Equation2ToString(e), Equation2ToString(f))))
                        {
                            // Console.WriteLine($"Merging {Equation2ToString(e)} into {Equation2ToString(f)}");
                            var newEq = new Equation2(f.Variables.Where(v => !e.Variables.Any(ev => ev.Name == v.Name)).ToList(), f.Sum - e.Sum);
                            if (!equationExists.Contains(Equation2ToString(newEq)))
                            {
                                toAdd.Add(newEq);
                                equationExists.Add(Equation2ToString(newEq));
                                haveMerged.Add((Equation2ToString(e), Equation2ToString(f)));
                                didSomething = true;
                            }
                        }
                    }
                equations.AddRange(toAdd);
            }
            return didSomething;
        }

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

            // Console.WriteLine($"Substituting {mostUsedVariable}, equivalent exp: {Equation2ToString(equivalentExpression)}");

            return equations.Where(e => e != smallestEquation)
                .SelectMany(e => Substitute(e, mostUsedVariable, equivalentExpression))
                .ToList();

            List<Equation2> Substitute(Equation2 eq, char varName, Equation2 expression)
            {
                if (!eq.Variables.Any(v => v.Name == varName))
                {
                    return List(eq);
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
                // Console.WriteLine($"Transformed {Equation2ToString(eq)} into {Equation2ToString(afterSubstitution)}");

                return List(afterSubstitution, eq);
            }
        }

        string Equation2ToString(Equation2 eq)
        {
            string Coefficient(long c) => 
                c == 1 ? "" : c == -1 ? "-" : c.ToString();
            return $"{eq.Variables.Select(v => $"{Coefficient(v.Coefficient)}{v.Name}").StrJoin(" + ")} = {eq.Sum}";
        }

        (bool Valid, Dictionary<char, (long, long)> Constraints) RefineConstraints(Dictionary<char, (long, long)> starting, ConstraintSystem system)
        {
            // return Go(Go(Go(Go(starting))));
            var last = starting;
            do
            {
                var (valid, next) = Go(last);
                if (!valid)
                    return (valid, next);
                else if (next.All(kvp => last[kvp.Key] == kvp.Value))
                    return (true, next);
                else
                    last = next;
            } while (true);

            (bool Valid, Dictionary<char, (long, long)> Constraints) Go(Dictionary<char, (long, long)> starting)
            {
                var result = starting.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                foreach (var equation in system.Equations)
                {
                    foreach (var v in equation.Variables)
                    {
                        var newMin = (equation.Sum - equation.Variables.Where(vv => vv != v)
                            .Select(vv => result[vv.Name].Item2).Sum());
                        var newMax = (equation.Sum - equation.Variables.Where(vv => vv != v)
                            .Select(vv => result[vv.Name].Item1).Sum());
                        if (newMin <= newMax)
                            result[v.Name] = ConstrainRange(result[v.Name], (newMin, newMax));
                        else
                            return (false, result);
                    }
                }
                return (true, result);
            }

            (long, long) ConstrainRange((long, long) r1, (long, long) r2) =>
                (Math.Max(r1.Item1, r2.Item1), Math.Min(r1.Item2, r2.Item2));
        }

        long MinimalButtonPressesForJoltage(ConstraintSystem system)
        {
            var (solutions, iterations) = SolveConstraint(system);
            if (!solutions.Any())
            {
                // Console.WriteLine($"F'd ");
                return 0;
            }
            var bestSolution = solutions.MinBy(s => s.Values.Sum());
            return bestSolution.Values.Sum();
        }

        long TestConstraintSolver(ConstraintSystem system, int i = 0)
        {
            // system.Equations.Select(Equation2ToString).ForEach(Console.WriteLine);
            // Console.WriteLine("After simplification:");
            SimplifySystem2(system.Equations);
            // Console.WriteLine($"Going to solve {i}");
            // system.Equations.Select(Equation2ToString).ForEach(Console.WriteLine);

            var (solutions, iterations) = ParallelSolveConstraint(system).Result;
            var bestSolution = solutions.MinBy(s => s.Values.Sum());
            Console.WriteLine($"{i} - Solution: {bestSolution.Values.Sum()}; Iterations: {iterations}\n");
            return bestSolution.Values.Sum();
            // solutions.ForEach(s => Console.WriteLine(s.Select(kvp => $"{kvp.Key}={kvp.Value}").StrJoin(", ")));
            // Console.WriteLine(bestSolution.Select(kvp => $"{kvp.Key}={kvp.Value}").StrJoin(", "));
            // Console.WriteLine(bestSolution.Values.Sum());
        }

        async Task<(List<Dictionary<char, long>> Solutions, long Iterations)> ParallelSolveConstraint(ConstraintSystem system)
        {
            var cts = new CancellationTokenSource();
            var attempts = Enumerable.Range(0, 10).Select(i => Task.Run(() => SolveConstraint(system, cts.Token, i != 0))).ToList();
            var firstResult = await await Task.WhenAny(attempts);
            cts.Cancel();
            return firstResult;
        }

        (List<Dictionary<char, long>> Solutions, long Iterations) SolveConstraint(ConstraintSystem system, CancellationToken? ct = null, bool shuffle = false)
        {
            // var variables = system.Equations.SelectMany(e => e.Variables.Select(v => v.Name)).Distinct().Order().ToList(); // system.MaxValues.Keys.Order().ToList();
            // var variables = system.OrderedVariables;
            var variables = 
                system.Equations.SelectMany(e => e.Variables.Select(v => v.Name))
                    .Distinct()
                    .OrderByDescending(v => system.Equations.Where(e => e.Variables.Any(ev => ev.Name == v)).Count())
                    .ToArray();
            if (shuffle)
                variables.Shuffle();
            // Console.WriteLine(variables.StrJoin(", "));
            var state = new Dictionary<char, long>();
            var cursor = 0;
            var iterations = 0;
            var solutions = new List<Dictionary<char, long>>();
            var bestSolutionThusFar = Int64.MaxValue;
            var constraints = system.MaxValues.ToDictionary(kvp => kvp.Key, kvp => (0L, kvp.Value));

            while (cursor >= 0 && !(ct.HasValue && ct.Value.IsCancellationRequested))
            {
                var stateCopy = state.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                var merge = new Dictionary<char, (long, long)>();
                if (cursor < variables.Length && stateCopy.ContainsKey(variables[cursor]))
                    merge[variables[cursor]] = (stateCopy[variables[cursor]], system.MaxValues[variables[cursor]]);
                    // stateCopy[variables[cursor]] = stateCopy.Remove(variables[cursor]);
                var redoneConstraints = system.MaxValues.ToDictionary(kvp => kvp.Key, kvp => (0L, kvp.Value))
                        .MergeWith(stateCopy.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value, kvp.Value)), (left, right) => right)
                        .MergeWith(merge, (left, right) => right);
                var (constraintsValid, constraintsX) = RefineConstraints(redoneConstraints, system);
                constraints = constraintsX;
                // constraints = RefineConstraints(system.MaxValues.ToDictionary(kvp => kvp.Key, kvp => (0L, kvp.Value))
                //         .MergeWith(state.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value, kvp.Value)), (left, right) => right), system);
                if (iterations % 10000 == 0)
                {
                    // Console.WriteLine(constraints.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Value.Item1} <= {kvp.Key} <= {kvp.Value.Item2}").StrJoin(", "));
                    // Console.WriteLine(state.OrderBy(kvp => kvp.Key).StrJoin(" "));
                }
                // Console.WriteLine(state.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key} = {kvp.Value}").StrJoin(", "));

                if (FoundSolution())
                {
                    // Console.WriteLine($"Found solution!: {iterations} = {state.Values.Sum()} (compared to {bestSolutionThusFar})");
                    // Console.WriteLine($"Solution: {state.OrderBy(kvp => kvp.Key).StrJoin("")}");
                    solutions.Add(state.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                    bestSolutionThusFar = Math.Min(state.Values.Sum(), bestSolutionThusFar);
                    // Console.WriteLine($"Best solution thus far now: {bestSolutionThusFar}");
                }

                iterations++;
                if (NeedToBackTrack(constraintsValid, out var reason))
                {
                    // Can we increase the current cursor?
                    // If so, do it, and move on.
                    // Otherwise, remove this, increment the last cursor.
                    // Console.WriteLine($"Backtracking: {reason} ");
                    if (cursor < variables.Length)
                    {
                        // if (state.ContainsKey(variables[cursor]))
                        // {
                        //     var stateCopy2 = state.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                        //     stateCopy2.Remove(variables[cursor]);
                        //     var redoneConstraints2 = system.MaxValues.ToDictionary(kvp => kvp.Key, kvp => (0L, kvp.Value))
                        //             .MergeWith(stateCopy2.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value, kvp.Value)), (left, right) => right);
                        //     var (_, constraints2) = RefineConstraints(redoneConstraints2, system);
                        //     constraints = constraints2;
                        // }
                        if (
                            state.ContainsKey(variables[cursor]) 
                            && state[variables[cursor]] < constraints[variables[cursor]].Item2
                            && state.Values.Sum() < bestSolutionThusFar
                            && system.Equations.All(eq => MaximumBoundEq(eq) < eq.Sum || MinimumBoundEq(eq) > eq.Sum)
                            && PossibleToEventuallyReachSolution()
                            )
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
                    else if (cursor >= variables.Length)
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
                    // var newOrder =
                    //     variables.Take(cursor).Concat(variables.Skip(cursor).OrderByDescending(v => constraints[v].Item1)).ToList();
                    // variables = newOrder;
                     // variables.OrderBy( variables.Sort(cursor, variables.Count-cursor, x => (constraints[x].Item2-constraints[x].Item1));
                    // if (cursor < variables.Count)
                }
            }

            return (solutions, iterations);

            bool PossibleToEventuallyReachSolution() =>
                (state.Values.Sum() + variables.Skip(cursor).Select(v => constraints[v].Item2).Sum()) >= system.MinimumSolution;

            bool NeedToBackTrack(bool constraintsValid, out string reason)
            {
                if (cursor >= variables.Length)
                {
                    reason = "Cursor exceeds variable count";
                    return true;
                }
                else if (!PossibleToEventuallyReachSolution())
                {
                    reason = "Impossible to allocate resources given max values that would reach the minimum solution.";
                    return true;
                }
                else if (!constraintsValid || !ConstraintsValid(constraints))
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
                    // Console.WriteLine("Backtracking because this state will never get better.");
                    reason = "This solution sucks compared to a previous one.";
                    return true;
                } 
                else if (system.Equations.Any(eq => MinimumBoundEq(eq) > eq.Sum || MaximumBoundEq(eq) < eq.Sum))
                {
                    reason = "An equation evaluates to more or less than its possible sum - can't get better.";
                    return true;
                }
                reason = "";
                return false;
            }

            bool FoundSolution() =>
                system.Equations.All(eq => EvalEq(eq) == eq.Sum);

            long EvalEq(Equation2 eq) => 
                eq.Variables.Select(v => state.TryGetValue(v.Name, out var value) ? value*v.Coefficient : 0).Sum();

            // doesn't work with coefficients
            long MinimumBoundEq(Equation2 eq) =>
                eq.Variables.Select(v => constraints.TryGetValue(v.Name, out var value) ? value.Item1 : 0).Sum();

            long MaximumBoundEq(Equation2 eq) =>
                eq.Variables.Select(v => constraints.TryGetValue(v.Name, out var value) ? value.Item2 : 0).Sum();
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

            var variablesFromMostToLeastPowerful = 
                equations.SelectMany(e => e.Variables.Select(v => v.Name))
                    .Distinct()
                    .OrderByDescending(v => equations.Where(e => e.Variables.Any(ev => ev.Name == v)).Count())
                    .ToList();

            var minimumSolution = m.Joltage.Max();

            return new (equations, maxes, variablesFromMostToLeastPowerful, minimumSolution);

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

        long ShortestPathJoltageWithMult(Machine m)
        {
            var visisted = new HashSet<string>();
            var hasBeenQueued = new HashSet<string>();
            hasBeenQueued.Add(m.Joltage.Select(_ => "0").StrJoin(","));
            // var queue = new Queue<(string, long)>();
            var queue = new PriorityQueue<(string, long), long>();
            queue.Enqueue((m.Joltage.Select(_ => "0").StrJoin(","), 0), m.Joltage.Sum());
            var target = m.Joltage.StrJoin(",");
            var mins = m.Joltage.Select(_ => 0).ToList();
            var iter = 0;
            while (queue.TryDequeue(out var here, out var p))
            {
                if (p < 0)
                    throw new Exception("neg p");
                var (currentState, i) = here;
                if (iter++ < 100)
                    Console.WriteLine(here);

                if (currentState == target)
                {
                    Console.WriteLine($"current state: {currentState}");
                    Console.WriteLine($"queue count: {queue.Count}");
                    Console.WriteLine($"Peek: {queue.Peek()}");
                    // Console.WriteLine(queue.Take(30).StrJoin(","));
                    return i;
                }
                if (!visisted.Contains(currentState))
                {
                    var parsedCurrent = currentState.Split(",").Select(Parse.Long).ToList();
                    if (parsedCurrent.Zip(mins).All (tup => tup.Item1 < tup.Item2))
                    //if (false)
                    {

                    }
                    else
                    {
                        for (int j = 0; j < parsedCurrent.Count; j++)
                        {
                            mins[j] = (int)Math.Max(mins[j], parsedCurrent[j]);
                        }
                        visisted.Add(currentState);
                        var next = m.Wiring.Select(w => ApplyWiringToJoltage(currentState, w)).Where(s => s != "").Select(n => (n, i + 1)).ToList();
                        var alsoNext = next.Select(n => (Double(n.n), n.Item2 * 2)).Where(t => t.Item1 != "").ToList();
                        // Console.WriteLine($"{here}  =>  {next.StrJoin(", ")}");
                        foreach (var nextState in next.Concat(alsoNext))
                            if (!hasBeenQueued.Contains(nextState.Item1))
                            {
                                hasBeenQueued.Add(nextState.Item1);
                                queue.Enqueue(nextState, Distance(nextState.Item1));
                            }
                        // if (!hasBeenQueued.Contains(alsoNext) && alsoNext != "")
                        // {
                        //     hasBeenQueued.Add(alsoNext);
                        //     queue.Enqueue((alsoNext, i * 2), Distance(alsoNext));
                        // }
                    }
                }
            }
            return Int64.MaxValue;

            string ApplyWiringToJoltage(string joltage, long[] wiring)
            {
                var parsedJoltage = joltage.Split(",").Select(Parse.Long).ToList();
                foreach (var i in wiring)
                    parsedJoltage[(int)i]++;
                for (int i = 0; i < m.Joltage.Count; i++)
                    if (parsedJoltage[i] > m.Joltage[i])
                        return "";
                return parsedJoltage.StrJoin(",");
            }

            string Double(string joltage)
            {
                var parsedJoltage = joltage.Split(",").Select(Parse.Long).ToList();
                for(int i = 0; i < parsedJoltage.Count; i++)
                    parsedJoltage[(int)i]*=2;
                for (int i = 0; i < m.Joltage.Count; i++)
                    if (parsedJoltage[i] > m.Joltage[i])
                        return "";
                return parsedJoltage.StrJoin(",");
            }

            long Distance(string joltage)
            {
                var parsedJoltage = joltage.Split(",").Select(Parse.Long).ToList();
                if (parsedJoltage.Zip(m.Joltage).Any(tup => tup.Item1 > tup.Item2))
                    return 9999999;
                //return (int)Math.Floor(Math.Sqrt(parsedJoltage.Zip(m.Joltage).Select(tup => (tup.Item1 - tup.Item2)*(tup.Item1 - tup.Item2)).Sum()));
                return parsedJoltage.Zip(m.Joltage).Select(tup => Math.Abs(tup.Item1 - tup.Item2)).Sum();
            }
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