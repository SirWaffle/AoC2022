using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace ConsoleApp1.Solutions
{
    internal class Day9 : AbstractPuzzle
    {
        public override void Part1()
        {
            Both(2);
        }             

        override public void Part2()
        {
            Both(10);
        }

        public void Both(int knots)
        {
            var rope = new List<(int x, int y)>(new (int, int)[knots]);

            Func<(int x, int y), (int x, int y)> calc = ((int x, int y) headMove) => {
                rope[0] = (rope[0].x + headMove.x, rope[0].y + headMove.y);

                for (int cur = 0; cur < rope.Count - 1; cur++)
                {
                    var diff = (x: rope[cur].x - rope[cur + 1].x,y: rope[cur].y - rope[cur + 1].y);
                    if (Math.Max(Math.Abs(diff.x), Math.Abs(diff.y)) >= 2)
                    {
                        rope[cur + 1] = (rope[cur + 1].x + Math.Sign(diff.x), rope[cur + 1].y + Math.Sign(diff.y));
                    }
                }
                return rope.Last();
            };

            //parse, calc, answer
            Console.WriteLine("num spaces moved: " + File.ReadAllText(InputFile!).Split("\n").Select(x => x.Replace("R", "1 0").Replace("L", "-1 0").Replace("U", "0 1").Replace("D", "0 -1").Trim().Split(" ")).Select(x => new List<(int, int)>(Enumerable.Repeat((int.Parse(x[0]), int.Parse(x[1])), int.Parse(x[2])))).SelectMany(x => x.Select(x => calc(x))).ToList().Distinct().Count());
        }
    }
}
