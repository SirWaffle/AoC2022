using ConsoleApp1.Utilities;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using static ConsoleApp1.Solutions.Day16;
using static ConsoleApp1.Utils;

namespace ConsoleApp1.Solutions
{
    internal class Day19 : AbstractPuzzle
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
            List<Blueprint> blueprints = new();

            //parse...
            var lines = File.ReadAllText(InputFile!).Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
            foreach (var line in lines)
            {
                //Blueprint 1: Each ore robot costs 4 ore. Each clay robot costs 2 ore. Each obsidian robot costs 3 ore and 14 clay. Each geode robot costs 2 ore and 7 obsidian.
                // 1:  ore   4 ore:  clay   2 ore:  obsidian   3 ore and 14 clay:  geode   2 ore and 7 obsidian:
                //lazy
                string ln = line.Replace("Each", "").Replace("costs", "").Replace("robot", "").Replace("Blueprint", "").Replace(".", ":");
                string[] splits = ln.Split(":", StringSplitOptions.RemoveEmptyEntries);

                Blueprint bp = new();
                bp.entries = new RobotEntry[0];
                bp.num = int.Parse(splits[0].Trim());

                int[] maxElements = new int[ELEMENT_COUNT];
                int[] sumElements = new int[ELEMENT_COUNT];

                for (int i = 1; i < splits.Length; i++)
                {
                    RobotEntry entry = new();
                    entry.cost = new Cost[0];
                    string[] entrySplits = splits[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);

                    entry.type = StringToElement(entrySplits[0].Trim().ToLower());

                    Cost cost = new();
                    for (int j = 1; j < entrySplits.Length;)
                    {
                        if (entrySplits[j] == "and")
                        {
                            ++j;
                            cost = new();
                        }
                        cost.amount = int.Parse(entrySplits[j++].Trim());
                        cost.type = StringToElement(entrySplits[j++].Trim().ToLower());

                        Array.Resize(ref entry.cost, entry.cost.Length + 1);
                        entry.cost[entry.cost.Length - 1] = cost;

                        sumElements[(int)cost.type] += cost.amount;
                        maxElements[(int)cost.type] = Math.Max(cost.amount, maxElements[(int)cost.type]);
                    }

                    Array.Resize(ref bp.entries, bp.entries.Length + 1);
                    bp.entries[bp.entries.Length - 1] = entry;
                }

                bp.costSum = sumElements;
                bp.costMax = maxElements;
                blueprints.Add(bp);
            }


            List<Sim> simPerBluePrint = new();

            //find optimal
            int bluprintsToSearchMax = blueprints.Count;
            if (part2)
            {
                bluprintsToSearchMax = Math.Min(3, blueprints.Count);
            }

            Stopwatch watch = new();
            watch.Start();

            for (int bpInd = 0; bpInd < bluprintsToSearchMax; bpInd++)
            {

                ThreadSafeSimStats.instance = new(part2);

                int searchDept = 24;
                if (part2 == true)
                    searchDept = 32;

                CrunchBlueprint(bpInd, blueprints[bpInd], searchDept);

                //wait for tasks
                Console.WriteLine("\n----- waiting for tasks -------");
                for (; ; )
                {
                    Console.WriteLine((watch.ElapsedMilliseconds) + " ms :Crunching bp " + (bpInd + 1) + " with: " + ThreadSafeSimStats.instance.crunchingTasks.Count() + " tasks, current simId: " + Sim.simId + " number discarded: " + ThreadSafeSimStats.instance.discardedBranches);
                    ThreadSafeSimStats.instance.ClearFInishedWork();
                    if (Task.WaitAll(ThreadSafeSimStats.instance.crunchingTasks.ToArray(), 5000))
                    {
                        Sim.simId = 0;
                        ThreadSafeSimStats.instance.ClearFInishedWork();
                        //done
                        break;
                    }
                }

                simPerBluePrint.Add(ThreadSafeSimStats.instance.maxGeodesSim);
                LogSimState(ref ThreadSafeSimStats.instance.maxGeodesSim);
            }


            Console.WriteLine("\n----- Finished Waiting -------");

            int sum = 0;
            for (int i = 0; i < bluprintsToSearchMax; i++)
            {
                if (part2 == false)
                {
                    sum += ((i + 1) * simPerBluePrint[i].oreCounts[(int)ElementType.Geode]);
                }
                else
                {
                    sum *= simPerBluePrint[i].oreCounts[(int)ElementType.Geode];
                }
                Console.WriteLine("Blueprint " + i + " max geodes = " + simPerBluePrint[i].oreCounts[(int)ElementType.Geode]);
            }

