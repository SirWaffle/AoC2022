using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace ConsoleApp1.Solutions
{
    internal class Day10 : AbstractPuzzle
    {
        public override void Part1()
        {
            Both();
        }             

        override public void Part2()
        {
            Both();
        }

        //8 ;'s
        public void Both()
        {
            Func<int,int,string> crtView = (int reg, int cyc) =>
                cyc % 40 >= reg - 1 && cyc % 40 <= reg + 1 ? "#" : ".";

            int sum = File.ReadAllText(InputFile!).Split("\n").Select(x => x.Trim())
                .Aggregate<string, List<(int reg, int pend, string crt)>>(
                    new List<(int, int, string)>() { (1, 0, " ") }, //initial state of machine,
                    (exeCyc, cmd) => {
                        int reg = exeCyc.Last().reg + exeCyc.Last().pend;

                        //noop and addx both get this one added
                        exeCyc.Add((reg, 0, crtView(reg, exeCyc.Count - 1)));

                        if(cmd != "noop") //inject second cycle for addx
                            exeCyc.Add((reg, int.Parse(cmd.Split(" ")[1]), crtView(reg, exeCyc.Count - 1)));

                        return exeCyc;
                    })
                .Select( (v,i) => {
                    Console.Write(v.crt + (i % 40 == 0 ? Environment.NewLine : ""));
                    return ((i - 20) % 40 == 0 && i >= 20 && i <= 220)? v.reg * i : 0;
                }).Sum();

            Console.WriteLine("str of reg: " + sum); 
        }
    }
}
