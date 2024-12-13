using System.Linq;
using System.Collections.Generic;
using static Advent.Extensions;
using System.Text.RegularExpressions;

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

    public static (int, int) ManhattanDistance(this (int, int) x, (int, int) y) =>
        (x.Item1 - y.Item1, x.Item2 - y.Item2);

    public static (List<T> True, List<T> False) Partition<T>(this IEnumerable<T> coll, Func<T, bool> predicate) =>
        (coll.Where(predicate).ToList(), coll.Where(x => !predicate(x)).ToList());

    public static (int, int) Add(this (int, int) x, (int, int) y) =>
        (x.Item1 + y.Item1, x.Item2 + y.Item2);

    public static (int, int) Subtract(this (int, int) x, (int, int) y) =>
        (x.Item1 - y.Item1, x.Item2 - y.Item2);

    public static (long, long) Add(this (long, long) x, (long, long) y) =>
        (x.Item1 + y.Item1, x.Item2 + y.Item2);

    public static (long, long) Subtract(this (long, long) x, (long, long) y) =>
        (x.Item1 - y.Item1, x.Item2 - y.Item2);

    public static List<(int, int)> Adjacent4(this (int, int) x) =>
        List(
            x.Add(Direction.Up),
            x.Add(Direction.Right),
            x.Add(Direction.Down),
            x.Add(Direction.Left)
        );

    public static IEnumerable<(T, T)> OrderedPairs<T>(this IEnumerable<T> coll)
    {
        var list = coll.ToList();
        for (int i = 0; i < list.Count; i++)
            for (int j = i + 1; j < list.Count; j++)
                yield return (list[i], list[j]);
    }

    public static IEnumerable<T> IterativeSeq<T>(T seed, Func<T, T> next)
    {
        var x = seed;
        while (true)
        {
            yield return x;
            x = next(x);
        }
    }

    public static Option<T> Some<T>(T value) => new Option<T>.Some(value);
    public static Option<T> None<T>() => new Option<T>.None();

    public static T AddSet<K, T>(this Dictionary<K, HashSet<T>> dict, K key, T val)
    {
        if (dict.TryGetValue(key, out var set))
            set.Add(val);
        else
            dict[key] = new HashSet<T> { val };
        return val;
    }
}

public record Option<T>
{
    public record Some(T Value): Option<T>;
    public record None: Option<T>;
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

    public IEnumerable<R> Select<R>(Func<int, int, T, R> f) =>
        array.SelectMany((sub, row) => sub.Select((x, col) => f(row, col, x)));

    public IEnumerable<R> SelectWhere<R>(Func<int, int, T, Option<R>> f)
    {
        var result = new List<R>();
        this.ForEach((row, col, x) => {
            if (f(row, col, x) is Option<R>.Some s)
                result.Add(s.Value);
        });
        return result;
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

    public static Matrix<int> IntegerMatrixFromFile(string file, int _null) =>
        new Matrix<int>(File.ReadAllLines(file).Select(l => l.ToCharArray().Select(c => c - '0').ToArray()).ToArray(), _null);
}

public static class Parse
{
    public static int[] IntArray(string file) =>
        new Regex("([0-9]+)").Matches(File.ReadAllText(file)).Select(v => Int32.Parse(v.Value)).ToArray();

    public static int[][] IntArrayLines(string file) =>
        File.ReadAllLines(file).Select(line => new Regex("([0-9]+)").Matches(line).Select(v => Int32.Parse(v.Value)).ToArray()).ToArray();

    public static long[] LongArray(string file) =>
        new Regex("([0-9]+)").Matches(File.ReadAllText(file)).Select(v => Int64.Parse(v.Value)).ToArray();
}