            Console.WriteLine("Score: " + sum);

            //be surei dont accidentaly fat finger some keys and close the console after this thing finishes, ha
            _ = Console.ReadLine();
            _ = Console.ReadLine();
            _ = Console.ReadLine();
            _ = Console.ReadLine();
        }


        public enum ElementType
        {
            Ore,
            Clay,
            Obsidian,
            Geode
        }

        static int ELEMENT_COUNT = 4;

        public ElementType StringToElement(string elem)
        {
            if (elem.ToLower() == "ore")
                return ElementType.Ore;

            if (elem.ToLower() == "clay")
                return ElementType.Clay;

            if (elem.ToLower() == "obsidian")
                return ElementType.Obsidian;

            if (elem.ToLower() == "geode")
                return ElementType.Geode;

            throw new Exception("missed one");
        }

        struct Cost
        {
            public int amount;
            public ElementType type;
        }

        struct RobotEntry
        {
            public ElementType type;
            public Cost[] cost;
        }

        struct Blueprint
        {
            public int num;
            public RobotEntry[] entries;

            public int[] costMax = new int[ELEMENT_COUNT];
            public int[] costSum = new int[ELEMENT_COUNT];

            public Blueprint() { }
        }

        struct Sim
        {
            public Blueprint blueprint;

            public bool hasPendingBot = false;
            public ElementType pendingBotType;
            public int[] botsByType = new int[ELEMENT_COUNT];
            public int[] oreCounts = new int[ELEMENT_COUNT];

            public int currentScore;
            public int time;
            public int simNum;
            public UInt64 simID;


            public static UInt64 simId = 1;

            public Sim()
            { }

            public Sim DeepCopy()
            {
                Sim sim = new(); //pool this?
                sim.blueprint = blueprint;

                sim.hasPendingBot = hasPendingBot;
                sim.pendingBotType = pendingBotType;

                oreCounts.CopyTo(sim.oreCounts, 0);
                botsByType.CopyTo(sim.botsByType, 0);

                sim.time = time;
                sim.simNum= simNum;
                sim.simID = simId++;
                return sim;
            }
        }


        //is this thing being *copied* uniquely to different threads? what is going on here...
        class ThreadSafeSimStats
        {
            public static ThreadSafeSimStats instance;

            public Sim maxGeodesSim = new();
            public int maxGeodes = 0;
            object maxLockObj = new();


            public List<Task> crunchingTasks = new();
            public int taskLimit = 10;
            int threadLimit = 10;
            public readonly int mainThreadId;
            object taskListLockObj = new();

            object maxPerDepthLock = new();
            int[] maxFoundPerDepth= new int[36];

            public UInt64 discardedBranches = 0;
            public bool Part2 = false;

            LimitedConcurrencyLevelTaskScheduler scheduler;
            CancellationToken cancelToken;

            public ThreadSafeSimStats(bool part2)
            {
                Part2 = part2;
                mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                scheduler = new LimitedConcurrencyLevelTaskScheduler(threadLimit);
            }

            public bool CanAddWork()
            {
                if (Thread.CurrentThread.ManagedThreadId == mainThreadId)
                    return true;

                //somtimes allows mroe tasks than we want...but avoids locking
                if (crunchingTasks.Count < taskLimit)
                    return true;
                /*
                lock (taskListLockObj)
                {
                    if (crunchingTasks.Count < taskLimit)
                        return true;
                }*/

                return false;

            }

            public void AddWork(Action work)
            {
                lock (taskListLockObj)
                {
                    Task t = Task.Factory.StartNew(work, cancelToken, TaskCreationOptions.LongRunning, scheduler);
                    //Console.WriteLine("Task count: " + crunchingTasks.Count + ". Task added: " + t.Id);
                    crunchingTasks.Add(t);
                }
            }

            public void ClearFInishedWork()
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
                            //Console.WriteLine("Task count: " + crunchingTasks.Count + ". Task completed: " + crunchingTasks[i].Id);
                            crunchingTasks.RemoveAt(i);
                            --i;
                        }
                    }
                }
            }

            public bool CheckAgainstMaxDepthScores(ref Sim sim, int depth, int depthLimit)
            {
                //if we are near the end with no bots, just exit, we're heading towards 0 anyways
                if (depth > depthLimit - 1 && sim.botsByType[(int)ElementType.Geode] == 0)
                    return false;


                int combinedScore = 0;

                //TODO: this seems to need to change for part2, maybe its just plain wrong
                combinedScore = sim.oreCounts[(int)ElementType.Geode];

                //prube based on *past* scores, to see how far ahead we can search when belkow the max
                int pastDepthCheck = 0;
                if (Part2)
                    pastDepthCheck = 3;

                if (depth >= pastDepthCheck)
                {
                    if (combinedScore < maxFoundPerDepth[depth - pastDepthCheck])
                        return false;
                }
                else if(depth >= depthLimit - pastDepthCheck)
                {
                    if (combinedScore < maxFoundPerDepth[depth])
                        return false;
                }

                lock (maxLockObj)
                {
                    if (combinedScore > maxFoundPerDepth[depth])
                    {
                        maxFoundPerDepth[depth] = combinedScore;
                        Console.Write("depth max reached: " + depth + "   max: " + maxFoundPerDepth[depth]);
                        Console.WriteLine(String.Format(", in sim {0} branch {1} with bp num {2},  geodes {3}, geobots {4}",
                                sim.simNum, sim.simID, sim.blueprint.num, sim.oreCounts[(int)ElementType.Geode], sim.botsByType[(int)ElementType.Geode]));
                    }
                }

                return true; //even or so
            }

            public bool CheckMax(ref Sim curSim)
            {
                int curGeodes = curSim.oreCounts[(int)ElementType.Geode];

                //hack to quickly exit out if its not even close..no need to lock anything
                if (curGeodes < maxGeodes)
                    return false;

                lock (maxLockObj)
                {
                    if (curGeodes > maxGeodes)
                    {
                        maxGeodesSim = curSim;
                        maxGeodes = curGeodes;
                        Console.WriteLine(String.Format("*MAX GEODES* New mx geode count found in sim {0} branch {1} with blueprint num {2} with geodes {3} at depth {4}",
                                                        maxGeodesSim.simNum, maxGeodesSim.simID, maxGeodesSim.blueprint.num, maxGeodesSim.oreCounts[(int)ElementType.Geode], curSim.time));
                        return true;
                    }
                }
                return false;
            }

        }

        void CrunchBlueprint(int simNum, Blueprint bp, int searchDepth)
        {
            Sim startSim = new();
            startSim.blueprint = bp;
            startSim.simNum = simNum;

            //we start with 1 ore robot
            startSim.botsByType[(int)ElementType.Ore] = 1;

            startSim.time = 1;

            LinkedList<Sim> search = new();
            search.AddLast(startSim);
            int timeLimit = searchDepth;

            //well, lets search. 
            SearchTaskLIST(search, timeLimit);
        }

        //score this sim for insertion order...
        static int SimSortScore(Sim sim)
        {
            int score =  50000 * (sim.botsByType[(int)ElementType.Geode] + sim.oreCounts[(int)ElementType.Geode]);

            //now to a lesser degree, add score for bots that can buils the geode bot
            int variation = 0;
            foreach (var cost in sim.blueprint.entries[(int)ElementType.Geode].cost)
            {
                variation++;
                score += -1000 + ((100 * (sim.botsByType[(int)cost.type] * cost.amount)) * ((int)cost.type + 1));

                //now add slight score for *each of those costs*
                foreach(var cost2 in sim.blueprint.entries[(int)cost.type].cost)
                {
                    score += -100 + ((1 * (sim.botsByType[(int)cost2.type] * cost.amount)) * ((int)cost.type + 1));
                }
            }

            return score;
        }

        void SearchTaskLIST(LinkedList<Sim> search, int depthLimit)
        {
            while (search.Count > 0)
            {
                Sim curSim = search.First.Value;
                search.RemoveFirst();
                curSim.time += 1;

                //collect
                for (int i = 0; i < ELEMENT_COUNT; i++)
                    curSim.oreCounts[i] += curSim.botsByType[i];

                //add pending bot to our counts
                if (curSim.hasPendingBot)
                {
                    curSim.hasPendingBot = false;
                    curSim.botsByType[(int)curSim.pendingBotType]++;
                }

                //prune brqanches by comparing against best scores
                if (!ThreadSafeSimStats.instance.CheckAgainstMaxDepthScores(ref curSim, curSim.time, depthLimit))
                {
                    //we're done, probably
                    ThreadSafeSimStats.instance.discardedBranches++;
                    continue;
                }

                if (curSim.time > 10 && curSim.oreCounts[(int)ElementType.Geode] > 0) //timeLimit
                {
                    _ = ThreadSafeSimStats.instance.CheckMax(ref curSim);
                }

                if (curSim.time > depthLimit)
                {
                    //end of the line
                    continue;
                }

                //find new robots to construct...
                for (int i = 0; i < curSim.blueprint.entries.Length; i++)
                {
                    //TODO: testing this
                    if (curSim.blueprint.entries[i].type != ElementType.Geode)
                    {
                        if (curSim.botsByType[(int)curSim.blueprint.entries[i].type] > curSim.blueprint.costMax[(int)curSim.blueprint.entries[i].type])
                            continue;
                    }

                    //see if we have the resources to build this
                    bool passedCostCheck = true;
                    foreach (var cost in curSim.blueprint.entries[i].cost)
                    {
                        if (curSim.oreCounts[(int)cost.type] < cost.amount)
                        {
                            passedCostCheck = false;
                            break;
                        }
                    }

                    if (passedCostCheck)
                    {
                        //add a new sim for each possible build...
                        Sim newSim = curSim.DeepCopy();

                        //do the build
                        foreach (var cost in newSim.blueprint.entries[i].cost)
                            newSim.oreCounts[(int)cost.type] -= cost.amount;

                        //add the robot as a new active robot, starting construction
                        newSim.pendingBotType = newSim.blueprint.entries[i].type;
                        newSim.hasPendingBot = true;

                        //get a sort score
                        newSim.currentScore = SimSortScore(newSim);

                        if ( ThreadSafeSimStats.instance.CanAddWork() )
                        {
                            LinkedList<Sim> newSearch = new();
                            newSearch.AddLast(newSim);
                            ThreadSafeSimStats.instance.AddWork(() => { SearchTaskLIST(newSearch, depthLimit); });
                        }
                        else
                        {
                            LinkedListNode<Sim>? insertPoint = search.First;

                            for (; insertPoint != search.Last && insertPoint != null && insertPoint.Value.currentScore > newSim.currentScore;)
                            {
                                insertPoint = insertPoint.Next;
                            }

                            if (insertPoint != null)
                                search.AddBefore(insertPoint, newSim);
                            else
                                search.AddLast(newSim);
                        }
                    } //passed cost check
                }


                //shit..might have to let this one build to completion...because no build is a choice
                //gonna have to reduce this...shiat
                curSim.currentScore = SimSortScore(curSim);

                if ( ThreadSafeSimStats.instance.CanAddWork() )
                {
                    LinkedList<Sim> newSearch = new();
                    newSearch.AddLast(curSim);
                    ThreadSafeSimStats.instance.AddWork(() => { SearchTaskLIST(newSearch, depthLimit); });
                }
                else
                {
                    LinkedListNode<Sim>? insertPoint = search.First;

                    for (; insertPoint != search.Last && insertPoint != null && insertPoint.Value.currentScore > curSim.currentScore;)
                    {
                        insertPoint = insertPoint.Next;
                    }

                    if (insertPoint != null)
                        search.AddBefore(insertPoint, curSim);
                    else
                        search.AddLast(curSim);

                }
            }
        }



        void LogSimState(ref Sim sim)
        {
            Console.WriteLine("\n----- Thread: " + Thread.CurrentThread.ManagedThreadId + " --- Sim " + sim.simNum + " branch " + sim.simID + " at time " + sim.time + " -------");
            for(int i = 0; i < ELEMENT_COUNT; ++i)
                Console.WriteLine("Have: " + ((ElementType)(i)).ToString() + ": " + sim.oreCounts[i]);

            for (int i = 0; i < ELEMENT_COUNT; ++i)
                Console.WriteLine("Bot count: " + ((ElementType)(i)).ToString() + ": " + sim.botsByType[i]);
        }


        List<int> GatherAvailableBuilds(ref Sim sim)
        {
            List<int> potentials = new(sim.blueprint.entries.Length);

            for(int i =0; i < sim.blueprint.entries.Length; i++)
            {
                //TODO: testing this
                if (sim.blueprint.entries[i].type != ElementType.Geode)
                {
                    if (sim.botsByType[(int)sim.blueprint.entries[i].type] > sim.blueprint.costMax[(int)sim.blueprint.entries[i].type])
                        continue;
                }

                //see if we have the resources to build this
                bool passedCostCheck = true;
                foreach(var cost in sim.blueprint.entries[i].cost)
                {
                    if (sim.oreCounts[(int)cost.type] < cost.amount)
                    {
                        passedCostCheck = false;
                        break;
                    }
                }

                if(passedCostCheck)
                {
                    potentials.Add(i);
                }
            }

            return potentials;
        }



     



    }
}
