using ConsoleApp1.Utilities;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using static ConsoleApp1.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleApp1.Solutions
{
    internal class Day24 : AbstractPuzzle
    {

        override public void Part1()
        {
            Both(false);
        }

        override public void Part2()
        {
            Both(true);
        }

        void Both(bool part2)
        {
            var lines = File.ReadAllText(InputFile!).Split("\r\n").ToList();
            lines.RemoveAt(0);
            lines.RemoveAt(lines.Count - 1);

            Blizzards blizz = new(lines[1].Replace("#", "").Length, lines.Count);

            int y = -1;
            foreach (var line in lines)
            {
                ++y;
                string modLine = line.Replace("#", "");

                //ignore the walls
                for (int x = 0; x < modLine.Length; x++)
                {
                    blizz.leftBlizz[y] += '.';
                    blizz.rightBlizz[y] += '.';
                    blizz.downBlizz[x] += '.';
                    blizz.upBlizz[x] += '.';

                    if (modLine[x] == '<')
                    {
                        blizz.leftBlizz[y] = blizz.leftBlizz[y].Remove(blizz.leftBlizz[y].Length - 1);
                        blizz.leftBlizz[y] += '<';
                    }
                    else if (modLine[x] == '>')
                    {
                        blizz.rightBlizz[y] = blizz.rightBlizz[y].Remove(blizz.rightBlizz[y].Length - 1);
                        blizz.rightBlizz[y] += '>';
                    }
                    if (modLine[x] == 'v')
                    {
                        blizz.downBlizz[x] = blizz.downBlizz[x].Remove(blizz.downBlizz[x].Length - 1);
                        blizz.downBlizz[x] += 'v';
                    }
                    else if (modLine[x] == '^')
                    {
                        blizz.upBlizz[x] = blizz.upBlizz[x].Remove(blizz.upBlizz[x].Length - 1);
                        blizz.upBlizz[x] += '^';
                    }
                }
            }

            Sim sim = new(blizz);
            sim.maxBounds = blizz.maxBounds;
            sim.start = new Point(0, -1);
            sim.end = new Point(sim.maxBounds.X - 1, sim.maxBounds.Y);
            sim.player.pos = sim.start;
            sim.step = 0;

            sim.Visualize();

            int depthLimit = 327; //328 was found as a best, wrong answer too high

            ThreadSafeSimStats.instance = new( 16, part2);
            ThreadSafeSimStats.instance.DoVisualization = false;
            ThreadSafeSimStats.instance.bestSimSteps = depthLimit;

            /*
            //testing visualization
            for (int stepNum = 0; stepNum < 10; stepNum++)
            {
                sim.step++;
                sim.Visualize();
            }*/

            if (ThreadSafeSimStats.instance.DoVisualization)
            {
                Console.Clear();
                Console.WindowWidth = sim.maxBounds.X + 1;
            }


            LinkedList<Sim> search = new();
            search.AddLast(sim);            

            Stopwatch watch = new();
            watch.Start();           

            //well, lets search. 
            SearchTaskLIST(search, depthLimit);

            //wait for tasks
            Console.WriteLine("\n----- waiting for tasks -------");
            for (; ; )
            {    
                Thread.Sleep(5000);
                UInt64 active = Sim.NumCreatedSims - ThreadSafeSimStats.instance.discardedBranches;
                Console.WriteLine(ThreadSafeSimStats.instance.crunchingTasks.Count() + " tasks\ncurrent simId: "
                                                    + Sim.NumCreatedSims + " number discarded: " + ThreadSafeSimStats.instance.discardedBranches + " Active: " + active
                                                    + "\nper sec: " + ((float)Sim.NumCreatedSims / ((float)watch.ElapsedMilliseconds / 1000)));
                Console.WriteLine("current best: " + ThreadSafeSimStats.instance.bestSimSteps);
                ThreadSafeSimStats.instance.ClearFinishedWork();
                if (Task.WaitAll(ThreadSafeSimStats.instance.crunchingTasks.ToArray(), 5000))
                {
                    ThreadSafeSimStats.instance.ClearFinishedWork();
                    break;
                }
            }



            Console.WriteLine("\n----- Finished Waiting -------");

            Console.WriteLine("Made it to finish at shortest number of steps: " + ThreadSafeSimStats.instance.bestSimSteps);
            //be surei dont accidentaly fat finger some keys and close the console after this thing finishes, ha
            _ = Console.ReadLine();
            _ = Console.ReadLine();
            _ = Console.ReadLine();
            _ = Console.ReadLine();
        }

        int visCount = 0;
        object visLock = new();

        void SearchTaskLIST(LinkedList<Sim> search, int depthLimit)
        {
            bool[] freeSpaces = new bool[4];
            char[] chars = new char[4];
            Point distVec = new();

            while (search.Count > 0)
            {
                Sim curSim = search.First.Value;
                search.RemoveFirst();

                if (ThreadSafeSimStats.instance.DoVisualization)
                {
                    if(ThreadSafeSimStats.instance.DoPathVisualization)
                        curSim.playerPath.Add(curSim.player.pos);

                    ++visCount;
                    if (visCount > 10000000)
                    {                       
                        if (Monitor.TryEnter(visLock))
                        {
                            try
                            {
                                visCount = 0;
                                Console.CursorTop = 0;
                                Console.CursorLeft = 0;
                                curSim.Visualize();
                            }
                            finally
                            {
                                visCount /= 2;
                                Monitor.Exit(visLock);
                            }
                        }
                    }
                }

                //if (curSim.step > depthLimit)
                //    continue;

                //exit conditions - a blizzard has moved into us
                bool standingInAnEmptySpace = curSim.blizz.IsSpaceFree(curSim.step, curSim.player.pos, ref chars);
                if(!standingInAnEmptySpace)
                {
                    ThreadSafeSimStats.instance.discardedBranches++;
                    continue;
                }

                //add a new search branch for each possible move from this point...
                //up, down, left, right, wait
                //looking at what will be empty *next* step
                curSim.blizz.GetNearbyFreeSpaces(curSim.step + 1, ref curSim.player.pos, ref freeSpaces, ref chars);
                bool moved = false;
                for (int dir = 0; dir < (int)Dir.MAX_DIR + 1; dir++)
                {
                    if (dir != (int)Dir.MAX_DIR && freeSpaces[dir] == false)
                    {
                        continue;
                    }
                    else if(dir == (int)Dir.MAX_DIR)
                    {
                        //wait action..
                        bool isWaitValid = curSim.blizz.IsSpaceFree(curSim.step + 1, curSim.player.pos, ref chars);
                        if (!isWaitValid)
                            continue;

                        //if we are at the starting point longer than a single cycle of the X blizzards, we have wited too long there
                        if (curSim.step + 1 > curSim.maxBounds.X && curSim.player.pos == curSim.start)
                        {
                            ThreadSafeSimStats.instance.discardedBranches++;
                            continue;
                        }
                    }

                    moved = true;
                    //add a new sim 
                    Sim newSim = curSim.DeepCopy();

                    //make move ( or wait)
                    if (dir != (int)Dir.MAX_DIR)
                    {
                        //make move...
                        newSim.player.pos = GetAdjustPointByDir(ref newSim.player.pos, (Dir)dir);
                    }

                    //step
                    newSim.step++;


                    //get a sort score
                    //PERF: removing function call
                    //newSim.score = SimSortScore(ref newSim);
                    distVec = newSim.end - newSim.player.pos;
                    int distToEnd = (newSim.end.X + newSim.end.Y) - (newSim.player.pos.X + newSim.player.pos.Y);
                    if (distToEnd > (depthLimit - newSim.step)) //we cant make it there
                    {
                        ThreadSafeSimStats.instance.discardedBranches++;
                        continue;
                    }

                    if (distToEnd > ThreadSafeSimStats.instance.bestSimSteps - newSim.step)
                    {
                        //we cant make it there as well as the last
                        ThreadSafeSimStats.instance.discardedBranches++;
                        continue;
                    }

                    //lets try to early out here based on best score for a completion so far...
                    bool lessThanBest = ThreadSafeSimStats.instance.CheckBestPath(ref newSim, false);
                    if (!lessThanBest)
                    {
                        ThreadSafeSimStats.instance.discardedBranches++;
                        continue;
                    }

                    //if we are next to the end, we have arrived...
                    //the correct spot is one above the end pos
                    if (newSim.player.pos.X == newSim.end.X)
                    {
                        if (newSim.player.pos.Y == newSim.end.Y - 1)
                        {
                            //we arrived, or will in one more step.
                            //lets record this and exit this branch
                            newSim.step++;
                            ThreadSafeSimStats.instance.CheckBestPath(ref newSim, true);
                            ThreadSafeSimStats.instance.discardedBranches++;
                            continue;
                        }
                    }

                    //int dfs = -1 * newSim.step; //plain old DFS
                    newSim.score = distToEnd;// + newSim.step;

                    //lets try to prune ineffective paths , and treat this thing as a 3d map
                    if(ThreadSafeSimStats.instance.PrunePathBasedOnPruneMap(ref newSim, newSim.score))
                    {
                        ThreadSafeSimStats.instance.discardedBranches++;
                        continue;
                    }

                    if (ThreadSafeSimStats.instance.CanAddWork())
                    {
                        LinkedList<Sim> newSearch = new();
                        newSearch.AddLast(newSim);
                        ThreadSafeSimStats.instance.AddWork(() => { SearchTaskLIST(newSearch, depthLimit); });
                    }
                    else
                    {
                        LinkedListNode<Sim>? insertPoint = search.First;

                        for (; insertPoint != search.Last && insertPoint != null && insertPoint.Value.score < newSim.score;)
                        {
                            insertPoint = insertPoint.Next;
                        }

                        if (insertPoint != null)
                            search.AddBefore(insertPoint, newSim);
                        else
                            search.AddLast(newSim);
                    }
                }

                if(!moved)
                    ThreadSafeSimStats.instance.discardedBranches++;
            }
        }


        enum Dir
        {
            Up, Right, Down, Left, MAX_DIR
        }

        struct Player
        {
            public Point pos;
        }

        static Point GetAdjustPointByDir(ref Point p, Dir dir)
        {
            Point newp = p;
            switch (dir)
            {
                case Dir.Up:
                    newp.Y--;
                    break;
                case Dir.Right:
                    newp.X++;
                    break;
                case Dir.Down:
                    newp.Y++;
                    break;
                case Dir.Left:
                    newp.X--;
                    break;
            }

            return newp;
        }

        class Blizzards
        {
            //blizzards start at pos 1, so more off by one fun
            public string[] upBlizz;
            public string[] downBlizz;

            public string[] leftBlizz;
            public string[] rightBlizz;

            public Point maxBounds;

            public Blizzards(int xSize, int ySize)
            {
                leftBlizz = new string[ySize];
                rightBlizz = new string[ySize];

                upBlizz = new string[xSize];
                downBlizz = new string[xSize];

                maxBounds = new Point(xSize, ySize);
            }

            public void GetNearbyFreeSpaces(int timeStep, ref Point p, ref bool[] spaces, ref char[] chars)
            {
                //up
                spaces[(int)Dir.Up] = IsSpaceFree(timeStep, GetAdjustPointByDir(ref p, Dir.Up), ref chars);
                //right
                spaces[(int)Dir.Right] = IsSpaceFree(timeStep, GetAdjustPointByDir(ref p, Dir.Right), ref chars);
                //down
                spaces[(int)Dir.Down] = IsSpaceFree(timeStep, GetAdjustPointByDir(ref p, Dir.Down), ref chars);
                //left
                spaces[(int)Dir.Left] = IsSpaceFree(timeStep, GetAdjustPointByDir(ref p, Dir.Left), ref chars);
            }

            public bool IsSpaceFree(int timeStep, Point p, ref char[] chars)
            {
                //TODO: hard code start / finish /
                if (p.X == 0 && p.Y == -1)
                    return true;
                
                // hardcoded walls:
                if (p.Y < 0 || p.Y == maxBounds.Y)
                    return false;

                if (p.X < 0 || p.X == maxBounds.X)
                    return false;

                //see if stuff is free
                GetCharsAtStep(timeStep, p, ref chars);
                for(int i =0; i < chars.Length;++i)
                {
                    if (chars[i] != '.')
                        return false;
                }

                return true;
            }

            public char[] GetCharsAtStep(int timeStep, Point p, ref char[] chars)
            {
                //right and down
                int modStepX = (p.X - timeStep);
                int modStepY = (p.Y - timeStep);

                while (modStepX < 0)
                    modStepX += maxBounds.X;

                while (modStepY < 0)
                    modStepY += maxBounds.Y;

                chars[3] = rightBlizz[p.Y][modStepX];
                chars[1] = downBlizz[p.X][modStepY];

                //left and up require rotating the other way
                modStepX = (p.X + timeStep);// % maxBounds.X;
                modStepY = (p.Y + timeStep);// % maxBounds.Y;

                while (modStepX >= maxBounds.X)
                    modStepX -= maxBounds.X;

                while (modStepY >= maxBounds.Y)
                    modStepY -= maxBounds.Y;

                chars[0] = upBlizz[p.X][modStepY]; 
                chars[2] = leftBlizz[p.Y][modStepX];
                
                return chars;
            }
        }

        struct Sim
        {
            public Blizzards blizz;
            public Player player;
            public Point start;
            public Point end;
            public Point maxBounds;

            public static UInt64 NumCreatedSims;

            public int score;
            public int step;

            //for visualizing, loose this once when not debugging
            public List<Point> playerPath = new(500);

            public Sim(Blizzards blz)
            {
                blizz = blz;
            }

            public Sim DeepCopy()
            {
                Sim newSim = this;

                //TODO: get rid of this when not debugging
                if (playerPath.Count != 0)
                {
                    newSim.playerPath = new(500);
                    newSim.playerPath.AddRange(playerPath);
                }

                NumCreatedSims++;
                return newSim;
            }

            public void Visualize()
            {
                char[] chars = new char[4];
                for (int y = 0; y < maxBounds.Y; y++)
                {
                    for (int x = 0; x < maxBounds.X; x++)
                    {
                        if (player.pos == new Point(x, y))
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write("E");
                            continue;
                        }
                        if (playerPath.Contains(new Point(x, y)))
                        {
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.Write("e");
                            continue;
                        }
                        blizz.GetCharsAtStep(step, new Point(x, y), ref chars);
                        int nonEmpty = 0;
                        char lastNonEmpty = 'X';
                        for (int i = 0; i < chars.Length; ++i)
                        {
                            if (chars[i] != '.')
                            {
                                lastNonEmpty = chars[i];
                                ++nonEmpty;
                            }
                        }

                        if (nonEmpty == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.Write(".");
                        }
                        else if (nonEmpty == 1)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.Write(lastNonEmpty);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.Write(nonEmpty);
                        }


                    }
                    Console.WriteLine();
                }

                Console.ForegroundColor= ConsoleColor.White;
                Console.WriteLine("Step: " + step + "\n\n");
            }
        }




        class ThreadSafeSimStats
        {
            public static ThreadSafeSimStats instance;

            public bool DoVisualization = false;
            public bool DoPathVisualization = false;

            public List<Task> crunchingTasks = new();
            public int taskLimit = 10;
            int threadLimit = 10;
            public readonly int mainThreadId;
            object taskListLockObj = new();

            public Sim bestSim = new();
            public int bestSimSteps = 9999999;
            object maxLockObj = new();

            public object pruneMaxLockObj = new();
            class ScoreHolder
            {
                public int score;
            }
            SortedDictionary<(Point, int step), ScoreHolder?> maxScorePruneMap = new();

            public UInt64 discardedBranches = 0;
            public bool Part2 = false;

            LimitedConcurrencyLevelTaskScheduler scheduler;
            CancellationToken cancelToken;

            public ThreadSafeSimStats(int _taskLimit, bool part2)
            {
                taskLimit = _taskLimit;
                threadLimit = taskLimit;
                Part2 = part2;
                mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                scheduler = new LimitedConcurrencyLevelTaskScheduler(Math.Max(2, threadLimit));
            }

            public bool PrunePathBasedOnPruneMap(ref Sim curSim, int simScore)
            {
                lock(pruneMaxLockObj)
                {
                    if(maxScorePruneMap.TryGetValue((curSim.player.pos, curSim.step), out ScoreHolder? score))
                    {
                        if (simScore <= score.score)
                            return true;
                        score.score = simScore;
                    }
                    else
                    {
                        maxScorePruneMap.Add((curSim.player.pos, curSim.step), new ScoreHolder() { score = simScore });
                    }
                }

                return false;
            }

            public bool CheckBestPath(ref Sim curSim, bool updateScore)
            {
                //hack to quickly exit out if its not even close..no need to lock anything
                if (curSim.step >= bestSimSteps)
                    return false;

                //exit out if we're jsut checking ourselves against the max
                if (!updateScore)
                    return true;

                lock (maxLockObj)
                {
                    if (curSim.step < bestSimSteps)
                    {
                        bestSim = curSim;
                        bestSimSteps = curSim.step;
                        Console.WriteLine(String.Format("Found new best path: {0}", bestSim.step));
                        return true;
                    }
                }
                return false;
            }

            public bool CanAddWork()
            {
                if (taskLimit == 0)
                    return false;

                if (Thread.CurrentThread.ManagedThreadId == mainThreadId)
                    return true;

                //somtimes allows mroe tasks than we want...but avoids locking
                if (crunchingTasks.Count < taskLimit)
                    return true;

                return false;

            }

            public void AddWork(Action work)
            {
                lock (taskListLockObj)
                {
                    Task t = Task.Factory.StartNew(work, cancelToken, TaskCreationOptions.LongRunning, scheduler);
                    crunchingTasks.Add(t);
                }
            }

            public void ClearFinishedWork()
            {
                lock (taskListLockObj)
                {
                    for (int i = 0; i < crunchingTasks.Count; i++)
                    {
                        if (crunchingTasks[i].Exception != null)
                        {
                            Console.WriteLine("EXCEPTION THROWN IN TASK " + crunchingTasks[i].Id + " : " + crunchingTasks[i].Exception.InnerException.ToString());
                            Debug.WriteLine("EXCEPTION THROWN IN TASK " + crunchingTasks[i].Id + " : " + crunchingTasks[i].Exception.InnerException.ToString());
                        }
                        if (crunchingTasks[i].IsCompleted)
                        {
                            crunchingTasks.RemoveAt(i);
                            --i;
                        }
                    }
                }
            }

        }//threadsafesimstats



    }//day24


}
