namespace Advent;

using System.Collections.Immutable;
using System.Data;
using System.Text;
using System.Xml;
using static Advent.Extensions;

public static class Day17
{
    public const int ADV = 0, BXL = 1, BST = 2, JNZ = 3, BXC = 4, OUT = 5, BDV = 6, CDV = 7;

    record State(ImmutableArray<int> Instructions, int Ip, long RegA, long RegB, long RegC, ImmutableList<int> Out, bool Halted);

    static State EmptyState(IEnumerable<int> instructions, long regA, long regB, long regC) =>
        new State(instructions.ToImmutableArray(), 0, regA, regB, regC, ImmutableList<int>.Empty, false);

    public static string Run(string file, string a)
    {
        var initialState = ReadInitialState(file) with { RegA = Base8ToBase10(a) };
        return RunProgram(initialState).finalState.Out.StrJoin(",");
    }

    public static (string, string) Run(string file)
    {
        var initialState = ReadInitialState(file);

        var _ = 1;

        // Did the following iteratively, building up prefix (base 8 number) in 4-digit chunks
        // to match the output in 4-digit chunks from right to left.
        var prefix = "560053275602";
    
        var mapping = Enumerable.Range(0, (int)Base8ToBase10("10000"))
            .Select(i => Base8ToBase10(prefix + Base8(i)))
            .Select(regA => initialState with {RegA = regA })
            .Select(s => (s.RegA, RunProgram(s).finalState.Out))
            .Select(tup => $"{tup.RegA} ({Base8(tup.RegA)}): {tup.Out.StrJoin(",")}");
        File.WriteAllLines("programs2.txt", mapping);

        return ("", "");

    }

    // chatgpt
    public static long Base8ToBase10(string octalNumber)
    {
        long base10 = 0;
        long baseValue = 1; // Start with 8^0 = 1

        // Handle negative octal numbers
        bool isNegative = octalNumber.StartsWith("-");
        if (isNegative)
            octalNumber = octalNumber.Substring(1); // Remove the '-' sign for processing

        // Iterate through the octal number from right to left
        for (int i = octalNumber.Length - 1; i >= 0; i--)
        {
            char digitChar = octalNumber[i];
            int digit = digitChar - '0'; // Convert character to integer

            // Validate octal digits (0-7)
            if (digit < 0 || digit > 7)
                throw new ArgumentException("Invalid octal number. Octal digits must be between 0 and 7.");

            base10 += digit * baseValue; // Add the value of the digit to the total
            baseValue *= 8; // Increase the base multiplier (8^1, 8^2, etc.)
        }

        return isNegative ? -base10 : base10;
    }

    // chatgpt
    static string Base8(long n)
    {
        var result = "";
        while (n > 0)
        {
            result = (n % 8) + result;
            n /= 8;
        }
        return result;
    }

    static (List<State> intermediaries, State finalState) RunProgram(State initialState)
    {
        var computations = IterativeSeq(initialState, Compute);
        var work = computations.TakeWhile(s => !s.Halted).ToList();
        var finalState = computations.First(s => s.Halted);
        return (work, finalState);
    }

    static State Compute(State state)
    {
        if (state.Ip >= state.Instructions.Length)
            return state with { Halted = true };
        var (opcode, operand) = (state.Instructions[state.Ip], state.Instructions[state.Ip + 1]);
        state = state with { Ip = state.Ip + 2 };

        return opcode switch
        {
            ADV => state with { RegA = state.RegA / (int)Math.Pow(2, OperandValue(state, operand)) },
            BXL => state with { RegB = state.RegB ^ operand },
            BST => state with { RegB = OperandValue(state, operand) % 8 },
            JNZ => state.RegA == 0 ? state : state with { Ip = operand },
            BXC => state with { RegB = state.RegB ^ state.RegC },
            OUT => state with { Out = state.Out.Add((int)(OperandValue(state, operand) % 8))},
            BDV => state with { RegB = state.RegA / (int)Math.Pow(2, OperandValue(state, operand)) },
            CDV => state with { RegC = state.RegA / (int)Math.Pow(2, OperandValue(state, operand)) }
        };

        long OperandValue(State state, int operand) =>
            operand switch
            {
                0 => 0, 1 => 1, 2 => 2, 3 => 3,
                4 => state.RegA, 5 => state.RegB, 6 => state.RegC
            };
    }

    static State ReadInitialState(string file)
    {
        var intArrays = Parse.IntArrayLines(file);
        return new State(intArrays.Last().ToImmutableArray(), 0, intArrays[0][0], intArrays[1][0], intArrays[2][0], ImmutableList<int>.Empty, false);
    }
}