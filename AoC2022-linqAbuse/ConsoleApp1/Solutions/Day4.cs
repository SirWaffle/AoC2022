using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace ConsoleApp1.Solutions
{
    internal class Day4 : AbstractPuzzle
    {
        override public void Part1()
        {
            Console.WriteLine("fully contained: " + File.ReadAllText(InputFile!).Replace(',', '-').Split('\n').Select(x => x.Split('-').Select(x => int.Parse(x)).ToList()).Where(x => (Math.Sign(x[0] - x[2]) != Math.Sign(x[1] - x[3]) || (x[0] - x[2] == 0 && x[1] - x[3] == 0))).Count());   
        }

        override public void Part2()
        {
            Console.WriteLine("overlapping: " + File.ReadAllText(InputFile!).Replace(',', '-').Split('\n').Select(x => x.Split('-').Select(x => int.Parse(x)).ToList()).Where(x => (Math.Sign(x[0] - x[2]) != Math.Sign(x[1] - x[3]) || (x[0] == x[2] && x[1] == x[3])) || (!(x[2] > x[1] || x[3] < x[0]))).Count());
        }
    }
}
