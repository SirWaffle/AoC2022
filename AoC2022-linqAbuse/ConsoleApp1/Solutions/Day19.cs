using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
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
            public List<Cost> cost;
        }

        struct Blueprint
        {
            public int num;
            public List<RobotEntry> entries;

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
            public List<int> bestScoresByStep = new(new int[36]);

            object maxLockObj = new();

            public List<Task> crunchingTasks = new();

            int curCoreAffinity = 1;
            int curThread = 1;
            public int threadLimit = 7;

            public readonly int mainThreadId;

            object taskListLockObj = new();

            object maxPerDepthLock = new();
            int[] maxFoundPerDepth= new int[36];
            int totalMaxFound = 0;

            public UInt64 discardedBranches = 0;
            public bool Part2 = false;

            public ThreadSafeSimStats(bool part2)
            {
                Part2 = part2;
                mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            }

            public ThreadSafeSimStats()
            {
                mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            }
            
            public bool CheckAgainstMax(ref Sim sim, int depth, int depthLimit, out int score)
            {
                //if we are at depth 24 with no bots, just exit, we're heading towards 0 anyways
                if(Part2 == true)
                {
                    if (depth >= depthLimit - 1 && sim.botsByType[(int)ElementType.Geode] == 0)
                    {
                        score = 0;
                        return false;
                    }
                }
                else if (depth >= 24 && sim.botsByType[(int)ElementType.Geode] == 0)
                {
                    score = 0;
                    return false;
                }

                int combinedScore = 0;


                //TODO: this seems to need to change for part2
                for (int i = (int)ElementType.Geode; i < ELEMENT_COUNT; ++i)
                {
                    //score it
                    int remainingTime = (depthLimit - depth);
                    int possibleBots = sim.botsByType[i];
                    int possibleBotsGeodes = 0;
                    for (int rt = 0; rt <= remainingTime; rt++)
                    {                        
                        possibleBotsGeodes += possibleBots;
                        possibleBots++;
                    }
                    int possibleTotalFull = possibleBotsGeodes;

                    combinedScore = sim.oreCounts[i] + possibleTotalFull;
                    if(Part2)
                    {
                        combinedScore = sim.oreCounts[i] + (possibleTotalFull);
                    }
                }

                score = combinedScore;

                if (Part2)
                {
                    if (score < maxFoundPerDepth[depth] - 10)
                        return false;
                }
                else
                {
                    if (score < maxFoundPerDepth[depth])
                        return false;
                }

                lock (maxLockObj)
                {
                    if (combinedScore > maxFoundPerDepth[depth])
                    {
                        maxFoundPerDepth[depth] = combinedScore;
                        Console.Write("depth max reached: " + depth + "   max: " +  maxFoundPerDepth[depth]);
                        Console.WriteLine(String.Format(", in sim {0} branch {1} with bp num {2},  geodes {3}, geobots {4}",
                                sim.simNum, sim.simID, sim.blueprint.num, sim.oreCounts[(int)ElementType.Geode], sim.botsByType[(int)ElementType.Geode]));
                    }
                }

                return true; //even or so
            }

            public void AddWork(Task work)
            {
                lock (taskListLockObj)
                {
                    crunchingTasks.Add(work);
                }
            }

            public void ClearFInishedWork()
            {
                lock (taskListLockObj)
                {
                    for(int i =0; i < crunchingTasks.Count; i++)
                    {
                        if (crunchingTasks[i].IsCompleted)
                        {
                            crunchingTasks.RemoveAt(i);
                            --i;
                        }
                    }
                }
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
                        //TODO concurrency safety, good ol semaphore slim
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
                bp.entries = new();
                bp.num = int.Parse(splits[0].Trim());

                int[] maxElements = new int[ELEMENT_COUNT];
                int[] sumElements = new int[ELEMENT_COUNT];

                for (int i = 1; i < splits.Length; i++)
                {
                    RobotEntry entry = new();
                    entry.cost = new List<Cost>();
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
                        entry.cost.Add(cost);

                        sumElements[(int)cost.type] += cost.amount;
                        maxElements[(int)cost.type] = Math.Max(cost.amount, maxElements[(int)cost.type]);
                    }

                    bp.entries.Add(entry);
                }

                bp.costSum = sumElements;
                bp.costMax = maxElements;
                blueprints.Add(bp);
            }


            List<Sim> simPerBluePrint = new();

            //find optimal
            int bluprintsToSearchMax = blueprints.Count;
            if(part2)
            {
                bluprintsToSearchMax = Math.Min(3, blueprints.Count);
            }

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
                    Thread.Sleep(10000);
                    ThreadSafeSimStats.instance.ClearFInishedWork();
                    Console.WriteLine("Crunching bp " + bpInd + " with: " + ThreadSafeSimStats.instance.crunchingTasks.Count() + " threads, current simId: " + Sim.simId + " number discarded: " + ThreadSafeSimStats.instance.discardedBranches);
                    if (Task.WaitAll(ThreadSafeSimStats.instance.crunchingTasks.ToArray(), 10))
                    {
                        Sim.simId = 0;
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
            int maxSearchedDepth = 0;

            SearchTaskLIST(search, timeLimit, maxSearchedDepth);
        }

        //score this sim for insertion order...
        static int highestSimScore = 0;
        static int SimSortScore(Sim sim)
        {
            int score =  50000 * (sim.botsByType[(int)ElementType.Geode] + sim.oreCounts[(int)ElementType.Geode]);

            if(sim.botsByType[(int)ElementType.Geode] != 0)
            {
                int x = 0;
                ++x;
            }

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

            if(score > highestSimScore)
            {
                highestSimScore = score;
            }
            return score;
        }

        void SearchTaskLIST(LinkedList<Sim> search, int depthLimit, int maxSearchedDepth)
        { 
            while (search.Count > 0)
            {                
                Sim curSim = search.First();
                search.RemoveFirst();
                               
                //collect and progress builds
                StepRobots(ref curSim);

                curSim.time += 1;

                //maybe this part is wrong?
                int score = 0;
                bool onTarget = ThreadSafeSimStats.instance.CheckAgainstMax(ref curSim, curSim.time, depthLimit, out score);
                
                if (!onTarget)
                {
                    //we're done, probably
                    ThreadSafeSimStats.instance.discardedBranches++;
                    continue;
                }

                if (curSim.time > maxSearchedDepth)
                {
                    maxSearchedDepth = curSim.time;
                    //Console.WriteLine("new depth reached: " + maxSearchedDepth);
                    //LogSimState(curSim);
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
                var potentials = GatherAvailableBuilds(ref curSim);

                if (potentials.Count > 0)
                {
                    //add a new sim for each possible build...
                    for(int i = potentials.Count - 1; i >= 0; --i)
                    {
                        Sim newSim = curSim.DeepCopy();
                        StepBuild(ref newSim, potentials[i]);

                        newSim.currentScore = SimSortScore(newSim);


                        if (Thread.CurrentThread.ManagedThreadId == ThreadSafeSimStats.instance.mainThreadId 
                            || ThreadSafeSimStats.instance.threadLimit >= ThreadSafeSimStats.instance.crunchingTasks.Count)
                        {
                            LinkedList<Sim> newSearch = new();
                            newSearch.AddLast(newSim);
                            Task t = Task.Factory.StartNew(
                                () => { SearchTaskLIST(newSearch, depthLimit, maxSearchedDepth); } 
                                , TaskCreationOptions.LongRunning);
                            ThreadSafeSimStats.instance.AddWork(t);
                        }
                        else
                        {
                            LinkedListNode<Sim>? insertPoint = search.First;

                            //for (; insertPoint != search.Last && insertPoint != null && insertPoint.Value.botsByType[(int)ElementType.Geode] > newSim.botsByType[(int)ElementType.Geode];)
                            for (; insertPoint != search.Last && insertPoint != null && insertPoint.Value.currentScore > newSim.currentScore;)
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

                //shit..might have to let this one build to completion...because no build is a choice
                //gonna have to reduce this...shiat
                curSim.currentScore = SimSortScore(curSim);

                if (Thread.CurrentThread.ManagedThreadId == ThreadSafeSimStats.instance.mainThreadId 
                    || ThreadSafeSimStats.instance.threadLimit >= ThreadSafeSimStats.instance.crunchingTasks.Count )
                {
                    LinkedList<Sim> newSearch = new();
                    newSearch.AddLast(curSim);
                    //if we split into potentials, just exit this, we need a limit...
                    //need to make my own pool to limit this... how abit this one just stays on the thread its currently on?
                    Task t = Task.Factory.StartNew(
                        () => { SearchTaskLIST(newSearch, depthLimit, maxSearchedDepth); }
                        , TaskCreationOptions.LongRunning);
                    ThreadSafeSimStats.instance.AddWork(t);
                }
                else
                {
                    LinkedListNode<Sim>? insertPoint = search.First;

                    //for(; insertPoint != search.Last && insertPoint != null && insertPoint.Value.botsByType[(int)ElementType.Geode] > curSim.botsByType[(int)ElementType.Geode];)
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
            List<int> potentials = new(sim.blueprint.entries.Count);

            for(int i =0; i < sim.blueprint.entries.Count; i++)
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

        void StepBuild(ref Sim sim, int buildIndex)
        {

            //remove costs from counts
            foreach (var cost in sim.blueprint.entries[buildIndex].cost)
                sim.oreCounts[(int)cost.type] -= cost.amount;

            //add the robot as a new active robot, starting construction
            sim.pendingBotType = sim.blueprint.entries[buildIndex].type;
            sim.hasPendingBot = true;            
        }

        void StepRobots(ref Sim sim)
        {
            //collect. TODO: fiond a way to delay one bot when its created...
            //all active robots
            for (int i = 0; i < ELEMENT_COUNT; i++)
                 sim.oreCounts[i] += sim.botsByType[i];
            
            if(sim.hasPendingBot)
            {
                sim.hasPendingBot = false;
                sim.botsByType[(int)sim.pendingBotType]++;
            }
        }

     



    }
}
