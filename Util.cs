using static Advent.Extensions;
using System.Text.RegularExpressions;
using System.Text;

namespace Advent;

public static class Extensions
{
    public static List<T> List<T>(params T[] xs) =>
        xs.ToList();
    
    public static Dictionary<K, V> Dictionary<K, V>(params (K, V)[] kvs) =>
        kvs.ToDictionary();

    public static List<string> KeepSmallest(this IEnumerable<string> colls)
    {
        if (colls.Count() == 0)
            return colls.ToList();
        var targetSize = colls.MinBy(c => c.Count()).Count();
        return colls.Where(c => c.Count() == targetSize).ToList();
    }

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

    public static int ManhattanDistance(this (int, int) x, (int, int) y) =>
        Math.Abs(x.Item1 - y.Item1) + Math.Abs(x.Item2 - y.Item2);

    public static int Sum(this (int, int) x) => x.Item1 + x.Item2;

    public static (List<T> True, List<T> False) Partition<T>(this IEnumerable<T> coll, Func<T, bool> predicate) =>
        (coll.Where(predicate).ToList(), coll.Where(x => !predicate(x)).ToList());

    public static (int, int) Add(this (int, int) x, (int, int) y) =>
        (x.Item1 + y.Item1, x.Item2 + y.Item2);

    public static (int, int) Multiply(this (int, int) x, int scalar) =>
        (x.Item1 * scalar, x.Item2 * scalar);

    public static (int, int) Subtract(this (int, int) x, (int, int) y) =>
        (x.Item1 - y.Item1, x.Item2 - y.Item2);

    public static (long, long) Add(this (long, long) x, (long, long) y) =>
        (x.Item1 + y.Item1, x.Item2 + y.Item2);

    public static (long, long) Subtract(this (long, long) x, (long, long) y) =>
        (x.Item1 - y.Item1, x.Item2 - y.Item2);

    public static T X<T>(this (T, T) tup) => tup.Item1;
    public static T Y<T>(this (T, T) tup) => tup.Item2;

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

    public static V AddDict<K, T, V>(this Dictionary<K, Dictionary<T, V>> dict, K key, T key2, V val)
    {
        if (dict.TryGetValue(key, out var d))
            return d[key2] = val;
        else
            dict[key] = new Dictionary<T, V> { {key2, val } };
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

    public static List<(int, int)> CardinalDirections => 
        List(Up,  Right,  Down,  Left);

    public static (int, int) TurnRight(this (int, int) direction) =>
        direction switch 
        {
            (-1, 0) => Right,
            (0, 1) => Down,
            (1, 0) => Left,
            (0, -1) => Up
        };

    public static (int, int) TurnLeft(this (int, int) direction) =>
        direction.TurnRight().TurnRight().TurnRight();
}

public class Matrix<T>(T[][] array, T _null)
{
    public T[][] Array => array;

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
        array.Select((a, row) => (row, System.Array.IndexOf(array[row], item))).Single(tup => tup.Item2 != -1);

    public List<T> GetSeq((int, int) start, (int, int) dir, int length) 
    {
        var (rowDir, colDir) = dir; 

        return Enumerable.Range(0, length).Select(i => 
            Get((start.Item1 + rowDir * i, start.Item2 + colDir * i))
        ).ToList();
    }

    public IEnumerable<T> GetSeq((int, int) start, (int, int) dir) =>
        IterativeSeq(start, n => n.Add(dir)).Select(Get);

    public static Matrix<char> CharacterMatrixFromFile(string file, char _null) =>
        new Matrix<char>(File.ReadAllLines(file).Select(l => l.ToCharArray()).ToArray(), _null);

    public static Matrix<char> CharacterMatrixFromString(string str, char _null) =>
        new Matrix<char>(str.Split("\n").Select(l => l.ToCharArray()).ToArray(), _null);

    public static Matrix<int> IntegerMatrixFromFile(string file, int _null) =>
        new Matrix<int>(File.ReadAllLines(file).Select(l => l.ToCharArray().Select(c => c - '0').ToArray()).ToArray(), _null);

    public static Matrix<T> Create(T seed, T _null, int rows, int cols) =>
        new Matrix<T>(Enumerable.Range(0, rows).Select(_ => Enumerable.Range(0, cols).Select(_ => seed).ToArray()).ToArray(), _null);

