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
    internal class Day6 : AbstractPuzzle
    {
        //1 ;
        override public void Part1()
        {
            Console.WriteLine("First packet: " + (File.ReadAllText(InputFile!)
                .Split("\n\r").Select(s => s
                    .TakeWhile((c, i) => s.Substring(i, 4).ToCharArray().Distinct().Count() != 4)
                 ).ToList()[0].Count() + 4));
       }

        //1 ;
        override public void Part2()
        {
            Console.WriteLine("First message: " + (File.ReadAllText(InputFile!)
                .Split("\n\r").Select(s => s
                    .TakeWhile((c, i) => s.Substring(i, 14).ToCharArray().Distinct().Count() != 14)
                ).ToList()[0].Count() + 14));
        }
    }
}
