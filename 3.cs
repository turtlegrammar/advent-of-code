using System.Text.RegularExpressions;

namespace Advent;

public static class Day3
{
    public static int Parse(string content)
    {
        var mulregex = new Regex(@"(mul\([0-9]+,[0-9]+\)|do\(\)|don't\(\))");
        var mulmatches = mulregex.Matches(content).ToList();
        var muls = mulmatches.Select(m => m.Value).ToList();

        var enabled = true;
        var eval = 0;

        foreach (var exp in muls)
        {
            if (exp == "do()")
                enabled = true;
            else if (exp == "don't()")
                enabled = false;
            else if (enabled)
                eval += ExtractProduct(exp);
        }

        return eval;

        int ExtractProduct(string mul)
        {
            var arr = mul.Split(",");
            return Int32.Parse(arr[0].Substring(4)) * Int32.Parse(arr[1].Substring(0, arr[1].Length - 1));
        }
    }

    public static int Run(string file) =>
        Parse(File.ReadAllText(file));
}