using System.Linq;
using System.Collections.Generic;
using static Advent.Extensions;

namespace Advent;

public static class Extensions
{
    public static List<T> List<T>(params T[] xs) =>
        xs.ToList();

    public static string StrJoin<T>(this IEnumerable<T> coll, string join = "") =>
        String.Join(join, coll.Select(x => x.ToString()));

    public static T[] EnumValues<T>() => (T[]) Enum.GetValues(typeof(T));

    public static List<T> ReverseList<T>(this List<T> coll) =>
        coll.Select(x => x).Reverse().ToList();

    public static (T, T) ToTuple2<T>(this IEnumerable<T> coll) 
    {
        var arr = coll.ToArray();
        return (arr[0], arr[1]);
    }

    public static (List<T> True, List<T> False) Partition<T>(this IEnumerable<T> coll, Func<T, bool> predicate) =>
        (coll.Where(predicate).ToList(), coll.Where(x => !predicate(x)).ToList());

    public static (int, int) Add(this (int, int) x, (int, int) y) =>
        (x.Item1 + y.Item1, x.Item2 + y.Item2);
}

public static class Direction
{
    public static (int, int) Up => (-1, 0);
    public static (int, int) UpRight => (-1, 1);
    public static (int, int) Right => (0, 1);
    public static (int, int) DownRight => (1, 1);
    public static (int, int) Down => (1, 0);
    public static (int, int) DownLeft => (1, -1);
    public static (int, int) Left => (0, -1);
    public static (int, int) UpLeft => (-1, -1);

    public static List<(int, int)> Directions => 
        List(Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft);

    public static (int, int) TurnRight(this (int, int) direction) =>
        direction switch 
        {
            (-1, 0) => Right,
            (0, 1) => Down,
            (1, 0) => Left,
            (0, -1) => Up
        };
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

    public T this[(int, int) index]
    {
        get { return Get(index); }
        set { array[index.Item1][index.Item2] = value; }
    }

    public (int, int) IndexOf(T item) =>
        array.Select((a, row) => (row, Array.IndexOf(array[row], item))).Single(tup => tup.Item2 != -1);

    public List<T> GetSeq((int, int) start, (int, int) dir, int length) 
    {
        var (rowDir, colDir) = dir; 

        return Enumerable.Range(0, length).Select(i => 
            Get((start.Item1 + rowDir * i, start.Item2 + colDir * i))
        ).ToList();
    }

    public static Matrix<char> CharacterMatrixFromFile(string file, char _null) =>
        new Matrix<char>(File.ReadAllLines(file).Select(l => l.ToCharArray()).ToArray(), _null);
}