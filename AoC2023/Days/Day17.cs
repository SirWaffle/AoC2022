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
   
        public class ObjectPool<T>
        {
            private readonly ConcurrentBag<T> _objects;
            private readonly Func<T> _objectGenerator;

            public ObjectPool(Func<T> objectGenerator)
            {
                _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
                _objects = new ConcurrentBag<T>();
            }

            public T Get() => /*_objects.TryTake(out T item) ? item :*/ _objectGenerator();

            public void Return(T item)
            {
             //   _objects.Add(item);
            }

            public int Count => _objects.Count;
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

            public List<(int x, int y, int step)> prevPoints = new();

            public Point()
            { }

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

                //prevPoints.Clear();
                //prevPoints.AddRange(o.prevPoints);
                return this;
            }

            public Point AddToPath()
            {
                prevPoints.Add((x, y, steps));
                return this;
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
                targetCostEstimate = Math.Abs(target.x - x) + Math.Abs(target.y - y);
                targetCostEstimate *= 9; //max heat loss per distance step
                targetCostEstimate += heatLoss;
                return this;
            }

            public int GetDir()
            {
                if (vy == 1) return 0;
                if (vx == 1) return 1;
                if (vy == -1) return 2;
                if (vx == -1) return 3;
                Debug.Assert(false);
                return 0;
            }
        }


        ObjectPool<Point> pointPool = new ObjectPool<Point>(() => new Point());

        List<List<int>> map = new();

        //ConcurrentBag<Point> search = new ConcurrentBag<Point>();
        List<Point> search = new List<Point>();

        //0 = +y, 1 = +x, 2 = -y, 3 = -x
        List<List<List<Point>>> visitedByDir = new();

        int minHeatLoss = 1000; // int.MaxValue; //876 too high? i think?; //800 too low
        //882 wrong

        private void ProccessSingleItem(Point workItem, Point target)
        {
            //update workitem movement
            workItem.x += workItem.vx;
            workItem.y += workItem.vy;
            workItem.steps += 1;
            workItem.streak += 1;
            workItem.CalcDistEst(target);

            if (workItem.x < 0 || workItem.x >= map[0].Count
             || workItem.y < 0 || workItem.y >= map.Count)
            {
                pointPool.Return(workItem);
                return;
            }

            workItem.heatLoss += map[workItem.y][workItem.x];


            if (minHeatLoss < workItem.heatLoss
                || visitedByDir[workItem.GetDir()][workItem.y][workItem.x].heatLoss < workItem.heatLoss)
            {
                pointPool.Return(workItem);
                return;
            }

            //change dir, N, S, E , W
            search.Add(pointPool.Get().Set(workItem).OrthoVel().ResetStreak().AddToPath());
            search.Add(pointPool.Get().Set(workItem).OrthoVel().InvVel().ResetStreak().AddToPath());
            search.Add(pointPool.Get().Set(workItem).InvVel().ResetStreak().AddToPath());

            if (workItem.streak < 3) //forwards only if our streak isnt too big
                search.Add(pointPool.Get().Set(workItem).AddToPath());
            else
                pointPool.Return(workItem);
        }

        public override void Part1Impl()
        {
            //InputFilePart1
            //InputFileSample
            map = File.ReadAllText(InputFilePart1).Split("\r\n", StringSplitOptions.None).Select(s => new List<int>(s.Select(c => int.Parse(c.ToString()))).ToList()).ToList();
            int height = map.Count;
            int width = map[0].Count;

            List<Point> allWork = new List<Point>();

            Point target = pointPool.Get();
            target.x = map[0].Count() - 1;
            target.y = map.Count() - 1;

            Point start = pointPool.Get();
            start.vx = 1;
            allWork.Add(start);

            start = pointPool.Get();
            start.vy = 1;
            allWork.Add(start);

            visitedByDir = Enumerable.Range(0, 4).Select( x=> map.Select(x => x.Select(x => new Point() { heatLoss = int.MaxValue }).ToList()).ToList()).ToList();
            List<Point> solutions = new();

            int logCount = 0;
            while(allWork.Count > 0 || search.Count > 0)
            {
                logCount += 1;

                //start crunch work
                foreach (var searchItem in search)
                {
                    //update visited nodes
                    //the visited nodes is culling things we should search, something is off here..ohh, the straight line crap might make it wonky?
                    if (visitedByDir[searchItem.GetDir()][searchItem.y][searchItem.x].heatLoss >= searchItem.heatLoss)
                    {
                        visitedByDir[searchItem.GetDir()][searchItem.y][searchItem.x] = searchItem;

                        if (searchItem.x == target.x && searchItem.y == target.y)
                        {
                            //hit the target, update some stuff
                            solutions.Add(searchItem);
                            if (searchItem.heatLoss < minHeatLoss)
                            {
                                minHeatLoss = searchItem.heatLoss;

                                solutions = solutions.OrderBy(x => x.heatLoss).ToList();
                                Visualize(map, solutions.First(), allWork);
                                Console.WriteLine("Lowest cost heat loss: " + solutions.First().heatLoss + " after " + solutions.First().steps);
                            }
                        }
                        else
                        {

                            bool inserted = false;
                            for (int i = 0; i < allWork.Count && inserted == false; i++)
                            {
                                if (searchItem.targetCostEstimate < allWork[i].targetCostEstimate)
                                {
                                    inserted = true;
                                    allWork.Insert(i, searchItem);
                                }
                            }

                            if (inserted == false)
                                allWork.Add(searchItem);
                        }
                    }
                    else
                    {
                        pointPool.Return(searchItem);
                    }
                }
                
                search.Clear();

                int workRange = allWork.Count() > 500? 500: allWork.Count;
                var curWork = allWork.GetRange(0, workRange);
                allWork.RemoveRange(0, workRange);

                if (logCount >= 500)
                {
                    Console.WriteLine("All work remaining: " + allWork.Count);

                    logCount = 0;
                    Visualize(map, solutions.Count() > 0 ? solutions.First() : new Point(), null);
                    if(solutions.Count() > 0)
                        Console.WriteLine("Lowest cost heat loss: " + solutions.First().heatLoss + " after " + solutions.First().steps);
                }

                //Parallel.ForEach(curWork, workItem =>
                foreach (var workItem in curWork)
                {
                    ProccessSingleItem(workItem, target);
                }//);
            }

            solutions = solutions.OrderBy(x => x.heatLoss).ToList();

            Visualize(map, solutions.First(), allWork);            
        }
        private void Visualize(List<List<int>> map, Point? path, List<Point>? allWork)
        {
            List<Point>? allPaths = null;
            if(allWork != null)
                allWork.SelectMany(l => l.prevPoints).DistinctBy(p => p.x + (p.y * 10000)).ToList();

            Console.SetCursorPosition(0, 0);
            for (int y = 0; y < map.Count; y++)
            {
                for (int x = 0; x < map[y].Count; x++)
                {
                    Console.BackgroundColor = ConsoleColor.Black;

                    if (path != null && path.Value.prevPoints.Any(p => p.x == x && p.y == y))
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                    }
                    else if (allPaths !=null && allPaths.Any(p => p.x == x && p.y == y))
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                    }
                    /*
                    else if (allWork.Any(p => p.x == x && p.y == y))
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                    }
                    */

                    Console.Write(map[y][x]);
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
