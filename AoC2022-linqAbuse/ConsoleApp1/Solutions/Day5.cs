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
    internal class Day5 : AbstractPuzzle
    {
        override public void Part1()
        {
            //deal with loading of init state later
            var initState = File.ReadAllText(InputFile!).Split("\r\n").Where(x => x.Contains("[")).ToList();

            List<string>[] stacks = new List<string>[20];
            for(int i =0; i < initState.Count(); ++i)
            {
                for (int stack = 1, sn = 0; stack < initState[i].Length; stack += 4, sn+=1)
                {
                    if (stacks[sn] == null)
                    {
                        stacks[sn] = new List<string>();
                    }
                    stacks[sn].Insert(0, initState[i][stack].ToString());

                }

            }
            var lol = stacks.Where(x => x != null).Select(x => x.Where(x => x.Trim().Length > 0).ToList()).ToList();


            //process moves
            var moves = File.ReadAllText(InputFile!).Split("\r\n").Where(x => x.Contains("move")).ToList().Select(x => x.Replace("move", "").Replace("from", "").Replace("to", "").Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList());
            foreach(var m in moves)
            {
                for (int j = 0; j < m[0]; ++j)//0 to M for single, m[0] to 0 for multiple
                {
                    if (lol[m[1] - 1].Count == 0)
                        break;

                    //one at a time
                    lol[m[2] - 1].Add(lol[m[1] - 1].Last());
                    lol[m[1] - 1].RemoveAt(lol[m[1] - 1].Count - 1);
                }
            }


            //output
            foreach(List<string> l in lol)
            {
                Console.WriteLine("stack top: " + (l.Count > 0 ? l[l.Count-1].Last(): "empty"));
            }
       }

        override public void Part2()
        {
            List<List<String>> stacks = File.ReadAllText(InputFile!).Split("\r\n").Where(x => x.Contains("[")).ToList().Select(x => x.ToCharArray().ToList().Select((c, i) => (i, c)).Where(x => Char.IsAsciiLetter(x.c)).Select(x => ( (x.i + 3 / 4), x.c ) ).GroupBy(x => x.Item1)).SelectMany(x => x.SelectMany(x => x.ToList())).GroupBy(x => x.Item1).ToList().Select(x => x.ToList().Select(x => (x.Item1, x.c.ToString())).Reverse().ToList()).ToList().OrderBy(x => x[0].Item1).Select(x => x.Select(x => x.Item2.ToString()).ToList()).ToList();

            Console.WriteLine("stack tops: " + File.ReadAllText(InputFile!).Split("\r\n").Where(x => x.Contains("move")).ToList().Select(x => x.Replace("move", "").Replace("from", "").Replace("to", "").Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList()).Select(x => (1, x)).GroupBy(x => x.Item1).Where(x => x.ToList().All(x => { x.x[0] = Math.Min(x.x[0], stacks[x.x[1] - 1].Count); stacks[x.x[2] - 1].AddRange(stacks[x.x[1] - 1].GetRange(stacks[x.x[1] - 1].Count - x.x[0], x.x[0])); stacks[x.x[1] - 1].RemoveRange(stacks[x.x[1] - 1].Count - x.x[0], x.x[0]); return true; })).SelectMany((i, x) => i.ToList()).Select((x, i) => i < stacks.Count ? stacks[i].Last() : " ").Reverse().Aggregate((a, b) => b + " " + a));

        }
    }
}
