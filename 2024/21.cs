using Advent; namespace Advent2024;

using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using static Advent.Extensions;

public static class Day21
{
    public static (long, long) Run(string file)
    {

        var numericKeypad = Dictionary(
            ('7', (0, 0)), ('8', (0, 1)), ('9', (0, 2)), 
            ('4', (1, 0)), ('5', (1, 1)), ('6', (1, 2)), 
            ('1', (2, 0)), ('2', (2, 1)), ('3', (2, 2)), 
            ('X', (3, 0)), ('0', (3, 1)), ('A', (3, 2))
        );

        var directionalKeypad = Dictionary(
            ('X', (0, 0)), ('^', (0, 1)), ('A', (0, 2)),
            ('<', (1, 0)), ('v', (1, 1)), ('>', (1, 2))
        );

        var numericKeyToKeyPaths = BuildKeyToKeyPaths(numericKeypad);
        var directionalKeyToKeyPaths = BuildKeyToKeyPaths(directionalKeypad);

        var costCache = new Dictionary<(string, int), long>();

        var cache = new Dictionary<(string, char), HashSet<string>>();

        var codes = File.ReadAllLines(file);

        var part1 = codes.Select(c => CleverEfficientComplexity(c, 2)).Select(t => t.Item1 * t.Item2).Sum();
        var part2 = codes.Select(c => CleverEfficientComplexity(c, 25)).Select(t => t.Item1 * t.Item2).Sum();

        return (part1, part2);

        (long, long) NaiveInefficientComplexity(string code, int directionalRobotKeyPadCount)
        {
            var xs = TypeOut(code, 'A', numericKeypad, numericKeyToKeyPaths).KeepSmallest().ToHashSet();
            var iterative = xs;
            for (int i = 0; i < directionalRobotKeyPadCount; i++)
                iterative = iterative.SelectMany(x => TypeOut(x, 'A', directionalKeypad, directionalKeyToKeyPaths)).KeepSmallest().ToHashSet();
            var length = iterative.MinBy(z => z.Count()).Count();
            return (length, Parse.Long(code));
        }

        (long, long) CleverEfficientComplexity(string code, int directionalRobotKeyPadCount)
        {
            var xs = TypeOut(code, 'A', numericKeypad, numericKeyToKeyPaths).KeepSmallest().ToHashSet();
            return (xs.Select(x => MemoizedComplexity(x, directionalRobotKeyPadCount)).Min(), Parse.Long(code));
        }

        long MemoizedComplexity(string code, int depth)
        {
            var keypad = directionalKeypad;
            var paths = directionalKeyToKeyPaths;

            if (costCache.TryGetValue((code, depth), out var cachedResult))
                return cachedResult;
            else if (depth == 0)
                return code.Length;
            else
            {
                var possibilities = TypeOut(code, 'A', keypad, paths).KeepSmallest();
                var smallests = new List<long>();
                foreach (var possibility in possibilities)
                {
                    var split = possibility.Split("A").Select(p => p + "A").ToList();
                    var smallest = split.Select(s => MemoizedComplexity(s, depth - 1)).Sum();
                    smallests.Add(smallest);
                }
                return costCache[(code, depth)] = smallests.Min() -1;
            }
        }


        List<string> TypeOut(string sequence, char cursor, Dictionary<char, (int, int)> keypad, Dictionary<char, Dictionary<char, List<string>>> paths)
        {
            if (sequence == "")
                return List("");

            var here = cursor == sequence[0] 
                ? List("A")
                : paths[cursor][sequence[0]].Select(p => p + "A");

            return TypeOut(sequence.Substring(1), sequence[0], keypad, paths).SelectMany(p => here.Select(h => h + p)).ToList();
        }

        Dictionary<char, Dictionary<char, List<string>>> BuildKeyToKeyPaths(Dictionary<char, (int, int)> keypad)
        {
            var neighbors = keypad.Keys.ToDictionary(
                c => c,
                c => keypad.Keys.Where(k => k != c && k != 'X' && keypad[c].ManhattanDistance(keypad[k]) == 1).ToDictionary(x => x, _ => 1L)
            );

            return keypad.Keys.Where(x => x != 'X').ToDictionary(
                k => k,
                k => {
                    var position = keypad[k];
                    var (_, parents) = Algorithms.ShortestPathsFrom(k, neighbors);
                    var paths = keypad.Keys.Where(x => x != 'X').ToDictionary(
                        k2 => k2, 
                        k2 => 
                             Algorithms.EnumeratePaths(parents, k2).Select(pss => 
                                List(k).Concat(pss).Zip(pss).Select(tup =>
                                    StringifyDirection(keypad[tup.First].Subtract(keypad[tup.Second]))
                                ).StrJoin())
                                .ToList()
                    );
                    return paths;
                }
            );
        }

        char StringifyDirection((int, int) d) =>
            d == Direction.Right ? '<'
            : d == Direction.Left ? '>'
            : d == Direction.Up ? 'v'
            : d == Direction.Down ? '^'
            : throw new Exception("NO!");
    }
}