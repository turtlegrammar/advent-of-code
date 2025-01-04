using static Advent.Extensions;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Advent;

public static class Day8
{
    public static char EMPTY = '.';
    public static char NULL = '_';

    public static (int, int) Run(string file)
    {
         var map = Matrix<char>.CharacterMatrixFromFile(file, _null:NULL);

         var locations = map.Select((row, col, x) => (x, (row, col)))
            .Where(t => t.Item1 != EMPTY)
            .GroupBy(tup => tup.Item1)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Select(v => v.Item2).ToList());

         var antinodes1 = locations
            .SelectMany(
                kvp => kvp.Value.OrderedPairs()
                    .SelectMany(p => Antinodes1(p.Item1, p.Item2))
                    .ToHashSet())
            .ToHashSet();

        var antinodes2 = locations
            .SelectMany(
                kvp => kvp.Value.OrderedPairs()
                    .SelectMany(p => Antinodes2(p.Item1, p.Item2))
                    .ToHashSet())
            .ToHashSet();
 
         return (antinodes1.Count, antinodes2.Count);

         List<(int, int)> Antinodes1((int, int) x, (int, int) y)
         {
            var diff = x.Subtract(y);
            return List(x.Subtract(diff), x.Add(diff), y.Subtract(diff), y.Add(diff))
                .Where(t => t != x && t != y && map.Get(t) != NULL).ToList();
         }

         HashSet<(int, int)> Antinodes2((int, int) x, (int, int) y)
         {
            var diff = x.Subtract(y);

            return IterativeSeq(x, p => p.Subtract(diff)).TakeWhile(p => map.Get(p) != NULL)
                .Concat(IterativeSeq(x, p => p.Add(diff)).TakeWhile(p => map.Get(p) != NULL))
                .Concat(IterativeSeq(y, p => p.Subtract(diff)).TakeWhile(p => map.Get(p) != NULL))
                .Concat(IterativeSeq(y, p => p.Add(diff)).TakeWhile(p => map.Get(p) != NULL))
                .ToHashSet();
         }
    }
}