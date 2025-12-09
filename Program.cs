using Advent;
using Advent2025;

var modifier = "";
if (args.Length > 0 && args[0] == "demo")
    modifier = "-demo";
Console.WriteLine(Day9.Run($"inputs/2025/day-9{modifier}.txt"));
