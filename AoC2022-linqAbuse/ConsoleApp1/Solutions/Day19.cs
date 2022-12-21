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
            Both();
        }

        override public void Part2()
        {
            Both();
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


        public struct Robot
        {
            public enum State
            {
                StartConstruction,
                EndConstruction,
                Ready,
                Collecting
            }

            public State state;
            public ElementType type;

            public override string ToString()
            {
                return type.ToString() + " Robot: " + state.ToString();
            }

        }

        struct Sim
        {
            public Blueprint blueprint;

            public List<Robot> activeRobots = new(1);
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
                sim.activeRobots = new(activeRobots.Count + 1);
                sim.activeRobots.AddRange(activeRobots);
                //weird, editing the clones oreCOunts is modifying both..wtf?
                sim.oreCounts = new int[ELEMENT_COUNT];
                for(int i = 0; i < ELEMENT_COUNT; ++i)
                {
                    sim.oreCounts[i] = oreCounts[i];
                }

                sim.botsByType = new int[ELEMENT_COUNT];
                for (int i = 0; i < ELEMENT_COUNT; ++i)
                {
                    sim.botsByType[i] = botsByType[i];
                }


                sim.time = time;
                sim.simNum= simNum;
                sim.simID = simId++;
                return sim;
            }
        }

        class ThreadSafeSimStats
        {
            public Sim maxGeodesSim = new();
            public int maxGeodes = 0;
            public List<int> bestScoresByStep = new(new int[24]);

            object maxLockObj = new();

            public List<Task> crunchingTasks = new();

            int curCoreAffinity = 1;
            int curThread = 1;
            public int threadLimit = 12;

            public readonly int mainThreadId;

            object taskListLockObj = new();

            object maxPerDepthLock = new();
            int[] maxFoundPerDepth= new int[26];
            int totalMaxFound = 0;

            public UInt64 discardedBranches = 0;

           
            public ThreadSafeSimStats()
            {
                mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            }
            
            public bool CheckAgainstMax(ref Sim sim, int depth, int depthLimit, out int score)
            {
                //if we are at depth 24 with no bots, just exit, we're heading towards 0 anyways
                if (depth >= 24 && sim.botsByType[(int)ElementType.Geode] == 0)
                {
                    score = 0;
                    return false;
                }

                int combinedScore = 0;

                for (int i = (int)ElementType.Geode; i < ELEMENT_COUNT; ++i)
                {
                    //score it
                    int remainingTime = (depthLimit - depth);
                    int possibleBots = sim.botsByType[i];
                    int possibleBotsGeodes = 0;
                    for (int rt = 0; rt < remainingTime; rt++)
                    {                        
                        possibleBotsGeodes += possibleBots;
                        possibleBots++;
                    }
                    int possibleTotalFull = possibleBotsGeodes;

                    combinedScore = sim.oreCounts[i] + possibleTotalFull;

                    //combinedScore += (i * 200) + ( sim.oreCounts[i] * 100 ) +  (possibleTotalFull);
                }

                score = combinedScore;

                //TODO: testing
                /*
                for (int i = depth; i < depthLimit; ++i)
                {
                    if (score < maxFoundPerDepth[i])
                        return false;
                }*/

                if (score < maxFoundPerDepth[depth]) //fuzz numbver i guess
                    return false;

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

        private static Object lockObj = new Object();


        void Both()
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
            for (int bpInd = 0; bpInd < blueprints.Count; bpInd++)
            {

                ThreadSafeSimStats simStats = new();

                CrunchBlueprint(bpInd, blueprints[bpInd], simStats);

                //wait for tasks
                Console.WriteLine("\n----- waiting for tasks -------");
                for (; ; )
                {
                    Thread.Sleep(10000);
                    simStats.ClearFInishedWork();
                    Console.WriteLine("Crunching bp " + bpInd + " with: " + simStats.crunchingTasks.Count() + " threads, current simId: " + Sim.simId + " number discarded: " + simStats.discardedBranches);
                    if (Task.WaitAll(simStats.crunchingTasks.ToArray(), 10))
                    {
                        Sim.simId = 0;
                        //done
                        break;
                    }
                }

                simPerBluePrint.Add(simStats.maxGeodesSim);
                LogSimState(ref simStats.maxGeodesSim);
            }


            Console.WriteLine("\n----- Finished Waiting -------");

            int sum = 0;
            for(int i = 0; i < simPerBluePrint.Count; i++)
            {
                sum += ( (i + 1) * simPerBluePrint[i].oreCounts[(int)ElementType.Geode]);
                Console.WriteLine("Blueprint " + i + " max geodes = " + simPerBluePrint[i].oreCounts[(int)ElementType.Geode]);
            }

            Console.WriteLine("Score: " + sum);

            _ = Console.ReadLine();
            _ = Console.ReadLine();
            _ = Console.ReadLine();
            _ = Console.ReadLine();
        }

        void CrunchBlueprint(int simNum, Blueprint bp, ThreadSafeSimStats simStats)
        {
            Sim startSim = new();
            startSim.blueprint = bp;
            startSim.simNum = simNum;

            //we start with 1 ore robot
            Robot rb = new();
            rb.type = StringToElement("ore");
            rb.state = Robot.State.Collecting;
            startSim.activeRobots.Add(rb);

            startSim.time = 1;

            LinkedList<Sim> search = new();
            search.AddLast(startSim);
            int timeLimit = 24;


            //well, lets search. 
            //TODO: will want to check for cycles to not search every terrible iteration...
            int maxSearchedDepth = 0;

            SearchTaskLIST(search, timeLimit, maxSearchedDepth, simStats, 99999999);// 10);
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
            //score += (variation * 500);

            if(score > highestSimScore)
            {
                highestSimScore = score;
            }
            return score;
        }
        void SearchTaskLIST(LinkedList<Sim> search, int depthLimit, int maxSearchedDepth, ThreadSafeSimStats simStats, int taskSplitDepth)
        { 
            int switcher = 0;
            while (search.Count > 0)
            {                
                //ok, running out of memory....
                Sim curSim = search.First();
                //++switcher;
                if (switcher > 100000)
                {
                    Thread.Sleep(10);
                    //try quick clean?
                    if (search.Count > 1000000)
                    {
                        LinkedListNode<Sim>? checkPoint = search.Last;
                        LinkedListNode<Sim>? deletePoint = search.Last;

                        for (; checkPoint != search.First && checkPoint != null; )
                        {
                            bool dontDel = simStats.CheckAgainstMax(ref checkPoint.ValueRef, checkPoint.Value.time, depthLimit, out _);

                            if (!dontDel)
                            {
                                deletePoint = checkPoint;
                                checkPoint = checkPoint.Previous;
                                search.Remove(deletePoint);
                                simStats.discardedBranches++;
                            }
                            else
                            {
                                checkPoint = checkPoint.Previous;
                            }

                            
                        }
                    }
                    switcher = 0;
                }

                search.RemoveFirst();
                               

                //collect and progress builds
                StepRobots(ref curSim);
                //LogSimState(curSim);
                curSim.time += 1;

                //lets see if we should exit...
                //maybe this part is wrong?
                int score = 0;
                bool onTarget = simStats.CheckAgainstMax(ref curSim, curSim.time, depthLimit, out score);
                
                if (!onTarget)
                {
                    //we're done, probably
                    simStats.discardedBranches++;
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
                    if (simStats.CheckMax(ref curSim))
                    {
                    }
                }

                if (curSim.time > depthLimit)
                {
                    simStats.discardedBranches++;
                    continue;
                }

                //find new robots to construct...
                var potentials = GatherAvailableBuilds(ref curSim);

                if (potentials.Count > 0)
                {
                    //add a new sim step for each possible build...
                    for(int i = potentials.Count - 1; i >= 0; --i)
                    {

                        Sim newSim = curSim.DeepCopy();
                        StepBuild(ref newSim, potentials[i]);

                        newSim.currentScore = SimSortScore(newSim);


                        if ( ( Thread.CurrentThread.ManagedThreadId == simStats.mainThreadId 
                               || taskSplitDepth > -1  )
                            //&& newSim.time == taskSplitDepth 
                            && simStats.threadLimit >= simStats.crunchingTasks.Count)
                        {
                            LinkedList<Sim> newSearch = new();
                            newSearch.AddLast(newSim);
                            Task t = Task.Factory.StartNew(
                                () => { SearchTaskLIST(newSearch, depthLimit, maxSearchedDepth, simStats, taskSplitDepth); } // -1); }
                                );
                            simStats.AddWork(t);
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

                if (Thread.CurrentThread.ManagedThreadId == simStats.mainThreadId && simStats.threadLimit > -1)
                {
                    LinkedList<Sim> newSearch = new();
                    newSearch.AddLast(curSim);
                    //if we split into potentials, just exit this, we need a limit...
                    //need to make my own pool to limit this... how abit this one just stays on the thread its currently on?
                    Task t = Task.Factory.StartNew(
                        () => { SearchTaskLIST(newSearch, depthLimit, maxSearchedDepth, simStats, taskSplitDepth); }
                        );
                    simStats.AddWork(t);
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
            { 
                Console.WriteLine("Have: " + ((ElementType)(i)).ToString() + ": " + sim.oreCounts[i]);
            }

            foreach (var rb in sim.activeRobots)
            {
                Console.WriteLine(rb.ToString());
            }

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
            {
                sim.oreCounts[(int)cost.type] -= cost.amount;

                if (sim.oreCounts[(int)cost.type] < 0)
                {
                    throw new Exception("negative ore count");
                }
            }

            //add the robot as a new active robot, starting construction
            Robot rb = new();
            rb.type = sim.blueprint.entries[buildIndex].type;
            rb.state = Robot.State.StartConstruction;

            sim.activeRobots.Add(rb);

            sim.botsByType[(int)rb.type]++;
        }

        void StepRobots(ref Sim sim)
        {
            //all active robots
            for (int i = 0; i < sim.activeRobots.Count; i++)
            {
                //start goes to end
                if (sim.activeRobots[i].state == Robot.State.StartConstruction)
                {
                    var rb = sim.activeRobots[i];
                    rb.state = Robot.State.Ready;
                    sim.activeRobots[i] = rb;
                }

                //end goes to ready
                /*
                //TODO: debug removal of else: pass by one real quick, too many states i think
                if (sim.activeRobots[i].state == Robot.State.EndConstruction)
                {
                    var rb = sim.activeRobots[i];
                    rb.state = Robot.State.Ready;
                    sim.activeRobots[i] = rb;
                }
                */
                //constructing goes to collecting
                if (sim.activeRobots[i].state == Robot.State.Ready)
                {
                    var rb = sim.activeRobots[i];
                    rb.state = Robot.State.Collecting;
                    sim.activeRobots[i] = rb;
                }
                else
                {
                    //collecting gives us ore
                    if (sim.activeRobots[i].state == Robot.State.Collecting)
                    {
                        sim.oreCounts[(int)sim.activeRobots[i].type]++;
                    }
                }
            }
        }

     



    }
}
