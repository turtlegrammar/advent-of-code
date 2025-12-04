using Advent;
using Advent2025;

var modifier = "";
if (args.Length > 0 && args[0] == "demo")
    modifier = "-demo";
Console.WriteLine(Day4.Run($"/home/coder/code/advent-of-code/inputs/2025/day-4{modifier}.txt"));
