using System.Text;

namespace Advent;

public record Robot((int, int) Position, (int, int) Velocity);

public class Day14
{
    public static int WIDTH_DEMO = 11;
    public static int HEIGHT_DEMO = 7;
    public static int WIDTH = 101;
    public static int HEIGHT = 103;

    public int _width;
    public int _height;
    public string file;

    public List<Robot> _robots = new List<Robot>();

    public Dictionary<(int, int), int> Positions = new Dictionary<(int, int), int>();

    public Dictionary<int, int> RunToSafetyFactor = new Dictionary<int, int>();

    public Day14(int width, int height, string file)
    {
        _width = width;
        _height = height;
        _robots = Parse.IntArrayLines(file).Select(arr => new Robot((arr[0], arr[1]), (arr[2], arr[3]))).ToList();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Positions[(x, y)] = 0;
        foreach (var robot in _robots)
            Positions[robot.Position]++;
    }

    public int _time = 0;

    public long SafetyFactorAfter100Runs()
    {
        var advanced = _robots.Select(r => Advance(100, r)).ToList();
        var quadrants = Quadrants(advanced);
        var safetyFactor = quadrants.Select(kvp => kvp.Value.Count).Aggregate((x, y) => x * y);

        return safetyFactor;
    }

    public void Write(string file)
    {
        var grid = new char[_width,_height];
        var sb = new StringBuilder();
        var positions = _robots.GroupBy(r => r.Position).ToDictionary(g => g.Key, g => g.Count());
        sb.Append(_time.ToString() + "\n");
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
                sb.Append(positions.TryGetValue((x, y), out var count) ? " X " : " . ");
            sb.Append("\n");
        }

        File.WriteAllText(file, sb.ToString());
    }

    public void Advance()
    {
        _time++;
        _robots = _robots.Select(r => Advance(1, r)).ToList();
        var quadrants = Quadrants(_robots);
        var safetyFactor = quadrants.Select(kvp => kvp.Value.Count).Aggregate((x, y) => x * y);
        RunToSafetyFactor[_time] = safetyFactor;
    }

    public void Advance(int times)
    {
        for (int i = 0; i < times; i++)
            Advance();
    }

    Robot Advance(int steps, Robot robot)
    {
        var (x, y) = robot.Position;
        var newX = (robot.Velocity.Item1 * steps + x) % _width;
        var newY = (robot.Velocity.Item2 * steps + y) % _height;

        var newP = (
            newX >= 0 ? newX : newX + _width,
            newY >= 0 ? newY : newY + _height
        );

        Positions[robot.Position]--;
        Positions[newP]++;

        return robot with { Position = newP };
    }

    Dictionary<int, HashSet<Robot>> Quadrants(List<Robot> robots)
    {
        var quadrants = new Dictionary<int, HashSet<Robot>>();
        foreach (var robot in robots)
        {
            if (robot.Position.Item1 < _width / 2)
            {
                if (robot.Position.Item2 < _height / 2)
                    quadrants.AddSet(1, robot);
                else if (robot.Position.Item2 > _height / 2)
                    quadrants.AddSet(2, robot);
            }
            else if (robot.Position.Item1 > _width / 2)
            {
                if (robot.Position.Item2 < _height / 2)
                    quadrants.AddSet(3, robot);
                else if (robot.Position.Item2 > _height / 2)
                    quadrants.AddSet(4, robot);
            }
        }
        return quadrants;
    }
}
// day14.Advance(101 * 103);
// Console.WriteLine(day14.RunToSafetyFactor.MinBy(kvp => kvp.Value));