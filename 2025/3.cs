namespace Advent2025;

using System.Data;
using static Advent.Extensions;
using Advent;

public static class Day3
{
    public static (long, long) Run(string file)
    {
        var banks = File.ReadAllLines(file).Select(line => line.Select(c => c - '0').ToList()).ToList();
        var maxes = banks.Select(b => MaxJoltage(b, 2));
        var twelveMaxes = banks.Select(b => MaxJoltage(b, 12));

        return (maxes.Sum(), twelveMaxes.Sum());

        long MaxJoltage(List<int> bank, int batteries)
        {
            var startingPoint = 0;
            var joltage = "";
            for (int i = 1; i <= batteries; i++)
            {
                var allButLasts = bank.Slice(startingPoint, bank.Count - startingPoint - (batteries - i));
                var leadingBattery = allButLasts.Max();
                var indexOfLeadingBattery = allButLasts.IndexOf(leadingBattery);

                startingPoint = startingPoint + indexOfLeadingBattery + 1;
                joltage += leadingBattery.ToString();
            }
            return Parse.Long(joltage);
        }
    }
}