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
            Dictionary<string, (Int64, List<(Int64, string)>)> fs = new Dictionary<string, (Int64, List<(Int64, string)>)>() { { "", (0, new()) } };
            List<string> curPath = new List<string>() { };

            //split into lists
            var splitLists = File.ReadAllText(InputFile!)
                .Split("\n")
                .Select((s, i) => s.Contains('$') ? (0, i, s.Trim()) : (1, i, s.Trim()))
                .GroupBy(x => x.Item1)
                .Select(x => (x.ToList()
                                .Where(x => x.Item1 == 0).ToList(),
                              x.ToList()
                                .Where(x => x.Item1 == 1).ToList()))
                .Select(x => x.Item1.Count != 0? x.Item1: x.Item2)
                .ToList();

            //create dir structure
            foreach (var cmd in splitLists[0])
            {
                if(cmd.Item3.Contains("cd"))
                {
                    if (cmd.Item3.Split(" ")[2] == "..")
                        curPath.RemoveAt(curPath.Count - 1);
                    else
                        curPath.Add(cmd.Item3.Split(" ")[2] == "/"? "/root": cmd.Item3.Split(" ")[2]); 
                }

                string filepath = curPath.Aggregate((x, y) => x + (x == "/" ? "" : "/") + y);
                (Int64, List<(Int64, string)>) entry;
                if (!fs.TryGetValue(filepath, out entry))
                    fs.Add(filepath, new(0, new()));

                if (cmd.Item3.Contains("ls"))
                {
                    int ind = cmd.i;
                    var filesInDir = splitLists[1].SkipWhile(x => x.i < cmd.i).TakeWhile(x => x.i == ++ind ).ToList();
                    foreach (var file in filesInDir)
                    {
                        if (file.Item3.Split(" ")[0] != "dir")
                        {
                            entry.Item1 += int.Parse(file.Item3.Split(" ")[0]);
                            entry.Item2.Add((int.Parse(file.Item3.Split(" ")[0]), file.Item3.Split(" ")[1]));
                            fs[filepath] = entry;
                        }
                    }
                }
            }

            //calculate directory sizes
            fs.OrderByDescending(x => x.Key.Where(x => x == '/').Count())
              .Where(x => x.Key != "").ToList()
              .ForEach(x => fs[x.Key.Substring(0, x.Key.LastIndexOf('/'))] = new(fs[x.Key.Substring(0, x.Key.LastIndexOf('/'))].Item1 + fs[x.Key].Item1,fs[x.Key.Substring(0, x.Key.LastIndexOf('/'))].Item2));

            Console.WriteLine("sum: " + fs
                .Where(x => x.Value.Item1 <= 100000)
                .Sum(x => x.Value.Item1)
            );

            Console.WriteLine( "To Free: " + fs
                .Where(x => x.Value.Item1 >= 30000000 - (70000000 - fs["/root"].Item1))
                .Min(x => x.Value.Item1)
            );
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