    public void Write(string file)
    {
        var sb = new StringBuilder();

        for (int x = 0; x < array.Length; x++)
        {
            for (int y = 0; y < array[0].Length; y++)
                sb.Append(Get(x, y));
            sb.Append("\n");
        }

        File.WriteAllText(file, sb.ToString());
    }

    public void Append(string file)
    {
        var sb = new StringBuilder();

        for (int x = 0; x < array.Length; x++)
        {
            for (int y = 0; y < array[0].Length; y++)
                sb.Append(Get(x, y));
            sb.Append("\n");
        }

        File.AppendAllText(file, sb.ToString());
    }
}

public static class Parse
{
    public static int Int(string s) =>
        new Regex("([0-9]*)").Matches(s).Select(v => v.Value == "" ? 0 : Int32.Parse(v.Value)).ToArray()[0];

    public static long Long(string s) =>
        new Regex("([0-9]*)").Matches(s).Select(v => v.Value == "" ? 0 : Int64.Parse(v.Value)).ToArray()[0];

    public static int[] IntArray(string file) =>
        new Regex("([0-9]+)").Matches(File.ReadAllText(file)).Select(v => Int32.Parse(v.Value)).ToArray();

    public static int[][] IntArrayLines(string file) =>
        File.ReadAllLines(file).Select(line => new Regex("(-{0,1}[0-9]+)").Matches(line).Select(v => Int32.Parse(v.Value)).ToArray()).ToArray();

    public static long[] LongArray(string file) =>
        new Regex("([0-9]+)").Matches(File.ReadAllText(file)).Select(v => Int64.Parse(v.Value)).ToArray();
}

public static class Algorithms
{
    // adapted from https://stackoverflow.com/a/77917268
    public static (Dictionary<T, long> costs, Dictionary<T, HashSet<T>> parents) ShortestPathsFrom<T>(
        T start,
        Dictionary<T, Dictionary<T, long>> neighbors)
    {
        var visited = new HashSet<T>();
        var costs = new Dictionary<T, long>() { };
        var parents = new Dictionary<T, HashSet<T>>();
        var queue = new PriorityQueue<T, long>();

        costs = neighbors[start].ToDictionary(x => x.Key, x => x.Value);
        foreach (var c in costs)
            queue.Enqueue(c.Key, c.Value);

        while (queue.TryDequeue(out var lowestCostNode, out var _))
        {
            // visited seems to not be necessary, but does speed it up some
            if (!visited.Contains(lowestCostNode))
            {
                var cost = costs[lowestCostNode];
                var ns = neighbors[lowestCostNode];
                foreach (var (neighbor, costToNeighbor) in ns)
                {
                    var newCost = cost + costToNeighbor;
                    // the = case doesn't update the cost but it does add a new parent
                    if (!costs.ContainsKey(neighbor) || newCost <= costs[neighbor]) 
                    {
                        costs[neighbor] = newCost;
                        // we never need to update https://github.com/dotnet/runtime/issues/44871#issuecomment-2039292354
                        queue.Enqueue(neighbor, newCost);
                        parents.AddSet(neighbor, lowestCostNode);
                    }
                }

                visited.Add(lowestCostNode);
            }
        }

        return (costs, parents);
    }

    public static HashSet<T> NodesAlongShortestPathsToNode<T>(Dictionary<T, HashSet<T>> parents, T node) =>
        parents.TryGetValue(node, out var ps)
        ? List(node).Concat(ps.SelectMany(p => NodesAlongShortestPathsToNode(parents, p))).ToHashSet()
        : new HashSet<T> { node };

    public static List<List<T>> EnumeratePaths<T>(Dictionary<T, HashSet<T>> parents, T node)
    {
        if (parents.TryGetValue(node, out var ps))
        {
            var parentPaths = ps.SelectMany(p => EnumeratePaths(parents, p)).ToList();
            return parentPaths.Select(path => path.Concat(List(node)).ToList()).ToList();
        }
        else
        {
            return List(List(node));
        }
    }
        // parents.TryGetValue(node, out var ps)
        // // ? List(node).Concat(ps.SelectMany(p => NodesAlongShortestPathsToNode(parents, p))).ToHashSet()
        // ? ps.Select(p => List(node, p).Concat(Enumer))
        // : List(List(node))

}