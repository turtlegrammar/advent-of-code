namespace Advent;

public static class Day5
{
    public static (int, int) Run(string file)
    {
        var content = File.ReadAllLines(file);
        var orders = content.TakeWhile(x => x != "").Select(s => s.Split("|").Select(Int32.Parse).ToTuple2());
        var updates = content.SkipWhile(x => x != "").Skip(1).Select(l => l.Split(",").Select(Int32.Parse).ToList());

        var before = orders.GroupBy(t => t.Item1).ToDictionary(g => g.Key, g => g.Select(t => t.Item2).ToHashSet());

        var (orderedUpdates, unorderedUpdates) = updates.Partition(Ordered);

        return (
            orderedUpdates.Select(l => l[l.Count/2]).Sum(), 
            unorderedUpdates.Select(TopologicalSort).Select(l => l[l.Count/2]).Sum());

        IEnumerable<(int, int)> OrderedPairs(List<int> updates)
        {
            for (int i = 0; i < updates.Count; i++)
                for (int j = i + 1; j < updates.Count; j++)
                    yield return (updates[i], updates[j]);
        }

        bool Ordered(List<int> updates) =>
            OrderedPairs(updates).All(tup => 
                before.TryGetValue(tup.Item2, out var beforeSet)
                ? !beforeSet.Contains(tup.Item1)
                : true);

        List<int> TopologicalSort(List<int> updates)
        {
            updates.Sort((x, y) => 
                before.TryGetValue(y, out var beforeY) 
                ? (beforeY.Contains(x) ? 1 : -1) 
                : -1);
            return updates;
        }
    }
}