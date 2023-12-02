using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC2023.Solutions
{
    internal class Day1: AbstractPuzzle<Day1>
    {
        public Day1()
        {
            DoPart1 = false;
            DoPart2 = true;
        }

        // 1 ;
        override public void Part1Impl()
        {
            Console.WriteLine(
                "Total: " +
                System.IO.File.ReadAllText(InputFilePart1).Split('\n', StringSplitOptions.None).ToList()
                    .Select(x => new string(x.ToList().Where(x=>char.IsNumber(x)).ToArray()))
                    .Select(x => new string(new char[] { x.First(), x.Last() })).Select(x => int.Parse(x)).Sum()
            );
        }

        override public void Part2Impl()
        {
            List<string> nums = new List<string>() { "ZZZZZZZZ", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

            Console.WriteLine("Total: " + System.IO.File.ReadAllText(InputFilePart2).Split('\n', StringSplitOptions.None).ToList().Select(inputLine => inputLine.ToLower())
                .Select(inputString => nums.Where(numberString => inputString.Contains(numberString))
                    .Select(numberString => Enumerable.Range(0, inputString.Length).Where(rangeCount => inputString.IndexOf(numberString, rangeCount) == rangeCount).Select(rangeCount => (rangeCount, nums.IndexOf(numberString))).ToList())
                    .Prepend(Enumerable.Range(0, inputString.Length).Where(rangeCount => char.IsNumber(inputString[rangeCount])).Select(rangeCount => (rangeCount, int.Parse(inputString[rangeCount].ToString()))).ToList())
                    .SelectMany(inputList => inputList).OrderBy(indValPair => indValPair.Item1).DistinctBy(indValPair => indValPair.Item1).Select(indValPair => indValPair.Item2.ToString())
                    .Aggregate((aggStr, inStr) => aggStr + inStr))
                .Select(numsStr => new string(new char[] { numsStr.First(), numsStr.Last() })).Select(twoDigitNumStr => int.Parse(twoDigitNumStr)).Sum());
        }
    }
}
