using ConsoleApp1.Solutions;
using System.Diagnostics;
using System.Net.Http.Headers;

string inputFile = @"InputFiles/Input20_1.txt";

//for doing a specific puzzle:
var puzz = new Day20();
puzz.InputFile = inputFile;

var sw = Stopwatch.StartNew();
puzz.Part1();
sw.Stop();
Console.WriteLine("execution time: " + sw.ElapsedMilliseconds);

return;


DoPuzzle(inputFile, new Day1());
DoPuzzle(inputFile, new Day2());



void DoPuzzle(string fileName, AbstractPuzzle puzz)
{
    puzz.InputFile = inputFile;
    Console.WriteLine("--- part 1 ---");
    puzz.Part1();
    Console.WriteLine("--- part 2 ---");
    puzz.Part2();
}
