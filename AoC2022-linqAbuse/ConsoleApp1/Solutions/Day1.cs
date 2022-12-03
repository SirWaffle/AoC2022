using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Solutions
{
    internal class Day1: AbstractPuzzle
    {
        override public void Part1()
        {
            Both();
        }

        override public void Part2()
        {
            Both();
        }

        public void Both()
        {
            int count = 0;
            Console.WriteLine("Total: " + System.IO.File.ReadAllText(InputFile!).Split('\n', StringSplitOptions.None).ToList().Select(x => x.Trim() == string.Empty ? (-1, ++count) : (count, int.Parse(x))).GroupBy(x => x.Item1).Where(x => x.Key != -1).Select(x => x.Sum(s => s.Item2)).OrderByDescending(x => x).ToList().GetRange(0, 3).Aggregate((sum, val) => { Console.WriteLine("CurVal: " + val  + "  curSum: " + sum); return sum + val; }));
        }
    }
}
