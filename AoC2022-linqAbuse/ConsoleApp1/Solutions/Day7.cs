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
    internal class Day7 : AbstractPuzzle
    {
        public void Both()
        {
            Dictionary<string, Int64> fs = new Dictionary<string, Int64>() { { "", 0 } };
            List<string> curPath = new List<string>() { };

            //split into lists
            var splitLists = File.ReadAllText(InputFile!).Split("\n").Select((s, i) => s.Contains('$') ? (0, i, s.Trim()) : (1, i, s.Trim())).GroupBy(x => x.Item1).Select(x => (x.ToList().Where(x => x.Item1 == 0).ToList(), x.ToList().Where(x => x.Item1 == 1).ToList())).Select(x => x.Item1.Count != 0? x.Item1: x.Item2).ToList();

            //sum each dir
            foreach(var cmd in splitLists[0].Select(x => (x.Item2, x.Item3.Contains("cd")? x.Item3.Split(" ")[2]: "ls")).ToList())
            {
                int ind = cmd.Item1;

                if (cmd.Item2 == "ls")
                    fs.Add(curPath.Aggregate((x, y) => x + (x == "/" ? "" : "/") + y), splitLists[1].SkipWhile(x => x.i < cmd.Item1).TakeWhile(x => x.i == ++ind).ToList().Where(x => x.Item3.Split(" ")[0] != "dir").Select(x => int.Parse(x.Item3.Split(" ")[0])).Sum());

                else if (cmd.Item2 == "..")
                    curPath.RemoveAt(curPath.Count - 1);
                else
                    curPath.Add(cmd.Item2 == "/" ? "/root" : cmd.Item2);                
            }

            //sum directory sizes
            fs.OrderByDescending(x => x.Key.Where(x => x == '/').Count()).Where(x => x.Key != "").ToList().ForEach(x => fs[x.Key.Substring(0, x.Key.LastIndexOf('/'))] = fs[x.Key.Substring(0, x.Key.LastIndexOf('/'))] + fs[x.Key]);

            //answers
            Console.WriteLine("sum: " + fs.Where(x => x.Value <= 100000).Sum(x => x.Value) + "  need to free: " + fs.Where(x => x.Value >= 30000000 - (70000000 - fs["/root"])).Min(x => x.Value) );
        }             
        //2104783 is correct sum

        override public void Part1()
        {
            Both();
        }

        override public void Part2()
        {
            Both();
        }
    }
}
