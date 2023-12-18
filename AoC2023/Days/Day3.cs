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

namespace AoC2023.Solutions
{
    internal class Day3: AbstractPuzzle<Day3>
    {
        public override void Init()
        {
            DoPart1 = false;
            DoPart2 = true;
        }

        override public void Part1Impl()
        {
            var xychar = System.IO.File.ReadAllText(InputFilePart1).Split('\n', StringSplitOptions.None).Select((line, lineNum) => Regex.Split(line, @"(\.)|(\d+)").Where(item => item != "" && item != "\n" && item != "\r").Select((str, ind) => (ind, lineNum, str)).Where(item => item.str != ".")).SelectMany(l => l);

            var groups = xychar.Select((item, ind) => (item.ind + xychar.Where(oi => oi.lineNum == item.lineNum && oi.ind < item.ind && oi.str.Length > 0).Sum(oi => oi.str.Length - 1), item.lineNum, item.str)).GroupBy(x => x.str.Length > 1 || x.str.All(ch => char.IsNumber(ch))).ToList();

            var sum = groups[0].Where(n => Enumerable.Range(n.Item1 - 1, n.str.Length + 2).Aggregate(false, (a, i) => a || groups[1].Any(sym => sym.lineNum >= n.lineNum - 1 && sym.lineNum <= n.lineNum + 1 && sym.Item1 == i))).Select(x => Int32.Parse(x.str)).Sum();

            Console.WriteLine( "Total: " + sum );
        } //537832

        override public void Part2Impl()
        {
            var xychar = System.IO.File.ReadAllText(InputFilePart1).Split('\n', StringSplitOptions.None).Select((line, lineNum) => Regex.Split(line, @"(\.)|(\d+)").Where(item => item != "" && item != "\n" && item != "\r").Select((str, ind) => (ind, lineNum, str)).Where(item => item.str != ".")).SelectMany(l => l);

            var groups = xychar.Select((item, ind) => (item.ind + xychar.Where(oi => oi.lineNum == item.lineNum && oi.ind < item.ind && oi.str.Length > 0).Sum(oi => oi.str.Length - 1), item.lineNum, item.str)).GroupBy(x => x.str.Length > 1 || x.str.All(ch => char.IsNumber(ch))).ToList();

            var gears = groups[1].Where(sym => sym.str == "*").Select(sym => (sym, groups[0].Where(n => Enumerable.Range(n.Item1 - 1, n.str.Length + 2).Aggregate(false, (a, i) => a || (sym.lineNum >= n.lineNum - 1 && sym.lineNum <= n.lineNum + 1 && sym.Item1 == i))))).Where(gearNums => gearNums.Item2.Count() == 2).Select(i => i.Item2.Aggregate(1, (a2, i2) => a2 * int.Parse(i2.str)));

            Console.WriteLine("Total: " + gears.Sum()); //81939900
        }
    }
}
