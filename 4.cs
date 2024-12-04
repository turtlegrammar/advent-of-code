using System.Linq;
using System.Collections.Generic;
using static Advent.Extensions;

namespace Advent;

public static class Day4
{
    public static int Run(string file)
    {
        var matrix = Matrix<char>.CharacterMatrixFromFile(file, _null: '_');

        var count = 0;

        matrix.ForEach((row, col, _) => 
            count += GetDirectionalStrings(row, col).Where(s => s == "XMAS").Count()
        );

        return count;

        List<string> GetDirectionalStrings(int row, int col) =>
            EnumValues<MatrixDirection>().Select(dir => 
                matrix.GetSeq((row, col), dir, 4).StrJoin()
            ).ToList();
    }

    public static int Run2(string file)
    {
        var matrix = Matrix<char>.CharacterMatrixFromFile(file, _null: '_');

        var count = 0;

        matrix.ForEach((row, col, _) => {
            if (GetDirectionalStrings(row, col).Where(s => s == "MAS").Count() == 2)
                count++;
        });

        return count;

        List<string> GetDirectionalStrings(int row, int col)
        {
            return List(
                matrix.GetSeq((row-1, col-1), MatrixDirection.DownDiagRight, 3).StrJoin(),
                matrix.GetSeq((row-1, col+1), MatrixDirection.DownDiagLeft, 3).StrJoin(),
                matrix.GetSeq((row+1, col-1), MatrixDirection.UpDiagRight, 3).StrJoin(),
                matrix.GetSeq((row+1, col+1), MatrixDirection.UpDiagLeft, 3).StrJoin()
            );
        }
    }
}