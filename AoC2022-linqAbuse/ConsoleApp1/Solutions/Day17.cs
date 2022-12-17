using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
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
            BitField[] bitsPerRow;

            public RockFormation(string rock, int w, int h)
            {
                rockStr = rock;
                wh = new Point(w, h);

                int maxBit = 0;
                bitsPerRow = new BitField[h];
                for(int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        if (IsSolidAtPoint(x, y))
                        {
                            bitsPerRow[y][x] = true;
                            if(maxBit < x)
                            {
                                maxBit = x;
                            }
                        }
                    }
                }

                //need to jsutify the bits to the left...so taht the first 1 is on the leftside at pos 0 for the board.. which is 7 wide...
                for(int i =0; i < bitsPerRow.Length;++i)
                {
                    //bitsPerRow[i].bits = bitsPerRow[i].bits << (Board.Width - maxBit - 1);
                }
            }

            public bool RowCollision(int y, int xShift, BitField b)
            {
                return bitsPerRow[y].Collides(b, xShift);
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

        struct BitField
        {
            public BitField() { }

            public void Set(bool[] vals)
            {
                for (int i = 0; i < vals.Length; i++)
                {
                    if (vals[i])
                        this[i] = true;
                }
            }

            public void SetAllTrue()
            {
                for(int i =0; i < 8; i++)
                {
                    this[i] = true;
                }
            }

            public bool Collides(BitField other, int lShift = 0)
            {
                return (other.bits & ( bits << lShift ) ) > 0;
            }


            public int bits = 0;

            public bool this[int i]
            {
                get
                {
                    switch(i)
                    {
                        case 0: return (bits & 0b_1) > 0;
                        case 1: return (bits & 0b_10) > 0;
                        case 2: return (bits & 0b_100) > 0;
                        case 3: return (bits & 0b_1000) > 0;
                        case 4: return (bits & 0b_10000) > 0;
                        case 5: return (bits & 0b_100000) > 0;
                        case 6: return (bits & 0b_1000000) > 0;
                        case 7: return (bits & 0b_10000000) > 0;
                        case 8: return (bits & 0b_100000000) > 0;
                        default: throw new Exception("out of range");
                    }
                }
                set
                {
                    if (value == true)
                    {
                        switch (i)
                        {
                            case 0: bits = bits | 0b_1; break;
                            case 1: bits = bits | 0b_10; break;
                            case 2: bits = bits | 0b_100; break;
                            case 3: bits = bits | 0b_1000; break;
                            case 4: bits = bits | 0b_10000; break;
                            case 5: bits = bits | 0b_100000; break;
                            case 6: bits = bits | 0b_1000000; break;
                            case 7: bits = bits | 0b_10000000; break;
                            case 8: bits = bits | 0b_100000000; break;
                            default: throw new Exception("out of range");
                        }
                    }
                    else
                    {
                        throw new Exception("cant set to false yet");
                        switch (i)
                        {
                            case 0: bits = bits | 0b_1; break;
                            case 1: bits = bits | 0b_10; break;
                            case 2: bits = bits | 0b_100; break;
                            case 3: bits = bits | 0b_1000; break;
                            case 4: bits = bits | 0b_10000; break;
                            case 5: bits = bits | 0b_100000; break;
                            case 6: bits = bits | 0b_1000000; break;
                            case 7: bits = bits | 0b_10000000; break;
                            case 8: bits = bits | 0b_100000000; break;
                            default: throw new Exception("out of range");
                        }
                    }
                }
            }
        }

        class Board
        {
            public const int Width = 7;
            public static Int64 FLoorHeight = 0;

            public Int64 w { get { return Width; } }

            public Int64 CurMaxHeight = FLoorHeight;
            public Dictionary<Int64, BitField> Heights = new();

            public Int64[] maxHeights = new Int64[Width];

            public Board()
            {
                BitField b = new();
                b.SetAllTrue();
                Heights.Add(0, b);
                CurMaxHeight = 0;
            }

            public bool CheckCollision(Point64 pos, RockFormation rock, bool horizontalMove, int moveAmount)
            {
                bool collided = false;

                Int64 yCheck = pos.Y - (rock.wh.Y - 1);
                if (yCheck <= CurMaxHeight)                    
                {
                    //only check the one side for move dir instead of whole thing...

                    Int64 yStart = pos.Y;
                    Int64 xStart = pos.X;
                    Int64 xLimit = pos.X + rock.wh.X;
                    Int64 yLimit = pos.Y - rock.wh.Y;


                    for (Int64 y = pos.Y; y > yLimit && !collided; y--)
                    {
                        if (CurMaxHeight >= y)
                        {
                            BitField ba = Heights[(int)y];
                            Int64 rockBY = Math.Abs(y - (pos.Y));

                            //need to bitshift...
                            collided = rock.RowCollision((int)rockBY, (int)pos.X, ba);
     
                        }//y curheight check
                    }//y
                }//col check loop

                return collided;
            }

            public void AddRestingRock(Point64 pos, RockFormation rock)
            {
                bool changed = false;
                for (Int64 y = pos.Y; y > pos.Y - rock.wh.Y; y--)
                {
                    if (y > CurMaxHeight)
                    {
                        changed = true;
                        Heights.TryAdd(y, new BitField());
                    }

                    BitField bits = Heights[(int)y];

                    for (Int64 x = pos.X; x < pos.X + rock.wh.X; x++)
                    {
                        Int64 rockX = x - (pos.X);
                        Int64 rockY = Math.Abs(y - (pos.Y));
                        //TODO: bitify
                        bool isSolid = rock.IsSolidAtPoint((int)rockX, (int)rockY);
                        if (isSolid)
                        {
                            changed = true;
                            bits[(int)x] = true;
                            maxHeights[(int)x] = Math.Max(maxHeights[(int)x], y);
                        }
                    }//x

                    if(changed)
                        Heights[(int)y] = bits; //y is the actual y pos, but pos.y gives good variationm to see diff rocks

                }//y

                //update heightmap and max height
                //CurMaxHeight = Heights.Last().ToList().Max();
                //CurMaxHeight = Heights.Count;
                CurMaxHeight = Math.Max(CurMaxHeight, pos.Y + 1); // Heights.Count;
                Heights.TryAdd(pos.Y + 1, new BitField());
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

            var sw = Stopwatch.StartNew();

            for (Int64 curRockNum = 0; curRockNum < numRocksToDrop ; ++curRockNum)
            {
                //lets clear out our map of values as we go... im sure we can ditch some pretty far down...
                if(curRockNum > 0 && curRockNum % 50000000 == 0)
                {
                    //remove anything inaccessible
                    //bad math breathing room...
                    var highestFull = board.maxHeights.Min() - 5;
                    if (highestFull > 0)
                    {
                        Console.WriteLine("Cleaning inaccessible locations....");
                        board.Heights = board.Heights.OrderByDescending(x => x.Key).Where(x => x.Key >= highestFull).ToDictionary(x => x.Key, x => x.Value);
                    }

                }


                RockFormation rock = rocks[(int)(curRockNum % rocks.Count)];
                //spawn:
                //  each rock appears so that its left edge is two units away from the left wall
                //  and its bottom edge is three units above the highest rock in the room (or the floor, if there isn't one).
                Point64 spawnPoint = new Point64(2, board.CurMaxHeight + 3 + rock.wh.Y);
                if (curRockNum != 0)
                    spawnPoint.Y -= 1;

                curRockPos = spawnPoint;

                if (curRockNum % 5000000 == 0)
                {
                    float percentDOne = (((float)curRockNum + 1) / (float)numRocksToDrop);
                    float elapsed = ((float)sw.ElapsedMilliseconds / (float)1000);
                    float timeRemaining = 100.0f / percentDOne;
                    timeRemaining *= elapsed;
                    Console.WriteLine("Progress: " + percentDOne + "    Dropping rock: " + curRockNum + "  current max height: " + board.CurMaxHeight + " board keys: " + board.Heights.Count());
                    Console.WriteLine("Elapsed seconds: " + elapsed + "   estiamted time to completion seconds: " + timeRemaining);
                }
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
                    if(board.CheckCollision(new Point64(newX, curRockPos.Y), rock, true, posAdjust))
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
                    if (!board.CheckCollision(new Point64(curRockPos.X, curRockPos.Y - 1), rock, false, -1))
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
