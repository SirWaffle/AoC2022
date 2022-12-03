using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Solutions
{
    internal abstract class AbstractPuzzle: IPuzzle
    {
        public string? InputFile { get; set; }

        abstract public void Part1();

        abstract public void Part2();
    }
}
