using ConsoleApp1.Utilities;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Numerics;
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

            ThreadSafeSimStats.instance = new(10, part2);

            /*
            //testing visualization
            for (int stepNum = 0; stepNum < 10; stepNum++)
            {
                sim.step++;
                sim.Visualize();
            }*/


            LinkedList<Sim> search = new();
            search.AddLast(sim);
            int depthLimit = 100;

            Stopwatch watch = new();
            watch.Start();

            //well, lets search. 
            SearchTaskLIST(search, depthLimit);

            //wait for tasks
            Console.WriteLine("\n----- waiting for tasks -------");
            for (; ; )
            {
                Thread.Sleep(5000);
                Console.WriteLine((watch.ElapsedMilliseconds) + " ms :Crunching  with: " + ThreadSafeSimStats.instance.crunchingTasks.Count() + " tasks, current simId: " + Sim.NumCreatedSims + " number discarded: " + ThreadSafeSimStats.instance.discardedBranches);
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


        //score this sim for insertion order...
        //low score is better
        static int SimSortScore(ref Sim sim)
        {
            Point dist = sim.end - sim.player.pos;
            return ( 10000 * (dist.X + dist.Y)) + sim.step;
        }


        List<Point> ex2_positions_by_step = new() { new Point(1,1), new Point(1,2), new Point(1,2), new Point(1,1),
                                            new Point(2, 1),new Point(3, 1),new Point(3, 2),new Point(2, 2), new Point(2, 1),
                                            new Point(3, 1),new Point(3, 1),new Point(3, 2),new Point(3, 3), new Point(4, 3),
                                            new Point(5, 3),new Point(6, 3),new Point(6, 4),new Point(6, 7),};

        void SearchTaskLIST(LinkedList<Sim> search, int depthLimit)
        {
            while (search.Count > 0)
            {
                Sim curSim = search.First.Value;
                search.RemoveFirst();                

                if (curSim.step > depthLimit)
                    continue;

                //if we are next to the end, we have arrived...
                //the correct spot is one above the end pos
                if(curSim.player.pos.X == curSim.end.X)
                {
                    if(curSim.player.pos.Y == curSim.end.Y - 1)
                    {
                        //we arrived, or will in one more step.
                        //lets record this and exit this branch
                        curSim.step++;
                        ThreadSafeSimStats.instance.CheckMax(ref curSim, true);
                        ThreadSafeSimStats.instance.discardedBranches++;
                        continue;
                    }
                }

                //check against ex2
                /*
                if (curSim.step >= ex2_positions_by_step.Count)
                    continue; //this branch is obv. not gonna work

                if (curSim.step > 0)
                {
                    Point ex2best = ex2_positions_by_step[curSim.step - 1];
                    ex2best.X--;
                    ex2best.Y--;
                    if (curSim.player.pos == ex2best)
                    {
                        curSim.Visualize();
                        int x = 0;
                        x++;
                    }
                }*/

                //exit conditions - a blizzard has moved into us
                bool standingInAnEmptySpace = curSim.blizz.IsSpaceFree(curSim.step, curSim.player.pos);
                if(!standingInAnEmptySpace)
                {
                    ThreadSafeSimStats.instance.discardedBranches++;
                    continue;
                }

                //lets try to early out here based on best score for a completion so far...
                bool lessThanBest = ThreadSafeSimStats.instance.CheckMax(ref curSim, false);
                if(!lessThanBest)
                {
                    ThreadSafeSimStats.instance.discardedBranches++;
                    continue;
                }

                //add a new search branch for each possible move from this point...
                //up, down, left, right, wait
                //looking at what will be empty *next* step
                bool[] freeSpaces = curSim.blizz.GetNearbyFreeSpaces(curSim.step + 1, ref curSim.player.pos);

                for (int dir = 0; dir < (int)Dir.MAX_DIR + 1; dir++)
                {
                    if (dir != (int)Dir.MAX_DIR && freeSpaces[dir] == false)
                    {
                        continue;
                    }
                    else if(dir == (int)Dir.MAX_DIR)
                    {
                        //wait action..
                        bool isWaitValid = curSim.blizz.IsSpaceFree(curSim.step + 1, curSim.player.pos);
                        if (!isWaitValid)
                            continue;
                    }

                    //add a new sim 
                    Sim newSim = curSim.DeepCopy();

                    //make move ( or wait)
                    if(dir != (int)Dir.MAX_DIR)
                    {
                        //make move...
                        newSim.player.pos = GetAdjustPointByDir(ref newSim.player.pos, (Dir)dir);
                    }

                    //get a sort score
                    newSim.score = SimSortScore(ref newSim);

                    //step
                    newSim.step++;

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

            public bool[] GetNearbyFreeSpaces(int timeStep, ref Point p)
            {
                bool[] spaces = new bool[4];
                //up
                spaces[(int)Dir.Up] = IsSpaceFree(timeStep, GetAdjustPointByDir(ref p, Dir.Up));
                //right
                spaces[(int)Dir.Right] = IsSpaceFree(timeStep, GetAdjustPointByDir(ref p, Dir.Right));
                //down
                spaces[(int)Dir.Down] = IsSpaceFree(timeStep, GetAdjustPointByDir(ref p, Dir.Down));
                //left
                spaces[(int)Dir.Left] = IsSpaceFree(timeStep, GetAdjustPointByDir(ref p, Dir.Left));

                return spaces;
            }

            public bool IsSpaceFree(int timeStep, Point p)
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
                char[] chars = GetCharsAtStep(timeStep, p);
                for(int i =0; i < chars.Length;++i)
                {
                    if (chars[i] != '.')
                        return false;
                }

                return true;
            }

            public char[] GetCharsAtStep(int timeStep, Point p)
            {
                char[] chars = new char[4];

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

            public static Int64 NumCreatedSims;

            public int score;
            public int step;

            public Sim(Blizzards blz)
            {
                blizz = blz;
            }

            public Sim DeepCopy()
            {
                Sim newSim = this;
                NumCreatedSims++;
                return newSim;
            }

            public void Visualize()
            {
                for(int y = 0; y < maxBounds.Y; y++)
                {
                    for (int x = 0; x < maxBounds.X; x++)
                    {
                        if(player.pos == new Point(x, y))
                        {
                            Console.Write("E");
                            continue;
                        }
                        char[] chars = blizz.GetCharsAtStep(step, new Point(x, y));
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
                            Console.Write('.');
                        else if (nonEmpty == 1)
                            Console.Write(lastNonEmpty);
                        else
                            Console.Write(nonEmpty.ToString());


                    }
                    Console.WriteLine();
                }

                Console.WriteLine("Step: " + step + "\n\n");
            }
        }




        class ThreadSafeSimStats
        {
            public static ThreadSafeSimStats instance;


            public List<Task> crunchingTasks = new();
            public int taskLimit = 10;
            int threadLimit = 10;
            public readonly int mainThreadId;
            object taskListLockObj = new();

            public Sim bestSim = new();
            public int bestSimSteps = 9999999;
            object maxLockObj = new();

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
                scheduler = new LimitedConcurrencyLevelTaskScheduler(threadLimit);
            }

            public bool CheckMax(ref Sim curSim, bool updateScore)
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
