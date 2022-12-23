using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.Serialization.Formatters;
using static ConsoleApp1.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleApp1.Solutions
{
    internal class Day23 : AbstractPuzzle
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

            SortedDictionary<Point, ProposedMove> proposedMap = new();
            SortedDictionary<Point, Elf> elfByPosMap = new();
            Dictionary<int, Elf> elvesMap = new();
            Point offset = new Point(0, 0);
            int elfId = 1;
            for(int y = 0; y < lines.Count; y++)
            {
                var line = lines[y];
                for(int x = 0; x< line.Length; x++)
                {
                    if (line[x] == '#')
                    {
                        Elf elf = new();
                        elf.id = elfId++;
                        elf.pos = new Point(x, y) + offset;
                        elfByPosMap.Add(elf.pos, elf);
                        elvesMap.Add(elf.id, elf);
                    }
                }
            }

            List<Dir> proposalOrder = new() { Dir.North, Dir.South, Dir.West, Dir.East };

            //crunch
            UInt64 rounds = 10;

            bool loopForever = false;
            if (part2)
                loopForever = true;

            UInt64 logEvery = 100000;
            UInt64 logStep = 0;

            for (UInt64 roundNum = 1; roundNum <= rounds || loopForever == true; roundNum++)
            {
                logStep++;
                if (logStep >= logEvery)
                {
                    Console.WriteLine("Round: " + roundNum);
                    Visualize(elfByPosMap, false);
                }

                //dont need to clear, depends on if delete/create gets more or less expensive than iterating everything in there, which will probably grow a ton
                //proposedMap.Clear();

                //part 1, get proposed dirs
                foreach (Elf elf in elvesMap.Values)
                {
                    Elf?[] neighbors = GetNeighbors(elf, elfByPosMap);

                    //figure out proposed dir
                    bool stayPut = true;
                    for(int i = 0; i < neighbors.Length; i++)
                    {
                        if (neighbors[i] != null)
                        {
                            stayPut = false;
                            break;
                        }
                    }

                    //dont move if nobody is near me
                    if (stayPut)
                        continue;

                    //add proposed dirs
                    for(int i = 0; i < proposalOrder.Count; i++)
                    {
                        if (AreSpacesClear(proposalOrder[i], ref neighbors))
                        {
                            //elf 11 went the wrong way..should have gone...up 1 i think? north?
                            if(elf.id == 11)// && roundNum == 1)
                            {
                                int x = 0;
                                ++x;
                            }
                            //add proposed dir, and we are done
                            AddProposedMove(elf, proposalOrder[i], proposedMap, roundNum);
                            break;
                        }
                    }

                }

                //scan proposals, move what can be moved.
                int moved = 0;
                foreach(var propMove in proposedMap.Values)
                {
                    if(propMove.proposedOnRound == roundNum)
                    {
                        if(propMove.proposedCount == 1)
                        {
                            ++moved;
                            //we were the only elf to propose this, so we can move.
                            MoveElf(propMove.firstProposedElf!, propMove.proposedPos, elfByPosMap);
                        }
                    }
                }

                if (moved == 0)
                {
                    Console.WriteLine("No elf moved on round: " + roundNum);
                    break;
                }
                else
                {
                    if (logStep >= logEvery)
                    {
                        logStep = 0;
                        Console.WriteLine("Num moved: " + moved);
                    }                        
                }

                //rotate proposalOrder
                var first = proposalOrder[0];
                proposalOrder.RemoveAt(0);
                proposalOrder.Add(first);
            }

            //maybe visualize, space will probably be too big...
            Visualize(elfByPosMap, false);
        }

        void Visualize(SortedDictionary<Point, Elf> elfByPosMap, bool showGrid)
        {
            Point minBounds = new Point();
            Point maxBounds = new Point();

            foreach (Point p in elfByPosMap.Keys)
            {
                minBounds.X = Math.Min(minBounds.X, p.X);
                minBounds.Y = Math.Min(minBounds.Y, p.Y);

                maxBounds.X = Math.Max(maxBounds.X, p.X);
                maxBounds.Y = Math.Max(maxBounds.Y, p.Y);
            }

            Point rect = new Point(maxBounds.X - minBounds.X, maxBounds.Y - minBounds.Y);

            if (showGrid)
            {
                for (int y = 0; y <= rect.Y; y++)
                {
                    for (int x = 0; x <= rect.X; x++)
                    {
                        //see if theres an elf in this pos...
                        Point offset = new Point(minBounds.X + x, minBounds.Y + y);
                        if (elfByPosMap.ContainsKey(offset))
                        {
                            //probably an elf...
                            //Console.Write("#");
                            Console.Write(elfByPosMap[offset].id.ToString("D2"));
                        }
                        else
                        {
                            Console.Write("..");
                        }
                        Console.Write(" ");
                    }
                    Console.WriteLine();
                }
            }

            rect.X++;
            rect.Y++;
            int emptyTiles = (rect.X * rect.Y) - elfByPosMap.Count;
            Console.WriteLine("Rect Size: " + rect + "   Empty rect space: " + emptyTiles);
        }

        void MoveElf(Elf elf, Point toPos, SortedDictionary<Point, Elf> elfByPosMap)
        {
            elfByPosMap.Remove(elf.pos);
            elf.pos = toPos;
            elfByPosMap.Add(toPos, elf);
        }

        void AddProposedMove(Elf elf, Dir dir, SortedDictionary<Point, ProposedMove> proposedMap, UInt64 round)
        {
            Point proposedPoint = GetAdjustPointByDir(ref elf.pos, dir);

            if(proposedMap.TryGetValue(proposedPoint, out ProposedMove propMove))
            {
                propMove.AddProposedMove(elf, round);
                proposedMap[proposedPoint] = propMove;
            }
            else
            {
                ProposedMove propMoveNew = new(proposedPoint);
                propMoveNew.AddProposedMove(elf, round);
                proposedMap.Add(proposedPoint, propMoveNew);
            }
        }

        bool AreSpacesClear(Dir dir, ref Elf?[] n)
        {
            if(dir == Dir.North)
            {
                return n[(int)Dir.NorthWest] == null && n[(int)Dir.North] == null && n[(int)Dir.NorthEast] == null;
            }
            else if(dir == Dir.East)
            {
                return n[(int)Dir.NorthEast] == null && n[(int)Dir.East] == null && n[(int)Dir.SouthEast] == null;
            }
            else if(dir == Dir.South)
            {
                return n[(int)Dir.SouthEast] == null && n[(int)Dir.South] == null && n[(int)Dir.SouthWest] == null;
            }
            else if(dir == Dir.West)
            {
                return n[(int)Dir.SouthWest] == null && n[(int)Dir.West] == null && n[(int)Dir.NorthWest] == null;
            }
            return false;
        }

        enum Dir
        {
            NorthWest,
            North, 
            NorthEast,
            West,
            East,
            SouthWest,            
            South,
            SouthEast,
        }

        Point GetAdjustPointByDir(ref Point p, Dir dir)
        {
            Point newp = p;
            switch(dir)
            {
                case Dir.NorthWest:
                    newp.X--;
                    newp.Y--;
                    break;
                case Dir.North:
                    newp.Y--;
                    break;
                case Dir.NorthEast:
                    newp.Y--;
                    newp.X++;
                    break;
                case Dir.East:
                    newp.X++;
                    break;
                case Dir.SouthEast:
                    newp.Y++;
                    newp.X++;
                    break;
                case Dir.South:
                    newp.Y++;
                    break;
                case Dir.SouthWest:
                    newp.Y++;
                    newp.X--;
                    break;
                case Dir.West:
                    newp.X--;
                    break;
            }

            return newp;
        }

        struct ProposedMove
        {
            public UInt64 proposedOnRound;
            public int proposedCount;
            public Elf? firstProposedElf;
            public Point proposedPos;

            public ProposedMove(Point pos)
            {
                proposedPos = pos;
            }

            public void AddProposedMove(Elf elf, UInt64 round)
            {
                if(round == proposedOnRound)
                {
                    proposedCount++;
                    firstProposedElf = null;
                }
                else
                {
                    proposedOnRound= round;
                    proposedCount = 1;
                    firstProposedElf = elf;
                }
            }
        }

        class Elf
        {
            public int id;
            public Point pos;

            public override string ToString()
            {
                return "Elf: " + id + " Pos: " + pos;
            }
        }

        Elf?[] GetNeighbors(Elf elf, SortedDictionary<Point, Elf> elfMap)
        {
            Elf?[] elves = new Elf?[8];
            int ind = 0;
            for(int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    Point p = elf.pos + new Point(x, y);
                    if (elfMap.TryGetValue(p, out Elf? neighborElf))
                    {
                        elves[ind] = neighborElf;
                    }
                    ++ind;       
                }
            }

            return elves;
        }

    }


}
