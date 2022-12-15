using ConsoleApp1.Solutions;
using System.Diagnostics;
using System.Net.Http.Headers;

string inputFile = @"InputFiles/Input15_1.txt";

//for doing a specific puzzle:
var puzz = new Day15();
puzz.InputFile = inputFile;

var sw = Stopwatch.StartNew();
puzz.Part2();
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
