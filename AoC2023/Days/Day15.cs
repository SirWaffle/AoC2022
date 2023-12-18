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
    internal class Day15 : AbstractPuzzle<Day15>
    {
        public override void Init()
        {
            DoPart1 = false;
            DoPart2 = true;
        }

        public override void Part1Impl()
        {
            var input = File.ReadAllText(this.InputFilePart1).Split(",");

            List<UInt64> hashes = new();
            foreach (var line in input)
            {
                UInt64 val = 0;
                for (int i = 0; i < line.Length; i++)
                {
                    int ascii = (int)line[i];
                    val += (UInt64)ascii;
                    val *= 17;
                    val = val % 256;
                }
                hashes.Add(val);
            }

            Console.WriteLine("answer: " + hashes.Aggregate((UInt64)0,(a,i)=>a+i));
        }

        override public void Part2Impl()
        {
            var input = File.ReadAllText(this.InputFilePart1).Split(",");

            Dictionary<UInt64, List<(string label, string val)>> d = new();
            for(UInt64 i = 0; i < 256; i++)
                d.Add(i, new List<(string label, string val)>());

            foreach (var line in input)
            {
                string label = string.Empty;
                var oporval = string.Empty;

                if (line.Split("=").Length == 2)
                {
                    label = line.Split("=")[0];
                    oporval = line.Split("=")[1];
                }
                else
                {
                    label = line.Split("-")[0];
                    oporval = "-";
                }

                UInt64 hash = (UInt64)label.Select(x => (int)x).Aggregate(0, (a, i) => ((a + i) * 17) % 256);

                if (oporval == "-")
                {
                    d[hash] = d[hash].Select(x => x).Where(x => x.label != label).ToList();
                }
                else
                {
                    if (d[hash].Where(x => x.label == label).Any())
                    {
                        for(int i =0; i < d[hash].Count; i++)
                        {
                            if (d[hash][i].label == label)
                            {
                                d[hash][i] = (label, oporval);
                                break;
                            }
                        }
                    }
                    else
                    {
                        d[hash].Add((label, oporval));
                    }
                }
            }

            UInt64 totals = Enumerable.Range(0, 256).Select(i => d[(ulong)i].Select((x, l) => (UInt64)(i + 1) * (UInt64)(l + 1) * UInt64.Parse(x.val)).Aggregate((ulong)0, (a,i)=>a+i)).Aggregate((ulong)0,(a,i)=>a + i);

            Console.WriteLine("p2: " +  totals);
            //291774
        }
    }
}
