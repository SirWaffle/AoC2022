using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using static ConsoleApp1.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleApp1.Solutions
{
    internal class Day17 : AbstractPuzzle
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

        public static string ToBinary(uint myValue)
        {
            string binVal = Convert.ToString(myValue, 2);
            int bits = 0;
            int bitblock = 8;

            for (int i = 0; i < binVal.Length; i = i + bitblock)
            { 
                bits += bitblock; 
            }

            return binVal.PadLeft(bits, '0');
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

        sealed class Board
        {
            public const int Width = 7;

            public Int64 w { get { return Width; } }

            public Int64 CurMaxHeight = 0;

            public List<BitField> Heights = new();

            public Int64[] maxHeights = new Int64[Width];


            Int64 yIndexOffset = 0;

            public Board()
            {
                BitField b = new();
                b.SetAllTrue();
                Heights.Add(b);
                CurMaxHeight = 0;
            }

            public int GetAdjustedYIndex(Int64 y)
            {
                return (int)(y - yIndexOffset);
            }


            int maxMatchLenDelta = 0;
            int maxMatchLen = 0;
            public void SearchForCycle()
            {
                //OH SNAP! cycle found at: NEW MAX MATCHLEN --> starting at: 349 and 3127 delta is: 2778 maxMatchLen: 1588832
                //if any of this is to be trusted....

                //this will eat a ton of time....
                for (int s = 0; s < Heights.Count - 1; s++)
                {
                    for(int e = s + 1; e < Heights.Count; e++)
                    {
                        if (Heights[s].bits == Heights[e].bits)
                        {
                            int matchLen = MatchLength(s, e);
                            
                            if (matchLen >= maxMatchLen)
                            {
                                maxMatchLen = matchLen;
                                maxMatchLenDelta = e - s;
                                Console.WriteLine("NEW MAX MATCHLEN --> starting at: " + s + " and " + e + " delta is: " + (e - s) + " maxMatchLen: " + maxMatchLen);
                            }
                        }
                    }
                }

                Console.WriteLine("maxMatchLen: " + maxMatchLen + "  with delta: " + maxMatchLenDelta);
            }

            public int MatchLength(int start1, int start2)
            {
                int count = 0;
                while (start1 < Heights.Count && start2 < Heights.Count && Heights[start1].bits == Heights[start2].bits)
                {
                    start1++;
                    start2++;
                    count++;
                }

                return count;
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

            public bool CheckCollision(Point64 pos, RockFormation rock, bool horizontalMove, int moveAmount)
            {
                Int64 yCheck = pos.Y - (rock.wh.Y - 1);
                if (yCheck <= CurMaxHeight)                    
                {
                    Int64 yLimit = Math.Min(CurMaxHeight + 1, yIndexOffset + Heights.Count);

                    //only check the one side for move dir instead of whole thing...
                    for (Int64 y = Math.Min(pos.Y, yLimit - 1); y > pos.Y - rock.wh.Y; y--)
                    {
                        BitField ba = Heights[GetAdjustedYIndex(y)];
                        Int64 rockBY = Math.Abs(y - (pos.Y));

                        if (rock.RowCollision((int)rockBY, (int)pos.X, ba))
                            return true;
                    }//y
                }//col check loop

                return false;
            }

            public void AddRestingRock(Point64 pos, RockFormation rock)
            {
                for (Int64 y = pos.Y; y > pos.Y - rock.wh.Y; y--)
                {
                    while (y >= (yIndexOffset + Heights.Count))
                        Heights.Add(new BitField());

                    BitField bits = Heights[GetAdjustedYIndex(y)];
                 
                    Int64 rockBY = Math.Abs(y - (pos.Y));
                    bits.bits = bits.bits | ( rock.bitsPerRow[rockBY].bits << (int)pos.X);
                    Heights[GetAdjustedYIndex(y)] = bits;             
                }//y

                CurMaxHeight = Math.Max(CurMaxHeight, pos.Y + 1);
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
            List<int> gasDirs = gasDirsStr.Select(x => x == '<' ? -1: 1).ToList();

            var rocks = CreateRockFormations();

            Int64 numRocksToDrop = 2022;
            numRocksToDrop = 1000000000000;
            //numRocksToDrop =(25345613) + (Random.Shared.Next(2778)); //NEW MAX MATCHLEN --> starting at: 349 and 3127 delta is: 2778 maxMatchLen: 1588832


            //lets try to estimate the height at the detected cycle...;
            Int64 cycleStart = 349;
            Int64 cycleLen = 2778;

            Int64 rocksPerCycle = 1745; //ROCKS per cycle, got from printing out height + rocks
            Int64 heightPerCycle = 2778;

            //lets project values based on number of rocks and see if im right...
            // h348 = r228, h350 = r229
            // h350 seems to be part 1 of the cycle... so cycle starts on ROCK 229

            Int64 preCycHeight = 348;
            Int64 preCycRock = 228;

            Int64 numCycles = (numRocksToDrop - preCycRock) / rocksPerCycle;
            Int64 remainingRocks = (numRocksToDrop - preCycRock) % rocksPerCycle;

            List<Int64> heightAtRockNum = new();

            Int64 predictedHeight = 0;

            Console.WriteLine("Num cycles predicted: " + numCycles + " with " + remainingRocks + " remaining. ");

            Board board = new();
            Point64 curRockPos = new Point64();
            Int64 curGasPos = 0;

            var sw = Stopwatch.StartNew();

            int trackRockheights = 2;

            int nextClean = 0;

            int curCycles = 0;
            int rockInd = 0;
            for (Int64 curRockNum = 0; curRockNum < numRocksToDrop ; ++curRockNum)
            {
                /*
                if(trackRockheights > 0)
                    heightAtRockNum.Add(board.Heights.Count() - 1);

                if (board.Heights.Count() == cycleStart)
                {
                    Console.WriteLine("Rock num at height: " + (board.Heights.Count - 1) + "   is  " + curRockNum);
                }
                if (board.Heights.Count() == cycleStart - 1)
                {
                    Console.WriteLine("Rock num at height: " + (board.Heights.Count - 1) + "   is  " + curRockNum);
                }
                else if ((( (board.Heights.Count() - 1) - cycleStart) - 1) % cycleLen == 0) //starts at 351 here?
                {
                    curCycles++;

                    Int64 thisPredictedHeight = preCycHeight + (heightPerCycle * (curCycles - 1));
                    //Console.WriteLine("Current cycle height prediction: " + thisPredictedHeight);


                    //Console.WriteLine("Rock num at height: " + (board.Heights.Count - 1) + "   is  " + curRockNum);

                    if (board.Heights.Count() > 1000)
                    {
                        if (trackRockheights > 0)
                        {
                            --trackRockheights;

                            for (int i = 0; i < heightAtRockNum.Count; ++i)
                            {
                                if (remainingRocks >= (i - 3) && remainingRocks <= (i + 3))
                                {
                                    Console.WriteLine("at rock:  " + (i + 1) + "  height: " + heightAtRockNum[i]);
                                }
                            }

                            predictedHeight = preCycHeight + (heightPerCycle * numCycles) + (heightAtRockNum[(int)remainingRocks - 1] - (preCycHeight + heightPerCycle));
                            Console.WriteLine("Predicted height at end: " + predictedHeight);
                            heightAtRockNum.Clear();

                            if (trackRockheights == 0)
                               break;
                            //not: 1591977077353
                            //seem to range from 0 to 3 off...so lets try adding on each time...
                            //not: 1591977077354  //+1
                            //not: 1591977077355  //+2
                            //not: 1591977077356  //+3
                            //not: 1591977077357  //+4 maybe?
                        }
                    }
                }*/

                //lets clear out our map of values as we go... im sure we can ditch some pretty far down...
                ++nextClean;
                if (nextClean >= 1000000000)
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

                RockFormation rock = rocks[rockInd];
                ++rockInd;
                if(rockInd > rocks.Count - 1)
                    rockInd = 0;
                          
                //spawn:
                //  each rock appears so that its left edge is two units away from the left wall
                //  and its bottom edge is three units above the highest rock in the room (or the floor, if there isn't one).
                curRockPos = new Point64(2, board.CurMaxHeight + 3 + rock.wh.Y);
                if (curRockNum != 0)
                    curRockPos.Y -= 1;

                //move:
                //gas, then drop, until collisions
                for (;;)
                {
                    int posAdjust = gasDirs[(int)curGasPos];

                    //left/right collision... need to check if we can move left/right, or if that would move us into collision...
                    //bounds with board
                    Int64 newX = curRockPos.X + posAdjust;
                    if (!(   newX < 0 
                          || newX + (rock.wh.X - 1) >= board.w
                          || board.CheckCollision(new Point64(newX, curRockPos.Y), rock, true, posAdjust)))
                    {
                        curRockPos.X = newX;
                    }

                    curGasPos++;
                    if (curGasPos >= gasDirs.Count)
                        curGasPos = 0;

                    //gravity.. lets try to go down... assume we are not colliding on y until we move...
                    //danger zone!
                    if (board.CheckCollision(new Point64(curRockPos.X, curRockPos.Y - 1), rock, false, -1))
                    {
                        board.AddRestingRock(curRockPos, rock);
                        break;  
                    }
                    else
                    {
                        curRockPos.Y -= 1;
                    }


                }//move loop                


            } //rocks

            Console.WriteLine("Highest solid point: " + (board.CurMaxHeight - 1) + " total board size: " + board.Heights.Count);

            Console.WriteLine("Predicted height: " + predictedHeight + "  is off by :" + (predictedHeight - (board.CurMaxHeight - 1)));
            Console.WriteLine("Cycle Search");
           // board.SearchForCycle();

            //Console.WriteLine("Projected height at the crazy high number: " + curHeight + "  with " + remaining + " rocks left to drop...");
            //1591977077347  is too low -- 1973
            //1590154551001   too low...  --1975
            //1590726960846 too low...
            //1592093956503 --- not right
        }//part1



        override public void Part2()
        {
    
        }//part2()




    } //class
} //namespace
