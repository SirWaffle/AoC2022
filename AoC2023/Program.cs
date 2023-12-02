using AoC2023.Solutions;
using System.Diagnostics;

var puzz = new Day1();

if (puzz.DoPart1)
{
    var sw = Stopwatch.StartNew();

    puzz.Part1();
    sw.Stop();

    Console.WriteLine("Part 1 execution time: " + sw.ElapsedMilliseconds);
}

if (puzz.DoPart2)
{
    var sw = Stopwatch.StartNew();

    puzz.Part2();
    sw.Stop();

    Console.WriteLine("Part 2 execution time: " + sw.ElapsedMilliseconds);
}