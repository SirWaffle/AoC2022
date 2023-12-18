using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices.Marshalling;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AoC2023.Utilities;

namespace AoC2023.Solutions
{
    internal class Day13 : AbstractPuzzle<Day13>
    {
        public override void Init()
        {
            DoPart1 = false;
            DoPart2 = true;
        }


        public string BitArrayToString(BitArray b)
        {
            string s = string.Empty;
            for(int i =  b.Length - 1; i >= 0 ; --i)
            {
                s += b[i] == false ? '0' : '1';
                s += " ";
            }
            return s;
        }

        public void LogBA(String msg, BitArray a)
        {
            Console.WriteLine(msg + ": " + BitArrayToString(a));
        }

        public void LogVertLineSolution(int solInd, int overlap, List<BitArray> rows)
        {
            foreach (var row in rows)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    if (i == solInd)
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.Write('|');
                    }

                    int test = i;
                    if (i >= solInd)
                        ++ test;

                    if(Math.Abs(solInd - test) <= overlap)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                    }

                    Console.Write(row[i] == false ? ' ' : '#');
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine();
            }
        }

        public void LogHorzLineSolution(int solInd, int overlap, List<BitArray> rows)
        {
            foreach (var row in rows)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    if (i == solInd)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.Write('|');
                    }

                    int test = i;
                    if (i >= solInd)
                        ++test;

                    if (Math.Abs(solInd - test) <= overlap)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                    }

                    Console.Write(row[i] == false ? ' ' : '#');
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine();
            }
        }

        public void NoSolution(int solInd, int overlap, List<BitArray> rows)
        {
            foreach (var row in rows)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    if (i == solInd)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.Write('|');
                    }

                    int test = i;
                    if (i >= solInd)
                        ++test;

                    if (Math.Abs(solInd - test) <= overlap)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                    }

                    Console.Write(row[i] == false ? ' ' : '#');
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine();
            }
        }

        public override void Part1Impl()
        {
            //InputFileSample
            //InputFilePart1
            var grids = File.ReadAllText(InputFilePart1).Split("\r\n\r\n", StringSplitOptions.TrimEntries).ToList();

            List<(int ind, int overlap, bool horz, bool vert)> allPossible = new();

            int gridNum = -1;
            foreach (var grid in grids)
            {
                ++gridNum;
                var sRow = grid.Split("\r\n", StringSplitOptions.TrimEntries);

                int gridWidth = sRow[0].Length;
                int gridHeight = sRow.Length;

                //create bitfields of the rows going forwards and backwards
                List<BitArray> bRow = new();
                List<BitArray> bRowRev = new();
                foreach (var line in sRow)
                {
                    bRow.Add(new BitArray(line.Length, false));
                    bRowRev.Add(new BitArray(line.Length, false));
                    for (int i = 0; i < line.Length; i++)
                    {
                        bRow[bRow.Count - 1].Set(i,line[i] == '#');
                        bRowRev[bRow.Count - 1].Set((line.Length - 1) - i, line[i] == '#');
                    }
                }

                //create bitfields of the cols going forwards and backwards
                List<BitArray> bCol = new();
                List<BitArray> bColRev = new();
                for (int x = 0; x < sRow.First().Length; x++)
                {
                    bCol.Add(new BitArray(sRow.Length, false));
                    bColRev.Add(new BitArray(sRow.Length, false));
                    for (int y = 0; y < sRow.Length; y++)
                    {
                        bCol[bCol.Count - 1].Set(y, sRow[y][x] == '#');
                        bColRev[bCol.Count - 1].Set((sRow.Length - 1) - y, sRow[y][x] == '#');
                    }
                }

                //horizontal sym / vertical line
                bool foundAny = false;

                List<(int ind, int overlap)> horzPossibles = ScanForSym("Hsym", gridWidth, gridHeight, bRow, bRowRev);
                horzPossibles.Where(x => x.overlap >= 2).OrderByDescending(x => x.overlap).ThenByDescending(x => x.ind).ToList();

                if (horzPossibles.Count > 0 && horzPossibles[0].overlap >= 1)
                {
                    foundAny = true;
                    allPossible.Add( (horzPossibles[0].ind, horzPossibles[0].overlap, true, false) );

                    //draw some cool ascii art
                    Console.WriteLine(gridNum + " - Solution: " + horzPossibles[0].ind);
                    LogVertLineSolution(horzPossibles[0].ind, horzPossibles[0].overlap, bRow);
                    Console.WriteLine();
                }

                //vertical sym / horz line
                List<(int ind, int overlap)> vertsPossibles = ScanForSym("Vsym", gridHeight, gridWidth, bCol, bColRev);
                vertsPossibles.Where(x => x.overlap >= 2).OrderByDescending(x => x.overlap).ThenByDescending(x => x.ind).ToList();

                if (vertsPossibles.Count > 0 && vertsPossibles[0].overlap >= 1)
                {
                    foundAny = true;
                    allPossible.Add((vertsPossibles[0].ind, vertsPossibles[0].overlap, false, true));

                    //draw some cool ascii art
                    Console.WriteLine(gridNum + " - Solution: " + vertsPossibles[0].ind);
                    LogHorzLineSolution(vertsPossibles[0].ind, vertsPossibles[0].overlap, bCol);
                    Console.WriteLine();

                }

                //draw some stuff for no solution
                if(foundAny == false)
                {
                    Console.WriteLine(gridNum + " - NO SOLUTIONS FOUND !!!!!!!!!!!!!!!");
                    LogHorzLineSolution(0, 0, bRow);
                    Console.WriteLine();
                    Console.BackgroundColor = ConsoleColor.Black;
                }
            
            }

            checked
            {
                var columnsLeftOfVertLine = allPossible.Where(x => x.horz == true).Sum(x => x.ind);
                var rowsAboveHorzLine = allPossible.Where(x => x.vert == true).Sum(x => x.ind);

                Console.WriteLine();
                Console.WriteLine("Answer p1:" + (columnsLeftOfVertLine + (100 * rowsAboveHorzLine)));
                //33728
            }
        }



        override public void Part2Impl()
        {
            //InputFileSample
            //InputFilePart1
            var grids = File.ReadAllText(InputFilePart1).Split("\r\n\r\n", StringSplitOptions.TrimEntries).ToList();

            List<(int ind, int overlap, bool horz, bool vert)> allPossible = new();

            int gridNum = -1;
            foreach (var grid in grids)
            {
                ++gridNum;
                var sRow = grid.Split("\r\n", StringSplitOptions.TrimEntries);

                int gridWidth = sRow[0].Length;
                int gridHeight = sRow.Length;

                //create bitfields of the rows going forwards and backwards
                List<BitArray> bRow = new();
                List<BitArray> bRowRev = new();
                foreach (var line in sRow)
                {
                    bRow.Add(new BitArray(line.Length, false));
                    bRowRev.Add(new BitArray(line.Length, false));
                    for (int i = 0; i < line.Length; i++)
                    {
                        bRow[bRow.Count - 1].Set(i, line[i] == '#');
                        bRowRev[bRow.Count - 1].Set((line.Length - 1) - i, line[i] == '#');
                    }
                }

                //create bitfields of the cols going forwards and backwards
                List<BitArray> bCol = new();
                List<BitArray> bColRev = new();
                for (int x = 0; x < sRow.First().Length; x++)
                {
                    bCol.Add(new BitArray(sRow.Length, false));
                    bColRev.Add(new BitArray(sRow.Length, false));
                    for (int y = 0; y < sRow.Length; y++)
                    {
                        bCol[bCol.Count - 1].Set(y, sRow[y][x] == '#');
                        bColRev[bCol.Count - 1].Set((sRow.Length - 1) - y, sRow[y][x] == '#');
                    }
                }

                //smudges!
                List<(int ind, int overlap, bool horz, bool vert)> possibleSmudgeSym = new();
                List<(int ind, int overlap, bool horz, bool vert)> nonSMudgeSol = new();

                for (int smudgeX = -1; smudgeX < sRow[0].Length; smudgeX++)
                {
                    for (int smudgeY = 0; smudgeY < sRow.Length; smudgeY++)
                    {

                        //TODO: add smudge to row/revrow /col/revcol
                        (bool oldRow, bool oldRevRow, bool OldCol, bool OldRevCol) olds = new();

                        if (smudgeX != -1)
                        { 
                            olds.oldRow = bRow[smudgeY][smudgeX];
                            bRow[smudgeY][smudgeX] = !bRow[smudgeY][smudgeX];

                            olds.oldRevRow = bRowRev[smudgeY][(sRow[0].Length - 1) - smudgeX];
                            bRowRev[smudgeY][(sRow[0].Length - 1) - smudgeX] = !bRowRev[smudgeY][(sRow[0].Length - 1) - smudgeX];

                            olds.OldCol = bCol[smudgeX][smudgeY];
                            bCol[smudgeX][smudgeY] = !bCol[smudgeX][smudgeY];

                            olds.OldRevCol = bColRev[smudgeX][(sRow.Length - 1) - smudgeY];
                            bColRev[smudgeX][(sRow.Length - 1) - smudgeY] = !bColRev[smudgeX][(sRow.Length - 1) - smudgeY];
                        }

                        //horizontal sym / vertical line
                        bool foundAny = false;

                        List<(int ind, int overlap)> horzPossibles = ScanForSym("Hsym", gridWidth, gridHeight, bRow, bRowRev);
                        horzPossibles = horzPossibles.Where(x => x.overlap >= 1).OrderByDescending(x => x.overlap).ThenByDescending(x => x.ind).ToList();

                        if (horzPossibles.Count > 0 && horzPossibles[0].overlap >= 1)
                        {
                            foundAny = true;
                            possibleSmudgeSym.Add((horzPossibles[0].ind, horzPossibles[0].overlap, true, false));

                            //draw some cool ascii art
                            //Console.WriteLine(gridNum + " - Solution: " + horzPossibles[0].ind);
                            //LogVertLineSolution(horzPossibles[0].ind, horzPossibles[0].overlap, bRow);
                            //Console.WriteLine();
                        }

                        //vertical sym / horz line
                        List<(int ind, int overlap)> vertsPossibles = ScanForSym("Vsym", gridHeight, gridWidth, bCol, bColRev);
                        vertsPossibles = vertsPossibles.Where(x => x.overlap >= 1).OrderByDescending(x => x.overlap).ThenByDescending(x => x.ind).ToList();

                        if (vertsPossibles.Count > 0 && vertsPossibles[0].overlap >= 1)
                        {
                            foundAny = true;
                            possibleSmudgeSym.Add((vertsPossibles[0].ind, vertsPossibles[0].overlap, false, true));

                            //draw some cool ascii art
                            //Console.WriteLine(gridNum + " - Solution: " + vertsPossibles[0].ind);
                            //LogHorzLineSolution(vertsPossibles[0].ind, vertsPossibles[0].overlap, bCol);
                            //Console.WriteLine();

                        }

                        //draw some stuff for no solution
                        if (foundAny == false)
                        {
                            //Console.WriteLine(gridNum + "(" + smudgeX + ", " + smudgeY + ") - NO SOLUTIONS FOUND !!!!!!!!!!!!!!!");
                            //NoSolution(0, 0, bRow);
                            //Console.WriteLine();
                            //Console.BackgroundColor = ConsoleColor.Black;
                        }

                        if(smudgeX == -1)
                        {
                            nonSMudgeSol.AddRange(possibleSmudgeSym);
                            possibleSmudgeSym.Clear();
                        }


                        //TODO: desmudge
                        if (smudgeX != -1)
                        {
                            bRow[smudgeY][smudgeX] = olds.oldRow;
                            bRowRev[smudgeY][(sRow[0].Length - 1) - smudgeX] = olds.oldRevRow;

                            bCol[smudgeX][smudgeY] = olds.OldCol;
                            bColRev[smudgeX][(sRow.Length - 1) - smudgeY] = olds.OldRevCol;
                        }


                        if (smudgeX == -1)
                            break;
                    } //smudgeY

                } //smudgeX

                if(possibleSmudgeSym.Count > 0)
                {
                    //remove old solution
                    if(nonSMudgeSol.Count > 0)
                    {
                         var noold = possibleSmudgeSym.Where(x => !(x.ind == nonSMudgeSol.First().ind &&
                                                                         x.overlap == nonSMudgeSol.First().overlap &&
                                                                         x.vert == nonSMudgeSol.First().vert &&
                                                                         x.horz == nonSMudgeSol.First().horz)).ToList();

                        if(noold.Count == 0)
                        {

                        }

                        possibleSmudgeSym = noold;
                    }

                    if (possibleSmudgeSym.Count > 0)
                    {
                        possibleSmudgeSym = possibleSmudgeSym.DistinctBy(x => x.ind).DistinctBy(x => x.overlap).ToList();
                        allPossible.Add(possibleSmudgeSym.First());
                    }
                    else if(nonSMudgeSol.Count > 0)
                    {

                    }
                }

            }//all grids

            checked
            {
                var columnsLeftOfVertLine = allPossible.Where(x => x.horz == true).Sum(x => x.ind);
                var rowsAboveHorzLine = allPossible.Where(x => x.vert == true).Sum(x => x.ind);

                Console.WriteLine();
                Console.WriteLine("Answer p1:" + (columnsLeftOfVertLine + (100 * rowsAboveHorzLine)));
                //33728
                //26822 - too low
            }
        }



        public List<(int ind, int overlap)> ScanForSym(string msg, int gridWidth, int gridHeight, List<BitArray> bRow, List<BitArray> bRowRev )
        {
            //scan for sym, lets do vertical line first
            List<(int ind, int overlap)> possibles = new();
            for (int x = 1; x < gridWidth; ++x)
            {
                //need an 'overlap mask', how much of the thing we are actually checkign against
                //that should be 1's equal to the smaller side...
                BitArray overlapMask = new BitArray(gridWidth, false);
                for (int i = 0; i < Math.Min(x, gridWidth - x); ++i)
                {
                    overlapMask.Set(i, true);
                }

                //LogBA(x + " OM  ", overlapMask);

                bool possible = true;
                for (int y = 0; y < gridHeight; ++y)
                {
                    BitArray check1 = new BitArray(bRow[y]);
                    BitArray check2 = new BitArray(bRowRev[y]);
                    //LogBA(y + " check1", check1);
                    //LogBA(y + " check2", check2);

                    //now left shift them so they match together...
                    check1.RightShift(x);
                    check2.RightShift(gridWidth - x);
                    //LogBA(y + " check1  S", check1);
                    //LogBA(y + " check2  S", check2);

                    check1.And(overlapMask);
                    check2.And(overlapMask);
                    //LogBA(y + " check1  OM", check1);
                    //LogBA(y + " check2  OM", check2);

                    var areEqual = check1.Cast<bool>().SequenceEqual(check2.Cast<bool>());
                    if (!areEqual)
                    {
                        possible = false;
                        break;
                    }
                }

                if (possible)
                {
                    possibles.Add((x, Math.Min(x, gridWidth - x)));
                    Console.WriteLine(msg + " ADDED POSSIBLE: loc: " + possibles.Last().ind + "   overlap size: " + possibles.Last().overlap);
                }
            }

            return possibles;
        }

    }
}
