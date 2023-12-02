using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Solutions
{
    internal abstract class AbstractPuzzle<PuzzleDay> where PuzzleDay : class
    {
        public string InputFilePart1
        {
            get;
            set;
        } = @"../../../InputFiles/" + typeof(PuzzleDay).Name + "_1.txt";

        public string InputFilePart2
        {
            get;
            set;
        } = @"../../../InputFiles/" + typeof(PuzzleDay).Name + "_2.txt";

        public bool DoPart1 { get; set; } = false;
        public bool DoPart2 { get; set; } = false;


        public void Part1() {
            if (DoPart1) Part1Impl();
        }
        abstract public void Part1Impl();

        public void Part2()
        {
            if(DoPart2) Part2Impl();
        }
        abstract public void Part2Impl();
    }
}
