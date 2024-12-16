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

        var (finalCosts, parents) = Algorithms.ShortestPathsFrom(new Node(startPoint, startDirection), neighbors);

        var finishNode = List(
            new Node(endPoint, Direction.Left), new Node(endPoint, Direction.Right),
            new Node(endPoint, Direction.Down), new Node(endPoint, Direction.Up)
        ).MinBy(n => finalCosts[n]);

        var coordinatesAlongShortestPath = Algorithms.NodesAlongShortestPathsToNode(parents, finishNode).Select(n => n.Coordinate).ToHashSet();

        return (finalCosts[finishNode], coordinatesAlongShortestPath.Count);
    }
}