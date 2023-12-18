using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AoC2023.Utilities;

namespace AoC2023.Solutions
{
    internal class Day11 : AbstractPuzzle<Day11>
    {
        public override void Init()
        {
            DoPart1 = false;
            DoPart2 = true;
        }

        class GalPos
        {
            public Int64 x;
            public Int64 y;
            public char c;

            public override string ToString() { return "(" + x + "," + y + ")"; }
        }

        override public void Part1Impl()
        {
            DoTheThing(1);
            //p1 = 9723824
        }
        override public void Part2Impl()
        {
            DoTheThing(1000000 - 1);
            //731244261352
        }

        void DoTheThing(Int64 expandAmount)
        { 
            checked
            {
                var input = File.ReadAllText(InputFilePart1);
                var width = input.IndexOf("\r");
                var height = input.Count(x => x == '\r') + 1;

                List<GalPos> gals = input.Replace("\r\n", "").Select((c, i) => new GalPos { x = i % width, y = (int)(i / width), c = c }).Where(x => x.c == '#').ToList();

                var expandByRow = Enumerable.Range(0, width).Select(i => (Int64)i).Where(i => gals.Find(g => g.y == i) == null).ToList();
                var expandByCol = Enumerable.Range(0, height).Select(i => (Int64)i).Where(i => gals.Find(g => g.x == i) == null).ToList();

                var expand = expandByRow.Select( val => gals.Select(g => g).Where(g => g.y > val).ToList()).ToList();
                expand.ForEach(l => l.ForEach(g => g.y += expandAmount));

                expand = expandByCol.Select(val => gals.Select(g => g).Where(g => g.x > val).ToList()).ToList();
                expand.ForEach(l => l.ForEach(g => g.x += expandAmount));

                var pairs = Enumerable.Range(0, gals.Count() - 1).Select(i => Enumerable.Range(i + 1, gals.Count() - i).Where(j => j < gals.Count()).Select(j => (gals[i], gals[j]))).SelectMany(l => l);
                var sum = pairs.Select(p => (p.Item1, p.Item2, Math.Abs(p.Item2.x - p.Item1.x) + Math.Abs(p.Item2.y - p.Item1.y))).Sum(d => d.Item3);

                Console.WriteLine("Answer p1: " + sum);
            }
        }

        void DoThingSMooshed(Int64 expandAmount)
        {
            var input = File.ReadAllText(InputFilePart1);
            var width = input.IndexOf("\r");
            var height = input.Count(x => x == '\r') + 1;

            List<GalPos> gals = input.Replace("\r\n", "").Select((c, i) => new GalPos { x = i % width, y = (int)(i / width), c = c }).Where(x => x.c == '#').ToList();

            Enumerable.Range(0, width).Select(i => (Int64)i).Where(i => gals.Find(g => g.y == i) == null).ToList().Select(val => gals.Select(g => g).Where(g => g.y > val).ToList()).ToList().ForEach(l => l.ForEach(g => g.y += expandAmount));
            Enumerable.Range(0, height).Select(i => (Int64)i).Where(i => gals.Find(g => g.x == i) == null).ToList().Select(val => gals.Select(g => g).Where(g => g.x > val).ToList()).ToList().ForEach(l => l.ForEach(g => g.x += expandAmount)); ;

            Console.WriteLine("Answer p1: " + Enumerable.Range(0, gals.Count() - 1).Select(i => Enumerable.Range(i + 1, gals.Count() - i).Where(j => j < gals.Count()).Select(j => (gals[i], gals[j]))).SelectMany(l => l).Select(p => (p.Item1, p.Item2, Math.Abs(p.Item2.x - p.Item1.x) + Math.Abs(p.Item2.y - p.Item1.y))).Sum(d => d.Item3));
        }



    }
}
