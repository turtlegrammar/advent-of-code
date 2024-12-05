using System.Linq;
using System.Collections.Generic;

namespace Advent;

public static class Extensions
{
    public static List<T> List<T>(params T[] xs) =>
        xs.ToList();

    public static string StrJoin<T>(this IEnumerable<T> coll, string join = "") =>
        String.Join(join, coll.Select(x => x.ToString()));

    public static T[] EnumValues<T>() => (T[]) Enum.GetValues(typeof(T));

    public static (T, T) ToTuple2<T>(this IEnumerable<T> coll) 
    {
        var arr = coll.ToArray();
        return (arr[0], arr[1]);
    }

    public static (List<T> True, List<T> False) Partition<T>(this IEnumerable<T> coll, Func<T, bool> predicate) =>
        (coll.Where(predicate).ToList(), coll.Where(x => !predicate(x)).ToList());
}

public enum MatrixDirection
{
    Up, UpRight, Right, DownRight,
    Down, DownLeft, Left, UpLeft
}

public class Matrix<T>(T[][] array, T _null)
{
    public void ForEach(Action<int, int, T> f)
    {
        for (int i = 0; i < array.Length; i++)
            for (int j = 0; j < array[i].Length; j++)
                f(i, j, array[i][j]);
    }

    public T Get(int i, int j) =>
        Get((i, j));

    public T Get((int, int) ij) =>
        ij.Item1 < 0 ? _null
        : ij.Item2 < 0 ? _null
        : ij.Item1 >= array.Length ? _null
        : ij.Item2 >= array[ij.Item1].Length ? _null
        : array[ij.Item1][ij.Item2];

    public List<T> GetSeq((int, int) start, MatrixDirection dir, int length) 
    {
        var (rowDir, colDir) = dir switch
        {
            MatrixDirection.Up => (-1, 0),
            MatrixDirection.UpRight => (-1, 1),
            MatrixDirection.Right => (0, 1),
            MatrixDirection.DownRight => (1, 1),
            MatrixDirection.Down => (1, 0),
            MatrixDirection.DownLeft => (1, -1),
            MatrixDirection.Left => (0, -1),
            MatrixDirection.UpLeft => (-1, -1)
        };

        return Enumerable.Range(0, length).Select(i => 
            Get((start.Item1 + rowDir * i, start.Item2 + colDir * i))
        ).ToList();
    }

    public static Matrix<char> CharacterMatrixFromFile(string file, char _null) =>
        new Matrix<char>(File.ReadAllLines(file).Select(l => l.ToCharArray()).ToArray(), _null);
}