using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace ConsoleApp1.Solutions
{
    internal class Day7 : AbstractPuzzle
    {

        //7 ;'s
        public void Both()
        {
            var fsandpath = ( new Dictionary<string, Int64>() { { "", 0 } }, new List<string>() { }, new List<List<(int,int,string)>>() );

            new Action[4] {
                //load and parse into lists
                () => fsandpath.Item3 = File.ReadAllText(InputFile!).Split("\n").Select((s, i) => s.Contains('$') ? (0, i, s.Trim()) : (1, i, s.Trim())).GroupBy(x => x.Item1).Select(x => (x.ToList().Where(x => x.Item1 == 0).ToList(), x.ToList().Where(x => x.Item1 == 1).ToList())).Select(x => x.Item1.Count != 0? x.Item1: x.Item2).ToList(),
                //create directory structure and sum files
                () => fsandpath.Item3 [0].Select(x => (x.Item2, x.Item3.Contains("cd") ? x.Item3.Split(" ")[2] : "ls")).ToList().Select(cmd => cmd.Item2 == "ls"? new Task(() => { int ind = cmd.Item1; fsandpath.Item1.Add(fsandpath.Item2.Aggregate((x, y) => x + (x == "/" ? "" : "/") + y), fsandpath.Item3 [1].SkipWhile(x => x.Item2 < cmd.Item1).TakeWhile(x => x.Item2 == ++ind).ToList().Where(x => x.Item3.Split(" ")[0] != "dir").Select(x => int.Parse(x.Item3.Split(" ")[0])).Sum()); }): cmd.Item2 == ".."? new Task(() => fsandpath.Item2.RemoveAt(fsandpath.Item2.Count - 1)): new Task(() => fsandpath.Item2.Add(cmd.Item2 == "/" ? "/root" : cmd.Item2))).All(x => { x.Start(); x.Wait(); return true; }),
                //total dir sizes
                () => fsandpath.Item1.OrderByDescending(x => x.Key.Where(x => x == '/').Count()).Where(x => x.Key != "").ToList().ForEach(x => fsandpath.Item1[x.Key.Substring(0, x.Key.LastIndexOf('/'))] = fsandpath.Item1[x.Key.Substring(0, x.Key.LastIndexOf('/'))] + fsandpath.Item1[x.Key]),
                //print answers
                () => Console.WriteLine("sum: " + fsandpath.Item1.Where(x => x.Value <= 100000).Sum(x => x.Value) + "  need to free: " + fsandpath.Item1.Where(x => x.Value >= 30000000 - (70000000 - fsandpath.Item1["/root"])).Min(x => x.Value) )
            }.ToList().ForEach(x => x());
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
