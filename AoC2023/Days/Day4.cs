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
    internal class Day4: AbstractPuzzle<Day4>
    {
        public override void Init()
        {
            DoPart1 = false;
            DoPart2 = true;
        }

        override public void Part1Impl()
        {
            double points = System.IO.File.ReadAllText(InputFilePart1).Split('\n', StringSplitOptions.None).Select(line => line.Replace("  "," ").Split(':')[1].Split('|').Select(s => s.Trim().Split(' ').Select(s => Int32.Parse(s))).ToList()).Select(nums => nums[0].Intersect(nums[1])).Where(l => l.Count() > 0).Aggregate(0.0, (a, i) => a + Math.Pow(2, i.Count() - 1));

            Console.WriteLine( "Total: " + points ); 
        } 

        override public void Part2Impl()
        {
            var winsPerCard = System.IO.File.ReadAllText(InputFilePart1).Split('\n', StringSplitOptions.None).Select(line => line.Replace("  ", " ").Split(':')[1].Split('|').Select(s => s.Trim().Split(' ').Select(s => Int32.Parse(s))).ToList()).Select(nums => nums[0].Intersect(nums[1])).Select(l => l.Count());

            var numCards = Enumerable.Repeat(1, winsPerCard.Count()).ToArray();

            var sum = winsPerCard.Select((wins, cardnum) => (cardnum, numCards[cardnum], Enumerable.Range(cardnum + 1, wins).Aggregate(0, (a, i) => a = numCards[i] += numCards[cardnum])));

            Console.WriteLine("Total: " + sum.Select(calc => calc.Item2).Sum()); //11787590
        }
    }
}
