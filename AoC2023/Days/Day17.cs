using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AoC2023.Utilities;

namespace AoC2023.Solutions
{
    internal class Day17 : AbstractPuzzle<Day17>
    {
        public override void Init()
        {
            DoPart1 = true;
            DoPart2 = false;
        }

        public struct Point
        {
            public int x = 0;
            public int y = 0;
            public int vx = 0;
            public int vy = 0;

            public int steps = 0;
            public int streak = 0;
            public int heatLoss = 0;
            public int targetCostEstimate = 0;

            public List<(int x, int y, int step)>? prevPoints = null;

            public Point()
            { }

            public override string ToString()
            {
                string str = String.Format("{0:X}", heatLoss);
                if (str.Length >= 4)
                    str = " ";

                while (str.Length < 4)
                    str += " ";
                return str;
            }

            public Point Set(Point o)
            {
                steps = o.steps;
                x = o.x;
                y = o.y;
                vx = o.vx;
                vy = o.vy;
                streak = o.streak;
                heatLoss = o.heatLoss;
                targetCostEstimate = o.targetCostEstimate;

                if (o.prevPoints == null)
                    prevPoints = new(1);
                else
                    prevPoints = new(o.prevPoints.Count + 1);

                if (o.prevPoints != null)
                {
                    prevPoints.Clear();
                    prevPoints.AddRange(o.prevPoints);
                }
                return this;
            }

            public Point AddToPath()
            {
                if (prevPoints != null)
                    prevPoints.Add((x, y, steps));
                return this;
            }

            public bool IsPointInPath(Point p)
            {
                if(prevPoints != null)
                {
                    for(int i = 0; i <  prevPoints.Count; i++)
                    {
                        if (prevPoints[i].x == x && prevPoints[i].y == y)
                            return true;
                    }
                }
                return false;
            }

            public Point ResetStreak()
            {
                streak = 0;
                return this;
            }

            public Point OrthoVel()
            {
                int t = vy;
                vy = vx;
                vx = t;
                return this;
            }

            public Point InvVel()
            {
                vx = -1 * vx;
                vy = -1 * vy;
                return this;
            }

            public Point CalcDistEst(Point target)
            {
                //targetCostEstimate = Math.Abs(target.x - x) + Math.Abs(target.y - y);
                //targetCostEstimate *= 9; //max heat loss per distance step

                //targetCostEstimate = steps;// + Math.Abs(target.x - x) + Math.Abs(target.y - y);

                targetCostEstimate = heatLoss;
                //targetCostEstimate = -1 * heatLoss; //max value - heatloss
                return this;
            }

            public int GetSecondarySorting()
            {
                return streak;
            }

            public int GetThirdSorting()
            {
                //return 0;
                if (vy == 1) return 0;
                if (vx == 1) return 1;
                if (vy == -1) return 2;
                if (vx == -1) return 3;
                Debug.Assert(false);
                return 0;
            }
        }

        List<List<int>> map = new();
        List<List<List<List<Point>>>> visitedByDir = new();
        ConcurrentBag<Point> search = new ConcurrentBag<Point>();
        List<Point> solutions = new();
        List<Point> allWork = new List<Point>();

        int minHeatLoss = 868; //1000; //1000; // int.MaxValue; 
        //876 too high?
        //800 too low 
        //not 888
        //882 wrong
        //853 wrong
        //880 wrong
        //878
        //868
        //867

        private void AddWorkFromProcessing(Point p)
        {
            search.Add(p);
        }

        private void AddWork(Point p)
        {
            bool inserted = false;
            for (int i = 0; i < allWork.Count && inserted == false; i++)
            {
                if (p.targetCostEstimate < allWork[i].targetCostEstimate)
                {
                    inserted = true;
                    allWork.Insert(i, p);
                }
            }

            if (inserted == false)
                allWork.Add(p);
        }

