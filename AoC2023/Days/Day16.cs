namespace AoC2023.Solutions
{
    internal class Day16 : AbstractPuzzle<Day16>
    {
        public override void Init()
        {
            DoPart1 = true;
            DoPart2 = false;
        }

        class Point
        {
            public int x = 0;
            public int y = 0;
            public int vx = 0;
            public int vy = 0;

            public override string ToString()
            {
                return "("+x+","+y+") " + vx + ", " + vy;
            }

            public Point Ortho()
            {
                int t = vy;
                vy = vx;
                vx = t;
                return this;
            }

            public Point Inv()
            {
                vx = -1 * vx;
                vy = -1 * vy;
                return this;
            }
        }

        public override void Part1Impl()
        {
            //InputFilePart1
            //InputFileSample
            var contrap = File.ReadAllText(InputFilePart1).Split("\r\n", StringSplitOptions.None).Select(s => new List<char>(s.ToList())).ToList();

            List<Point> startingPoints = new();

            //part 1 
            //startingPoints.Add(new Point() { x = -1, vx = 1 });

            //part 2
            //go East, West, South, North
            startingPoints.AddRange(Enumerable.Range(0, contrap.Count).Select(r => new Point() { x = -1, y = r, vx = 1 }));
            startingPoints.AddRange(Enumerable.Range(0, contrap.Count).Select(r => new Point() { x = contrap[r].Count, y = r, vx = -1 }));
            startingPoints.AddRange(Enumerable.Range(0, contrap[0].Count).Select(r => new Point() { x = r, y = -1, vy = 1 }));
            startingPoints.AddRange(Enumerable.Range(0, contrap[0].Count).Select(r => new Point() { x = r, y = contrap.Count, vy = -1 }));

            List<(int startX, int startY, int energized)> energized = new();

            Parallel.ForEach(startingPoints, startingPoint =>
            {
                Console.WriteLine("Scanning particle config: " + startingPoint.x + " " + startingPoint.y);
                List<Point> paths = new();
                var parts = new List<Point>() { new Point() { x = startingPoint.x, y = startingPoint.y, vx = startingPoint.vx, vy = startingPoint.vy } };

                while (parts.Count() > 0)
                {
                    //draw
                    if (false)  Visualize(contrap, paths, parts);

                    for (int i = 0; i < parts.Count; i++)
                    {
                        var curPart = parts[i];
                        curPart.x += curPart.vx;
                        curPart.y += curPart.vy;
                        if (curPart.y < 0 || curPart.y >= contrap.Count() || curPart.x < 0 || curPart.x >= contrap[0].Count())
                        {
                            //out of bounds
                            parts.RemoveAt(i);
                            --i;
                            continue;
                        }

                        switch (contrap[curPart.y][curPart.x])
                        {
                            case '.': break;
                            case '/': curPart.Ortho().Inv(); break;
                            case '\\': curPart.Ortho(); break;
                            case '|':
                                if (curPart.vx != 0)
                                {
                                    curPart.Ortho();
                                    parts.Add(new Point() { x = curPart.x, y = curPart.y, vx = curPart.vx, vy = curPart.vy }.Inv());
                                }
                                break;
                            case '-':
                                if (curPart.vy != 0)
                                {
                                    curPart.Ortho();
                                    parts.Add(new Point() { x = curPart.x, y = curPart.y, vx = curPart.vx, vy = curPart.vy }.Inv());
                                }
                                break;
                        }

                        //remove particles that are tracing already found paths
                        if (paths.Any(x => x.x == curPart.x && x.y == curPart.y && x.vx == curPart.vx && x.vy == curPart.vy))
                        {
                            parts.RemoveAt(i);
                            --i;
                        }
                        else //add path
                        {
                            paths.Add(new Point() { x = curPart.x, y = curPart.y, vx = curPart.vx, vy = curPart.vy });
                        }
                    }
                }

                energized.Add((startingPoint.x, startingPoint.y, paths.DistinctBy(x => x.x + (10000 * x.y)).Count()));
            });
            Console.WriteLine("answer p1: " + energized.OrderByDescending(x => x.energized).First());
            //8125 p1
            //8489 p2
        }

        private void Visualize(List<List<char>> contrap, List<Point> paths, List<Point> parts)
        {
            {
                Console.SetCursorPosition(0, 0);
                for (int y = 0; y < contrap.Count; y++)
                {
                    for (int x = 0; x < contrap[y].Count; x++)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;

                        if (paths.Any(p => p.x == x && p.y == y))
                        {
                            Console.BackgroundColor = ConsoleColor.Yellow;
                        }
                        if (parts.Any(p => p.x == x && p.y == y))
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                        }
                        Console.Write(contrap[y][x]);
                    }
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine();
                }
            }
        }

        override public void Part2Impl()
        {
        }
    }
}
