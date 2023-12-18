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
    internal class Day5 : AbstractPuzzle<Day5>
    {
        public override void Init()
        {
            DoPart1 = false;
            DoPart2 = true;
        }
        override public void Part1Impl()
        {
            List<(string cat, List<List<Int64>> catEntries)> parsed = File.ReadAllText(InputFilePart1).Replace("\r\n", "\n").Split("\n\n").Select(s => (s.Split(":")[0].Trim(), s.Split(":")[1].Trim().Split("\n").Where(s => s.Trim() != String.Empty).Select(s => s.Split(" ").Select(s => s.Trim().AsInt64()).ToList()).ToList())).ToList();

            var ConvertUsingMap = (long value, List<List<long>> map) =>
            {
                var conv = map.Where(entry => (value >= entry[1]) && (value <= entry[1] + entry[2])).FirstOrDefault(l => true, new List<long>() { 0, 0, 0 });
                return (value - conv[1]) + conv[0];
            };

            var locations = parsed[0].catEntries.SelectMany(x => x).Select(seed => parsed.Skip(1).Aggregate(seed, (value, input) => value = ConvertUsingMap(value, input.catEntries))).Order().ToList();

            Console.WriteLine("p1 Answer: " + locations.ToArray().ToString()); //340994526
        }

        override public void Part2Impl()
        {
            List<long> EmptyList = new();

            List<(string cat, List<List<Int64>> catEntries)> parsed = File.ReadAllText(InputFilePart1).Replace("\r\n", "\n").Split("\n\n").Select(s => (s.Split(":")[0].Trim(), s.Split(":")[1].Trim().Split("\n").Where(s => s.Trim() != String.Empty).Select(s => s.Split(" ").Select(s => s.Trim().AsInt64()).ToList()).ToList())).ToList();

            var locations = parsed[0].catEntries.SelectMany(x => x).Chunk(2).Select(seeds => EnumerableExt.Range64(seeds[0], seeds[1]).AsParallel().Select(seed => parsed.Skip(1).Aggregate(seed, (value, input) => value = input.catEntries.FirstOrDefault(entry => (value >= entry[1]) && (value <= entry[1] + entry[2]), EmptyList).wj_SelectSelf<long>(found => found.Count() == 0 ? value : (value - found.ElementAt(1)) + found.ElementAt(0))))).Select(l => l.Order().First()).Order();

            Console.WriteLine("p2 Answer: " + locations.First().ToString());




            /*
             //as one line, way slower
            List<long> EmptyList = new(); //prevent GC allocs for empoty list creations

            //this is a LOT slower
            var location = File.ReadAllText(InputFilePart1).Replace("\r\n", "\n").Split("\n\n").Select(s => (s.Split(":")[0].Trim(), s.Split(":")[1].Trim().Split("\n").Where(s => s.Trim() != String.Empty).Select(s => s.Split(" ").Select(s => s.Trim().AsInt64())))).ToArray().AsParallel().wj_SelectSelf(parsed => parsed.ElementAt(0).Item2.SelectMany(x => x).Chunk(2).Select(seeds => EnumerableExt.Range(seeds[0], seeds[1]).AsParallel().Select(seed => parsed.Skip(1).Aggregate(seed, (value, input) => value = input.Item2.FirstOrDefault(entry => (value >= entry.ElementAt(1)) && (value <= entry.ElementAt(1) + entry.ElementAt(2)), EmptyList).wj_SelectSelf<long>(found => found.Count() == 0 ? value : (value - found.ElementAt(1)) + found.ElementAt(0))))).Select(l => l.Order().First()).Order().First());

            Console.WriteLine("p2 Answer: " + location);
            */



            /*
            var seedRanges = parsed[0].catEntries.SelectMany(x => x).Chunk(2).Select(seeds => (seeds[0], seeds[1])).ToList();

            TaskLauncher launcher = new();
            List<Task<List<long>>> tasks = new();
            foreach(var sr in seedRanges)
            {
                Func<List<long>> cruncher = () => {
                    return EnumerableExt.Range(sr.Item1, sr.Item2).AsParallel().Select(seed => parsed.Skip(1).AsParallel().Aggregate(seed, (value, input) => value = ConvertUsingMap(value, input.catEntries))).Order().ToList();
                };

                tasks.Add(launcher.AddWork(cruncher));
            }

            Task.WaitAll(tasks.ToArray());
            
            long lowest = tasks.Select(x => x.Result[0]).Order().First();
            Console.WriteLine("Totals: " + lowest); //52210644  , 299 seconds mor or less with task launcher
            */
        }
    }
}
