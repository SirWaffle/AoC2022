using System;
using System.Collections.Generic;
using System.Drawing;
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
    internal class Day14 : AbstractPuzzle
    {
        public enum State
        {
            Empty,
            Solid,
            Sand
        }

        public struct Point
        {
            public int x;
            public int y;
        }

        public override void Part1()
        {
            Both(false);
        }

        override public void Part2()
        {
            Both(true);
        }

        //27902 too high
        public void Both(bool part2)
        { 
            //lets be lazy...
            Dictionary<Point, State> tiles = new();

            //parse...
            var lines = File.ReadAllText(InputFile!).Split("\r\n").Select(x => x.Trim());

            foreach(var line in lines)
            {
                var points = line.Split("->").Select(x => x.Trim().Split(',').Select(x=> x.Trim()).ToList()).Select(x=> new Point() { x = int.Parse(x[0]), y = int.Parse(x[1]) }).ToList();

                for (int i =0; i < points.Count - 1; i++)
                {
                    Point start = points[i];
                    Point end = points[i + 1];

                    for(;;)
                    {
                        if (!tiles.ContainsKey(start))
                            tiles.Add(start, State.Solid);

                        if (start.x == end.x && start.y == end.y)
                            break;

                        if (start.x != end.x)
                            start.x += Math.Sign(end.x - start.x);
                        if(start.y != end.y)
                            start.y += Math.Sign(end.y - start.y);
                    }
                }
            }

            //things to track...
            Point sandSpawn = new Point() { x = 500, y = 0 };
            Point minBound = new Point() { x = 99999, y = 0 };
            Point maxBound = new Point() { x = -9999, y = -9999 };

            minBound.x = tiles.Min(x => x.Key.x);
            minBound.y = tiles.Min(x => x.Key.y);
            maxBound.x = tiles.Max(x => x.Key.x);
            maxBound.y = tiles.Max(x => x.Key.y);

            //simulate
            Point activeSand = sandSpawn;
            bool sandActive = false;

            int sandAtRest = 0;

            //do one step at a time for animation, weee
            for(;;)
            {
                //spawn sand if we need it
                if(sandActive == false)
                {
                    activeSand = sandSpawn;
                    sandActive = true;
                }

                //move sand
                if (part2 && sandActive == true)
                {

                    if (activeSand.y == maxBound.y + 1)
                    {
                        tiles.Add(activeSand, State.Sand);
                        sandActive = false;
                        ++sandAtRest;
                    }
                }

                if(sandActive == true)
                { 
                    if(tiles.TryGetValue(new Point() { x = activeSand.x, y = activeSand.y + 1}, out State state) && state != State.Empty)
                    {
                        if (tiles.TryGetValue(new Point() { x = activeSand.x - 1, y = activeSand.y + 1 }, out state) && state != State.Empty)
                        {
                            if (tiles.TryGetValue(new Point() { x = activeSand.x + 1, y = activeSand.y + 1 }, out state) && state != State.Empty)
                            {
                                if(part2)
                                {
                                    if(activeSand.x == sandSpawn.x && activeSand.y == sandSpawn.y)
                                    {
                                        tiles.Add(activeSand, State.Sand);
                                        sandActive = false;
                                        ++sandAtRest;
                                        break;
                                        //we done
                                    }
                                }
                                tiles.Add(activeSand, State.Sand);
                                sandActive = false;
                                ++sandAtRest;
                            }
                            else
                            {
                                activeSand.y += 1;
                                activeSand.x += 1;
                            }
                        }
                        else
                        {
                            activeSand.y += 1;
                            activeSand.x -= 1;
                        }
                    }
                    else
                    {
                        activeSand.y += 1;
                    }
                }

                //exit case when sand hits the void
                if (part2 == false)
                {
                    if (activeSand.x > maxBound.x || activeSand.x < minBound.x || activeSand.y > maxBound.y)
                    {
                        break;
                    }
                }
            }

            Console.WriteLine("Sand at rest: " + sandAtRest);
        }

        /*
        public void display()
        {
            if (doDisplay)
            {

                //display
                Console.Clear();

                var ordered = tiles.ToList();
                ordered.Add(new KeyValuePair<Point, State>(activeSand, State.Sand));
                ordered = ordered.OrderBy(x => x.Key.x + (x.Key.y * 10000)).ToList();


                int lastY = minBound.y;
                int lastX = minBound.x;
                string row = "";
                foreach (var tile in ordered)
                {
                    if (lastY != tile.Key.y)
                    {
                        for (int i = lastY; i < tile.Key.y; ++i)
                        {
                            if (i == lastY)
                                Console.WriteLine(row);
                            else
                                Console.WriteLine("");
                        }
                        row = "";
                        lastY = tile.Key.y;
                        lastX = minBound.x;
                    }

                    for (int xi = lastX; xi < tile.Key.x - 1; ++xi)
                        row += ".";

                    lastX = tile.Key.x;

                    row += tile.Value switch
                    {
                        State.Sand => "O",
                        State.Empty => ".",
                        State.Solid => "#",
                        _ => throw new Exception("invalid state")
                    };
                }

                if (row.Length > 0)
                    Console.WriteLine(row);

                Thread.Sleep(10);
            }
        }*/
    }
}
