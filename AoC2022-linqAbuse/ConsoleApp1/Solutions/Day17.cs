using System.Collections.Concurrent;
using System.Data;
using static ConsoleApp1.Utils;

namespace ConsoleApp1.Solutions
{
    internal class Day17 : AbstractPuzzle
    {
        //hard coded rock formations..
        //make em all 4x4
        //aligned left/top
        class RockFormation
        {
            string rockStr;
            public Point wh;

            public RockFormation(string rock, int w, int h)
            {
                rockStr = rock;
                wh = new Point(w, h);
            }

            //expects 0 based..
            public bool IsSolidAtPoint(int x, int y)
            {
                int ind = (y * wh.X) + x;
                if(rockStr.Length > ind)
                {
                    if (rockStr[ind] == '#')
                        return true;
                    return false;
                }

                return false;
            }
        }

        class Board
        {
            public const Int64 Width = 7;
            public static Int64 FLoorHeight = 0;

            public Int64 w { get { return Width; } }

            public Int64 CurMaxHeight = FLoorHeight;
            public List<Int64[]> Heights = new List<Int64[]>();

            public Board()
            {
                Heights.Add(new Int64[7] { 1,1,1,1,1,1,1});
            }

            public bool CheckCollision(Point64 pos, RockFormation rock)
            {
                bool collided = false;

                Int64 yCheck = pos.Y - (rock.wh.Y - 1);
                if (yCheck <= CurMaxHeight)
                {
                    for (Int64 y = pos.Y; y > pos.Y - rock.wh.Y && !collided; y--)
                    {
                        for (Int64 x = pos.X; x < pos.X + rock.wh.X && !collided; x++)
                        {
                            //scan across width and see if theres a possible overlap
                            if (Heights.Count > y && Heights[(int)y][x] > 0) //y - 1 sorta works
                            {
                                //possible overlap, see if this part of the rock is solid or not...
                                //grab index into rock shape...
                                Int64 rockX = x - (pos.X);
                                Int64 rockY = Math.Abs(y - (pos.Y));

                                collided = rock.IsSolidAtPoint((int)rockX, (int)rockY);
                            }

                        }//x
                    }//y
                }//col check loop

                return collided;
            }

            public void AddRestingRock(Point64 pos, RockFormation rock)
            {
                for (Int64 y = pos.Y; y > pos.Y - rock.wh.Y; y--)
                {
                    for (Int64 x = pos.X; x < pos.X + rock.wh.X; x++)
                    {
                        while (Heights.Count <= y)
                            Heights.Add(new Int64[Width]);

                        Int64 rockX = x - (pos.X);
                        Int64 rockY = Math.Abs(y - (pos.Y));

                        bool isSolid = rock.IsSolidAtPoint((int)rockX, (int)rockY);
                        if (isSolid)
                        {
                            Heights[(int)y][x] = Math.Max(Heights[(int)y][x], pos.Y); //y is the actual y pos, but pos.y gives good variationm to see diff rocks
                        }
                    }//x
                }//y

                //update heightmap and max height
                CurMaxHeight = Heights.Last().ToList().Max();
                CurMaxHeight = Heights.Count;
            }

            public void DrawBoard()
            {
                var rev = Heights.Reverse<Int64[]>().ToList();
                foreach (var row in rev)
                {
                    Console.WriteLine(row.ToList().Select(x => x.ToString()).Aggregate((a,b) => a + " " + b));
                }
            }
        }

        List<RockFormation> CreateRockFormations()
        {
            List<RockFormation> rocks = new();
            rocks.Add(new RockFormation("####", 4, 1)); // -
            rocks.Add(new RockFormation(".#.###.#.", 3, 3)); // +            
            rocks.Add(new RockFormation("..#..####", 3, 3)); // backwards l            
            rocks.Add(new RockFormation("####", 1, 4)); // |
            rocks.Add(new RockFormation("####", 2, 2)); //box
            return rocks;
        }
        //TODO: probably actually have to remember the shapes, not just the hieght map... sad...
        override public void Part1()
        {
            var gasDirsStr = File.ReadAllText(InputFile!).Trim();
            List<int> gasDirs = gasDirsStr.Select(x => x == '<' ? -1: 1).ToList();


            var rocks = CreateRockFormations();

            Int64 numRocksToDrop = 2022;
            numRocksToDrop = 1000000000000;
            //numRocksToDrop = 11; //ten is the test, need to count heights though...

            Board board = new();

            Point64 curRockPos = new Point64();
            Int64 curGasPos = 0;

            for(Int64 curRockNum = 0; curRockNum < numRocksToDrop ; ++curRockNum)
            {
                RockFormation rock = rocks[(int)(curRockNum % rocks.Count)];
                //spawn:
                //  each rock appears so that its left edge is two units away from the left wall
                //  and its bottom edge is three units above the highest rock in the room (or the floor, if there isn't one).
                Point64 spawnPoint = new Point64(2, board.CurMaxHeight + 3 + rock.wh.Y);
                if (curRockNum != 0)
                    spawnPoint.Y -= 1;

                curRockPos = spawnPoint;

                if(curRockNum % 100000 == 0)
                    Console.WriteLine("Dropping rock: " + curRockNum + "  current max height: " + board.CurMaxHeight);
                //board.DrawBoard();

                //move:
                //gas, then drop, until collisions
                for (;;)
                {
                    int posAdjust = gasDirs[(int)(curGasPos % gasDirs.Count)];
                    curGasPos++;

                    //left/right collision... need to check if we can move left/right, or if that would move us into collision...
                    //bounds with board
                    Int64 newX = curRockPos.X + posAdjust;
                    if (newX < 0)
                        newX = 0;
                    if (newX + (rock.wh.X - 1) >= board.w)
                        newX = board.w - rock.wh.X;

                    //now check if we are now colliding with other blocks...
                    //maybe we got ourselves a false positive collision...
                    if(board.CheckCollision(new Point64(newX, curRockPos.Y), rock))
                    {
                        //Console.WriteLine("   --Rock X moves by " + posAdjust + " to x: " + curRockPos.X + " not doing, collided");
                    }
                    else
                    {
                        //Console.WriteLine("   --Rock X moves by " + posAdjust + " to x: " + newX);
                        curRockPos.X = newX;
                    }

                    

                    

                    //gravity.. lets try to go down... assume we are not colliding on y until we move...
                    //danger zone!
                    if (!board.CheckCollision(new Point64(curRockPos.X, curRockPos.Y - 1), rock))
                    {
                        curRockPos.Y -= 1;
                        //Console.WriteLine("   --Rock Y moves by -1 to y: " + curRockPos.Y);
                    }
                    else //rock pos is at the spot we would overlap on next step
                    {
                        //Console.WriteLine("   --Rock Y coms to rest at y: " + curRockPos.Y);
                        board.AddRestingRock(curRockPos, rock);
                        break;
                    }


                }//move loop                


            } //rocks

            Console.WriteLine("Highest solid point: " + (board.CurMaxHeight - 1));

        }//part1

  

        override public void Part2()
        {
    
        }//part2()




    } //class
} //namespace
