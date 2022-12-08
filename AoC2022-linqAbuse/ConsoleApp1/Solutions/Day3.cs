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
    internal class Day3 : AbstractPuzzle
    {
        //1 ;
        override public void Part1()
        {
            Console.WriteLine("total priority val: " + File.ReadAllText(InputFile!).Split('\n').Select(x => x.Substring(0, x.Length / 2).ToCharArray().ToList().Intersect(x.Substring(x.Length / 2, x.Length / 2 ).ToCharArray().ToList()).Select(x => ((int)x) - (Char.IsUpper(x)?(((int)'A') - 27): (((int)'a') - 1))).Sum()).Sum());
        }

        //1 ;
        override public void Part2()
        {
            int count = 0;
            Console.WriteLine("total priority val: " + File.ReadAllText(InputFile!).Split('\n').GroupBy(x => count++ / 3 ).Select(x => x.ToList()[0].Trim().ToCharArray().ToList().Intersect(x.ToList()[1].Trim().ToCharArray().ToList().Intersect(x.ToList()[2].Trim().ToCharArray().ToList())).Select(x => ((int)x) - (Char.IsUpper(x) ? (((int)'A') - 27) : (((int)'a') - 1))).Sum()).Sum());
        }
    }
}