        private bool ShouldCull(ref Point workItem, ref Point target)
        {
            int dist = Math.Abs(target.x - workItem.x) + Math.Abs(target.y - workItem.y);
            int estHeatloss = workItem.heatLoss + dist - 1; //-1 for breathing room
            Point best = visitedByDir[workItem.GetThirdSorting()][workItem.GetSecondarySorting()][workItem.y][workItem.x];
            if (minHeatLoss < estHeatloss
                || best.heatLoss < workItem.heatLoss
                || (best.heatLoss == workItem.heatLoss && best.steps < workItem.steps)
                )
            {
                if (dist == 0)
                    search.Add(workItem); //dont accidentally eat a solution
                return true;
            }

            //must be better than anything in the ;secondary sorting (streak) map that are lower than this streak

            //for(int s = 0; s < workItem.GetSecondarySorting(); ++s)
            {
                if (visitedByDir[workItem.GetThirdSorting()][0][workItem.y][workItem.x].heatLoss < workItem.heatLoss &&
                    visitedByDir[workItem.GetThirdSorting()][0][workItem.y][workItem.x].steps <= workItem.steps)
                    return true;
            }

            return false;
        }

        private void ProccessSingleItem(ref Point workItem, ref Point target)
        {
            if (ShouldCull(ref workItem, ref target))
                return;

            //update workitem movement
            workItem.x += workItem.vx;
            workItem.y += workItem.vy;
            workItem.steps += 1;
            workItem.streak += 1;

            //if this point is already in the path, drop it
            if (workItem.IsPointInPath(workItem) == true)
                return;

            if (workItem.x < 0 || workItem.x >= map[0].Count
             || workItem.y < 0 || workItem.y >= map.Count)
            {
                return;
            }

            workItem.heatLoss += map[workItem.y][workItem.x];

            if (ShouldCull(ref workItem, ref target))
                return;

            //change dir, N, S, E , W
            search.Add(new Point().Set(workItem).OrthoVel().ResetStreak().AddToPath().CalcDistEst(target));
            search.Add(new Point().Set(workItem).OrthoVel().InvVel().ResetStreak().AddToPath().CalcDistEst(target));

            //if we can roll backwards..probably not, since i get a lower score of 101 on the example
            //AddWorkFromProcessing(pointPool.Get().Set(workItem).InvVel().ResetStreak().AddToPath().CalcDistEst(target));

            if (workItem.streak < 3) //forwards only if our streak isnt too big
                search.Add(new Point().Set(workItem).AddToPath().CalcDistEst(target));
        }

        public override void Part1Impl()
        {
            //InputFilePart1
            //InputFileSample
            map = File.ReadAllText(InputFilePart1).Split("\r\n", StringSplitOptions.None).Select(s => new List<int>(s.Select(c => int.Parse(c.ToString()))).ToList()).ToList();
            int height = map.Count;
            int width = map[0].Count;

            Point target = new Point();
            target.x = map[0].Count() - 1;
            target.y = map.Count() - 1;

            Point start = new Point();
            start.vx = 1;
            allWork.Add(start);

            start = new Point();
            start.vy = 1;
            allWork.Add(start);

            visitedByDir = Enumerable.Range(0, 4).Select(x => Enumerable.Range(0, 4).Select(x => map.Select(x => x.Select(x => new Point() { heatLoss = int.MaxValue, streak = int.MaxValue, steps = int.MaxValue }).ToList()).ToList()).ToList()).ToList();

            int logCount = -2;
            while (allWork.Count > 0 || search.Count > 0)
            {
                //start crunch work
                foreach (var searchItem in search)
                {

                    if (visitedByDir[searchItem.GetThirdSorting()][searchItem.GetSecondarySorting()][searchItem.y][searchItem.x].heatLoss > searchItem.heatLoss)
                    { 
                        visitedByDir[searchItem.GetThirdSorting()][searchItem.GetSecondarySorting()][searchItem.y][searchItem.x] = searchItem;
                    }

                    if (searchItem.x == target.x && searchItem.y == target.y)
                    {
                        //hit the target, update some stuff
                        solutions.Add(searchItem);
                        if (searchItem.heatLoss < minHeatLoss)
                        {
                            minHeatLoss = searchItem.heatLoss;

                            solutions = solutions.OrderBy(x => x.heatLoss).ToList();
                            //Visualize(map, solutions.First(), allWork);
                            //Visualize(map, visitedByDir, solutions.First(), allWork);
                            Console.WriteLine("Lowest cost heat loss: " + solutions.First().heatLoss + " after " + solutions.First().steps);
                            Console.WriteLine("All work remaining: " + allWork.Count);
                        }
                    }
                    else
                    {
                        AddWork(searchItem);
                    }
                }

                search.Clear();
                int workAmount = 200;
                int workRange = allWork.Count() > workAmount ? workAmount : allWork.Count;
                var curWork = allWork.GetRange(0, workRange);
                allWork.RemoveRange(0, workRange);

                if (allWork.Count > workRange * 2)
                {
                    curWork.AddRange(allWork.GetRange(allWork.Count - 2, 2));
                    allWork.RemoveRange(allWork.Count - 2, 2);
                }

                Task.Run(() =>
                {
                    ++logCount;
                    if (logCount >= 2500 || logCount == -1)
                    {
                        logCount = 0;
                        Visualize(map, visitedByDir, solutions.Count() > 0 ? solutions.First() : new Point(), null);

                        if (solutions.Count() > 0)
                            Console.WriteLine("Lowest cost heat loss: " + solutions.First().heatLoss + " after " + solutions.First().steps);

                        Console.WriteLine("All work remaining: " + allWork.Count);
                    }
                });

                Parallel.ForEach(curWork, workItem =>
                //foreach (var workItem in curWork)
                {
                    ProccessSingleItem(ref workItem ,ref target);
                });
            }

            solutions = solutions.OrderBy(x => x.heatLoss).ToList();

            Visualize(map, visitedByDir, solutions.Count() > 0 ? solutions.First() : new Point(), null);
            //Visualize(map, solutions.Count() > 0 ? solutions.First() : new Point(), null);
            Console.WriteLine("Lowest cost heat loss: " + solutions.First().heatLoss + " after " + solutions.First().steps);

            Console.WriteLine("All work remaining: " + allWork.Count);
        }

