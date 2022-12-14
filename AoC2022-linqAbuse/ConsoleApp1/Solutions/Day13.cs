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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConsoleApp1.Solutions
{
    internal class Day13 : AbstractPuzzle
    {
        public override void Part1()
        {
            Both();
        }             

        override public void Part2()
        {
            Both();
        }

 
        public void Both()
        {
            var packets = File.ReadAllText(InputFile!).Split("\r\n\r\n").Select(x => x.Trim()).Select(x => x.Split("\r\n").Select(x => x.Replace("[", "[,0,").Replace("]", ",],").Replace(",,",",").Split(',').Select(x => x.Length > 0 ? x: "-")).ToList()).ToList();
            List<bool> inOrder = new List<bool>(new bool[packets.Count]);
            int pair = -1;
            foreach (var p in packets)
            {
                ++pair;

                var left = p[0].ToList();
                var right = p[1].ToList();

                int li = 0;
                int ri = 0;
                for (li = 0, ri = 0; li < left.Count && ri < right.Count;)
                {
                    if (Math.Abs(ri - li) > 2)
                    {
                        //like problem;
                        Console.WriteLine("par: " + (pair + 1) + " offsets too far apart");
                    }
                    //compare...
                    if (left[li] == "[" && right[ri] == "[")
                    {
                        li++;
                        ri++;
                        continue;
                    }
                    else if (left[li] == "]" && right[ri] == "]")
                    {
                        li++;
                        ri++;
                        continue;
                    }
                    else if (left[li] == "[" && int.TryParse(right[ri],out _))
                    {
                        //lets inject some fake lists
                        right.Insert(ri, "[");
                        right.Insert(ri + 1, "0");
                        right.Insert(ri + 3, "]");
                        li++;
                        ri++;
                        continue;
                    }
                    else if (int.TryParse(left[li], out _) && right[ri] == "[")
                    {
                        left.Insert(li, "[");
                        left.Insert(ri + 1, "0");
                        left.Insert(li + 3, "]");
                        ri++;
                        li++;
                        continue;
                    }

                    if (left[li] == "[" && right[ri] == "]")
                    {
                        inOrder[pair] = false;
                        break;
                    }
                    else if (left[li] == "]" && right[ri] == "[")
                    {
                        //unsure about this
                        inOrder[pair] = true;
                        break;
                    }

                    if (left[li] == "]" && int.TryParse(right[ri], out _))
                    {
                        inOrder[pair] = true;
                        break;
                    }
                    else if (int.TryParse(left[li], out _) && right[ri] == "]")
                    {
                        inOrder[pair] = false;
                        break;
                    }

                    if (int.Parse(left[li].ToString()) < int.Parse(right[ri].ToString()))
                    {
                        inOrder[pair] = true;
                        break;
                    }
                    else if (int.Parse(left[li].ToString()) > int.Parse(right[ri].ToString()))
                    {
                        inOrder[pair] = false;
                        break;
                    }
                    else
                    {
                        ri++;
                        li++;
                        if (ri >= right.Count)
                        {
                            inOrder[pair] = true;
                            break;
                        }
                        else if(li >= left.Count)
                        {
                            inOrder[pair] = false;
                            break;
                        }
                        continue;
                    }
                }

                var ls = left.TakeWhile((x, i)=> i <= li).ToArray();
                var rs = right.TakeWhile((x, i) => i <= ri).ToArray();
                Console.WriteLine("Pair / order: " + (pair + 1) + " " + inOrder[pair]);
                Console.WriteLine("LEFT: " + ls.Aggregate((x,y) => x + "," + y));
                Console.WriteLine("RGHT: " + rs.Aggregate((x, y) => x + "," + y));
                Console.Write("--------------");
            }

            int sum = 0;
            for(int i = 0; i< inOrder.Count; i++)
            {
                //output
                Console.WriteLine("Pair / order: " + (i + 1) + " " + inOrder[i]);
                if (inOrder[i] == true)
                    sum += (i + 1);
            }
            Console.WriteLine(("sum: " + sum)); //5385 too high, 4614 too low

        }
    }
}
