using System.Data;
using static Advent.Extensions;

using Advent;
namespace Advent2023;

public static class Day3
{
    public static (long, long) Run(string file)
    {
        var m = Matrix<char>.CharacterMatrixFromFile(file, '.');
        var partNumbers = new List<long>();

        var locationPartMap = new Dictionary<(int, int), int>();
        var partIdToPartNumber = new Dictionary<int, long>();

        var lastPartId = 0;
        for (int r = 0; r < m.Array.Length; r++)
        {
            var curNum = "";
            var adjacentSymbol = false;
            for (int c = 0; c < m.Array[r].Length; c++)
            {
                if (IsDigit(m[(r, c)]))
                {
                    curNum += m[(r, c)];
                    if (!adjacentSymbol)
                        adjacentSymbol = (r, c).Adjacent8().Select(p => m[p]).Any(IsSymbol);
                }
                else
                {
                    TryPromoteNumber(c);
                }
            }
            TryPromoteNumber(m.Array[r].Length);

            void TryPromoteNumber(int col)
            {
                col--;
                if (curNum != "" && adjacentSymbol)
                {
                    partNumbers.Add(Parse.Long(curNum));
                    while (IsDigit(m[(r, col)]))
                    {
                        locationPartMap[(r, col)] = lastPartId;
                        col--;
                    }
                    partIdToPartNumber[lastPartId] = Parse.Long(curNum);

                    lastPartId++;
                }

                curNum = "";
                adjacentSymbol = false;
            }
        }

        var gears = m.Select((row, col, c) =>
        {
            if (c == '*')
            {
                var adjacentParts = (row, col).Adjacent8()
                    .Select(p => locationPartMap.TryGetValue(p, out var part) ? part : -1)
                    .Where(x => x != -1).ToHashSet();
                if (adjacentParts.Count == 2)
                    return adjacentParts.Select(p => partIdToPartNumber[p]).Aggregate((x, y) => x * y);
            }
            return 0;
        });

        return (partNumbers.Sum(), gears.Sum());

        bool IsDigit(char c) => c >= '0' && c <= '9';
        bool IsSymbol(char c) => !IsDigit(c) && c != '.' && c != '_';
    } 
}