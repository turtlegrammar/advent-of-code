namespace Advent;

using System.Data;
using static Advent.Extensions;

public static class Day23
{

    public static (long, string) Run(string file)
    {
        var graph = new Graph<string>();
        Parse.AlphanumLines(file).ForEach(pair => graph.BidirectionallyConnect(pair[0], pair[1]));

        var triplets = new HashSet<(string, string, string)>();
        foreach (var n in graph.Nodes)
        {
            foreach (var m in graph.Connections[n])
            {
                foreach (var c in graph.Connections[m])
                {
                    if (graph.Connections[n].Contains(c) && n != c)
                        triplets.Add((n, m, c).Order());
                }
            }
        }

        var ttriplets = triplets.Where(t => t.Item1.StartsWith("t") || t.Item2.StartsWith("t") || t.Item3.StartsWith("t")).ToList();

        var maxSet = graph.Nodes.SelectMany(n => AllConnected(graph, n)).MaxBy(c => c.Count);
        var answer2 = maxSet.Order().StrJoin(",");

        return (ttriplets.Count, answer2);

        List<HashSet<T>> AllConnected<T>(Graph<T> g, T node)
        {
            var results = g.Connections[node].Select(c => new HashSet<T> { node, c}).ToList();
            for (int i = 0; i < g.Connections.Count; i++) {
                foreach (var c in g.Connections[node]) {
                    foreach (var r in results) {
                        if (r.All(x => g.Connections[x].Contains(c)))
                            r.Add(c);
                    }
                }
            }
            return results;
        }
    }
}