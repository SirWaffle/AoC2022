using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Solutions
{
    internal abstract class AbstractPuzzle<PuzzleDay> where PuzzleDay : class
    {
        public string InputFilePart1
        {
            get;
            set;
        } = @"../../../InputFiles/" + typeof(PuzzleDay).Name + "_1.txt";

        public string InputFilePart2
        {
            get;
            set;
        } = @"../../../InputFiles/" + typeof(PuzzleDay).Name + "_2.txt";

        public string InputFileSample
        {
            get;
            set;
        } = @"../../../InputFiles/" + typeof(PuzzleDay).Name + "_sample.txt";


        public bool DoPart1 { get; set; } = false;
        public bool DoPart2 { get; set; } = false;

        public ConcurrentDictionary<string, dynamic> Stats { get; set; } = new();


        public AbstractPuzzle()
        {
            Init();
        }

        abstract public void Init();

        public dynamic? GetStat(string name)
        { 
            Stats.TryGetValue(name, out var stat);
            return stat;
        }

        public void SetStat(string name, dynamic value)
        {
            Stats.AddOrUpdate(name, value, (Func<string, dynamic, dynamic>)((k, v) => value));
        }

        public void LogStats()
        {
            List<string> keys = Stats.Keys.ToList();

            Log("---------- Stats ----------");
            foreach(var key in keys)
            {
                dynamic stat = GetStat(key)!;
                Log( String.Format("{0,-10} -> {1,-15}", key, stat.ToString()));
            }
            Log("---------------------------\n\n\n");
        }

        public void Log(string message) 
        { 
            Console.WriteLine(message);
        }

        public void Part1(Stopwatch timer) {
            if (DoPart1) Part1Impl();
            timer.Stop();
        }
        abstract public void Part1Impl();

        public void Part2(Stopwatch timer)
        {
            if(DoPart2) Part2Impl();
            timer.Stop();
        }
        abstract public void Part2Impl();
    }
}
