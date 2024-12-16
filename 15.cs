namespace Advent;
using static Advent.Extensions;

public static class Day15
{
    public const char BOX = 'O'; public const char WALL = '#';
    public const char BOX_LEFT = '['; public const char BOX_RIGHT = ']';
    public const char ROBOT = '@'; public const char EMPTY = '.';
    public const char UP = '^'; public const char DOWN = 'v';
    public const char LEFT = '<'; public const char RIGHT = '>';

    public static (long, long) Run(string file)
    {
        var input = File.ReadAllLines(file);
        var (mapStr, movement) = (input.TakeWhile(s => s != "").StrJoin("\n"), input.SkipWhile(s => s != "").Skip(1).StrJoin());
        var map = Matrix<char>.CharacterMatrixFromString(mapStr, _null:'_');

        map = new Matrix<char>(
            map.Array.Select(arr => arr.SelectMany(c => c switch { WALL => List(WALL, WALL), BOX => List(BOX_LEFT, BOX_RIGHT), EMPTY => List(EMPTY, EMPTY), ROBOT => List(ROBOT, EMPTY)}).ToArray()).ToArray(),
            _null: '_');

        var robotPosition = map.IndexOf(ROBOT);

        // map.Write("map.txt");
        foreach (var m in movement)
        {
            Move(m);
            // map.Append("map.txt");
        }

        var boxes = new List<(int, int)>();
        map.ForEach((x, y, v) => { if (v == BOX_LEFT) boxes.Add((x, y)); });
        var sum = boxes.Select(b => b.Item1 * 100 + b.Item2).Sum();

        return (0, sum);

        void Move(char move)
        {
            if (move == RIGHT || move == LEFT)
                MoveLeftRight(move);
            else
                MoveUpDown(move);
        }

        void MoveLeftRight(char move)
        {
            var dir = move switch
            {
                // UP => Direction.Up, DOWN => Direction.Down,
                LEFT => Direction.Left, RIGHT => Direction.Right
            };

            var inPath = map.GetSeq(robotPosition, dir).TakeWhile(c => c != WALL).ToList();
            var firstEmpty = inPath.IndexOf(EMPTY);
            if (firstEmpty == -1)
                return;
            
            for (int i = firstEmpty; i > 0; i--)
            {
                map[robotPosition.Add(dir.Multiply(i))] = map[robotPosition.Add(dir.Multiply(i - 1))];
                map[robotPosition.Add(dir.Multiply(i - 1))] = EMPTY;
            }

            robotPosition = robotPosition.Add(dir);
        }

        void MoveUpDown(char move)
        {
            var dir = move switch { UP => Direction.Up, DOWN => Direction.Down };
            if (CanMove(robotPosition, dir))
                DoMove(robotPosition, dir);
        }

        void DoMove((int, int) position, (int, int) direction)
        {
            var c = map.Get(position);
            if (c == EMPTY)
                return;
            else if (c == ROBOT)
            {
                DoMove(position.Add(direction), direction);
                map[position.Add(direction)] = ROBOT;
                map[position] = EMPTY;
                robotPosition = position.Add(direction);
            }
            else if (c == BOX_LEFT)
            {
                DoMove(position.Add(direction), direction);
                DoMove(position.Add(Direction.Right).Add(direction), direction);
                map[position.Add(direction)] = BOX_LEFT;
                map[position.Add(Direction.Right).Add(direction)] = BOX_RIGHT;
                map[position] = EMPTY;
                map[position.Add(Direction.Right)] = EMPTY;
            }
            else if (c == BOX_RIGHT)
            {
                DoMove(position.Add(direction), direction);
                DoMove(position.Add(Direction.Left).Add(direction), direction);
                map[position.Add(direction)] = BOX_RIGHT;
                map[position.Add(Direction.Left).Add(direction)] = BOX_LEFT;
                map[position] = EMPTY;
                map[position.Add(Direction.Left)] = EMPTY;
            }
        }

        bool CanMove((int, int) position, (int, int) direction)
        {
            return map.Get(position) switch
            {
                EMPTY => true,
                BOX_LEFT => CanMove(position.Add(direction), direction) && CanMove(position.Add(Direction.Right).Add(direction), direction),
                BOX_RIGHT => CanMove(position.Add(direction), direction) && CanMove(position.Add(Direction.Left).Add(direction), direction),
                ROBOT => CanMove(position.Add(direction), direction),
                WALL => false
            };
        }
    }
}