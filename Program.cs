using Advent;
using Advent2025;

var modifier = "";
if (args.Length > 0 && args[0] == "demo")
    modifier = "-demo";
Console.WriteLine(Day6.Run($"inputs/2025/day-6{modifier}.txt"));
