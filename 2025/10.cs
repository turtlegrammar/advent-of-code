namespace Advent2025;

using System.Data;
using static Advent.Extensions;
using System.Linq;
using Advent;
using System.Collections.Generic;
using System.Reflection.Metadata;

public static class Day10
{
    public record Machine(string TargetState, List<int[]> Wiring, List<int> Joltage);

    public static (long, long) Run(string file)
    {
        var lines = File.ReadAllLines(file);
        var machines = lines.Select(ParseMachine).ToList();
        var paths = machines.Select(ShortestPath).ToList();
        // Console.WriteLine(paths.StrJoin(" "));

        return (paths.Sum(), 0);

        int ShortestPath(Machine m)
        {
            var visisted = new HashSet<string>();
            var queue = new Queue<(string, int)>();
            queue.Enqueue((m.TargetState.Select(_ => ".").StrJoin(""), 0));
            while (queue.TryDequeue(out var here))
            {
                var (currentState, i) = here;
                if (currentState == m.TargetState)
                    return i;
                if (!visisted.Contains(currentState))
                {
                    visisted.Add(currentState);
                    var next = m.Wiring.Select(w => ApplyWiring(currentState, w)).ToList();
                    // Console.WriteLine($"{here}  =>  {next.StrJoin(", ")}");
                    foreach (var nextState in next)
                        if (!visisted.Contains(nextState))
                            queue.Enqueue((nextState, i + 1));
                }
            }
            return Int32.MaxValue;
        }

        string ApplyWiring(string indicatorDiagram, int[] wiring)
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
            var joltage = Parse.Ints(bySpace.Last()).ToList();
            var wiring = bySpace.Slice(1, bySpace.Count-2).Select(Parse.Ints).ToList();
            return new(targetState, wiring, joltage);
        }
    }
}