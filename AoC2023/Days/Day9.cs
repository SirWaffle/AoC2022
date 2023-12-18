using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AoC2023.Utilities;

namespace AoC2023.Solutions
{
    internal class Day9 : AbstractPuzzle<Day9>
    {
        public override void Init()
        {
            DoPart1 = true;
            DoPart2 = false;
        }


        override public void Part1Impl()
        {
            var lines = File.ReadAllText(InputFilePart1).Split("\r\n", StringSplitOptions.None).Select(line => line.Split(' ', StringSplitOptions.None).Select(str => Int64.Parse(str.Trim())));

            List<long> predicted = new();
            foreach (var line in lines)
            {
                List<List<long>> rows = new() { line.ToList() };
                
                for(var vals = rows.First(); !vals.All(x => x == 0); vals = rows.Last())
                    rows.Add(vals.Skip(1).Select((v, i) => vals[i + 1] - vals[i]).ToList());

                predicted.Add(rows.Reverse<List<long>>().Aggregate((long)0, (a, i) => a + i.LastOrDefault(0)));
            }

            Console.WriteLine("Answer p1: " + predicted.Sum());
            //1974232246
        }

        override public void Part2Impl()
        {
            var lines = File.ReadAllText(InputFilePart1).Split("\r\n", StringSplitOptions.None).Select(line => line.Split(' ', StringSplitOptions.None).Select(str => Int64.Parse(str.Trim())).ToList()).ToList();

            List<long> predicted = new();
            foreach (var line in lines)
            {
                List<List<long>> rows = new() { line };

                for (var vals = rows.First(); !vals.All(x => x == 0); vals = rows.Last())
                    rows.Add(vals.Skip(1).Select((v, i) => vals[i + 1] - vals[i]).ToList());

                predicted.Add(rows.Reverse<List<long>>().Aggregate((long)0, (a, i) => i.FirstOrDefault(0) - a));

               /*
                if (vals.Count() == 0)
                    vals.Add(0);

                //now back up..
                
                rows.Reverse();
                vals = rows.First();                
                foreach (var row in rows.Skip(1))
                {
                    row.Add(row.First() - vals.Last());
                    vals = row;
                }
                predicted.Add(vals.Last());
                */
            }

            Console.WriteLine("Answer p2: " + predicted.Sum());
            //928
        }
    }
}
