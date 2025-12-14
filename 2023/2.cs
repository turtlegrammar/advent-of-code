using System.Data;
using static Advent.Extensions;

using Advent;
using System.Text.RegularExpressions;
namespace Advent2023;

public static class Day2
{
    record Game(int Id, List<Dictionary<string, int>> Draws);
    public static (long, long) Run(string file)
    {
        var lines = File.ReadAllLines(file);
        var games = lines.Select(ParseGame).ToList();

        var part1 = games.Where(g =>
            g.Draws.All(draw => 
                (draw.TryGetValue("red", out var r) ?  r<= 12 : true ) &&
                (draw.TryGetValue("green", out var g) ?  g<= 13 : true ) &&
                (draw.TryGetValue("blue", out var b) ?  b<= 14 : true )
            )
        ).Select(g => g.Id).ToList();

        var part2 = games.Select(g => g.Draws.MergeWith((x, y) => Math.Max(x, y)).Values.Aggregate((x, y) => x * y)).Sum();

        return (part1.Sum(), part2);

        Game ParseGame(string line)
        {
            var drawDicts = new List<Dictionary<string, int>>();
            var gameId = Parse.Int(Regex.Match(line, "(\\d+)").Value);
            var draws = line.Split(": ").Last().Split("; ");

            return new Game(
                gameId, 
                draws.Select(d => 
                    Regex.Matches(d, "(\\d+ [a-z]+)")
                    .Select(m => m.Value.Split(" ").Pipe(s => (s[1], Parse.Int(s[0]))))
                    .ToDictionary())
                .ToList()
            );
        }
    } 
}