        List<(int, ConsoleColor)> outputColors = new List<(int, ConsoleColor)>()
        {
            { (750, ConsoleColor.Green) },
            { (850, ConsoleColor.White) },
            { (870, ConsoleColor.Cyan) },
            { (880, ConsoleColor.Gray) },
            { (890, ConsoleColor.DarkGray) },
            { (900, ConsoleColor.Red) },
            { (1000, ConsoleColor.Magenta) },
            { (9999, ConsoleColor.DarkGreen) },
        };

        private void Visualize(List<List<int>> map, List<List<List<List<Point>>>> visited, Point? path, List<Point>? allWork)
        {
            List<Point>? allPaths = null;
            //if (allWork != null)
            //    allWork.SelectMany(l => l.prevPoints).DistinctBy(p => p.x + (p.y * 10000)).ToList();

            Console.SetCursorPosition(0, 0);
            for (int y = 0; y < map.Count; y++)
            {
                for (int x = 0; x < map[y].Count; x++)
                {
                    Console.BackgroundColor = ConsoleColor.Black;

                    
                    if (path != null && path.Value.prevPoints != null && path.Value.prevPoints.Any(p => p.x == x && p.y == y))
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                    }
                    /*
                    else if (allPaths != null && allPaths.Any(p => p.x == x && p.y == y))
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                    }
                    else if (allWork.Any(p => p.x == x && p.y == y))
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                    }
                    */

                    if (Console.BackgroundColor == ConsoleColor.Black)
                    {
                        if (visited.All(l => l.All( l => l[y][x].heatLoss == int.MaxValue)))
                            Console.BackgroundColor = ConsoleColor.Black;
                        else
                        {
                            for (int i = 1; i < outputColors.Count; i++)
                            {
                                if (visited.Any(l => l.Any(l => l[y][x].heatLoss < outputColors[i].Item1)))
                                {
                                    Console.BackgroundColor = outputColors[i - 1].Item2;
                                    break;
                                }
                            }
                        }
                    }

                    if (x == map[y].Count - 1 && y == map.Count - 1)
                        Console.BackgroundColor = ConsoleColor.Yellow;

                    Console.Write(map[y][x].ToString());
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine();
            }
        }



        override public void Part2Impl()
        {
        }

    }
}
