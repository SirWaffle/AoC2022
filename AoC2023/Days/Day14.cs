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
    internal class Day14 : AbstractPuzzle<Day14>
    {
        public override void Init()
        {
            DoPart1 = false;
            DoPart2 = true;
        }

        //transpose a list of items, as if its a matrix/grid
        List<string> Transpose(List<string> map)
        {
            List<string> tMap = new();
            for (int x = 0; x < map[0].Length; ++x)
            {
                string t = String.Empty;
                for (int y = 0; y < map.Count; ++y)
                    t += map[y][x];

                tMap.Add(t);
            }

            return tMap;
        }

        public override void Part1Impl()
        {
            var map = File.ReadAllText(InputFileSample).Split("\r\n", StringSplitOptions.TrimEntries).ToList();

            //transpose so N is to the left <-
            List<string> tMap = Transpose(map);

            //collapse it all down to the left
            List<string> collapsed = new List<string>();
            for (int i = 0; i < tMap.Count; ++i)
            {
                string[] s = tMap[i].Split('#');

                for (int j = 0; j < s.Length; ++j)
                {
                    int count = s[j].Count(x => x == '.');
                    s[j] = s[j].Replace(".", "");
                    s[j] = s[j] + new string(Enumerable.Repeat('.', count).ToArray());
                }

                collapsed.Add(String.Join("#", s));
            }

            var weight = collapsed.Select(x => x.Select((c, i) => c == 'O' ? x.Length - i : 0).Sum()).Sum();
            Console.WriteLine("Answer p1: " + weight);
            //106990
        }




        //struct to track found cycles
        struct CD
        {
            public int i = 0;
            public int j = 0;
            public int reps = 0;
            public int len = 0;
            public CD() { }
        }

        bool FindCycle(List<int> list, List<int> cycle)
        {            
            cycle.Clear();

            CD loopPoints = new();
            for (int i = list.Count - 2; i >= 0; --i)
            {
                for(int j = i + 1; j < list.Count; ++j)
                //int j = list.Count - 1; //always require matching the last value
                {
                    //we will want to check previous entries for a loop
                    int loopLen = j - i;
                    int loopStart = i;
                    int prevLoopInd = loopStart;
                    bool match = true;
                    for (int numReps = 1; match == true; ++numReps)
                    {
                        prevLoopInd = prevLoopInd - loopLen; //go back the length of the possible loop
                        if (prevLoopInd < 0)
                            break; //no loop

                        //find a possible loop point by matching entries, and if the length is bigger than any we have found so far
                        if (list[loopStart] == list[j] && list[prevLoopInd] == list[loopStart])
                        {
                            //this is a possible loop, we have 3 matching indexes, the same distance apart. 
                            //now scan
                            match = true;
                            for (int li = 0; li < loopLen; ++li)
                            {
                                int d = list[prevLoopInd + li];
                                int e = list[i + li];
                                //the values starting from prevLoopInd and from i have to match
                                if (d != e)
                                {
                                    match = false;
                                    break;
                                }
                            }

                            if (match == true && numReps * loopLen >= (loopPoints.j - loopPoints.i) * loopPoints.reps)
                            {
                                //we may have found a loop, cache it for later...
                                loopPoints.i = i;
                                loopPoints.j = j;
                                loopPoints.reps = numReps;
                                loopPoints.len = j - i;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            if (loopPoints.len <= 1)
                return false;

            cycle.AddRange(list.GetRange(loopPoints.i, loopPoints.len));
            return true;

        }

        override public void Part2Impl()
        {
            var map = File.ReadAllText(InputFilePart1).Split("\r\n", StringSplitOptions.TrimEntries).ToList();

            //transpose, start with N <---
            List<string> tMap = Transpose(map);

            List<int> weights = new();

            int totalSpinCycles = 1000000000; //total spin cycles
            int maxWeightCycle = 0; //largest found cycle
            int weightCycleStart = 0; //track when the weight cycle started as we find larger ones

            int dir = 0; //dir we are facing

            for (int loops = 0; loops < totalSpinCycles || dir != 0; ++loops)
            {
                //collapse it all down to the left
                List<string> collapsed = new List<string>();
                for (int i = 0; i < tMap.Count; ++i)
                {
                    string[] s = tMap[i].Split('#');

                    for (int j = 0; j < s.Length; ++j)
                    {
                        int count = s[j].Count(x => x == '.');
                        s[j] = s[j].Replace(".", "");

                        if (dir == 2 || dir == 3) //we append to the left of the string, to simulate things moving --->
                            s[j] = new string(Enumerable.Repeat('.', count).ToArray()) + s[j];
                        else //add to end of string, to simulate things moving <----
                            s[j] = s[j] + new string(Enumerable.Repeat('.', count).ToArray());
                    }

                    collapsed.Add(String.Join("#", s));
                }

                if (++dir >= 4) //dir change
                    dir = 0;

                tMap = Transpose(collapsed); //swap  N <--- to  N ^

                if (dir == 0) //north, calc weights
                {
                    var weight = tMap.Select(x => x.Select((c, i) => c == 'O' ? x.Length - i : 0).Sum()).Sum();
                    weights.Add(weight);

                    if (weights.Count > 1 && loops > 1) //start cycle searching
                    {
                        List<int> weightCycle = new List<int>();
                        FindCycle(weights, weightCycle);

                        if (weightCycle.Count > 0)
                        {
                            var spinCycleCount = weights.Count;
                            if (weightCycle.Count > maxWeightCycle)
                            {
                                maxWeightCycle = weightCycle.Count;
                                weightCycleStart = spinCycleCount - weightCycle.Count;
                            }

                            Debug.Assert(weightCycle[(spinCycleCount - weightCycleStart) % weightCycle.Count] == weight);

                            var remSpinCycles = totalSpinCycles - spinCycleCount;
                            var est = weightCycle[((spinCycleCount - weightCycleStart) + remSpinCycles) % weightCycle.Count];

                            Console.WriteLine("         max cycle: " + weightCycle.Count + "  estimate at end: " + est);
                            //100531
                        }
                    }

                }//calculating weight after a cycle


            }//main loop

        } //Part2Impl func
    }
}
