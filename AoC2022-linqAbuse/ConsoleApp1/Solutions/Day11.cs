using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConsoleApp1.Solutions
{
    internal class Day11 : AbstractPuzzle
    {
        public override void Part1()
        {
            Both(20, (item, testFactor) => item / 3);
        }             

        override public void Part2()
        {
            Both(10000, (item, testFactor) => item % testFactor);
        }

        public class Monk
        {            
            public enum Op
            {
                None,
                Add,
                Sub,
                Div,
                Mult
            }

            public List<UInt64> items = new();

            public Op op = Op.None;
            public bool useOld = false;
            public UInt64 opVal = 0;

            public UInt64 testOpNum = 0;
            public int passTestMonkNum = -1;
            public int failTestMonkNum = -1;

            public UInt64 inspectedCount = 0;
        }

        public void Both(int numRounds, Func<UInt64, UInt64, UInt64> anxiolytic)
        {
            //monkey data
            List<Monk> monks = new List<Monk>();

            //parse
            var monkStrs = File.ReadAllText(InputFile!).Split("\r\n\r\n").Select( m => m.Split("\r\n").Select( x => x.Split(":")[1].Trim()).ToList());            
            UInt64 totalCombinedTestFactors = 1;

            foreach (var monkData in monkStrs)
            {
                Monk m = new();
                m.items = monkData[1].Split(",").Select(x => x.Trim()).Select(x => UInt64.Parse(x)).ToList();

                if (monkData[2].Contains('*'))
                    m.op = Monk.Op.Mult;
                else if (monkData[2].Contains('+'))
                    m.op = Monk.Op.Add;

                var val = monkData[2].Split(' ').Last().Trim();
                if (val == "old")
                    m.useOld = true;
                else
                    m.opVal = UInt64.Parse(val);

                m.testOpNum = UInt64.Parse(monkData[3].Split(' ').Last().Trim());

                totalCombinedTestFactors *= m.testOpNum;

                m.passTestMonkNum = int.Parse(monkData[4].Split(' ').Last().Trim());
                m.failTestMonkNum = int.Parse(monkData[5].Split(' ').Last().Trim());

                monks.Add(m);
            }

            //run game
            for(int rounds = 0; rounds < numRounds; ++rounds)
            {
                for(int player = 0; player < monks.Count; ++player)
                {
                    Monk m = monks[player];
                    while(m.items.Count() > 0)
                    {
                        m.inspectedCount++;

                        //pop item from front
                        UInt64 itemVal = m.items[0];
                        m.items.RemoveAt(0);

                        //do op
                        UInt64 opVal = m.useOld == true ? itemVal : m.opVal;                        
                        itemVal = m.op switch
                        {
                            Monk.Op.Mult => itemVal * opVal,
                            Monk.Op.Div => itemVal / opVal,
                            Monk.Op.Add => itemVal + opVal,
                            Monk.Op.Sub => itemVal - opVal,
                            _ => throw new Exception("invalid op")
                        };

                        //handle anxiety
                        itemVal = anxiolytic(itemVal, totalCombinedTestFactors);

                        //do check and give to next monkey, push to back
                        if ( itemVal % m.testOpNum == 0)
                            monks[m.passTestMonkNum].items.Add(itemVal);
                        else
                            monks[m.failTestMonkNum].items.Add(itemVal);                       
                    }
                }
            }

            //output
            Console.WriteLine("total val: " + monks.OrderByDescending(x => x.inspectedCount).Take(2).Select(x => x.inspectedCount).Aggregate((x, y) => x * y));

        }
    }
}
