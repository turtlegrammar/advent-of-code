using Advent; namespace Advent2024;

public static class Day13
{
    record Claw((int, int) AButton, (int, int) BButton, (long, long) Prize);
    public static int A_COST = 3;
    public static int B_COST = 1;

    public static (long, long) Run(string file)
    {
        var input = Parse.IntArrayLines(file).Chunk(4).Select(arr => new Claw((arr[0][0], arr[0][1]), (arr[1][0], arr[1][1]), (arr[2][0], arr[2][1]))).ToList();
        var input2 = input.Select(claw => claw with { Prize = (claw.Prize.Item1 + 10000000000000, claw.Prize.Item2 + 10000000000000)}).ToList();

        return (
            input.Select(Presses).Where(Reachable).Select(Cost).Sum(),
            input2.Select(Presses).Where(Reachable).Select(Cost).Sum()
        );

        (long, long) Presses(Claw claw)
        {
            var bottomTerm = claw.BButton.Item1 * claw.AButton.Item2 - claw.AButton.Item1 * claw.BButton.Item2;
            if (bottomTerm == 0)
                return (-1, -1);
            var b = (claw.AButton.Item2 * claw.Prize.Item1 - claw.AButton.Item1 * claw.Prize.Item2) / bottomTerm;
            var a = (claw.Prize.Item1 - claw.BButton.Item1 * b)/claw.AButton.Item1;
            var a2 = (claw.Prize.Item2 - claw.BButton.Item2*b) /claw.AButton.Item2;
            return a == a2 && a >= 0 && b >= 0 
                // This check is necessary because we may have lost precision with integer division above
                && a * claw.AButton.Item1 + b * claw.BButton.Item1 == claw.Prize.Item1
                && a * claw.AButton.Item2 + b * claw.BButton.Item2 == claw.Prize.Item2
                ? (a, b) 
                : (-1, -1);
        }

        long Cost((long, long) presses) =>
            presses.Item1 * A_COST + presses.Item2 * B_COST;

        bool Reachable((long, long) presses) =>
            presses.Item1 >= 0 && presses.Item2 >= 0;
    }
}