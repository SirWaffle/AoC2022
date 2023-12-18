using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AoC2023.Utilities;

namespace AoC2023.Solutions
{
    internal class Day10 : AbstractPuzzle<Day10>
    {
        public override void Init()
        {
            DoPart1 = true;
            DoPart2 = false;
        }


        Dictionary<char, (int x, int y)> data_offsets = new() { { 'N', (0, -1) }, { 'S', (0, 1) }, { 'E', (1, 0) }, { 'W', (-1, 0) }, };

        Dictionary<char, List<char>> data_connections = new() {
                { '|', new List<char>{ 'N', 'S' } },
                { '-', new List<char>{ 'E', 'W' } },
                { 'L', new List<char>{ 'N', 'E' } },
                { 'J', new List<char>{ 'N','W' } },
                { '7', new List<char>{ 'S','W' } },
                { 'F', new List<char>{ 'S','E'} },
                { '.', new List<char>{ } },
                { 'S', new List<char>{ 'N', 'E', 'S', 'W' } },
            };


        class MapPoint
        {
            public char c = ' '; //map sym
            public int x = 0; //x pos
            public int y = 0; //y pos
            public int d = -1; //distance

            public bool s = false; //seen - short circuit some stuff
            public List<char> connections = new(); //cache
            public List<(int x, int y)> offsets = new(); //cache

            public MapPoint() { }
        }

        override public void Part1Impl()
        {
            var inputstr = File.ReadAllText(InputFilePart1);
            var width = inputstr.IndexOf("\r");
            var height = inputstr.Count(x => x == '\r') + 1;
            var map = File.ReadAllText(InputFilePart1).Replace("\r\n", String.Empty).Select((c, i) => new MapPoint() { c = c, x = i % width, y = (int)(i / width), d = c == 'S' ? 0 : -1, connections = data_connections[c], offsets = data_connections.Where(con => con.Key == c).Select(con => con.Value.Select(cv => data_offsets[cv])).SelectMany(l => l).ToList()}).ToList();

            List<MapPoint> searchHeads = new List<MapPoint> { map.Where(mp => mp.c == 'S').First() };
            while (searchHeads.Count > 0)
            {
                var curPos = searchHeads.Last();
                searchHeads.RemoveAt(searchHeads.Count - 1);

                var viable = curPos.offsets.Select(off => (curPos.x + off.x, curPos.y + off.y)).Where(np => np.Item1 >= 0 && np.Item1 <= width - 1 && np.Item2 >= 0 && np.Item2 <= height).Select(mapPoint => map.ElementAt(mapPoint.Item1 + (mapPoint.Item2 * width))).Where(mp => mp.s == false || (mp.d > curPos.d + 1)).Where(n => n.offsets.Any(no => n.x + no.x == curPos.x && n.y + no.y == curPos.y)).ToList();

                //set new distances and seen
                viable.TrueForAll(mp => (mp.d = curPos.d + 1) > 0 && (mp.s = true));

                searchHeads.AddRange(viable);
            }

            DrawMap(map, width);

            Console.WriteLine("");
            Console.WriteLine("most steps = " + map.Where(mp => mp.d >= 0).OrderByDescending(mp => mp.d).First());
            //6812


            //
            //  Part 2
            //

            //lets now find how many tiles are enclosed...
            //resize and flood fill
            List<(int val, bool inLoop)> resizedMap = new();
            resizedMap.AddRange(Enumerable.Range(0, map.Count * 9).Select(x => (-1, false)));

            //extend pipes and mark pipe tiles in our now 3x3 grid tiles
            for (int x = 1; x < (width * 3); x += 3)
            {
                for (int y = 1; y < (height * 3); y += 3)
                {
                    if (map[(x - 1)/3 + (((y  - 1)/3) * width)].d >= 0) 
                    {
                        for(int xm = -1; xm < 2; xm++) //part of the loop, mark the 9x9 square as 'loop tile square'
                            for (int ym = -1; ym < 2; ym++)
                                resizedMap[(x + xm) + ((y + ym) * (width * 3))] = (-1, true);

                        //mark the actual pipe tiles
                        var mark = map[(x - 1) / 3 + (((y - 1) / 3) * width)].offsets.Select(off => (x + off.x, y + off.y)).Select(p => p.Item1 + (p.Item2 * (width * 3))).Where(p => p < resizedMap.Count()).ToList();
                        mark.Add(x + (y * (width * 3)));
                        foreach (var markedspot in mark)
                            resizedMap[markedspot] = (999, true);
                    }
                }
            }

            //flood fill from outside in
            var floodfillSearch = new List<(int x, int y)> { (0 ,0) };
            while (floodfillSearch.Count > 0)
            {
                var curPos = floodfillSearch.Last();
                floodfillSearch.RemoveAt(floodfillSearch.Count - 1);

                resizedMap[curPos.x + (curPos.y * (width * 3))] = (-10, resizedMap[curPos.x + (curPos.y * (width * 3))].inLoop);

                floodfillSearch.AddRange(new List<(int x, int y)>() { (curPos.x, curPos.y + 1), (curPos.x, curPos.y - 1), (curPos.x + 1, curPos.y), (curPos.x - 1, curPos.y), }.Where(pos => pos.x >= 0 && pos.x < (width * 3) && pos.y >= 0 && pos.y < (height * 3)).Where(validpos => resizedMap[validpos.x + (validpos.y * (width * 3))].val == -1).ToList());
            }
            var enclosed = resizedMap.Where(point => point.val == -1 && point.inLoop == false).Count();
     
            Console.WriteLine("");
            Console.WriteLine("Enclosed: " + (enclosed / 9) );
            //527
        }

        override public void Part2Impl()
        {
        }


        void DrawMap(List<MapPoint> map, int width)
        {
            Console.SetCursorPosition(1, 0);
            foreach (var mp in map)
            {
                Console.BackgroundColor = ConsoleColor.Black;

                if (mp.x % width == 0)
                    Console.Write(Environment.NewLine);

                if (mp.d == -1)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write(mp.c);
                }
                else if (mp.d == -10)
                {
                    Console.BackgroundColor = ConsoleColor.Cyan;
                    Console.Write(mp.c);
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.Write(' ');
                }
            }
            Console.BackgroundColor = ConsoleColor.Black;
        }

    }
}
