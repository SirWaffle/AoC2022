using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices.Marshalling;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AoC2023.Utilities;

namespace AoC2023.Solutions
{
    internal class Day12 : AbstractPuzzle<Day12>
    {
        public override void Init()
        {
            DoPart1 = false;
            DoPart2 = true;
        }
        class Entry
        {
            public static Int64 nextId = 0;
            public Int64 id;
            public Int64 val;
            public Int64 type; //type: 0 = ., 1 = #, 2 = user #?
            public string asStr = String.Empty;

            public override string ToString()
            {
                return val.ToString() + " id: " + id.ToString();
            }

            public Entry Stringify()
            {
                Debug.Assert(type == 2);

                if (asStr != String.Empty)
                {
                    Debug.Assert(asStr.Length == val);
                    return this;
                }

                for (int i = 0; i < val; ++i)
                    asStr += '#';

                return this;
            }
        }

        public override void Part1Impl()
        {
            checked
            {
                List<(string row, List<Entry> nums)> input = File.ReadAllText(this.InputFilePart1).Split("\r\n", StringSplitOptions.TrimEntries)
                    .Select(x => (x.Split(" ", StringSplitOptions.TrimEntries)[0], x.Split(" ", StringSplitOptions.TrimEntries)[1].Split(",", StringSplitOptions.TrimEntries)
                            .Select(x => new Entry { id = ++Entry.nextId, val = int.Parse(x), type = 2 } ).ToList() ) ).ToList();

                //permutation func
                Func<IEnumerable<Entry>, int, IEnumerable<IEnumerable<Entry>>> PermFunc = null;
                PermFunc = (IEnumerable<Entry> list, int len) => {
                    if (len == 1) 
                        return list.Select(t => new Entry[]{ new Entry{ id = t.id, val = t.val, type = t.type } });

                    return PermFunc(list, len - 1)
                        .SelectMany(t => list.Where(e => !t.Any(ti => ti.id == e.id)),
                            (t1, t2) => t1.Concat(new Entry[] { new Entry { id = t2.id, val = t2.val, type = t2.type } }));
                };

                //perms + uniqueness by + remove dupes based on value
                List<(string row, List<List<Entry>> nums)> perms =
                    input.AsParallel().Select(x =>
                        (x.row,
                        new List<List<Entry>>() { { x.nums.Select(x => x.Stringify() ).ToList() }  }
                        //remove dupe permutations based on val
                        //PermFunc(x.nums, x.nums.Count()).Select(l=>l.Select(ent => ent.Stringify()).ToList()).DistinctBy(x => x.Aggregate((Int64)0, (a, i) => (a * 10) + i.val)).ToList()
                        //allow dupes
                        //PermFunc(x.nums, x.nums.Count()).Select(l => l.Select(ent => ent.Stringify()).ToList()).ToList()
                    )).ToList();


                List<int> corrects = new();
                int lineInd = 0;
                //DFS, no matter, but gotta do a search.
                foreach (var line in perms)
                {
                    corrects.Add(0);

                    foreach (var perm in line.nums)
                    {
                        //lazy

                        HashSet<string> solutions = new();

                        var searchHeads = new List<(string s, int rowInd, int permInd)>();
                        searchHeads.Add((String.Empty, 0, 0));
                        while (searchHeads.Count > 0)
                        {
                            var cur = searchHeads.Last();
                            searchHeads.RemoveAt(searchHeads.Count - 1);

                            if (cur.s.Length > line.row.Length)
                                continue;

                            //Console.WriteLine(line.row + " - " + string.Join("  ", perm.Select(x => x.val.ToString() + ":" + x.asStr)) + "     :" + cur.s);

                            //check string validity, .'s and #'s must be in the right place
                            bool fail = false;
                            for(int si = 0; si < cur.s.Length; si++)
                            {
                                if (line.row[si] == '.' && cur.s[si] != '.')
                                {
                                    fail = true;
                                    break;
                                }
                                else if (line.row[si] == '#' && cur.s[si] != '#')
                                {
                                    fail = true;
                                    break;
                                }
                            }

                            if (fail == true)
                                continue;

                            if (cur.s.Length == line.row.Length)
                            {
                                //if we have used up all the permuted vals, we are solved
                                if (cur.permInd == perm.Count())
                                {
                                    //ensure we have the correct number of and length runs...
                                    var splits = cur.s.Split('.', StringSplitOptions.RemoveEmptyEntries).OrderBy(x => x.Length).Zip(perm.OrderBy(x => x.val)).Where(a => a.First.Length == a.Second.val).ToList();

                                    if (splits.Count == perm.Count())
                                    {
                                        solutions.Add(cur.s);
                                        corrects[corrects.Count - 1]++;
                                    }

                                    continue; //break if we want to count each permutation once
                                }
                            }

                            //append .'s and the next ind 
                            if (cur.permInd < perm.Count() && cur.rowInd + perm[cur.permInd].asStr.Length <= line.row.Length)
                            {
                                Debug.Assert(perm[cur.permInd].asStr.Length == perm[cur.permInd].val);
                                searchHeads.Add((cur.s + perm[cur.permInd].asStr, cur.rowInd + perm[cur.permInd].asStr.Length, cur.permInd + 1));
                            }

                            searchHeads.Add((cur.s + '.', cur.rowInd + 1, cur.permInd));
                        }

                        
                        foreach(var sol in solutions)
                        {
                            Console.WriteLine(lineInd + "  " + line.row + " - " + string.Join("  ", perm.Select(x => x.val.ToString())) + "     :" + sol);
                        } 
                        
                    }
                    Console.WriteLine("corrects: " + corrects.Last());
                    lineInd++;
                    //if (lineInd == 32)
                    //    return;
                }


                var sum = corrects.Sum();
                Console.WriteLine("Answer p1: " + sum);
                //7191
            }
        }

