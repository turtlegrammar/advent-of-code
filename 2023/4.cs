using System.Data;
using static Advent.Extensions;

using Advent;
using static Advent.ParseC;
namespace Advent2023;

public static class Day4
{
    public static (long, long) Run(string file)
    {
        var parse = FileLines(Splitter("|", s => s.IntList()).Tuple(IntSubList(0).Skipping(1), IntSubList(1)));
        var cards = parse(file);

        var part1 = cards.Select(c => 
            c.Item1.Intersect(c.Item2).Count().Pipe(count => count == 0 ? 0 : (long)Math.Pow(2, count-1))
        ).Sum();

        var copies = Enumerable.Range(0, cards.Count).ToDictionary(i => i, _ => 1);
        for (int i = 0; i < cards.Count; i++)
        {
            var wins = cards[i].Item1.Intersect(cards[i].Item2).Count();
            Enumerable.Range(i + 1, wins).ToList().ForEach(j => copies[j] += copies[i]);
        }

        return (part1, copies.Values.Sum());
    } 
}