namespace Advent2025;

using System.Data;
using static Advent.Extensions;
using System.Linq;
using Advent;
using System.Collections.Generic;
using System.Collections.Concurrent;
public static class Day10
{
    public record Variable(char Name);
    public record Equation(List<Variable> Variables, long Sum);
    
    public record ConstraintSystem(List<Equation> Equations, Dictionary<char, long> MaxValues, List<char> OrderedVariables, long MinimumSolution);

    public record Machine(string TargetState, List<long[]> Wiring, List<long> Joltage);

    public static (long, long) Run(string file)
    {
        var lines = File.ReadAllLines(file);
        var machines = lines.Select(ParseMachine).ToList();
        var part1 = machines.Select(ShortestPathWiring).ToList();

        var constraintSystems = machines.Select(MachineToConstraints).ToList();

        // 152 takes a long time...
        var part2 = constraintSystems.Select(MinimumSumOfSolutionToConstraintSystem).Sum();

        return (part1.Sum(),part2);

        bool ConstraintsValid(Dictionary<char, (long, long)> cs) =>
            cs.All(kvp => kvp.Value.Item1 <= kvp.Value.Item2);

        bool MakeBasicExtrapolationsFromEquations(List<Equation> equations)
        {
            var didSomething = true;
            var haveMerged = new HashSet<(string, string)>();
            var equationExists = equations.Select(Equation2ToString).ToHashSet();
            while (didSomething)
            {
                var toAdd = new List<Equation>();
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
                            var newEq = new Equation(f.Variables.Where(v => !e.Variables.Any(ev => ev.Name == v.Name)).ToList(), f.Sum - e.Sum);
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

        string Equation2ToString(Equation eq)
        {
            return $"{eq.Variables.Select(v => v.Name).StrJoin(" + ")} = {eq.Sum}";
        }

        (bool Valid, Dictionary<char, (long, long)> Constraints) RefineConstraints(Dictionary<char, (long, long)> starting, ConstraintSystem system)
        {
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

        long MinimumSumOfSolutionToConstraintSystem(ConstraintSystem system, int i = 0)
        {
            MakeBasicExtrapolationsFromEquations(system.Equations);
            // system.Equations.Select(Equation2ToString).ForEach(Console.WriteLine);

            var (solutions, iterations) = ParallelSolveConstraintSystem(system).Result;
            var bestSolution = solutions.MinBy(s => s.Values.Sum());
            Console.WriteLine($"{i} - Solution: {bestSolution.Values.Sum()}; Iterations: {iterations}\n");
            return bestSolution.Values.Sum();
        }

        // This doesn't actually do the work in parallel - it kicks off multiple instances in parllel
        // with different initial state (order of variables to walk through), shares the best known solution
        // between threads, and then exists as soon as the first thread is done.
        // I found that different initial states affected the performance significantly, but could never quite figure out
        // the perfect heuristic for ordering variables. So, why not just shuffle randomly and see which is best...
        async Task<(List<Dictionary<char, long>> Solutions, long Iterations)> ParallelSolveConstraintSystem(ConstraintSystem system)
        {
            var cts = new CancellationTokenSource();
            var state = new ConcurrentDictionary<string, object>();
            state.TryAdd("bestSolutionThusFar", Int64.MaxValue);
            var sharedSolutions =  new ConcurrentStack<Dictionary<char, long>>();
            state.TryAdd("solutions", sharedSolutions);
            var attempts = Enumerable.Range(0, 5).Select(i => Task.Run(() => 
                SolveConstraintSystem(system, cts.Token, i != 0, state))).ToList();
            var (_, iterations) = await await Task.WhenAny(attempts);
            cts.Cancel();
            return (sharedSolutions.ToList(), iterations);
        }

        (List<Dictionary<char, long>> Solutions, long Iterations) SolveConstraintSystem(
            ConstraintSystem system, 
            CancellationToken? ct,
            bool shuffle,
            ConcurrentDictionary<string, object> sharedState)
        {
            var (_, constraints) = RefineConstraints(system.MaxValues.ToDictionary(kvp => kvp.Key, kvp => (0L, kvp.Value)), system);
            var variables = 
                system.Equations.SelectMany(e => e.Variables.Select(v => v.Name))
                    .Distinct()
                    .OrderByDescending(v => system.Equations.Where(e => e.Variables.Any(ev => ev.Name == v)).Count())
                    .ToArray();
            if (shuffle)
                variables.Shuffle();
            var state = new Dictionary<char, long>();
            var cursor = 0;
            var iterations = 0;
            var solutions = new List<Dictionary<char, long>>();
            var bestSolutionThusFar = (long)sharedState["bestSolutionThusFar"];

            while (cursor >= 0 && !(ct.HasValue && ct.Value.IsCancellationRequested))
            {
                bestSolutionThusFar = (long)sharedState["bestSolutionThusFar"];
                // This is ugly, but basically, rebuild constraints, then modify the constraint
                // on the current variable to be inclusive of its current value to its max value
                var stateCopy = state.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                var merge = new Dictionary<char, (long, long)>();
                if (cursor < variables.Length && stateCopy.ContainsKey(variables[cursor]))
                    merge[variables[cursor]] = (stateCopy[variables[cursor]], system.MaxValues[variables[cursor]]);
                var redoneConstraints = system.MaxValues.ToDictionary(kvp => kvp.Key, kvp => (0L, kvp.Value))
                        .MergeWith(stateCopy.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value, kvp.Value)), (left, right) => right)
                        .MergeWith(merge, (left, right) => right);
                var (constraintsValid, constraintsX) = RefineConstraints(redoneConstraints, system);
                constraints = constraintsX;
                // if (iterations % 10000 == 0)
                // {
                    // Console.WriteLine(constraints.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Value.Item1} <= {kvp.Key} <= {kvp.Value.Item2}").StrJoin(", "));
                    // Console.WriteLine(state.OrderBy(kvp => kvp.Key).StrJoin(" "));
                // }

                if (FoundSolution())
                {
                    // Console.WriteLine($"Found solution!: {iterations} = {state.Values.Sum()} (compared to {bestSolutionThusFar})");
                    // Console.WriteLine($"Solution: {state.OrderBy(kvp => kvp.Key).StrJoin("")}");
                    solutions.Add(state.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                    bestSolutionThusFar = Math.Min(state.Values.Sum(), bestSolutionThusFar);
                    var sharedSolutions = (ConcurrentStack<Dictionary<char, long>>)sharedState["solutions"];
                    sharedSolutions.Push(state.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                    sharedState.AddOrUpdate("bestSolutionThusFar", bestSolutionThusFar, (k, x) => Math.Min((long)x, bestSolutionThusFar));
                    // Console.WriteLine($"Best solution thus far now: {bestSolutionThusFar}");
                }

                iterations++;
                if (NeedToBackTrack(constraintsValid, out var reason))
                {
                    // Can we increase the current cursor?
                    // If so, do it, and move on.
                    // Otherwise, remove this, increment the last cursor.
                    if (cursor < variables.Length)
                    {
                        // Is it possible to keep incrementing the value at the current cursor?
                        if (
                            state.ContainsKey(variables[cursor]) 
                            && state[variables[cursor]] < constraints[variables[cursor]].Item2
                            && state.Values.Sum() < bestSolutionThusFar
                            && system.Equations.All(eq => MaximumBoundEq(eq) < eq.Sum || MinimumBoundEq(eq) > eq.Sum)
                            && PossibleToEventuallyReachSolution()
                            && !BestCaseSolutionSucks()
                            )
                        {
                            state[variables[cursor]] = Math.Max(state[variables[cursor]] + 1, constraints[variables[cursor]].Item1);
                        }
                        // If not, we need to reset it and step back.
                        else
                        {
                            state.Remove(variables[cursor]);
                            cursor--;
                            if (cursor >=0)
                                state[variables[cursor]]++;
                        }
                    }
                    else if (cursor >= variables.Length)
                    {
                        cursor--;
                    }
                }
                else
                {
                    if (!state.ContainsKey(variables[cursor]))
                        state[variables[cursor]] = constraints[variables[cursor]].Item1;
                    cursor++;
                }
            }

            return (solutions, iterations);

            bool PossibleToEventuallyReachSolution() =>
                (state.Values.Sum() - (state.TryGetValue(variables[cursor], out var cv) ? cv : 0) 
                + variables.Skip(cursor).Select(v => constraints[v].Item2).Sum()) >= system.MinimumSolution;

            bool BestCaseSolutionSucks() =>
                (state.Values.Sum() + variables.Skip(cursor+1).Select(v => constraints[v].Item1).Sum()) >= bestSolutionThusFar;

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
                else if (BestCaseSolutionSucks())
                {
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

            long EvalEq(Equation eq) => 
                eq.Variables.Select(v => state.TryGetValue(v.Name, out var value) ? value : 0).Sum();

            // doesn't work with coefficients
            long MinimumBoundEq(Equation eq) =>
                eq.Variables.Select(v => constraints.TryGetValue(v.Name, out var value) ? value.Item1 : 0).Sum();

            long MaximumBoundEq(Equation eq) =>
                eq.Variables.Select(v => constraints.TryGetValue(v.Name, out var value) ? value.Item2 : 0).Sum();
        }

        ConstraintSystem MachineToConstraints(Machine m)
        {
            /*
            If we have the following wire specs:
            a: [0, 0, 0, 1]
            b: [0, 1, 0, 1]
            c: [0, 0, 1, 0]
            d: [0, 0, 1, 1]
            e: [1, 0, 1, 0]
            f: [1, 1, 0, 0]

            And the target joltage is {3, 5, 4, 7}, then we can express that as a system of equations:

            e + f = 3
            b + f = 5
            c + d + e = 4
            a + b + d = 7

            Finding the minimum number of wire applications to get the joltage is 
            equivalent to finding a minimum (a, b, c, d, e, f) that satisfy the above.

            */
            var wiresToEqs = m.Wiring.Select(GenVector).ToList();
            var equations = new List<Equation>();
            for (var column = 0; column < m.Joltage.Count; column++)
            {
                var variables = new List<Variable>();
                for (var weq = 0; weq < wiresToEqs.Count; weq++)
                    if (wiresToEqs[weq][column] > 0)
                        variables.Add(new Variable((char)('a' + (char)weq)));
                equations.Add(new Equation(variables, m.Joltage[column]));
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
                    foreach (var nextState in next)
                        if (!visisted.Contains(nextState))
                            queue.Enqueue((nextState, i + 1));
                }
            }
            return Int64.MaxValue;
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
    }
}