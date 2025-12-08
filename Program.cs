using Advent;
using Advent2023;

var modifier = "";
if (args.Length > 0 && args[0] == "demo")
    modifier = "-demo";
Console.WriteLine(Day3.Run($"inputs/2023/day-3{modifier}.txt"));
