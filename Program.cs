using Advent;
using Advent2024;

var modifier = "";
if (args.Length > 0 && args[0] == "demo")
    modifier = "-demo";
Console.WriteLine(Day25.Run($"inputs/2024/day-25{modifier}.txt"));
