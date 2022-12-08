using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace ConsoleApp1.Solutions
{
    internal class Day8 : AbstractPuzzle
    {


        public override void Part1()
        {
            //parse
            var ps = File.ReadAllText(InputFile!).Split("\n").SelectMany((s, y) => s.Trim().ToCharArray().Select((c, i) => (i, y, int.Parse(c.ToString()))));

            //horiz
            int max = 0;
            var h = ps.GroupBy(x => x.i)
                    .Select(x => { max = -1; return x.ToList().Where(x => { var t = x.Item3 > max; if (t) max = x.Item3; return t; }); })
                    .Concat(
                        ps.GroupBy(x => x.i).Select(x => { max = -1; return x.Reverse().ToList().Where(x => { var t = x.Item3 > max; if (t) max = x.Item3; return t; }); })
                    ).SelectMany(x => x).ToList();

            //vert
            var v = ps.GroupBy(x => x.y)
                    .Select(x => { max = -1; return x.ToList().Where(x => { var t = x.Item3 > max; if (t) max = x.Item3; return t; }); })
                    .Concat(
                        ps.GroupBy(x => x.y).Select(x => { max = -1; return x.Reverse().ToList().Where(x => { var t = x.Item3 > max; if (t) max = x.Item3; return t; }); })
                    ).SelectMany(x => x).ToList();

            //answer
            Console.WriteLine("can see: " + (ps.Count() - h.Concat(v).Distinct().Count()));
        }             

        override public void Part2()
        {
            //parse
            var ps = File.ReadAllText(InputFile!).Split("\n").SelectMany((s, y) => s.Trim().ToCharArray().Select((c, i) => (i, y, int.Parse(c.ToString()))));

            //scoring....
            Func<(int, int, int), List<(int, int, int)>, int> CalcTotal = ((int, int, int) cp, List<(int, int, int)> l) =>
                l.Count != 0 && l.Last().Item3 < cp.Item3 
                &&  !(l.Last().Item1 == 0 || l.Last().Item2 == 0 || l.Last().Item1 == ps.Max(x => x.i) || l.Last().Item2 == ps.Max(x => x.y))
                ? l.Count() + 1 : l.Count();

            //get best spot + answer
            Console.WriteLine("(Score, (best point + tree)): " +
                ps.Select(cp =>
                    (CalcTotal(cp, ps.Where(x => x.i == cp.i && x.y < cp.y).OrderByDescending(x => x.y).TakeWhile(x => x.Item3 < cp.Item3).ToList())
                    * CalcTotal(cp, ps.Where(x => x.i == cp.i && x.y > cp.y).OrderBy(x => x.y).TakeWhile(x => x.Item3 < cp.Item3).ToList())
                    * CalcTotal(cp, ps.Where(x => x.y == cp.y && x.i < cp.i).OrderByDescending(x => x.i).TakeWhile(x => x.Item3 < cp.Item3).ToList())
                    * CalcTotal(cp, ps.Where(x => x.y == cp.y && x.i > cp.i).OrderBy(x => x.i).TakeWhile(x => x.Item3 < cp.Item3).ToList())
                    , cp)).OrderByDescending(x => x.Item1).First());
        }
    }
}
