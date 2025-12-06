using Advent;
using Advent2024;

var modifier = "";
if (args.Length > 0 && args[0] == "demo")
    modifier = "-demo";
Console.WriteLine(Day21.Run($"inputs/2024/day-21{modifier}.txt"));
