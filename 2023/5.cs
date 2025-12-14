using System.Data;
using static Advent.Extensions;

using Advent;
using static Advent.ParseC;
using System.Text.RegularExpressions;
namespace Advent2023;

public static class Day5
{
    public record MapRange(long DestinationRangeStart, long SourceRangeStart, long Size);
    record Map(string SourceCategory, string TargetCategory, List<MapRange> Ranges);

    public static (long, long) Run(string file)
    {
        var parse = FileBlocks2(
            lines => lines[0].StartsWith("seeds: "),
            FirstString().LongList(),
            ParseMap
        );

        var (seeds, maps) = parse(file);

        var part1 = seeds[0].Select(RunThroughMaps).Min();

        var seedRanges = seeds[0].Chunk(2);

        return (part1, 0);

        long RunThroughMaps(long value)
        {
            foreach (var map in maps)
                value = ApplyMap(value, map);
            return value;
        }

        long ApplyMap(long value, Map map)
        {
            foreach (var range in map.Ranges)
            {
                if (value >= range.SourceRangeStart && value < range.SourceRangeStart + range.Size)
                {
                    return range.DestinationRangeStart  + (value - range.SourceRangeStart);
                }
            }
            return value;
        }

        Map ParseMap(List<string> lines)
        {
            var header = new Regex("(\\-to\\-| )").Split(lines[0]);
            var ranges = lines.Skip(1).Select(l => l.LongList().Pipe(x => new MapRange(x[0], x[1], x[2]))).ToList();
            return new Map(header[0], header[2], ranges);
        }
    } 
}