namespace Advent;

using System.Data;
using static Advent.Extensions;

public static class Day16
{
    record Node((int, int) Coordinate, (int, int) Direction);

    public const char EMPTY = '.'; public const char WALL = '#';
    public static (long, long) Run(string file)
    {
        var maze = Matrix<char>.CharacterMatrixFromFile(file, _null: '_');

        var startPoint = maze.IndexOf('S');
        var endPoint = maze.IndexOf('E');
        var startDirection = Direction.Right;

        var nodes = maze.Select((row, col, val) => 
            val != WALL 
            ? Direction.CardinalDirections.Select(d => new Node((row, col), d)).ToList() 
            : List<Node>()).SelectMany(x => x).ToList();

        var neighbors = new Dictionary<Node, Dictionary<Node, long>>();
        foreach (var n in nodes)
        {
            if (maze.Get(n.Coordinate.Add(n.Direction)) != WALL)
                neighbors.AddDict(n, new Node(n.Coordinate.Add(n.Direction), n.Direction), 1);
            neighbors.AddDict(n, new Node(n.Coordinate, n.Direction.TurnLeft()), 1000);
            neighbors.AddDict(n, new Node(n.Coordinate, n.Direction.TurnRight()), 1000);
        }

        var (finalCosts, parents) = ShortestPaths(new Node(startPoint, startDirection), neighbors);

        var finishNode = List(
            new Node(endPoint, Direction.Left), new Node(endPoint, Direction.Right),
            new Node(endPoint, Direction.Down), new Node(endPoint, Direction.Up)
        ).MinBy(n => finalCosts[n]);

        var coordinatesAlongShortestPath = NodesAlongShortestPathsToNode(parents, finishNode).Select(n => n.Coordinate).ToHashSet();

        return (finalCosts[finishNode], coordinatesAlongShortestPath.Count);

        // adapted from https://stackoverflow.com/a/77917268
        (Dictionary<Node, long> costs, Dictionary<Node, HashSet<Node>> parents) ShortestPaths(
            Node start,
            Dictionary<Node, Dictionary<Node, long>> neighbors)
        {
            var visited = new HashSet<Node>();
            var costs = new Dictionary<Node, long>() { };
            var parents = new Dictionary<Node, HashSet<Node>>();
            costs = neighbors[start].ToDictionary(x => x.Key, x => x.Value);

            var lowestCostUnvisitedNode = FindLowestCostUnvisitedNode();
            while (lowestCostUnvisitedNode != null)
            {
                var cost = costs[lowestCostUnvisitedNode];
                var ns = neighbors[lowestCostUnvisitedNode];
                foreach (var (neighbor, costToNeighbor) in ns)
                {
                    var newCost = cost + costToNeighbor;
                    if (!costs.ContainsKey(neighbor) || newCost <= costs[neighbor]) 
                    {
                        costs[neighbor] = newCost;
                        parents.AddSet(neighbor, lowestCostUnvisitedNode);
                    }
                }

                visited.Add(lowestCostUnvisitedNode);
                lowestCostUnvisitedNode = FindLowestCostUnvisitedNode();
            }

            return (costs, parents);

            // This significantly degrades the performance
            // C# doesn't have a priority queue with an update function.
            Node? FindLowestCostUnvisitedNode()
            {
                var cs = costs.Where(kvp => !visited.Contains(kvp.Key)).ToList();
                if (cs.Count == 0)
                    return null;
                return cs.MinBy(n => n.Value).Key;
            }
        }

        HashSet<Node> NodesAlongShortestPathsToNode(Dictionary<Node, HashSet<Node>> parents, Node n) =>
            parents.TryGetValue(n, out var ps)
            ? List(n).Concat(ps.SelectMany(p => NodesAlongShortestPathsToNode(parents, p))).ToHashSet()
            : new HashSet<Node> { n };
    }
}