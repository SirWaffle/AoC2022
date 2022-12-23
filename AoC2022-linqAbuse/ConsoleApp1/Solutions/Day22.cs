using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.Serialization.Formatters;
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

            int w = 50; //49 or 50
            int h = 50;

            //0 - 49
            //50 - 99
            //100 - 149


            //make cube faces out of spots on the map
            Cube cubeMap = new Cube();
            cubeMap.faces = new Face[6];
            cubeMap.faces[0] = new Face(1, new Point(2 * w, 0), new Point(3 * w - 1, h - 1));
            cubeMap.faces[1] = new Face(2, new Point(w, 0), new Point(2 * w - 1, h - 1));
            cubeMap.faces[2] = new Face(3, new Point(w, h), new Point(2 * w - 1, 2 * h - 1));
            cubeMap.faces[3] = new Face(4, new Point(w, 2 * h), new Point(2 * w - 1, 3 * h - 1));
            cubeMap.faces[4] = new Face(5, new Point(0, 2 * h), new Point(w - 1, 3 * h - 1));
            cubeMap.faces[5] = new Face(6, new Point(0, 3 * h), new Point(w - 1, 4 * h - 1));
            cubeMap.LinkEdges();
     

            //turn the commands into a list
            List <Command> commands = new();
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

            Console.WindowWidth = bounds.X + 5;

            //put player at starting point
            Player player = new Player();
            player.pos = tileMap[0].First().pos;

            List<Tile> visitedPoints = new();

            //now, execute commands
            for(int cmdNum = 0; cmdNum < commands.Count; ++cmdNum)
            {
                Command cmd = commands[cmdNum];

                //move
                for (int moveNum = 0; moveNum < cmd.moveAmount; ++moveNum)
                {
                    Point nextValidPos = player.pos;

                    if (player.facing == Player.Facing.Up || player.facing == Player.Facing.Down)
                    {
                        if (part2)
                        {
                            if (!GetNextValidPointYPart2(ref player, player.NextPoint(), cubeMap, tileMap, bounds, out nextValidPos))
                            {
                                //cant move this way anymore...we are done
                                moveNum = cmd.moveAmount;
                            }
                        }
                        else
                        {
                            if (!GetNextValidPointY(player, player.NextPoint(), tileMap, bounds, out nextValidPos))
                            {
                                //cant move this way anymore...we are done
                                moveNum = cmd.moveAmount;
                            }
                        }
                    }
                    else //left right movement
                    {
                        if (part2)
                        {
                            if (!GetNextValidPointXPart2(ref player, player.NextPoint(), cubeMap, tileMap, bounds, out nextValidPos))
                            {
                                //cant move this way anymore...we are done
                                moveNum = cmd.moveAmount;
                            }
                        }
                        else
                        {
                            if (!GetNextValidPointX(player, player.NextPoint(), tileMap, bounds, out nextValidPos))
                            {
                                //cant move this way anymore...we are done
                                moveNum = cmd.moveAmount;
                            }
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



                    /*

                    for (int yDisp = 0; yDisp < bounds.Y + 2; ++yDisp)
                    {
                        List<Tile>? tileList = null;
                        tileMap.TryGetValue(yDisp, out tileList);

                        for (int xDisp = 0; xDisp < bounds.X + 2; ++xDisp)
                        {
                            Face? f = cubeMap.GetFaceForPoint(new Point(xDisp, yDisp));

                            if (false)//f.HasValue)
                            {
                                var edgeDir = f.Value.GetEdgeForPoint(new Point(xDisp, yDisp));
                                if (edgeDir != Face.EdgeDir.None)
                                {
                                    //lets write out the number of the connected face
                                    LinkedEdge? link = cubeMap.GetLinkedEdge(f.Value, edgeDir);
                                    if (link.HasValue)
                                    {
                                        Console.Write(link.Value.face.id);
                                    }
                                    else
                                    {
                                        Console.Write(f.Value.id);
                                    }
                                    continue;
                                }
                                else
                                {
                                    Console.Write(f.Value.id);
                                    continue;
                                }
                            }

                            var visited = visitedPoints.Where(t => t.pos.X == xDisp && t.pos.Y == yDisp).Select(t => t.type).ToList();
                            if (visited.Count > 0)
                            {
                                if (visited[0] == Tile.Type.BreadCrumb)
                                    Console.Write("x");
                            }
                            else if (tileList != null)
                            {
                                var matching = tileList.Where(t => t.pos.X == xDisp).Select(t => t.type).ToList();
                                if (matching.Count == 0)
                                {
                                    if (xDisp >= tileList.First().pos.X && xDisp <= tileList.Last().pos.X)
                                    {
                                        Console.Write("_");
                                    }
                                    else
                                    {
                                        Console.Write(" ");
                                    }
                                }
                                else if (matching[0] == Tile.Type.Wall)
                                    Console.Write("^");
                                else if (matching[0] == Tile.Type.Walkable)
                                    Console.Write("_");
                            }
                        }
                        Console.Write('\n');
                    }
                    Console.ReadKey();
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
                    Face? f = cubeMap.GetFaceForPoint(new Point(xDisp, yDisp));

                    if (false)//f.HasValue)
                    {
                        var edgeDir = f.Value.GetEdgeForPoint(new Point(xDisp, yDisp));
                        if (edgeDir != Face.EdgeDir.None)
                        {
                            //lets write out the number of the connected face
                            LinkedEdge? link = cubeMap.GetLinkedEdge(f.Value, edgeDir);
                            if (link.HasValue)
                            {
                                Console.Write(link.Value.face.id);
                            }
                            else
                            {
                                Console.Write(f.Value.id);
                            }
                            continue;
                        }
                        else
                        {
                            Console.Write(f.Value.id);
                            continue;
                        }
                    }
                   
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

            //20460 too low
            //122178 too low
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
            List<Tile>? tileList = null;
            tileMap.TryGetValue(next.Y, out tileList);

            if(tileList == null)
            {
                if (next.Y == -1)
                    next.Y = bounds.Y;
                if (next.Y > bounds.Y)
                    next.Y = 0;
                
                tileMap.TryGetValue(next.Y, out tileList);
            }

            bool doScan = next.X < tileList.First().pos.X || next.X > tileList.Last().pos.X;

            int lastValidY = -1;
            if (doScan)
            {
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

        bool GetNextValidPointXPart2(ref Player player, Point next, Cube cubeMap, Dictionary<int, List<Tile>> tileMap, Point bounds, out Point nextValidPos)
        {
            int rot = 0;
            Point remapped = cubeMap.GetRemappedPoint(player.pos, next, out rot);
            var tileList = tileMap[remapped.Y];

            if (tileList.Where(x => x.pos.X == remapped.X && x.type == Tile.Type.Wall).Any())
            {
                //movement blocked, cant move
                nextValidPos = player.pos;
                return false;
            }

            nextValidPos = remapped;

            //sanity check we arent somewhere we shouldnt be...
            tileList = tileMap[nextValidPos.Y];
            if (tileList.Where(x => x.pos.X == remapped.X && x.type == Tile.Type.Wall).Any())
            {
                throw new Exception("shouldnt be here, bad pos");
            }
            //sanity check we arent somewhere we shouldnt be...
            if (nextValidPos.X < tileList.First().pos.X || nextValidPos.X > tileList.Last().pos.X)
            {
                throw new Exception("shouldnt be here, bad pos");
            }

            //rotate player by rot
            player.Rotate(rot);
            return true;
        }

        bool GetNextValidPointYPart2(ref Player player, Point next, Cube cubeMap, Dictionary<int, List<Tile>> tileMap, Point bounds, out Point nextValidPos)
        {
            int rot = 0;
            Point remapped = cubeMap.GetRemappedPoint(player.pos, next, out rot);
            var tileList = tileMap[remapped.Y];

            if (tileList.Where(x => x.pos.X == remapped.X && x.type == Tile.Type.Wall).Any())
            {
                //movement blocked, cant move
                nextValidPos = player.pos;
                return false;
            }

            nextValidPos = remapped;

            //sanity check we arent somewhere we shouldnt be...
            tileList = tileMap[nextValidPos.Y];
            if (tileList.Where(x => x.pos.X == remapped.X && x.type == Tile.Type.Wall).Any())
            {
                throw new Exception("shouldnt be here, bad pos");
            }
            //sanity check we arent somewhere we shouldnt be...
            if (nextValidPos.X < tileList.First().pos.X || nextValidPos.X > tileList.Last().pos.X)
            {
                throw new Exception("shouldnt be here, bad pos");
            }

            //rotate player by rot
            player.Rotate(rot);
            return true;
        }

        struct FaceLinkData
        {
            public Face from;
            public Face.EdgeDir fromEdge;

            public Face to;
            public Face.EdgeDir toEdge;

            public int rotation;

            public FaceLinkData(Face fid, Face.EdgeDir fedg, Face tid, Face.EdgeDir todg, int relativerRot)
            {
                fromEdge= fedg;
                from = fid;
                toEdge = todg;
                to = tid;
                rotation = relativerRot;
            }

            public LinkedEdge GetOtherConnectedEdge(int faceId)
            {
                LinkedEdge linked = new();

                if (faceId == from.id)
                {
                    linked.face = to;
                    linked.edge = toEdge;
                    linked.rot = rotation;
                }
                else
                {
                    linked.face = from;
                    linked.edge = fromEdge;
                    linked.rot = 4 + ( -1 * rotation);
                }

                return linked;
            }
        }

        struct LinkedEdge
        {
            public Face face;
            public Face.EdgeDir edge;
            public int rot;
        }

        struct Face
        {
            public enum EdgeDir
            {
                Up, Down, Left, Right, None
            }

            public int id;
            public Point UL;
            public Point LR;

            //upper left, uppr right, bottom left, bottom right
            public Face(int _id, Point ul, Point lr)
            {
                id = _id;
                UL = ul;
                LR = lr;
            }

            public EdgeDir GetEdgeForPoint(Point p)
            {
                if (p.X == UL.X)
                    return EdgeDir.Left;
                if (p.X == LR.X)
                    return EdgeDir.Right;
                if (p.Y == UL.Y)
                    return EdgeDir.Up;
                if (p.Y == LR.Y)
                    return EdgeDir.Down;

                return EdgeDir.None;
            }

            public EdgeDir GetCrossedEdgeFromNextPoint(Point p)
            {
                if (p.X == UL.X - 1)
                    return EdgeDir.Left;
                if (p.X == LR.X + 1)
                    return EdgeDir.Right;
                if (p.Y == UL.Y - 1)
                    return EdgeDir.Up;
                if (p.Y == LR.Y + 1)
                    return EdgeDir.Down;

                return EdgeDir.None;
            }

            public bool IsPointOnFace(Point p)
            {
                if(p.X >= UL.X && p.X <= LR.X)
                {
                    if (p.Y >= UL.Y && p.Y <= LR.Y)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        struct Cube
        {
            public Face[] faces;
            Dictionary<(int faceId, Face.EdgeDir edge), FaceLinkData> edgeLinkage = new();

            public Cube()
            { }

            public Point GetRemappedPoint(Point oldPos, Point nextPos, out int playerRotation)
            {
                Point p = nextPos;
                playerRotation = 0;

                //find out where we went out of bounds..old pos can get the face, new pos to get the edge
                Face? oldFace = GetFaceForPoint(oldPos);
                Face? nextFace = GetFaceForPoint(nextPos);

                if(nextFace.HasValue == false || nextFace.Value.id != oldFace.Value.id)
                {
                    //went off the face, lets adjust the point. if theres no linked edge, we probably safely traveled from two connected ones on the grid already

                    Face.EdgeDir edge = oldFace.Value.GetCrossedEdgeFromNextPoint(nextPos);
                    LinkedEdge? linked = GetLinkedEdge(oldFace.Value, edge);
                    if(linked.HasValue)
                    {
                        //we need to warp the point
                        //TODO: figure out player rotation

                        PointFloat edgeMidTooOldPos;
                        if (edge == Face.EdgeDir.Up)
                            edgeMidTooOldPos = new PointFloat(oldPos.X - (oldFace.Value.UL.X + 24.5f), 0);
                        else if (edge == Face.EdgeDir.Down)
                            edgeMidTooOldPos = new PointFloat(oldPos.X - (oldFace.Value.UL.X + 24.5f), 0);
                        else if(edge == Face.EdgeDir.Left)
                            edgeMidTooOldPos = new PointFloat(0, oldPos.Y - (oldFace.Value.UL.Y + 24.5f));
                        else
                            edgeMidTooOldPos = new PointFloat(0, oldPos.Y - (oldFace.Value.UL.Y + 24.5f));

                       // edgeMidTooOldPos = oldPos - edgeMidTooOldPos;

                        //now rotate...
                        // (y, -x) = clockwise 90 degree
                        int rotDir = linked.Value.rot;

                        for(int i = 0; i < rotDir; ++i)
                        {
                            edgeMidTooOldPos = new PointFloat(edgeMidTooOldPos.Y, -1 * edgeMidTooOldPos.X);
                        }

                        //then get edge of arrived at face/edge
                        //and get its midpoint
                        PointFloat arrivedAtEdgeMidPoint;
                        if (linked.Value.edge == Face.EdgeDir.Up)
                            arrivedAtEdgeMidPoint = new PointFloat((linked.Value.face.UL.X + 24.5f) , linked.Value.face.UL.Y);
                        else if (linked.Value.edge == Face.EdgeDir.Down)
                            arrivedAtEdgeMidPoint = new PointFloat((linked.Value.face.UL.X + 24.5f), linked.Value.face.LR.Y);
                        else if (linked.Value.edge == Face.EdgeDir.Left)
                            arrivedAtEdgeMidPoint = new PointFloat(linked.Value.face.UL.X, (linked.Value.face.UL.Y + 24.5f));
                        else
                            arrivedAtEdgeMidPoint = new PointFloat(linked.Value.face.LR.X, (linked.Value.face.UL.Y + 24.5f));


                        //adjsut the point...
                        PointFloat adjusted = arrivedAtEdgeMidPoint + edgeMidTooOldPos;
                        p = new Point((int)adjusted.X, (int)adjusted.Y);
                        playerRotation = linked.Value.rot;
                    }
                }



                return p;
            }

            public Face? GetFaceForPoint(Point pos)
            {
                foreach(Face face in faces)
                {
                    if (face.IsPointOnFace(pos))
                        return face;
                }

                return null;
            }

            public LinkedEdge? GetLinkedEdge(Face face, Face.EdgeDir edge)
            {
                if(!edgeLinkage.ContainsKey((face.id, edge)))
                {
                    return null;
                }
                return edgeLinkage[(face.id, edge)].GetOtherConnectedEdge(face.id);
            }

            public void LinkEdges()
            {
                //only need to link faces that have warps...right? or maybe all, in case we have to manage change in directions
                //for now, just warp edges..
                AddLinkage(new FaceLinkData(faces[0], Face.EdgeDir.Up, faces[5], Face.EdgeDir.Down, 0));
                AddLinkage(new FaceLinkData(faces[0], Face.EdgeDir.Right, faces[3], Face.EdgeDir.Right, 2));
                AddLinkage(new FaceLinkData(faces[0], Face.EdgeDir.Down, faces[2], Face.EdgeDir.Right, 1));

                AddLinkage(new FaceLinkData(faces[1], Face.EdgeDir.Up, faces[5], Face.EdgeDir.Left, 1));
                AddLinkage(new FaceLinkData(faces[1], Face.EdgeDir.Left, faces[4], Face.EdgeDir.Left, 2));

                AddLinkage(new FaceLinkData(faces[2], Face.EdgeDir.Left, faces[4], Face.EdgeDir.Up, 3));

                AddLinkage(new FaceLinkData(faces[3], Face.EdgeDir.Down, faces[5], Face.EdgeDir.Right, 1));
            }

            void AddLinkage(FaceLinkData fink)
            {
                edgeLinkage.Add((fink.from.id, fink.fromEdge), fink);
                edgeLinkage.Add((fink.to.id, fink.toEdge), fink);
            }
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
                Walkable,
                Wall,
                BreadCrumb,
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
