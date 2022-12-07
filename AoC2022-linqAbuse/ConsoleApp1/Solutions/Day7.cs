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
        override public void Part1()
        {
            var split = File.ReadAllText(InputFile!)
                .Split("\n")
                .Select((s, i) => s.Contains('$')? (0, i, s.Trim()): (1, i, s.Trim()))
                .GroupBy(x => x.Item1);

            Dictionary<string, (Int64, List<(Int64, string)>)> fs = new Dictionary<string, (Int64, List<(Int64, string)>)>() { { "", (0, new()) } };
            List<string> curPath = new List<string>() { };

            var cmds = split.ToList().SelectMany(g => g.ToList().Where(x => x.Item1 == 0));

            var files = split.ToList().SelectMany(g => g.ToList().Where(x => x.Item1 == 1));

            foreach (var cmd in cmds)
            {
                if(cmd.Item3.Contains("cd"))
                {
                    var cmdSplit = cmd.Item3.Split(" ");
                    if (cmdSplit[2] == "..")
                    {
                        curPath.RemoveAt(curPath.Count - 1);
                    }
                    else
                    {
                        curPath.Add(cmdSplit[2] == "/"? "/root": cmdSplit[2]);
                    }

                    string filepath = curPath.Aggregate((x, y) => x + (x == "/" ? "" : "/") + y);
                    (Int64, List<(Int64, string)>) entry;
                    if (!fs.TryGetValue(filepath, out entry))
                    {
                        entry = new(0, new());
                        fs.Add(filepath, entry);
                    }
                }

                if(cmd.Item3.Contains("ls"))
                {
                    int ind = cmd.i;
                    var filesInDir = files.SkipWhile(x => x.i < cmd.i).TakeWhile(x => x.i == ++ind ).ToList();
                    string filepath = curPath.Aggregate( (x, y) => x + (x == "/"? "" : "/") + y);
                    foreach(var file in filesInDir)
                    {
                        var fSplit = file.Item3.Split(" ");
                        if (fSplit[0] == "dir")
                            continue;

                        (Int64, List<(Int64, string)>) entry = new(fs[filepath].Item1, fs[filepath].Item2);

                        int size = int.Parse(fSplit[0]);

                        entry.Item1 += size;
                        entry.Item2.Add( (int.Parse(fSplit[0]), fSplit[1]) );

                        fs[filepath] = entry;
                    }
                }
            }

            //somehow calculate values for all dirs without recursion and *hopefully* without loops
            fs.OrderByDescending(x => x.Key.Where(x => x == '/').Count())
              .Where(x => x.Key != "").ToList()
              .ForEach(x => fs[x.Key.Substring(0, x.Key.LastIndexOf('/'))] = new(fs[x.Key.Substring(0, x.Key.LastIndexOf('/'))].Item1 + fs[x.Key].Item1,fs[x.Key.Substring(0, x.Key.LastIndexOf('/'))].Item2));

            var sum = fs
                .Where(x => x.Value.Item1 <= 100000)
                .Sum(x => x.Value.Item1);

            //2104783 is proper
            Console.WriteLine("sum: " + sum.ToString());
        }

        override public void Part2()
        {
            Console.WriteLine("First message: " + (File.ReadAllText(InputFile!)
                .Split("\n\r").Select(s => s
                    .TakeWhile((c, i) => s.Substring(i, 14).ToCharArray().Distinct().Count() != 14)
                ).ToList()[0].Count() + 14));
        }
    }
}
