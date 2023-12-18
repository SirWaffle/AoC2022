using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AoC2023.Utilities;

namespace AoC2023.Solutions
{
    internal class Day8 : AbstractPuzzle<Day8>
    {
        public override void Init()
        {
            DoPart1 = true;
            DoPart2 = false;
        }

        override public void Part1Impl()
        {
            var answer = new List<dynamic> { 0, "AAA" }.Chunk(2).Select( dirpos => File.ReadAllText(InputFilePart1).Split("\r\n\r\n", StringSplitOptions.None).Chunk(2).Select(input => (input[0].Replace('R', '2').Replace('L', '1').Select(x => Int32.Parse(x.ToString())).ToList(), input[1].Split("\r\n", StringSplitOptions.TrimEntries).Select(x => x.Replace(',', '=').Replace("(", "").Replace(")", "").Split('=', StringSplitOptions.TrimEntries)).ToDictionary(x => x[0]))).Select(dirmap => EnumerableExt.Range64(1, Int64.MaxValue).Where(step => (dirpos[1] = dirmap.Item2[dirpos[1]][dirmap.Item1[dirpos[0]++ % dirmap.Item1.Count]]) == "ZZZ").Select(step => step).First()).First()).First();
            Console.WriteLine("Answer p1: " + answer);
        }

        public void Part1Impl_o()
        {
            var input = File.ReadAllText(InputFilePart1).Split("\r\n\r\n", StringSplitOptions.None);

            var dirs = input[0].Replace('R', '2').Replace('L','1').Select(x => Int32.Parse(x.ToString())).ToList();
            var curDir = 0;

            var map = input[1].Split("\r\n", StringSplitOptions.TrimEntries).Select(x => x.Replace(',', '=').Replace("(", "").Replace(")", "").Split('=', StringSplitOptions.TrimEntries)).ToDictionary(x => x[0]);
            var curSpot = "AAA";
            
            var steps = 0;
         
            while(curSpot != "ZZZ")
            {
                ++steps;
                curSpot = map[curSpot][dirs[curDir]];

                ++curDir;
                if (curDir >= dirs.Count)
                    curDir = 0;
            }


            var score = steps;
            Console.WriteLine("Answer p1: " + steps); //16697
        }

        override public void Part2Impl()
        {
            var input = File.ReadAllText(InputFilePart1).Split("\r\n\r\n", StringSplitOptions.None);

            var dirs = input[0].Replace('R', '2').Replace('L', '1').Select(x => Int32.Parse(x.ToString())).ToArray();
            var curDir = 0;

            var map = input[1].Split("\r\n", StringSplitOptions.TrimEntries).Select(x => x.Replace(',', '=').Replace("(", "").Replace(")", "").Split('=', StringSplitOptions.TrimEntries)).ToDictionary(x => x[0]);
            var curSpot = map.Where(x => x.Key.EndsWith("A")).Select(x => x.Key).ToArray();

            var hits = curSpot.Select(x => new List<Int64>()).ToArray();
            var deltas = curSpot.Select(x => new List<Int64>()).ToArray();

            Int64 steps = 0;
            for(;;++steps)
            {
                int done = 0;
                for (int i = 0; i < curSpot.Length; ++i)
                {
                    curSpot[i] = map[curSpot[i]][dirs[curDir]];
                    if (curSpot[i][curSpot[i].Length - 1] == 'Z')
                    {
                        ++done;
                        hits[i].Add(steps);
                        if (hits[i].Count() > 1)
                        {
                            deltas[i].Add(hits[i][hits[i].Count() - 1] - hits[i][hits[i].Count() - 2]);
                            SetStat("Delta " + i, String.Join(",", deltas[i]));
                        }
                    }
                }

                if (done == curSpot.Length)
                    break;

                ++curDir;
                if (curDir >= dirs.Length)
                    curDir = 0;

                //possible cycle check
                if (deltas.Where(x => x.Count() > 5).Select(x => x.Distinct().Count() < 3).Count() == deltas.Count())
                    break;
            }

            foreach(var delt in deltas)
                Console.WriteLine("Deltas: " + String.Join(",", delt));

            Func<Int64, Int64, Int64> GCD = null;
            GCD = (Int64 a, Int64 b) => {
                return b == 0 ? a : GCD(b, a % b);
            };

            var score = deltas.Select(x => x.Last()).Aggregate((a, i) => (a * i) / GCD(a, i));

            Console.WriteLine("Answer p2: " + score);

        }
    }
}
