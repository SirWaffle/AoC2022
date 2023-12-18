using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC2023.Solutions
{
    internal class Day2: AbstractPuzzle<Day2>
    {
        public override void Init()
        {
            DoPart1 = false;
            DoPart2 = true;
        }

        // 1 ;
        override public void Part1Impl()
        {
            List<int> imp = new List<int>();
            foreach (var kvp in new Dictionary<string, int>() { { "red", 12 }, { "green", 13 }, { "blue", 14 } })
            {
                imp = imp.Union(System.IO.File.ReadAllText(InputFilePart1).Split('\n', StringSplitOptions.None).Select(l => (Int32.Parse(l.Split(':')[0].Split(' ')[1].Trim()), l.Split(kvp.Key).SkipLast(1).Where(r => Int32.Parse(r.Trim().Split(' ').Last().Trim()) > kvp.Value).ToList())).Where(x => x.Item2.Count() > 0).Select(x => x.Item1).ToList()).ToList();
            }

            Console.WriteLine( "Total: " + Enumerable.Range(1, 100).Where(x => !imp.Any(y => y == x)).Sum());

            return;
            /*
            var maxes = new Dictionary<string, int>() { { "red", 12 }, { "green", 13 }, { "blue", 14 } };
            var lines = System.IO.File.ReadAllText(InputFilePart1).Split('\n', StringSplitOptions.None).ToList();
            //var posMax = lines.Where(l => l.Split(':')[1].Trim().Split(' ').ToList().Where(str => str.Trim().All(c => char.IsDigit(c)) && int.Parse(str.Trim()) > 12).Any()).ToList();
            var impr = lines.Select(l => (Int32.Parse(l.Split(':')[0].Split(' ')[1].Trim()), l.Split("red").SkipLast(1).Where(r => Int32.Parse(r.Trim().Split(' ').Last().Trim()) > 12).ToList())).ToList();
            var impb = lines.Select(l => (Int32.Parse(l.Split(':')[0].Split(' ')[1].Trim()), l.Split("blue").SkipLast(1).Where(r => Int32.Parse(r.Trim().Split(' ').Last().Trim()) > 14).ToList())).ToList();
            var impg = lines.Select(l => (Int32.Parse(l.Split(':')[0].Split(' ')[1].Trim()), l.Split("green").SkipLast(1).Where(r => Int32.Parse(r.Trim().Split(' ').Last().Trim()) > 13).ToList())).ToList();

            var ru = impr.Where(x => x.Item2.Count() > 0).Select(x => x.Item1).ToList();
            var bu = impb.Where(x => x.Item2.Count() > 0).Select(x => x.Item1).ToList();
            var gu = impg.Where(x => x.Item2.Count() > 0).Select(x => x.Item1).ToList();

            var imp = ru.Union(bu).Union(gu).ToList();
            var pos = Enumerable.Range(1, 100).Where(x => !imp.Any(y => y == x)).ToList();

            Console.WriteLine(
                "Total: " + pos.Sum()
            );*/



            /*
            var imp = lines.Select(l => (Int32.Parse(l.Split(':')[0].Split(' ')[1].Trim()), l.Split("red").SkipLast(1).Where(r => Int32.Parse(r.Trim().Split(' ').Last().Trim()) > 12).ToList())).Where(x => x.Item2.Count() > 0).Select(x => x.Item1).Union(
                  lines.Select(l => (Int32.Parse(l.Split(':')[0].Split(' ')[1].Trim()), l.Split("blue").SkipLast(1).Where(r => Int32.Parse(r.Trim().Split(' ').Last().Trim()) > 14).ToList())).Where(x => x.Item2.Count() > 0).Select(x => x.Item1)).Union(
                  lines.Select(l => (Int32.Parse(l.Split(':')[0].Split(' ')[1].Trim()), l.Split("green").SkipLast(1).Where(r => Int32.Parse(r.Trim().Split(' ').Last().Trim()) > 13).ToList())).Where(x => x.Item2.Count() > 0).Select(x => x.Item1));
            */



            /*
             //best beforethe one thats running now
            var lines = System.IO.File.ReadAllText(InputFilePart1).Split('\n', StringSplitOptions.None).ToList();


            var pos = Enumerable.Range(1, 100).Where(impgame => !lines.Select(l => (Int32.Parse(l.Split(':')[0].Split(' ')[1].Trim()), l.Split("red").SkipLast(1).Where(r => Int32.Parse(r.Trim().Split(' ').Last().Trim()) > 12).ToList())).Where(x => x.Item2.Count() > 0).Select(x => x.Item1).Union(
                 lines.Select(l => (Int32.Parse(l.Split(':')[0].Split(' ')[1].Trim()), l.Split("blue").SkipLast(1).Where(r => Int32.Parse(r.Trim().Split(' ').Last().Trim()) > 14).ToList())).Where(x => x.Item2.Count() > 0).Select(x => x.Item1)).Union(
                 lines.Select(l => (Int32.Parse(l.Split(':')[0].Split(' ')[1].Trim()), l.Split("green").SkipLast(1).Where(r => Int32.Parse(r.Trim().Split(' ').Last().Trim()) > 13).ToList())).Where(x => x.Item2.Count() > 0).Select(x => x.Item1)).Any(y => y == impgame)).Sum();


            Console.WriteLine(
                "Total: " + pos
            );
            */

            /*
            Console.WriteLine(
                "Total: " +
                System.IO.File.ReadAllText(InputFilePart1).Split('\n', StringSplitOptions.None).ToList()
                    .Select(line => (line.Trim().Split(':')[0], line.Trim().Split(':')[1]))
                    .Select(gameNpulls => (Int32.Parse(gameNpulls.Item1.Split(' ')[1]), gameNpulls.Item2.Split(';').ToList().Select(turn => turn.Split(',').ToList()).ToList()))
                    .Select(gnumNpullLines => (gnumNpullLines.Item1, gnumNpullLines.Item2.SelectMany(x => x).GroupBy(y => y.Split(' ')[1]).Select(z => z.OrderByDescending(w => w.Split(' ')[0]).First())))
                    .ToList().Select(gnmaxl => (gnmaxl.Item1, gnmaxl.Item2.

                    .Sum()
            );
            */
        }

        override public void Part2Impl() //70924
        {

            Console.WriteLine(
                "Total: " + System.IO.File.ReadAllText(InputFilePart1).Split('\n', StringSplitOptions.None).Select(line => new List<String>() { "red", "green", "blue" }.Select(col => (line + "ggggg").Split(col).SkipLast(1).Select(s => Int32.Parse(s.Trim().Split(' ').Last().Trim())).OrderDescending().FirstOrDefault(0)).Aggregate(1, (a, i) => a * i)) 
            );

            return;

            /*
            var tops = System.IO.File.ReadAllText(InputFilePart1).Split('\n', StringSplitOptions.None).Select(line => 
            (

            (line + "ggggg").Split("red").SkipLast(1).Select(s => Int32.Parse(s.Trim().Split(' ').Last().Trim())).OrderDescending().FirstOrDefault(0),
            (line + "ggggg").Split("green").SkipLast(1).Select(s => Int32.Parse(s.Trim().Split(' ').Last().Trim())).OrderDescending().FirstOrDefault(0),
            (line + "ggggg").Split("blue").SkipLast(1).Select(s => Int32.Parse(s.Trim().Split(' ').Last().Trim())).OrderDescending().FirstOrDefault(0)

            )
            ).Select(x => x.Item1 * x.Item2 * x.Item3).ToList(); 

            Console.WriteLine(
                "Total: " + tops.Sum() 
            );*/
        }
    }
}
