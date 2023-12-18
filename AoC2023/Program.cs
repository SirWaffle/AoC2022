using AoC2023.Solutions;
using System.Collections.Concurrent;
using System.Diagnostics;

var puzz = new Day17();

if (puzz.DoPart1)
{
    var sw = Stopwatch.StartNew();
    puzz.Part1(sw);

    /*
    Task t = Task.Factory.StartNew( () => { puzz.Part1(sw); }, TaskCreationOptions.LongRunning);

    while(t.IsCompleted == false)
    {
        Thread.Sleep(10000);
        Console.WriteLine("Puzzle 1 Task still working: " + sw.ElapsedMilliseconds + "ms");
        puzz.LogStats();
    }
    */

    Console.WriteLine("Part 1 execution time: " + sw.ElapsedMilliseconds + "ms");
    puzz.LogStats();
}

if (puzz.DoPart2)
{
    var sw = Stopwatch.StartNew();
    puzz.Part2(sw);

    /*
    Task t = Task.Factory.StartNew(() => { puzz.Part2(sw); }, TaskCreationOptions.LongRunning);

    while (t.IsCompleted == false)
    {
        Thread.Sleep(10000);
        Console.WriteLine("Puzzle 2 Task still working: " + sw.ElapsedMilliseconds + "ms");
        puzz.LogStats();
    }
    */

    Console.WriteLine("Part 2 execution time: " + sw.ElapsedMilliseconds + "ms");
    puzz.LogStats();
}