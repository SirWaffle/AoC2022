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
            var rope = new List<(int, int)>(new (int, int)[knots]);

            Func<(int, int), (int, int)> calc = ((int, int) headMove) => {
                rope[0] = (rope[0].Item1 + headMove.Item1, rope[0].Item2 + headMove.Item2);

                for (int hi = 0; hi < rope.Count - 1; hi++)
                {
                    var knot = rope[hi];
                    var nextKnot = rope[hi + 1];

                    var diff = (knot.Item1 - nextKnot.Item1, knot.Item2 - nextKnot.Item2);
                    if (Math.Max(Math.Abs(diff.Item1), Math.Abs(diff.Item2)) >= 2)
                    {
                        rope[hi + 1] = (nextKnot.Item1 + Math.Sign(diff.Item1), nextKnot.Item2 + Math.Sign(diff.Item2));
                    }
                }
                return rope.Last();
            };

            //parse, calc, answer
            Console.WriteLine("num spaces moved: " + File.ReadAllText(InputFile!).Split("\n").Select(x => x.Replace("R", "1 0").Replace("L", "-1 0").Replace("U", "0 1").Replace("D", "0 -1").Trim().Split(" ")).Select(x => new List<(int, int)>(Enumerable.Repeat((int.Parse(x[0]), int.Parse(x[1])), int.Parse(x[2])))).SelectMany(x => x.Select(x => calc(x))).ToList().Distinct().Count());
        }
    }
}
