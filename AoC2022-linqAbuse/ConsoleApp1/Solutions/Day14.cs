﻿using System;
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
using static ConsoleApp1.Utils;
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

        public override void Part1()
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
            Dictionary<Point, State> tiles = new();

            //parse...
            var lines = File.ReadAllText(InputFile!).Split("\r\n").Select(x => x.Trim());

            foreach(var line in lines)
            {
                var points = line.Split("->").Select(x => x.Trim().Split(',').Select(x=> x.Trim()).ToList()).Select(x=> new Point() { X = int.Parse(x[0]), Y = int.Parse(x[1]) }).ToList();

                for (int i =0; i < points.Count - 1; i++)
                {
                    Point start = points[i];
                    Point end = points[i + 1];

                    for(;;)
                    {
                        if (!tiles.ContainsKey(start))
                            tiles.Add(start, State.Solid);

                        if (start.X == end.X && start.Y == end.Y)
                            break;

                        if (start.X != end.X)
                            start.X += Math.Sign(end.X - start.X);
                        if(start.Y != end.Y)
                            start.Y += Math.Sign(end.Y - start.Y);
                    }
                }
            }

            //things to track...
            Point sandSpawn = new Point() { X = 500, Y = 0 };
            Point minBound = new Point() { X = 99999, Y = 0 };
            Point maxBound = new Point() { X = -9999, Y = -9999 };

            minBound.X = tiles.Min(x => x.Key.X);
            minBound.Y = tiles.Min(x => x.Key.Y);
            maxBound.X = tiles.Max(x => x.Key.X);
            maxBound.Y = tiles.Max(x => x.Key.Y);

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

                    if (activeSand.Y == maxBound.Y + 1)
                    {
                        tiles.Add(activeSand, State.Sand);
                        sandActive = false;
                        ++sandAtRest;
                    }
                }

                if(sandActive == true)
                { 
                    if(tiles.TryGetValue(new Point() { X = activeSand.X, Y = activeSand.Y + 1}, out State state) && state != State.Empty)
                    {
                        if (tiles.TryGetValue(new Point() { X = activeSand.X - 1, Y = activeSand.Y + 1 }, out state) && state != State.Empty)
                        {
                            if (tiles.TryGetValue(new Point() { X = activeSand.X + 1, Y = activeSand.Y + 1 }, out state) && state != State.Empty)
                            {
                                tiles.Add(activeSand, State.Sand);
                                sandActive = false;
                                ++sandAtRest;

                                if(part2 == true && activeSand.X == sandSpawn.X && activeSand.Y == sandSpawn.Y)
                                {
                                    break;
                                    //we done
                                }
                            }
                            else
                            {
                                activeSand.Y += 1;
                                activeSand.X += 1;
                            }
                        }
                        else
                        {
                            activeSand.Y += 1;
                            activeSand.X -= 1;
                        }
                    }
                    else
                    {
                        activeSand.Y += 1;
                    }
                }

                //exit case when sand hits the void
                if (part2 == false)
                {
                    if (activeSand.X > maxBound.X || activeSand.X < minBound.X || activeSand.Y > maxBound.Y)
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
