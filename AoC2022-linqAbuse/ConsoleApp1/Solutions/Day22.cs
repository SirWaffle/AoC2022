using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using static ConsoleApp1.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleApp1.Solutions
{
    internal class Day22 : AbstractPuzzle
    {

        override public void Part1()
        {
            Both(false);
        }



        override public void Part2()
        {
            Both(true);
        }


        void Both(bool part2)
        {
            var lines = File.ReadAllText(InputFile!).Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();

            string commandString = lines.Last();
            lines.RemoveAt(lines.Count - 1);

            Dictionary<int, List<Tile>> tileMap = new();
            Point bounds = new();

            //build a sparse matrix type thing based on points of interest
            int y = -1;            
            foreach(string line in lines)
            {
                y++;
                List<Tile> tileList = new();
                bool wasEmptySpace = true;
                for(int  x = 0; x < line.Length;++x)
                {
                    if (line[x] == '#')
                    {
                        Tile t = new Tile();
                        t.pos = new Point(x, y);
                        t.type = Tile.Type.Wall;
                        tileList.Add(t);
                        wasEmptySpace = false;
                    }
                    else if (line[x] == '.')
                    {
                        if(wasEmptySpace || x + 1 == line.Length)
                        {
                            Tile t = new Tile();
                            t.pos = new Point(x, y);
                            t.type = Tile.Type.Walkable;
                            tileList.Add(t);
                        }

                        wasEmptySpace = false;
                    }
                    else if (line[x] == ' ')
                    {
                        if (!wasEmptySpace)
                        {
                            if (line[x- 1] != ' ')
                            {
                                Tile t = new Tile();
                                t.pos = new Point(x - 1, y);
                                if (line[x-1] == '#')
                                {
                                    t.type = Tile.Type.Wall;
                                }
                                else
                                {
                                    t.type = Tile.Type.Walkable;
                                }

                                tileList.Add(t);
                            }
                        }

                        wasEmptySpace = true;
                    }
                }

                if (tileList.Count > 0)
                {
                    bounds.Y = Math.Max(bounds.Y, y);
                    bounds.X = Math.Max(bounds.X, line.Length);
                    tileMap.Add(y, tileList);
                }
            }

            //turn the commands into a list
            List<Command> commands = new();
            string digits = string.Empty;  
            for(int i =0; i < commandString.Length; ++i)
            {
                if (char.IsAsciiDigit(commandString[i]))
                {
                    digits += commandString[i];

                    if( i + 1 == commandString.Length)
                    {
                        Command command = new();
                        command.moveAmount = int.Parse(digits);
                        digits = string.Empty;
                        command.dir = 0;
                        commands.Add(command);
                    }
                }
                else
                {
                    if(digits.Length > 0)
                    {
                        Command command = new();
                        command.moveAmount = int.Parse(digits);
                        digits = string.Empty;
                        char dir = commandString[i];

                        if (dir == 'L')
                            command.dir = -1;
                        else if (dir == 'R')
                            command.dir = 1;
                        else
                            throw new Exception("invalid dir in parse");

                        commands.Add(command);
                    }
                }
            }

            Console.WindowWidth = bounds.X + 2;

            //put player at starting point
            Player player = new Player();
            player.pos = tileMap[0].First().pos;

            List<Tile> visitedPoints = new();

            //now, execute commands
            for(int cmdNum = 0; cmdNum < commands.Count; ++cmdNum)
            {
                Command cmd = commands[cmdNum];

                //move
                for(int moveNum = 0; moveNum < cmd.moveAmount; ++moveNum)
                {
                    Point nextValidPos = player.pos;

                    if (player.facing == Player.Facing.Up || player.facing == Player.Facing.Down)
                    {
                        if (!GetNextValidPointY(player, player.NextPoint(), tileMap, bounds, out nextValidPos))
                        {
                            //cant move this way anymore...we are done
                            moveNum = cmd.moveAmount;
                        }
                    }
                    else //left right movement
                    {
                        if(!GetNextValidPointX(player, player.NextPoint(), tileMap, bounds, out nextValidPos))
                        {
                            //cant move this way anymore...we are done
                            moveNum = cmd.moveAmount;
                        }
                    }

                    //move player
                    if (player.pos != nextValidPos)
                    {
                        Tile path = new();
                        path.type = Tile.Type.BreadCrumb;
                        path.pos = player.pos;
                        visitedPoints.Add(path);
                        player.pos = nextValidPos;
                    }

                    //draw whole map...
                    /*
                    for (int yDisp = 0; yDisp < bounds.Y + 1; ++yDisp)
                    {
                        var tileListDisp = tileMap[yDisp];
                        for (int xDisp = 0; xDisp < bounds.X; ++xDisp)
                        {
                            var visited = visitedPoints.Where(t => t.pos.X == xDisp && t.pos.Y == yDisp).Select(t => t.type).ToList();
                            if (visited.Count > 0)
                            {
                                if (visited[0] == Tile.Type.BreadCrumb)
                                    Console.Write("x");
                            }
                            else
                            {
                                var matching = tileListDisp.Where(t => t.pos.X == xDisp).Select(t => t.type).ToList();
                                if (matching.Count == 0)
                                {
                                    if (xDisp >= tileListDisp.First().pos.X && xDisp <= tileList.Last().pos.X)
                                    {
                                        Console.Write(".");
                                    }
                                    else
                                    {
                                        Console.Write(" ");
                                    }
                                }
                                else if (matching[0] == Tile.Type.Wall)
                                    Console.Write("#");
                                else if (matching[0] == Tile.Type.Walkable)
                                    Console.Write(".");
                            }
                        }
                        Console.Write('\n');
                    }

                    Console.ReadLine();
                    Console.Clear();*/
                }

                //then rotate
                player.Rotate(cmd.dir);
            }            

            //draw whole map...
            for (int yDisp = 0; yDisp < bounds.Y + 2; ++yDisp)
            {
                List<Tile>? tileList = null;
                tileMap.TryGetValue(yDisp, out tileList);

                for(int xDisp = 0; xDisp < bounds.X + 2; ++xDisp)
                {
                    var visited = visitedPoints.Where(t => t.pos.X == xDisp && t.pos.Y == yDisp ).Select(t => t.type).ToList();
                    if (visited.Count > 0)
                    {
                        if (visited[0] == Tile.Type.BreadCrumb)
                            Console.Write("x");
                    }
                    else if(tileList != null)
                    {
                        var matching = tileList.Where(t => t.pos.X == xDisp).Select(t => t.type).ToList();
                        if (matching.Count == 0)
                        {
                            if (xDisp >= tileList.First().pos.X && xDisp <= tileList.Last().pos.X)
                            {
                                Console.Write(".");
                            }
                            else
                            {
                                Console.Write(" ");
                            }
                        }
                        else if (matching[0] == Tile.Type.Wall)
                            Console.Write("#");
                        else if (matching[0] == Tile.Type.Walkable)
                            Console.Write(".");
                    }
                }
                Console.Write('\n');
            }


            Console.WriteLine("final player: " + player);

            int score = (1000 * (player.pos.Y + 1)) + (4 * (player.pos.X+1)) + ((int)player.facing);
            Console.WriteLine("password: " + score);
            if (score >= 143052)
                Console.WriteLine("password is too high");
            else if( score <= 75332)
                Console.WriteLine("password is too low");

            //! 102056
        }

        bool GetNextValidPointX(Player player, Point next, Dictionary<int, List<Tile>> tileMap, Point bounds, out Point nextValidPos )
        {
            var tileList = tileMap[next.Y];

            //collision check, x needs to be between first and last, and not on a wall in there
            if (tileList.First().pos.X > next.X)
            {
                next = tileList.Last().pos;
            }
            else if (tileList.Last().pos.X < next.X)
            {
                next = tileList.First().pos;
            }

            if (tileList.Where(x => x.pos == next && x.type == Tile.Type.Wall).Any())
            {
                //movement blocked, cant move
                nextValidPos = player.pos;
                return false;
            }
            //else we are fine to move

            //sanity check we arent somewhere we shouldnt be...
            tileList = tileMap[next.Y];
            if (tileList.Where(x => x.pos.X == next.X && x.type == Tile.Type.Wall).Any())
            {
                throw new Exception("shouldnt be here, bad pos");
            }
            //sanity check we arent somewhere we shouldnt be...
            if (next.X < tileList.First().pos.X || next.X > tileList.Last().pos.X)
            {
                throw new Exception("shouldnt be here, bad pos");
            }

            nextValidPos = next;
            return true;
        }

        bool GetNextValidPointY(Player player, Point next, Dictionary<int, List<Tile>> tileMap, Point bounds, out Point nextValidPos)
        {
            if (player.pos.X == 95 && player.pos.Y == 149)
            {
                int x = 0;
                ++x;
            }
            //if we're outside bounds, wrap...
            //TODO: wrap to total top, or wrap within the connected block? ie, in the same room
            //lets..wrap in same room. if moving down we search up, if moving up we search down\
            //TODO: to find next space on other side of room...scan through valid spots in the width of the room until we hit an invalid, thats probably a room boundary
            List<Tile>? tileList = null;
            tileMap.TryGetValue(next.Y, out tileList);

            if(tileList == null)
            {
                Console.Write("Wrapped from Y: " + next.Y);
                if (next.Y == -1)
                    next.Y = bounds.Y;
                if (next.Y > bounds.Y)
                    next.Y = 0;
                Console.WriteLine(" to " + next.Y);

                
                tileMap.TryGetValue(next.Y, out tileList);
            }

            bool doScan = next.X < tileList.First().pos.X || next.X > tileList.Last().pos.X;

            int lastValidY = -1;
            if (doScan)
            {
                Console.Write("Wrapped from Y: " + next.Y);
                for (int newY = 1; newY < bounds.Y + 1; newY++)
                {
                    int nextCheck = (next.Y + newY); //when moving down
                    if (player.facing == Player.Facing.Up)
                        nextCheck = next.Y - newY;

                    if (nextCheck < 0)
                        nextCheck += (bounds.Y + 1);
                    else if (nextCheck > bounds.Y)
                        nextCheck -=  (bounds.Y + 1);

                    if (tileMap.TryGetValue(nextCheck, out tileList))
                    {
                        if (next.X >= tileList.First().pos.X && next.X <= tileList.Last().pos.X)
                        {
                            lastValidY = nextCheck;
                        }
                        else
                        {
                            next.Y = lastValidY != -1 ? lastValidY : player.pos.Y;
                            tileMap.TryGetValue(next.Y, out tileList);
                            Console.WriteLine(" to " + next.Y);
                            break;
                        }
                    }
                    else
                    {
                        throw new Exception("shouldnt get here");
                    }
                }

            }

            //now check for a wall, which will block us from wrapping / moving up / moving down
            if (tileList.Where(x => x.pos.X == next.X && x.type == Tile.Type.Wall).Any())
            {
                //movement blocked, cant move
                nextValidPos = player.pos;
                return false;
            }

            //sanity check we arent somewhere we shouldnt be...
            tileList = tileMap[next.Y];
            if (tileList.Where(x => x.pos.X == next.X && x.type == Tile.Type.Wall).Any())
            {
                throw new Exception("shouldnt be here, bad pos");
            }
            //sanity check we arent somewhere we shouldnt be...
            if (next.X < tileList.First().pos.X || next.X > tileList.Last().pos.X)
            {
                throw new Exception("shouldnt be here, bad pos");
            }

            nextValidPos = next;
            return true;
        }

        struct Command
        {
            public int moveAmount;
            public int dir; //-1 left, +1 right, 0 none

            public override string ToString()
            {
                return string.Format("Command: {0}) :{1}", moveAmount, dir);
            }
        }

        struct Tile
        {
            public enum Type
            {
                WalkableEdge,
                Walkable,
                Wall,
                BreadCrumb,
                WrapedPath
            }

            public Type type;
            public Point pos;

            public override string ToString()
            {
                return string.Format("Tile: {0}) :{1}", pos, type);
            }
        }

        struct Player
        {
            public enum Facing
            {
                Right,
                Down,
                Left,
                Up,
                MAX_FACING
            }

            public Facing facing = Facing.Right;
            public Point pos;

            public Point NextPoint()
            {
                Point next = pos;
                if (facing == Facing.Up)
                    next.Y++;
                else if (facing == Facing.Right)
                    next.X++;
                else if (facing == Facing.Down)
                    next.Y--;
                else if (facing == Facing.Left)
                    next.X--;
                else
                    throw new Exception("Facing dir is messed up");

                return next;
            }

            public void Rotate(int dir)
            {
                if(dir < 0)
                {
                    facing += 1;
                    if (facing >= Facing.MAX_FACING)
                        facing = (Facing)0;
                }

                if (dir > 0)
                {
                    facing -= 1;
                    if (facing < 0)
                        facing = (Facing)((int)Facing.MAX_FACING - 1);
                }
            }

            public Player()
            { }

            public override string ToString()
            {
                return string.Format("Player: {0}) :{1}", pos, facing);
            }
        }

    }


}
