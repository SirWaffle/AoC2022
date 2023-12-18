using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AoC2023.Utilities;

namespace AoC2023.Solutions
{
    internal class Day6 : AbstractPuzzle<Day6>
    {
        public override void Init()
        {
            DoPart1 = false;
            DoPart2 = true;
        }
        override public void Part1Impl()
        {
            var lines = File.ReadAllText(InputFilePart1).Split('\n', StringSplitOptions.None).Select(line => line.Split(':')[1].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

            var pairs = lines.ElementAt(0).Zip(lines.ElementAt(1)).Select(pair => (int.Parse(pair.First), int.Parse(pair.Second)));

            var answer = pairs.Select(pair => Enumerable.Range(0, pair.Item1 - 1).AsParallel().Where(time => time * (pair.Item1 - time) > pair.Item2)).Where(range => range.Count() > 0).Aggregate(1, (a, i) => a * i.Count());

            Console.WriteLine("Answer p1: " + answer.ToString()); //840336
        }

        override public void Part2Impl()
        {
            var lines = File.ReadAllText(InputFilePart1).Split('\n', StringSplitOptions.None).Select(line => String.Join("", line.Split(':')[1].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)));

            var pair = (Int64.Parse(lines.ElementAt(0)), Int64.Parse(lines.ElementAt(1)));

            var winngingRange = EnumerableExt.Range64(0, pair.Item1 - 1).AsParallel().Where(time => time * (pair.Item1 - time) > pair.Item2);

            Console.WriteLine("Answer p2: " + winngingRange.Count()); //41382569
        }
    }
}
