using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using static ConsoleApp1.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleApp1.Solutions
{
    internal class Day17Opti : AbstractPuzzle
    {
        //hard coded rock formations..
        //make em all 4x4
        //aligned left/top
        struct RockFormation
        {
            public Point wh;
            public BitField[] bitsPerRow;
            public Int64 allBits;

            public RockFormation(string rock, int w, int h)
            {
                wh = new Point(w, h);

                bitsPerRow = new BitField[h];
                for(int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        if (IsSolidAtPoint(x, y, rock))
                        {
                            bitsPerRow[y][x] = true;
                        }
                    }
                    allBits = allBits << 7;
                    allBits |= bitsPerRow[y].bits;
                }
            }

            public bool RowCollision(int y, int xShift, BitField b)
            {
                return bitsPerRow[y].Collides(b, xShift);
            }

            //expects 0 based..
            bool IsSolidAtPoint(int x, int y, string rock)
            {
                int ind = (y * wh.X) + x;
                if(rock.Length > ind)
                {
                    if (rock[ind] == '#')
                        return true;
                    return false;
                }

                return false;
            }
        }


        struct BitField
        {
            public BitField() { }

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


            public UInt32 bits = 0;

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
                    }
                }
            }
        }

        struct Board
        {
            public const int Width = 7;

            public Int64 w = 7;

            public Int64 CurMaxHeight = 0;

            public List<BitField> Heights = new();
            //Span<BitField> Heights = new Span<BitField>(new BitField[1024UL * 1024UL * 1024UL * 8UL]);
            public Int64[] maxHeights = new Int64[Width];


            public Int64 yIndexOffset = 0;

            public Board()
            {
                BitField b = new();
                b.SetAllTrue();
                Heights.Add(b);
                CurMaxHeight = 0;
            }

            public void Clean()
            {
                int highestFull = 0;
                for (int y = Heights.Count - 1; y > 0 ; y--)
                {
                    if( (Heights[y].bits & 0b_0111_1111) == 0b_0111_1111)
                    {
                        //row is filled
                        highestFull = y;
                        break;
                    }
                }

                //want one less, at least
                highestFull -= 1;

                if (highestFull > 0)
                {
                    Console.WriteLine("Cleaning inaccessible locations....");

                    //visualize before we lkay waste to it:
                    //for (int i = highestFull; i > 0; i--)
                    //    Console.WriteLine(ToBinary(Heights[i].bits).Replace('0', ' ').Replace('1', '#'));

                    Heights.RemoveRange(0, highestFull - 1);
                    yIndexOffset += highestFull;
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

        override public void Part1()
        {
            var gasDirsStr = File.ReadAllText(InputFile!).Trim();
            var gasDirs = gasDirsStr.Select(x => x == '<' ? -1: 1).ToList().ToArray();

            var rocks = CreateRockFormations().ToArray();

            Int64 numRocksToDrop = 2022;
            numRocksToDrop = 1000000000000;
            //numRocksToDrop =(25345613) + (Random.Shared.Next(2778)); //NEW MAX MATCHLEN --> starting at: 349 and 3127 delta is: 2778 maxMatchLen: 1588832

            Board board = new();
            Point64 curRockPos = new Point64();
            Int64 curGasPos = 0;

            Console.WriteLine("starting...");
            var sw = Stopwatch.StartNew();

            RockFormation rock = new();

            int nextClean = 0;

            int rockInd = 0;
            for (Int64 curRockNum = 0; curRockNum < numRocksToDrop ; ++curRockNum)
            {
                //lets clear out our map of values as we go... im sure we can ditch some pretty far down...
                ++nextClean;
                if (nextClean >= 100000000) //1000000000)
                {
                    nextClean = 0;
                    float percentDOne = (((float)curRockNum + 1) / (float)numRocksToDrop);
                    float elapsed = ((float)sw.ElapsedMilliseconds / (float)1000);
                    float timeRemaining = 100.0f / percentDOne;
                    timeRemaining *= elapsed;
                    Console.WriteLine("Progress: " + percentDOne + "    Dropping rock: " + curRockNum + "  current max height: " + board.CurMaxHeight + " board keys: " + board.Heights.Count());
                    Console.WriteLine("Elapsed seconds: " + elapsed + "   estiamted time to completion seconds: " + timeRemaining);
                    board.Clean(); 
                }

                rock = rocks[rockInd];
                ++rockInd;
                if(rockInd > rocks.Length - 1)
                    rockInd = 0;

                //spawn:
                //  each rock appears so that its left edge is two units away from the left wall
                //  and its bottom edge is three units above the highest rock in the room (or the floor, if there isn't one).
                curRockPos.X = 2;
                curRockPos.Y = board.CurMaxHeight + 3 + rock.wh.Y;
                if (curRockNum != 0)
                    curRockPos.Y -= 1;

                //move:
                //gas, then drop, until collisions
                Point64 pos = new();
                for (;;)
                {
                    int posAdjust = gasDirs[(int)curGasPos];

                    //left/right collision... need to check if we can move left/right, or if that would move us into collision...
                    //bounds with board
                    Int64 newX = curRockPos.X + posAdjust;
                    if (!(   newX < 0 
                          || newX + (rock.wh.X - 1) >= board.w))
                          //|| board.CheckCollision(new Point64(newX, curRockPos.Y), rock, true, posAdjust)))
                    {
                        pos.X = newX;
                        pos.Y = curRockPos.Y;
                        bool ccol = false;
                        Int64 cyCheck = pos.Y - (rock.wh.Y - 1);
                        if (cyCheck <= board.CurMaxHeight)
                        {
                            Int64 yLimit = Math.Min(board.CurMaxHeight + 1, board.yIndexOffset + board.Heights.Count);

                            for (Int64 y = Math.Min(pos.Y, yLimit - 1); y > pos.Y - rock.wh.Y; y--)
                            {
                                BitField ba = board.Heights[(int)(y - board.yIndexOffset)];
                                Int64 rockBY = Math.Abs(y - (pos.Y));

                                //removing function call speeds it up
                                if ((ba.bits & (rock.bitsPerRow[(int)rockBY].bits << (int)pos.X)) > 0)
                                {
                                    ccol = true;
                                    break;
                                };

                                //if (rock.RowCollision((int)rockBY, (int)pos.X, ba))
                                //    return true;
                            }//y
                        }//col check loop

                        if (!ccol)
                        {
                            curRockPos.X = newX;
                        }                        
                    }

                    curGasPos++;
                    if (curGasPos >= gasDirs.Length)
                        curGasPos = 0;

                    //gravity.. lets try to go down... assume we are not colliding on y until we move...
                    //danger zone!
                    pos.X = curRockPos.X;
                    pos.Y = curRockPos.Y - 1;
                    bool col = false;
                    Int64 yCheck = pos.Y - (rock.wh.Y - 1);
                    if (yCheck <= board.CurMaxHeight)
                    {
                        Int64 yLimit = Math.Min(board.CurMaxHeight + 1, board.yIndexOffset + board.Heights.Count);

                        for (Int64 y = Math.Min(pos.Y, yLimit - 1); y > pos.Y - rock.wh.Y; y--)
                        {
                            BitField ba = board.Heights[(int)(y - board.yIndexOffset)];
                            Int64 rockBY = Math.Abs(y - (pos.Y));

                            //removing function call speeds it up
                            if ((ba.bits & (rock.bitsPerRow[(int)rockBY].bits << (int)pos.X)) > 0)
                            {
                                col = true;
                                break;
                            };

                            //if (rock.RowCollision((int)rockBY, (int)pos.X, ba))
                            //    return true;
                        }//y
                    }//col check loop

                    if (col)//board.CheckCollision(new Point64(curRockPos.X, curRockPos.Y - 1), rock, false, -1))
                    {
                        for (Int64 y = curRockPos.Y; y > curRockPos.Y - rock.wh.Y; y--)
                        {
                            while (y >= (board.yIndexOffset + board.Heights.Count))
                                board.Heights.Add(new BitField());

                            BitField bits = board.Heights[(int)(y - board.yIndexOffset)];

                            Int64 rockBY = Math.Abs(y - (curRockPos.Y));
                            bits.bits = bits.bits | (rock.bitsPerRow[rockBY].bits << (int)curRockPos.X);
                            board.Heights[(int)(y - board.yIndexOffset)] = bits;
                        }//y

                        board.CurMaxHeight = Math.Max(board.CurMaxHeight, curRockPos.Y + 1);
                        //board.AddRestingRock(curRockPos, rock);
                        break;  
                    }
                    else
                    {
                        curRockPos.Y -= 1;
                    }


                }//move loop                


            } //rocks

            Console.WriteLine("Highest solid point: " + (board.CurMaxHeight - 1) + " total board size: " + board.Heights.Count);
        }//part1



        override public void Part2()
        {
    
        }//part2()




    } //class
} //namespace
