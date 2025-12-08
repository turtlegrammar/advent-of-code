using System.Linq;
using System.Collections.Generic;
using System.Data;
using static Advent.Extensions;
using System.Linq;

using Advent;
using System.Text.RegularExpressions;
namespace Advent2023;

public static class Day1
{
    public static (long, long) Run(string file)
    {
        var numMapping = Dictionary(
            ("one", "1"), ("two", "2"), ("three", "3"), ("four", "4"),
            ("five", "5"), ("six", "6"), ("seven", "7"), ("eight", "8"), ("nine", "9"));

        var lines = File.ReadAllLines(file);

        var part1 = lines.Select(l => l.Where(c => c >= '0' && c <= '9').StrJoin().Pipe(s => Parse.Long($"{s.First()}{s.Last()}"))).Sum();

        var parsedLines2 = lines
            .Select(l => {
                var first = Regex.Matches(l, "(\\d|one|two|three|four|five|six|seven|eight|nine)").Select(m => m.Value).First();
                var last = new Regex("(\\d|one|two|three|four|five|six|seven|eight|nine)", RegexOptions.RightToLeft).Matches(l).Select(m => m.Value).First();
                return Parse.Long(ParseDigit(first) + ParseDigit(last));
            }).ToList();

        return (part1, parsedLines2.Sum());

        string ParseDigit(string s) =>
            numMapping.TryGetValue(s, out var d) ? d : s;
    } 
}