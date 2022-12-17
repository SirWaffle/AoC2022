using System.Collections.Concurrent;

namespace ConsoleApp1.Solutions
{
    internal class Day16 : AbstractPuzzle
    {

        public record ValveCell(String Valve, int flowRate, List<(string valve, int traversalCost)> connectedValves);


        public class ValvePath
        {
            public ValveCell valveCell;
            public int steps = 0;
            public HashSet<ValvePath> path = new();

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

                    ValvePath best = ShortestPath(valves, valvesWithFlow[from].Valve, valvesWithFlow[to].Valve)!;
                    connectedValves.Add((valvesWithFlow[to].Valve, best.steps));//open whenever we arrive there
                }

                ValveCell v = new(valvesWithFlow[from].Valve, valvesWithFlow[from].flowRate, connectedValves);
                simplifiedValveGrid.Add(valvesWithFlow[from].Valve, v);
            }


            //lets be smarter than path finding...
            //we want every permutation of paths that add up to num steps
            int stepCount = 30;
            valvesWithFlow = simplifiedValveGrid.Where(x => x.Value.flowRate > 0).Select(x => x.Value).ToList();

            List<List<(int steps, ValveCell)>> possible = new();
            for (int i = 0; i < simplifiedValveGrid["AA"].connectedValves.Count; i++)
            {
                ValveCell startCell = simplifiedValveGrid[simplifiedValveGrid["AA"].connectedValves[i].valve];
                int startSteps = simplifiedValveGrid["AA"].connectedValves[i].traversalCost;


                int steps = startSteps + 1;

                List<(int steps, ValveCell)> curPath = new();
                List<ValveCell> avail = new(valvesWithFlow);
                avail.Remove(startCell);
                curPath.Add((steps + 1, startCell));
                FindValvePath(valvesWithFlow, startCell, curPath, avail, steps + 1, possible, stepCount);
            }


            Console.WriteLine("Found total: " + possible.Count);
            int maxScore = 0;
            for (int i = 0; i < possible.Count; i++)
            {
                var vals = possible[i].Select(x => x).ToArray();

                int score = 0;
                for (int j = 0; j < vals.Length; j++)
                {
                    score += (vals[j].Item2.flowRate * ((stepCount + 1) - vals[j].steps));
                }
                if (score > maxScore)
                {
                    maxScore = score;
                    Console.WriteLine("new max found: " + maxScore);
                    Console.WriteLine("Path: " + vals.ToList().Select(x => x.Item2.Valve + "(" + x.steps + ")").Aggregate((a, b) => a + "   " + b) + "  score: ");
                }
            }

