using System.Collections.Concurrent;
using System.Drawing;

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
            public const int Width = 7;
            public static int FLoorHeight = 0;

            public int w { get { return Width; } }

            public int CurMaxHeight = FLoorHeight;
            public int[] Heights = new int[Width];

            public bool CheckCollision(Point pos, RockFormation rock)
            {
                bool collided = false;

                int yCheck = pos.Y - (rock.wh.Y - 1);
                if (yCheck <= CurMaxHeight)
                {
                    for (int y = pos.Y; y > pos.Y - rock.wh.Y && !collided; y--)
                    {
                        for (int x = pos.X; x < pos.X + rock.wh.X && !collided; x++)
                        {
                            //drop y by one since we're looking into the future
                            //scan across width and see if theres a possible overlap
                            if (Heights[x] >= y)
                            {
                                //possible overlap, see if this part of the rock is solid or not...
                                //grab index into rock shape...
                                int rockX = x - (pos.X);
                                int rockY = Math.Abs(y - (pos.Y));

                                collided = rock.IsSolidAtPoint(rockX, rockY);
                            }

                        }//x
                    }//y
                }//col check loop

                return collided;
            }

            public void AddRestingRock(Point pos, RockFormation rock)
            {
                //update the heightmap based on rock formation
                for (int y = 0; y < rock.wh.Y; y++)
                {
                    for (int x = 0; x < rock.wh.X; x++)
                    {
                        bool isSolid = rock.IsSolidAtPoint(x, y);
                        if (isSolid)
                        {
                            Heights[pos.X + x] = Math.Max(Heights[pos.X + x], pos.Y - y);
                        }
                    }
                }

                //update heightmap and max height
                CurMaxHeight = Heights.ToList().Max();
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

            int numRocksToDrop = 2022;
            //numRocksToDrop = 11; //ten is the test, need to count heights though...
            //heights for each col in sample after 10 are:
            //14  14  13  13  17  15  0

            //works up to rock 7 ( 7 being 0 based ) -- the apperance of the straight line again...
            //rock 8 is landing wrong...

            //3106 too low, but the answer to someoneelses puzzle...
            //3400 too high
            //3190, too low

            Board board = new();

            Point curRockPos = new Point();
            int curGasPos = 0;

            for(int curRockNum = 0; curRockNum < numRocksToDrop ; ++curRockNum)
            {
                RockFormation rock = rocks[curRockNum % rocks.Count];
                //spawn:
                //  each rock appears so that its left edge is two units away from the left wall
                //  and its bottom edge is three units above the highest rock in the room (or the floor, if there isn't one).
                Point spawnPoint = new Point(2, board.CurMaxHeight + 3 + rock.wh.Y);

                curRockPos = spawnPoint;

                Console.WriteLine("Dropping rock: " + curRockNum + "  current max height: " + board.CurMaxHeight + "  board heights: " + board.Heights.ToList().Select(x => x.ToString()).Aggregate((a,b)=> a + " " + b));

                //move:
                //gas, then drop, until collisions
                for (;;)
                {
                    int posAdjust = gasDirs[curGasPos % gasDirs.Count];
                    curGasPos++;

                    //left/right collision... need to check if we can move left/right, or if that would move us into collision...
                    //bounds with board
                    int newX = curRockPos.X + posAdjust;
                    if (newX < 0)
                        newX = 0;
                    if (newX + (rock.wh.X - 1) >= board.Heights.Length)
                        newX = board.Heights.Length - rock.wh.X;

                    //now check if we are now colliding with other blocks...
                    //maybe we got ourselves a false positive collision...
                    if(board.CheckCollision(new Point(newX, curRockPos.Y), rock))
                    {
                        //undo the xMove
                        newX = curRockPos.X;
                    }

                    curRockPos.X = newX;

                    Console.WriteLine("   --Rock X moves by " + posAdjust + " to x: " + curRockPos.X);

                    //gravity.. lets try to go down... assume we are not colliding on y until we move...
                    //danger zone!
                    if (!board.CheckCollision(new Point(curRockPos.X, curRockPos.Y - 1), rock))
                    {
                        curRockPos.Y -= 1;
                        Console.WriteLine("   --Rock Y moves by -1 to y: " + curRockPos.Y);
                    }
                    else //rock pos is at the spot we would overlap on next step
                    {

                        board.AddRestingRock(curRockPos, rock);
                        break;
                    }


                }//move loop                


            } //rocks

            Console.WriteLine("Highest solid point: " + board.CurMaxHeight);

        }//part1

  

        override public void Part2()
        {
    
        }//part2()




    } //class
} //namespace
