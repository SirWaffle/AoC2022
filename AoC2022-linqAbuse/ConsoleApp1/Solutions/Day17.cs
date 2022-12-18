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
        class RockFormation
        {
            string rockStr;
            public Point wh;
            public BitField[] bitsPerRow;

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

        public static string ToBinary(int myValue)
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
            //public Dictionary<Int64, BitField> Heights = new();

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


            int maxCycLen = 0;
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
                                Console.WriteLine("NEW MAX MATCHLEN --> starting at: " + s + " and " + e + " delta is: " + (e - s) + " maxMatchLen: " + maxMatchLen);
                            }
                            else if(e - s < 5000 && matchLen > 1000)
                            {
                                //Console.WriteLine("starting at: " + s + " and " + e + " delta is: " + (e - s) + " len: " + matchLen);
                            }

                            //jump ahead the length and see if theres repeats
                            bool failedOut = false;
                            int delta = e - s;
                            int reps = 0;
                            for (int rep = e; rep < Heights.Count; rep += delta)
                            {
                                if(Heights[rep].bits != Heights[s].bits)
                                {
                                    failedOut = true;
                                }
                                reps++;
                            }

                            if(failedOut == false && maxCycLen < delta && reps > 1)
                            {
                                //if (MatchLength(s, e) == delta)
                                {
                                    maxCycLen = delta;
                                    //maybe we have a cycle!
                                    //Console.WriteLine("potential cycle starting at: " + s + " of length " + delta + " with " + reps + " repetitions");
                                    //Console.WriteLine("matchLen: " + MatchLength(s, e));
                                }
                            }
                        }
                    }
                }

                Console.WriteLine("maxMatchLen: " + maxMatchLen);
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
                var highestFull = maxHeights.Min() - 5;
                if (highestFull > yIndexOffset)
                {
                    //Console.WriteLine("Cleaning inaccessible locations....");
                    //visualize before we lkay waste to it:
                    int actualInd = GetAdjustedYIndex(highestFull);

                    for (int i = 0; i < actualInd; ++i)
                        Console.WriteLine(ToBinary(Heights[i].bits).Replace('0', ' ').Replace('1', '#'));

                    Heights.RemoveRange(0, actualInd);
                    yIndexOffset += actualInd;
                }
            }

            public void VisualizeTop()
            {
                Console.WriteLine(ToBinary(Heights.Last().bits));//.ToString("D8"));
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
                bool changed = false;
                for (Int64 y = pos.Y; y > pos.Y - rock.wh.Y; y--)
                {
                    while (y >= (yIndexOffset + Heights.Count))
                    {
                        changed = true;
                        Heights.Add(new BitField());
                    }

                    BitField bits = Heights[GetAdjustedYIndex(y)];

                    //todo, bad math...? at least behaving differently                    
                    //Int64 rockBY = Math.Abs(y - (pos.Y));
                    //int newVal = bits.bits | ( rock.bitsPerRow[rockBY].bits << (int)pos.X);
                    //bits.bits = newVal;
                    //Heights[GetAdjustedYIndex(y)] = bits;
                    
                    
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

                    /*
                    if(newVal != bits.bits)
                    {
                        int badmath = 0;
                        ++badmath;
                    }*/
                    if(changed)
                        Heights[GetAdjustedYIndex(y)] = bits;
                    

                }//y

                //update heightmap and max height
                CurMaxHeight = Math.Max(CurMaxHeight, pos.Y + 1); // Heights.Count;
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
            //numRocksToDrop = 1000000000000;
            numRocksToDrop = 4000; //NEW MAX MATCHLEN --> starting at: 349 and 3127 delta is: 2778 maxMatchLen: 1588832

            Board board = new();

            Point64 curRockPos = new Point64();
            Int64 curGasPos = 0;

            var sw = Stopwatch.StartNew();

            for (Int64 curRockNum = 0; curRockNum < numRocksToDrop ; ++curRockNum)
            {
                if(board.Heights.Count() > 345 && board.Heights.Count() < 352)
                {
                    Console.WriteLine("Rock num at height: " + board.Heights.Count + "   is  " + curRockNum);
                }
                if (board.Heights.Count() > 3124 && board.Heights.Count() < 3130)
                {
                    Console.WriteLine("Rock num at height: " + board.Heights.Count + "   is  " + curRockNum);
                }

                if (curRockNum == 782)
                {
                    Console.WriteLine("Height at rockNum 782: " + board.Heights.Count);
                }

                //lets clear out our map of values as we go... im sure we can ditch some pretty far down...                
                if (curRockNum > 0)// && curRockNum % 500000000 == 0)
                {
                    float percentDOne = (((float)curRockNum + 1) / (float)numRocksToDrop);
                    float elapsed = ((float)sw.ElapsedMilliseconds / (float)1000);
                    float timeRemaining = 100.0f / percentDOne;
                    timeRemaining *= elapsed;
                    //Console.WriteLine("Progress: " + percentDOne + "    Dropping rock: " + curRockNum + "  current max height: " + board.CurMaxHeight + " board keys: " + board.Heights.Count());
                    //Console.WriteLine("Elapsed seconds: " + elapsed + "   estiamted time to completion seconds: " + timeRemaining);
                    //board.Clean(); 
                }


                RockFormation rock = rocks[(int)(curRockNum % rocks.Count)];
                //spawn:
                //  each rock appears so that its left edge is two units away from the left wall
                //  and its bottom edge is three units above the highest rock in the room (or the floor, if there isn't one).
                Point64 spawnPoint = new Point64(2, board.CurMaxHeight + 3 + rock.wh.Y);
                if (curRockNum != 0)
                    spawnPoint.Y -= 1;

                curRockPos = spawnPoint;

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
                    else
                    {
                        //now check if we are now colliding with other blocks...
                        //maybe we got ourselves a false positive collision...
                        if (board.CheckCollision(new Point64(newX, curRockPos.Y), rock, true, posAdjust))
                        {
                            newX = curRockPos.X;
                            //Console.WriteLine("   --Rock X moves by " + posAdjust + " to x: " + curRockPos.X + " not doing, collided");                            
                        }
                        else
                        {
                            //Console.WriteLine("   --Rock X moves by " + posAdjust + " to x: " + newX);
                        }
                    }

                    curRockPos.X = newX;

                    

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

            Console.WriteLine("Cycle Search");
            board.SearchForCycle();

            Console.WriteLine("Highest solid point: " + (board.CurMaxHeight - 1) + " total board size: " + board.Heights.Count);

            //NEW MAX MATCHLEN --> starting at: 349 and 3127 delta is: 2778 maxMatchLen: 1588832
            //hrm, need to know how many rocks dropped... to make this cycle...
            Int64 cycleLen = 2778;

            Int64 massiveNumber = 1000000000000;

            /*
                Rock num at height: 347   is  227
                Rock num at height: 349   is  228
                Rock num at height: 351   is  229
                Rock num at height: 3125   is  1973
                Rock num at height: 3129   is  1974
            */
            double droppedPerCycle = 1973.5 - 228;
            Int64 heightPerCycle = 3127 - (349 - 1);

            Int64 curHeight = 349 - 1;
            Int64 dropped = 228;

            int numCycles = (int)((massiveNumber - dropped) / droppedPerCycle);
            int remaining = (int)((massiveNumber - dropped) % droppedPerCycle);

            curHeight += (numCycles * heightPerCycle);

            //782 leftovert rocks, add the height at 782 rocks
            curHeight += 1242;


            Console.WriteLine("Projected height at the crazy high number: " + curHeight + "  with " + remaining + " rocks left to drop...");
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
