using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Solutions
{
    internal class Day2 : AbstractPuzzle
    {
        override public void Part1()
        {
            //todo
        }

        //4 ;'s
        override public void Part2()
        {
            var scoresList = new (char shape, int score)[] { ('A', 1), ('B', 2), ('C', 3), ('X', 1), ('Y', 2), ('Z', 3) }.ToList();
            int[] ldw = new int[] { 0, 3, 6 };
            int[] mod = new int[] { 2, 0, 1 };

            Console.WriteLine("score: " + System.IO.File.ReadAllText(InputFile!).Split('\n').ToList().Select(s => (scoresList.Where(x => x.shape == s.Split(" ")[0][0]).Select(x => x.score).FirstOrDefault(), scoresList.Where(x => x.shape == s.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1][0]).Select(x => x.score).FirstOrDefault())).Select(x => ldw[x.Item2 - 1] + (((x.Item1 - 1) + mod[x.Item2 - 1]) % 3) + 1).Sum());
        }
    }
}
