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
            public enum Action
            {
                Traverse,
                Open,
            }

            public ValveCell valveCell;
            public Action action;
            public int steps = 0;
            public int flowScore = 0;
            public int potentialPoints = 0;
            public HashSet<ValvePath> path = new();
            public HashSet<ValveCell> openedValves = new();

            public HashSet<(string, string)> traversedEdges = new();

            public ValvePath()  { }

            public static bool operator ==(ValvePath x, ValvePath y)
            {
                return x.valveCell.Valve == y.valveCell.Valve && x.action == y.action;
            }

            public static bool operator !=(ValvePath x, ValvePath y)
            {
                return !(x == y);
            }
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
                    connectedValves.Add((valvesWithFlow[to].Valve, best.steps));
                }

                ValveCell v = new(valvesWithFlow[from].Valve, valvesWithFlow[from].flowRate, connectedValves);
                simplifiedValveGrid.Add(valvesWithFlow[from].Valve, v);
            }

            //lets jsut BFS this in its entireity
            //var maxScore = BFS(valves, valvesWithFlow, "AA", String.Empty, false);
            var maxScore = BFS(simplifiedValveGrid, valvesWithFlow, "AA", String.Empty, false);

            Console.WriteLine("max score: " + maxScore.flowScore);
            //2155 to low
            //1925 too low
        }



        override public void Part2()
        {

        }




        public ValvePath BFS(Dictionary<String, ValveCell> valves, List<ValveCell> valvesWithFlow, string startValve, string endValve, bool findShortestPath)
        {
            int totalActionPoints = 30;

            List<ValvePath> search = new();
            Dictionary<string, (int steps, int score)> maxPerCell = new();
            foreach (var valve in valves)
            {
                maxPerCell.Add(valve.Key, (99999, 0));
            }

            ValvePath start = new();
            start.valveCell = valves[startValve];
            start.steps = 0;
            start.flowScore = 0;
            start.potentialPoints = valvesWithFlow.Select(x => x.flowRate).Aggregate((total, next) => total + (next * 30));
            start.path = new() { start };

            search.Add(start);

            ValveCell? end = null;
            if(endValve != string.Empty)
            {
                end = valves[endValve];
            }

            ValvePath maxScore = new();

            int searches = 0;

            while (search.Count > 0)
            {
                ++searches;
                if(searches % 1000 == 0)
                {
                    Console.WriteLine("----iters: {0}   patsh reamining: {1}      ", searches, search.Count);
                }


                ValvePath cur = search[0];
                search.RemoveAt(0);

                if(end != null && cur.valveCell == end)
                {
                    Console.WriteLine("shortest path from " + start.valveCell.Valve + " to " + end.Valve + " is " + cur.steps);
                    Thread.Sleep(10);
                    return cur;
                }

                (int steps, int score) maxScoreInCell = maxPerCell[cur.valveCell.Valve];

                //lets record th ebest path found here so far, if we arent it, we just dissappear
                if (cur.steps >= maxScoreInCell.steps && cur.flowScore < maxScoreInCell.score)
                {
                    //continue;
                }
                else
                {
                    maxPerCell[cur.valveCell.Valve] = (cur.steps, cur.flowScore);
                }

                //also early out if theres no potential to catch up
                if (!findShortestPath)
                {
                    if (cur.potentialPoints + cur.flowScore < maxScore.flowScore)
                        continue;
                }

                if (maxScore.flowScore < cur.flowScore)
                {
                    maxScore = cur;
                    Console.WriteLine("new max score: " + maxScore.flowScore);
                    Thread.Sleep(10);
                }

                if (cur.steps > totalActionPoints)
                    continue;

                //see if we want to open the valve, if so, increase the score..
                if (   !findShortestPath
                    && cur.action != ValvePath.Action.Open
                    && cur.valveCell.flowRate > 0
                    && !cur.openedValves.Contains(cur.valveCell))
                {
                    ValvePath newPath = new();
                    newPath.valveCell = cur.valveCell;
                    newPath.path = new(cur.path);
                    newPath.action = ValvePath.Action.Open;
                    newPath.steps = cur.steps + 1;
                    newPath.openedValves = new(cur.openedValves) { cur.valveCell };
                    newPath.flowScore = cur.flowScore + (newPath.valveCell.flowRate * (totalActionPoints - newPath.steps));
                    newPath.path.Add(newPath);

                    InsertSearchNodeByScore(search, valvesWithFlow, newPath);
                }

                //add available paths in valid range...
                for (int i = 0; i < cur.valveCell.connectedValves.Count; ++i)
                {
                    ValveCell next = valves[cur.valveCell.connectedValves[i].valve];

                    //should also cut out duplicate edges, now that i've simplified the grid and made full attachments.
                    if(!findShortestPath)
                    {
                        //dont allow for duplicate traversed edges
                        if (cur.traversedEdges.Contains((cur.valveCell.Valve, next.Valve))) 
                        {
                            continue;
                        }
                    }

                    //hrm, we can revisit stuff..
                    //lets jsut not immediatly return to th previous cell
                    if (cur.path.Last().valveCell.Valve == next.Valve)
                        continue;

                    if (findShortestPath)
                    {
                        //not allowed to cross over where we already searched
                        if (cur.path != null && cur.path.Any(x => x.valveCell.Valve == next.Valve))
                            continue;
                    }

                    ValvePath newPath = new();
                    newPath.valveCell = next;
                    newPath.path = new(cur.path);
                    newPath.openedValves = new(cur.openedValves);
                    newPath.action = ValvePath.Action.Traverse;
                    newPath.steps = cur.steps + cur.valveCell.connectedValves[i].traversalCost;
                    newPath.flowScore = cur.flowScore;
                    newPath.path.Add(newPath);
                    newPath.traversedEdges = new(cur.traversedEdges);
                    newPath.traversedEdges.Add((cur.valveCell.Valve, newPath.valveCell.Valve));

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

    }




}
