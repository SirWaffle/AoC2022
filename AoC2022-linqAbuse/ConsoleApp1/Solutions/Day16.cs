using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;
using static ConsoleApp1.Solutions.Day14;
using static ConsoleApp1.Solutions.Day15;
using static ConsoleApp1.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleApp1.Solutions
{
    internal class Day16 : AbstractPuzzle
    {

        public record ValveCell(String Valve, int flowRate, List<(string valve, int traversalCost)> connectedValves);


        public class ValvePath
        {
            public ValveCell valveCell;
            public int steps = 0;
            public int flowScore = 0;
            public int potentialPoints = 0;
            public HashSet<ValvePath> path = new();
            public HashSet<ValveCell> openedValves = new();

            public ValvePath()  { }
        }

        override public void Part1()
        {
            Dictionary<String, ValveCell> valves = new();

            //parse...
            var lines = File.ReadAllText(InputFile!).Split("\r\n").Select(x => x.Trim());
            foreach (var line in lines)
            {
                string valve = line.Split(" ")[1].Trim();
                int flow = int.Parse(line.Split("rate=")[1].Split(";")[0].Trim());
                var toValves = line.Replace("valves", "valve").Split("to valve")[1].Split(",").Select(x => (x.Trim(), 1)).ToList();

                ValveCell v = new(valve, flow, toValves);
                valves.Add(valve, v);
            }

            //simplify the graph, connect valves with flow values by shortest path to every other valve with a flow value
            //create new graph to search, then search for total score based on flow
            Dictionary<String, ValveCell> simplifiedValveGrid = new();

            var valvesWithFlow = valves.Where(x => x.Value.flowRate > 0).Select(x => x.Value).ToList();

            //also add our starting point
            valvesWithFlow.Insert(0, valves["AA"]);

            for (int from = 0; from < valvesWithFlow.Count; from++)
            {
                List<(string valve, int traversalCost)> connectedValves = new();

                for (int to = 0; to < valvesWithFlow.Count; to++)
                {
                    if (from == to)
                        continue;

                    ValvePath best = BFS(valves, valvesWithFlow, valvesWithFlow[from].Valve, valvesWithFlow[to].Valve, true);
                    connectedValves.Add((valvesWithFlow[to].Valve, best.steps));//open whenever we arrive there
                }

                ValveCell v = new(valvesWithFlow[from].Valve, valvesWithFlow[from].flowRate, connectedValves);
                simplifiedValveGrid.Add(valvesWithFlow[from].Valve, v);
            }


            //lets jsut BFS this in its entireity
            valvesWithFlow = simplifiedValveGrid.Where(x => x.Value.flowRate > 0).Select(x => x.Value).ToList();
            var maxScore = BFS(simplifiedValveGrid, valvesWithFlow, "AA", String.Empty, false);

            Console.WriteLine("max score: " + maxScore.flowScore);

            foreach (var p in maxScore.path)
            {
                Console.WriteLine(p.valveCell.Valve + "  at: " + p.steps + "    score: " + p.flowScore);
            }
        }


        public ValvePath BFS(Dictionary<String, ValveCell> valves, List<ValveCell> valvesWithFlow, string startValve, string endValve, bool findShortestPath)
        {
            int totalActionPoints = 30;

            List<ValvePath> search = new();

            //remove search heads that are lower in score than the best
            Dictionary<string, (int steps, int score)> maxPerCell = new();
            foreach (var valve in valves)
                maxPerCell.Add(valve.Key, (99999, 0));

            //start node
            ValvePath start = new();
            start.valveCell = valves[startValve];
            start.steps = 0;
            start.flowScore = 0;
            start.potentialPoints = valvesWithFlow.Select(x => x.flowRate).Aggregate((total, next) => total + (next * 29));
            start.path = new() { start };

            search.Add(start);

            //for findin gbest path isntead of best score
            ValveCell? end = null;
            if(endValve != string.Empty)
                end = valves[endValve];

            ValvePath maxScore = new();

            int searches = 0;

            while (search.Count > 0)
            {
                ++searches;
                if(searches % 1000 == 0)
                    Console.WriteLine("----iters: {0}   paths remaining: {1}      ", searches, search.Count);


                ValvePath cur = search[0];
                search.RemoveAt(0);

                //found our shortest path
                if(end != null && cur.valveCell == end)
                {
                    Console.WriteLine("shortest path from " + start.valveCell.Valve + " to " + end.Valve + " is " + cur.steps);
                    Thread.Sleep(10);
                    return cur;
                }

                //open valves
                if (!findShortestPath)
                {
                    if (cur.valveCell.flowRate > 0 && !cur.openedValves.Any(x => x.Valve == cur.valveCell.Valve))
                    {
                        cur.openedValves = new(cur.openedValves) { cur.valveCell };
                        cur.steps += 1;
                        cur.flowScore = cur.flowScore + (cur.valveCell.flowRate * (totalActionPoints - cur.steps));                        
                    }
                }

                
                //reduce search space by dropping worse paths
                (int steps, int score) maxScoreInCell = maxPerCell[cur.valveCell.Valve];
                if (cur.steps >= maxScoreInCell.steps && cur.flowScore < maxScoreInCell.score)
                {
                    continue;
                }
                else
                {
                    maxPerCell[cur.valveCell.Valve] = (cur.steps, cur.flowScore);
                }

                //also early out if theres no potential to catch up based on projected highest possible score remaining
                if (!findShortestPath)
                {
                    if (cur.potentialPoints + cur.flowScore < maxScore.flowScore)
                        continue;
                }

                //update max score / best path
                if (maxScore.flowScore < cur.flowScore)
                {
                    maxScore = cur;
                    Console.WriteLine("new max score: " + maxScore.flowScore);
                    Thread.Sleep(10);
                }

                //no more valves, done with this one
                if (cur.openedValves.Count == valvesWithFlow.Count)
                    continue;


                //add available paths in valid range...
                for (int i = 0; i < cur.valveCell.connectedValves.Count; ++i)
                {
                    ValveCell next = valves[cur.valveCell.connectedValves[i].valve];

                    //if we opened the valve, no need to go there
                    if(!findShortestPath)
                    {
                        if (cur.openedValves.Contains(next)) 
                            continue;
                    }

                    //sopecial case for shoprtest path
                    if (findShortestPath)
                    {
                        //not allowed to cross over where we already searched
                        if (cur.path != null && cur.path.Any(x => x.valveCell.Valve == next.Valve))
                            continue;
                    }

                    if(!findShortestPath)
                    {
                        //prevent too many steps
                        if (cur.steps + cur.valveCell.connectedValves[i].traversalCost > totalActionPoints)
                            continue;
                    }

                    //add new path
                    ValvePath newPath = new();
                    newPath.valveCell = next;
                    newPath.path = new(cur.path!);
                    newPath.openedValves = new(cur.openedValves);
                    newPath.steps = cur.steps + cur.valveCell.connectedValves[i].traversalCost;
                    newPath.flowScore = cur.flowScore;
                    newPath.path.Add(newPath);


                    //insert based on best scores, faster, helps eliminate paths, finds shortest route
                    if (findShortestPath)
                    {
                        bool added = false;
                        for(int j = 0; j < search.Count && added == false; ++j)
                        {
                            if (search[j].steps > newPath.steps)
                            {
                                added = true;
                                search.Insert(j, newPath);
                            }
                        }
                        if(!added)
                        {
                            search.Add(newPath);
                        }
                    }
                    else //prioritize score...
                    {
                        InsertSearchNodeByScore(search, valvesWithFlow, newPath);
                    }
                }
            }

            return maxScore;
        }


        void InsertSearchNodeByScore(List<ValvePath> search, List<ValveCell> valvesWithFlow, ValvePath newPath)
        {
            bool added = false;

            int potentialPoints = valvesWithFlow.Except(newPath.openedValves).Select(x => x.flowRate).Aggregate((total, next) => total + (next * (30 - newPath.steps)));
            newPath.potentialPoints = potentialPoints;

            //steps, then score
            for (int j = 0; j < search.Count && added == false; ++j)
            {
                int sortScoreA = search[j].potentialPoints;
                int sortScoreB = potentialPoints;
                if(sortScoreA <= sortScoreB)
                {
                    if(search[j].flowScore <= newPath.flowScore)
                    {
                        added = true;
                        search.Insert(j, newPath);
                    }
                }
            }
            if (!added)
            {
                search.Add(newPath);
            }
        }




















        override public void Part2()
        {
            Dictionary<String, ValveCell> valves = new();

            //parse...
            var lines = File.ReadAllText(InputFile!).Split("\r\n").Select(x => x.Trim());
            foreach (var line in lines)
            {
                string valve = line.Split(" ")[1].Trim();
                int flow = int.Parse(line.Split("rate=")[1].Split(";")[0].Trim());
                var toValves = line.Replace("valves", "valve").Split("to valve")[1].Split(",").Select(x => (x.Trim(), 1)).ToList();

                ValveCell v = new(valve, flow, toValves);
                valves.Add(valve, v);
            }

            //simplify the graph, connect valves with flow values by shortest path to every other valve with a flow value
            //create new graph to search, then search for total score based on flow
            Dictionary<String, ValveCell> simplifiedValveGrid = new();

            var valvesWithFlow = valves.Where(x => x.Value.flowRate > 0).Select(x => x.Value).ToList();

            //also add our starting point
            valvesWithFlow.Insert(0, valves["AA"]);

            for (int from = 0; from < valvesWithFlow.Count; from++)
            {
                List<(string valve, int traversalCost)> connectedValves = new();

                for (int to = 0; to < valvesWithFlow.Count; to++)
                {
                    if (from == to)
                        continue;

                    ValvePath best = BFS(valves, valvesWithFlow, valvesWithFlow[from].Valve, valvesWithFlow[to].Valve, true);
                    connectedValves.Add((valvesWithFlow[to].Valve, best.steps));//open whenever we arrive there
                }

                ValveCell v = new(valvesWithFlow[from].Valve, valvesWithFlow[from].flowRate, connectedValves);
                simplifiedValveGrid.Add(valvesWithFlow[from].Valve, v);
            }


            //lets jsut BFS this in its entireity
            valvesWithFlow = simplifiedValveGrid.Where(x => x.Value.flowRate > 0).Select(x => x.Value).ToList();
            var maxScore = BFSpart2(simplifiedValveGrid, valvesWithFlow, "AA");

            Console.WriteLine("max score: " + maxScore.flowScore);

            foreach (var p in maxScore.path)
            {
                Console.WriteLine(p.valveCell.Valve + "  at: " + p.steps + "    score: " + p.flowScore);
            }
        }

        public class MultiPath
        {
            public ValveCell[] valveCell = new ValveCell[2];
            public int[] steps = new int[2];
            public int flowScore = 0;
            public int potentialPoints = 0;
            public HashSet<ValvePath> path = new();
            public HashSet<ValveCell> openedValves = new();

            public MultiPath() { }
        }

        public MultiPath BFSpart2(Dictionary<String, ValveCell> valves, List<ValveCell> valvesWithFlow, string startValve)
        {
            int totalActionPoints = 26;

            List<MultiPath> search = new();

            //remove search heads that are lower in score than the best
            Dictionary<string, (int steps, int score)> maxPerCell = new();
            foreach (var valve in valves)
                maxPerCell.Add(valve.Key, (99999, 0));

            //start node
            ValvePath start = new();
            start.valveCell = valves[startValve];
            start.steps = 0;
            start.flowScore = 0;
            start.potentialPoints = valvesWithFlow.Select(x => x.flowRate).Aggregate((total, next) => total + (next * (totalActionPoints - 1)));
            start.path = new() { start };

            MultiPath startMP = new();
            startMP.valveCell[0] = valves[startValve];
            startMP.valveCell[1] = valves[startValve];
            startMP.flowScore = 0;
            startMP.potentialPoints = valvesWithFlow.Select(x => x.flowRate).Aggregate((total, next) => total + (next * (totalActionPoints - 1)));
            startMP.path = new() { start };

            search.Add(startMP);


            MultiPath maxScore = new();

            int searches = 0;

            while (search.Count > 0)
            {
                ++searches;
                if (searches % 1000 == 0)
                    Console.WriteLine("----iters: {0}   paths remaining: {1}      ", searches, search.Count);

                //lets just find the next step of the lowest step valued path between the two
                MultiPath cur = search[0];
                search.RemoveAt(0);

                int playerIndex = cur.steps[0] <= cur.steps[1] ? 0 : 1;
                int otherIndex = cur.steps[0] <= cur.steps[1] ? 1 : 0;

                //open valves
                if (cur.valveCell[playerIndex].flowRate > 0 && !cur.openedValves.Any(x => x.Valve == cur.valveCell[playerIndex].Valve))
                {
                    cur.openedValves = new(cur.openedValves) { cur.valveCell[playerIndex] };
                    cur.steps[playerIndex] += 1;
                    cur.flowScore = cur.flowScore + (cur.valveCell[playerIndex].flowRate * (totalActionPoints - cur.steps[playerIndex]));
                }


                //reduce search space by dropping worse paths
                (int steps, int score) maxScoreInCell = maxPerCell[cur.valveCell[playerIndex].Valve];
                if (cur.steps[playerIndex] >= maxScoreInCell.steps && cur.flowScore < maxScoreInCell.score)
                {
                    continue;
                }
                else
                {
                    maxPerCell[cur.valveCell[playerIndex].Valve] = (cur.steps[playerIndex], cur.flowScore);
                }

                //also early out if theres no potential to catch up based on projected highest possible score remaining
                if (cur.potentialPoints + cur.flowScore < maxScore.flowScore)
                        continue;

                //update max score / best path
                if (maxScore.flowScore < cur.flowScore)
                {
                    maxScore = cur;
                    Console.WriteLine("new max score: " + maxScore.flowScore);
                    Thread.Sleep(10);
                }

                //no more valves, done with this one
                if (cur.openedValves.Count == valvesWithFlow.Count)
                    continue;


                //add available paths in valid range...
                for (int i = 0; i < cur.valveCell[playerIndex].connectedValves.Count; ++i)
                {
                    ValveCell next = valves[cur.valveCell[playerIndex].connectedValves[i].valve];

                    //if we opened the valve, no need to go there
                        if (cur.openedValves.Contains(next))
                            continue;


                        //prevent too many steps
                        if (cur.steps[playerIndex] + cur.valveCell[playerIndex].connectedValves[i].traversalCost > totalActionPoints)
                            continue;


                    //add new path
                    MultiPath newPath = new();
                    newPath.valveCell[otherIndex] = cur.valveCell[otherIndex];
                    newPath.valveCell[playerIndex] = next;
                    newPath.path = new(cur.path!);
                    newPath.openedValves = new(cur.openedValves);
                    newPath.steps[playerIndex] = cur.steps[playerIndex] + cur.valveCell[playerIndex].connectedValves[i].traversalCost;
                    newPath.steps[otherIndex] = cur.steps[otherIndex];
                    newPath.flowScore = cur.flowScore;

                    ValvePath traveled = new();
                    traveled.valveCell = next;
                    traveled.steps = newPath.steps[playerIndex];
                    traveled.flowScore = newPath.flowScore;
                    newPath.path.Add(traveled);


                    //insert based on best scores, faster, helps eliminate paths, finds shortest route
                    bool added = false;

                    int potentialPoints = valvesWithFlow.Except(newPath.openedValves).Select(x => x.flowRate).Aggregate((total, next) => total + (next * (26 - newPath.steps[playerIndex])));
                    newPath.potentialPoints = potentialPoints;

                    //steps, then score
                    for (int j = 0; j < search.Count && added == false; ++j)
                    {
                        int sortScoreA = search[j].potentialPoints;
                        int sortScoreB = potentialPoints;
                        if (sortScoreA <= sortScoreB)
                        {
                            if (search[j].flowScore <= newPath.flowScore)
                            {
                                added = true;
                                search.Insert(j, newPath);
                            }
                        }
                    }
                    if (!added)
                    {
                        search.Add(newPath);
                    }
                }
            }

            return maxScore;
        }


    }




}
