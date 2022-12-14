using ConsoleApp1.Solutions;


string inputFile = @"InputFiles/Input12_1_ex.txt";

//for doing a specific puzzle:
var puzz = new Day12();
puzz.InputFile = inputFile;
puzz.Part1();

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
