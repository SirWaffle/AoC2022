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
using static ConsoleApp1.Solutions.Day14;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConsoleApp1.Solutions
{
    internal class Day12 : AbstractPuzzle
    {
        public struct Point
        {
            public int x = 0;
            public int y = 0;

            public Point(int _x, int _y) 
            {
                x = _x;
                y = _y;
            }

            public static Point operator +(Point x, Point y)
            {
                return new Point(x.x + y.x,  x.y + y.y);
            }

            public static Point operator -(Point x, Point y)
            {
                return new Point(x.x - y.x, x.y - y.y);
            }

            public static bool operator ==(Point x, Point y)
            {
                return x.x == y.x && x.y == y.y;
            }

            public static bool operator !=(Point x, Point y)
            {
                return !(x == y);
            }

            public override string ToString()
            {
                return "(" + x + "," + y + ")";
            }
        }


        override public void Part1()
        {
            Both(false);
        }


        override public void Part2()
        {
            Both(true);
        }

        public void Both(bool part2)
        {
            //lets be lazy...
            Dictionary<Point, int> tiles = new();
            Point start= new Point();
            Point end = new Point();

            //parse and make tiles in dict...
            var lines = File.ReadAllText(InputFile!).Split("\r\n").Select(x => x.Trim());
            Point p = new Point();
            foreach(var line in lines)
            {                
                for(int i = 0; i < line.Length; ++i)
                {
                    p.x += 1;

                    char c = line[i];
                    if(c == 'E')
                    {
                        end = p;
                        c = 'z';
                    }
                    else if(c == 'S')
                    {
                        start = p;
                        c = 'a';
                    }

                    tiles.Add(p, (int)c - (int)'a');
                }

                p.y += 1;
                p.x = 0;
            }

            //for fun, lets exhaustive search until everything is at optimal values
            Point[] dirs = new Point[4] { new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0) };
            Dictionary<Point, int> explored = new();
            List<Point> search = new List<Point>();

            if (part2 == false)
            {
                explored.Add(start, 0);
                search.Add(start);
            }
            else
            {
                explored.Add(end, 0);
                search.Add(end);
            }


            int scannedSteps = 0;

            while(search.Count > 0)
            {
                ++scannedSteps;

                Point cur = search[0];
                search.RemoveAt(0);

                int score = explored[cur];
                int curHeight = tiles[cur];

                //add available paths in valid range...
                for (int i = 0; i < dirs.Length; ++i)
                {
                    Point newp = cur + dirs[i];

                    if (tiles.TryGetValue(newp, out int value))
                    {
                        if (part2 == false)
                        {
                            //rules, can only move one level up, as many down as we want
                            if (tiles[newp] > curHeight + 1)
                                continue;
                        }
                        else
                        {
                            //inverted rules to go backwards
                            if (tiles[newp] < curHeight - 1)
                                continue;
                        }

                        if (explored.TryGetValue(newp, out int existingScore))
                        {
                            //we may have already explored this spot! if we did , check our value against it, if we are lower, keep me, remove them
                            //else dont add to list
                            if(existingScore > score + 1)
                            {
                                //take it over...
                                if (!search.Contains(newp))
                                {
                                    search.Add(newp);
                                }
                                explored[newp] = score + 1;
                            }
                        }
                        else
                        {
                            search.Add(newp);
                            explored.Add(newp, score + 1);
                        }                        
                    }
                }
            }

            Console.WriteLine("Final Step #" + scannedSteps);

            if (part2 == false)
            {
                Console.WriteLine("Path length at end point: " + explored[end]);
            }
            else
            {
                //we want the lowest value spot that is 'a' ( height 0 )
                var lowSpots = tiles.Where(x => x.Value == 0).Select(x => x.Key).ToList();
                int lowScore = 9999;
                foreach(Point lowp in lowSpots)
                {
                    if (!explored.ContainsKey(lowp))
                        continue;

                    int score = explored[lowp];
                    if(score < lowScore)
                    {
                        lowScore = score;
                    }
                }
                Console.WriteLine("Path to nearest a: " + lowScore);
            }
        }             


 

    }
}