            Console.WriteLine("Max value found: " + maxScore);
        }

        public void FindValvePath(List<ValveCell> valves, ValveCell from, List<(int steps, ValveCell)> curPath, List<ValveCell> avail, int steps, List<List<(int step, ValveCell)>> found, int stepLimit)
        {
            if (avail.Count == 0)
            {
                found.Add(curPath);
            }

            bool branched = false;
            foreach (var cell in avail)
            {
                int cost = from.connectedValves.Where(x => x.valve == cell.Valve).First().traversalCost;

                if (steps + cost + 1 > stepLimit)
                {
                    continue;
                }

                branched = true;
                List<ValveCell> adjusted = new(avail);
                adjusted.Remove(cell);
                List<(int steps, ValveCell)> newPath = new(curPath);
                newPath.Add((steps + cost + 1, cell));
                FindValvePath(valves, cell, newPath, adjusted, steps + cost + 1, found, stepLimit);
            }

            if (!branched)
            {
                found.Add(curPath);
            }
        }

        public ValvePath? ShortestPath(Dictionary<String, ValveCell> valves, string startValve, string endValve)
        {
            List<ValvePath> search = new();

            //start node
            ValvePath start = new();
            start.valveCell = valves[startValve];
            start.steps = 0;
            start.path = new() { start };

            search.Add(start);

            //for finding best path 
            ValveCell end = valves[endValve];

            while (search.Count > 0)
            {
                ValvePath cur = search[0];
                search.RemoveAt(0);

                //found our shortest path
                if (end != null && cur.valveCell == end)
                {
                    Console.WriteLine("shortest path from " + start.valveCell.Valve + " to " + end.Valve + " is " + cur.steps);
                    Thread.Sleep(10);
                    return cur;
                }

                //add available paths in valid range...
                for (int i = 0; i < cur.valveCell.connectedValves.Count; ++i)
                {
                    ValveCell next = valves[cur.valveCell.connectedValves[i].valve];

                    //not allowed to cross over where we already searched
                    if (cur.path != null && cur.path.Any(x => x.valveCell.Valve == next.Valve))
                        continue;

                    //add new path
                    ValvePath newPath = new();
                    newPath.valveCell = next;
                    newPath.path = new(cur.path!);
                    newPath.steps = cur.steps + cur.valveCell.connectedValves[i].traversalCost;
                    newPath.path.Add(newPath);

                    //insert based on best scores, faster, helps eliminate paths, finds shortest route (makes this BFS)
                    bool added = false;
                    for (int j = 0; j < search.Count && added == false; ++j)
                    {
                        if (search[j].steps > newPath.steps)
                        {
                            added = true;
                            search.Insert(j, newPath);
                        }
                    }
                    if (!added)
                    {
                        search.Add(newPath);
                    }
                }
            }

            return null;
        }

     


        override public async void Part2()
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

                    ValvePath best = ShortestPath(valves, valvesWithFlow[from].Valve, valvesWithFlow[to].Valve)!;
                    connectedValves.Add((valvesWithFlow[to].Valve, best.steps));//open whenever we arrive there
                }

                ValveCell v = new(valvesWithFlow[from].Valve, valvesWithFlow[from].flowRate, connectedValves);
                simplifiedValveGrid.Add(valvesWithFlow[from].Valve, v);
            }


            //lets be smarter than path finding...
            //we want every permutation of paths that add up to num steps
            int stepCount = 26;
            valvesWithFlow = simplifiedValveGrid.Where(x => x.Value.flowRate > 0).Select(x => x.Value).ToList();

            List<Task> taskList = new();
            ConcurrentQueue<List<(int steps, bool wasEle, ValveCell)>> possible = new();
            for (int playerStartInd = 0; playerStartInd < simplifiedValveGrid["AA"].connectedValves.Count; playerStartInd++) 
            {
                for (int eleStartInd = 0; eleStartInd < simplifiedValveGrid["AA"].connectedValves.Count; eleStartInd++)
                {
                    //ele and player not to same space
                    if (eleStartInd == playerStartInd)
                        continue;

                    ValveCell playerStartCell = simplifiedValveGrid[simplifiedValveGrid["AA"].connectedValves[playerStartInd].valve];
                    int playerStartSteps = simplifiedValveGrid["AA"].connectedValves[playerStartInd].traversalCost + 1;

                    ValveCell eleStartCell = simplifiedValveGrid[simplifiedValveGrid["AA"].connectedValves[eleStartInd].valve];
                    int eleStartSteps = simplifiedValveGrid["AA"].connectedValves[eleStartInd].traversalCost + 1;
                   
                    List<ValveCell> avail = new(valvesWithFlow);
                    avail.Remove(playerStartCell);
                    avail.Remove(eleStartCell);

                     List<(int steps, bool wasEle, ValveCell)> curPath = new();
                    curPath.Add((playerStartSteps + 1, false, playerStartCell));
                    curPath.Add((eleStartSteps + 1, true, eleStartCell));

                    Task t = new Task(() => { ValvePathMultiPlayer(playerStartCell, eleStartCell, curPath, avail, playerStartSteps + 1, eleStartSteps + 1, possible, stepCount); });
                    taskList.Add(t);
                }
            }

            int maxScore = 0;
            Task maxer = new Task(() => {
                for(; ;)
                {
                    List<(int steps, bool wasEle, ValveCell valve)>? possibleList;
                    while (possible.TryDequeue(out possibleList))
                    {

                        int score = 0;
                        for (int j = 0; j < possibleList.Count; j++)
                        {
                            score += (possibleList[j].valve.flowRate * ((stepCount + 1) - possibleList[j].steps));
                        }
                        if (score > maxScore)
                        {
                            maxScore = score;
                            Console.WriteLine("new best path: " + possibleList.Select(x => x.valve.Valve + "(" + x.steps + ", " + x.wasEle + ")").Aggregate((a, b) => a + "   " + b) + "  score: " + maxScore);
                            Thread.Sleep(1);
                        }
                    }
                    Thread.Sleep(10);
                    if (Task.WaitAll(taskList.ToArray(), 1) && possible.Count > 0)
                        break;
                }; 
            }) ;

            maxer.Start();

            foreach (Task t in taskList)
                t.Start();

            Task.WaitAll(taskList.ToArray());

            await maxer;

            Console.WriteLine("Max value found: " + maxScore);
        }

        public void ValvePathMultiPlayer(ValveCell playerCell, ValveCell eleCell, List<(int steps, bool wasEle, ValveCell)> curPath, List<ValveCell> avail, int playerSteps, 
            int eleSteps, ConcurrentQueue<List<(int steps, bool wasEle, ValveCell)>> found, int stepLimit)
        {
            if (avail.Count == 0)
            {
                found.Enqueue(curPath);
            }

            bool isEle = eleSteps < playerSteps ? true : false;
            int curStep = playerSteps;
            if (isEle)
                curStep = eleSteps;


            bool branched = false;
            foreach(var cell in avail)
            {
                int cost = 0;
                if(isEle)
                    cost = eleCell.connectedValves.Where(x => x.valve == cell.Valve).First().traversalCost;
                else
                    cost = playerCell.connectedValves.Where(x => x.valve == cell.Valve).First().traversalCost;

                if (curStep + cost + 1 > stepLimit)
                    continue;

                branched = true;
                List<ValveCell> adjusted = new(avail);
                adjusted.Remove(cell);

                List<(int steps, bool wasEle, ValveCell)> newPath = new(curPath);
                newPath.Add((curStep + cost + 1, isEle, cell));

                int newPlayerStep = playerSteps;
                int newEleStep = eleSteps;

                if (isEle)
                    newEleStep = eleSteps + cost + 1;
                else
                    newPlayerStep = playerSteps + cost + 1;


                ValveCell newPlayerCell = playerCell;
                ValveCell newEleCell = eleCell;

                if (isEle)
                    newEleCell = cell;
                else
                    newPlayerCell = cell;

                ValvePathMultiPlayer(newPlayerCell, newEleCell, newPath, adjusted, newPlayerStep, newEleStep, found, stepLimit);
            }

            if(!branched)
            {
                found.Enqueue(curPath);
            }
        }



    }
}