        private static IEnumerable<bool> IterateUntilFalse(Func<bool> condition)
        {
            while (condition()) yield return true;
        }


        class SearchItem
        {
            public StringBuilder s = new(512, 2048);
            public int len;
            public int permInd;
            public bool justPlaced;

            public SearchItem(int _len, int _permInd, bool _justPlaced)
            {
                len = _len;
                permInd = _permInd;
                justPlaced = _justPlaced;
            }

            public SearchItem(StringBuilder _s, char c, int _len, int _permInd, bool _justPlaced)
            {
                s = s.Append(_s);
                s.Append(c);
                len = _len;
                permInd = _permInd;
                justPlaced = _justPlaced;
             }

            public SearchItem(StringBuilder _s, ref string c, int _len, int _permInd, bool _justPlaced)
            {
                s = s.Append(_s);
                s.Append(c);
                len = _len;
                permInd = _permInd;
                justPlaced = _justPlaced;
            }
        }

        override public void Part2Impl()
        {
            //checked
            {
                //string inf = @"../../../InputFiles/Day12_1_sample.txt";

                List<(string row, List<Entry> nums)> input = File.ReadAllText(this.InputFilePart1).Split("\r\n", StringSplitOptions.TrimEntries)
                    .Select(x => (x.Split(" ", StringSplitOptions.TrimEntries)[0], x.Split(" ", StringSplitOptions.TrimEntries)[1].Split(",", StringSplitOptions.TrimEntries)
                            .Select(x => new Entry { id = ++Entry.nextId, val = int.Parse(x), type = 2 }).ToList())).ToList();

                //perms + uniqueness by + remove dupes based on value
                //5x it
                //p2
                
                List<(string row, List<List<Entry>> nums)> perms =
                    input.AsParallel().Select(x =>
                        (String.Join("?", Enumerable.Repeat(x.row, 5).ToList()).Replace("..",".").Replace("..",".").Replace("..", ".").Replace("..", ".").Replace("..", ".").Replace("..", ".").Replace("..", ".").Replace("..", "."),
                        new List<List<Entry>>() { { Enumerable.Repeat(x.nums.Select(x => x.Stringify()).ToList(), 5).SelectMany(l => l).ToList() } }
                    )).ToList();
                
                
                //p1
                /*
                List<(string row, List<List<Entry>> nums)> perms =
                input.AsParallel().Select(x =>
                    (x.row.Replace("..", ".").Replace("..", ".").Replace("..", ".").Replace("..", ".").Replace("..", ".").Replace("..", ".").Replace("..", ".").Replace("..", "."),
                    new List<List<Entry>>() { { x.nums.Select(x => x.Stringify()).ToList() } }
                )).ToList();

                List<(UInt64 ind, UInt64 correct)> contvals = new();
                */

                List<UInt64> corrects = Enumerable.Range(0, perms.Count).Select(x => (UInt64)0).ToList();
                int totalInds = 0;

                //continue...
                
                string cont = @"../../../InputFiles/Day12_2_wip.txt";

                List<(UInt64 ind, UInt64 correct)> contvals = File.ReadAllText(cont).Split("\r\n", StringSplitOptions.TrimEntries)
                    .Select(x => (UInt64.Parse(x.Split("completed:")[1].Trim().Split(" ")[0]),
                    UInt64.Parse(x.Split("corrects:")[1].Trim())
                    )
                    ).ToList();


                contvals.ForEach(x => corrects[(int)x.ind] = x.correct);
                

                    //DFS, no matter, but gotta do a search.
                Parallel.For(0, perms.Count, (int ind) =>
                {
                    var line = perms[ind];
                    {
                        int correctsIndex = ind;
                        if (contvals.Where(x=> x.ind == (UInt64)correctsIndex).Any())
                        {
                            int ci = Interlocked.Add(ref totalInds, 1);
                            Console.WriteLine("Processed: " + ci + "  Already completed: " + ind + " corrects: " + corrects[ind]);
                            return;
                        }
                        
                        foreach (var perm in line.nums)
                        {
                            //lazy
                            string fullDots = String.Join("", Enumerable.Repeat(".", line.row.Length));

                            //iterate over each 'digit'

                            var digits = new List<string> { line.row }; // line.row.Split(".", StringSplitOptions.RemoveEmptyEntries);
                            List<UInt64> digitsCorrect = new();

                            int digitStartingLen = 0;
                            int digitNextLen = 0;

                            for (int digInd = 0; digInd < digits.Count; ++digInd)
                            {                                
                                String dig = digits[digInd];
                                digitsCorrect.Add(0);
                                ReadOnlySpan<char> rowstr = dig.AsSpan();


                                //cut digit perm down to only what might fit inside this window for the digit...
                                List<Entry> digitPerm = perm;
                                digitStartingLen = digitNextLen;
                                digitNextLen += dig.Length;
                                int digitRemaining = line.row.Length - digitNextLen;

                                //calc our window




                                //lets make some early outs on remaining length
                                //ind 0 = all,. 1 = all - the first, etc.
                                Int64[] remLen = digitPerm.Select(x => (Int64)0).Append(0).ToArray();
                                for (int i = 0; i < digitPerm.Count - 1; ++i)
                                {
                                    Int64 total = 0;
                                    for (int j = i; j < digitPerm.Count; ++j)
                                    {
                                        total += digitPerm[j].val;
                                    }
                                    total += digitPerm.Count - (i + 1);
                                    remLen[i] = total;
                                }

                                var searchHeads = new Stack<SearchItem>();
                                searchHeads.Push(new SearchItem(0, 0, false));

                                SearchItem? cur = null;
                                while (searchHeads.TryPop(out cur))
                                {
                                    if (cur.len > rowstr.Length)
                                        continue;

                                    //check string validity, .'s and #'s must be in the right place
                                    bool fail = false;
                                    for (int si = 0; si < cur.len; si++)
                                    {
                                        if (rowstr[si] == '?')
                                            continue;

                                        if (rowstr[si] == '#' && cur.s[si] != '#')
                                        {
                                            fail = true;
                                            break;
                                        }
                                        else if (rowstr[si] == '.' && cur.s[si] != '.')
                                        {
                                            fail = true;
                                            break;
                                        }
                                    }

                                    if (fail == true)
                                        continue;

                                    if (cur.len == rowstr.Length)
                                    {
                                        //if we have used up all the permuted vals, we are solved
                                        if (cur.permInd == perm.Count)
                                        {
                                            digitsCorrect[digitsCorrect.Count - 1]++;                                            
                                        }

                                        continue; //break if we want to count each permutation once
                                    }

                                    if (cur.justPlaced) //required to have a . here
                                    {
                                        if (rowstr[cur.len] != '#')
                                            searchHeads.Push(new SearchItem(cur.s, '.', cur.len + 1, cur.permInd, false));
                                    }
                                    else if (cur.permInd < perm.Count() && cur.len + perm[cur.permInd].val <= rowstr.Length && remLen[cur.permInd] <= rowstr.Length - cur.len)
                                    {
                                        if (rowstr[cur.len] != '.')
                                            searchHeads.Push(new SearchItem(cur.s, ref perm[cur.permInd].asStr, cur.len + (int)perm[cur.permInd].val, cur.permInd + 1, true));

                                        if (rowstr[cur.len] != '#')
                                            searchHeads.Push(new SearchItem(cur.s, '.', cur.len + 1, cur.permInd, false));
                                    }
                                    else
                                    {
                                        searchHeads.Push(new SearchItem(cur.s, ref fullDots, cur.len + (rowstr.Length - cur.len), cur.permInd, false));
                                    }

                                }


                            }

                            corrects[correctsIndex] = digitsCorrect.Aggregate((UInt64)0, (a, i) => a + i);
                            //corrects[correctsIndex] = digitsCorrect.Aggregate((UInt64)1, (a, i) => a * i);

                        }

                        Interlocked.Add(ref totalInds, 1);
                        Console.WriteLine( "Processed: " + totalInds + "  Just completed: " + ind + " corrects: " + corrects[ind]);
                    }
                });


                var sum = corrects.Aggregate((UInt64)0, (a, i) => a + i);
                Console.WriteLine("Answer p1: " + sum);
                //7191
            }
        }



    }
}
