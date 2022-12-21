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
using System.Xml.Linq;
using System.Xml.Schema;
using static ConsoleApp1.Solutions.Day11.MonkE;
using static ConsoleApp1.Solutions.Day14;
using static ConsoleApp1.Utils;

namespace ConsoleApp1.Solutions
{
    internal class Day21 : AbstractPuzzle
    {

        override public void Part1()
        {
            Both(false);
        }



        override public void Part2()
        {
            Both(true);
        }

        class MonkE
        {
            public string name;
            public bool valSet;
            public Int64 val;
            public string op;
            public List<string> inputNames = new();
            public List<MonkE> inputMonkEs = new();

            public MonkE? parent; 

            public MonkE(string _name)
            {
                name = _name;
            }

            public void SetVal(Int64 val)
            {
                this.val = val;
                valSet = true;
            }

            public string GetChainToRootStr()
            {
                Console.WriteLine("Following chain to: " + this.ToString() + "   " + GetInputString());

                if (parent == null)
                    return name;

                return name + " -> " + parent.GetChainToRootStr();
            }

            public List<MonkE> GetMonkEChainToRoot()
            {
                if(val < 0)
                {
                    Console.WriteLine("Chain has a negative number at " + name + " of " + val);
                }
                if (parent == null)
                    return new List<MonkE>() { this } ;

                var list = new List<MonkE>() { this };
                list.AddRange(parent.GetMonkEChainToRoot());
                return list;
            }

            public string GetInputString()
            {
                if(inputNames.Count == 0)
                {
                    return name + " inputs: None";
                }
                return String.Format(name + " inputs:  0: {0} -> {1}   1: {2} -> {3}",
                    inputMonkEs[0].valSet, inputMonkEs[0].val,
                    inputMonkEs[1].valSet, inputMonkEs[1].val);
            }

            public void Reset()
            {
                if (inputMonkEs.Count > 0)
                {
                    val = 0;
                    valSet = false;

                    foreach (MonkE m in inputMonkEs)
                        m.Reset();
                }
            }

            public Int64 DoOp()
            {
                if (valSet)
                    return val;

                Int64 res =  op[0] switch
                {
                    '*' => inputMonkEs[0].DoOp() * inputMonkEs[1].DoOp(),
                    '/' => inputMonkEs[0].DoOp() / inputMonkEs[1].DoOp(),
                    '+' => inputMonkEs[0].DoOp() + inputMonkEs[1].DoOp(),
                    '-' => inputMonkEs[0].DoOp() - inputMonkEs[1].DoOp(),
                    _ => throw new Exception("invalid op")
                };

                val = res;
                valSet = true;
                return val;
            }

            public override string ToString()
            {
                return String.Format("{0} : op({3}) val( {1} -> {2} )", name, valSet, val, op);
            }
        }

        public Int64 DoOp(string op, Int64 i1, Int64 i2)
        {
            return  op switch
            {
                "*" => i1 * i2,
                "/" => i1 / i2,
                "+" => i1 + i2,
                "-" => i1 - i2,
                _ => throw new Exception("invalid op")
            };
        }

        void Both(bool part2)
        {
            var lines = File.ReadAllText(InputFile!).Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

            var nameToMonkE = new Dictionary<string, MonkE>();
            var nameToListeners = new Dictionary<string, List<string>>();

            //cczh: sllz + lgvd
            //zczc: 2
            for (int i =0; i < lines.Count(); ++i)
            {
                var splits = lines[i].Split(":");
                MonkE m = new(splits[0].Trim().ToLower());

                var exp = splits[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (exp.Length == 3)
                {
                    m.inputNames.Add(exp[0].Trim());
                    m.op = exp[1].Trim()[0].ToString();
                    m.inputNames.Add(exp[2].Trim());
                }
                else
                {
                    m.SetVal(Int64.Parse(exp[0].Trim()));
                    if (m.val < 0)
                        Console.WriteLine("Theres a negative number!");
                }

                nameToMonkE.Add(m.name, m);

                foreach (var listenM in m.inputNames)
                {
                    if (!nameToListeners.ContainsKey(listenM))
                    {
                        nameToListeners.Add( listenM, new List<string>() {  m.name } );
                    }
                    else
                    {
                        nameToListeners[listenM].Add( m.name );
                    }
                }
            }

            //wireup the chain of listeners..
            foreach(MonkE m in nameToMonkE.Values)
            {
                foreach(string name in m.inputNames)
                {
                    m.inputMonkEs.Add(nameToMonkE[name]);

                    if (nameToMonkE[name].parent != null)
                        throw new Exception("can we have multiple moneys lsitening for this one??"); //apparently not

                    nameToMonkE[name].parent = m;
                }
            }


            MonkE root = nameToMonkE["root"];
            MonkE humn = nameToMonkE["humn"];
                       

            Console.WriteLine("root value: " + root.DoOp());
            Console.WriteLine(humn.GetInputString());
            Console.WriteLine(root.GetInputString());

            //Console.WriteLine(humn.GetChainToRootStr());


            //now lets solve for humn number:
            List<MonkE> rootToHumn = humn.GetMonkEChainToRoot();
            rootToHumn.Reverse();

            Int64 rootVal = root.inputMonkEs[0].val;
            if (root.inputMonkEs[0] == rootToHumn[1])
                rootVal = root.inputMonkEs[1].val;

            //follow chain back and invert..
            for(int i = 1; i < rootToHumn.Count - 1; i++) //last is humn, no op, first is root
            {
                //string op = InvertOp(rootToHumn[i].op);
                Int64 invertVal = rootToHumn[i].inputMonkEs[0].val;
                if(rootToHumn[i].inputMonkEs[0] == rootToHumn[i + 1])
                     invertVal = rootToHumn[i].inputMonkEs[1].val;

                //deal with division here..
                Int64 newRoot = 0;

                string op;

                if (rootToHumn[i].op == "/")
                {
                    //depends on the order, if we have to multi or div...
                    if(rootToHumn[i].inputMonkEs[0] == rootToHumn[i + 1])
                    {
                        //multi
                        op = "*";
                        newRoot = DoOp(op, rootVal, invertVal);
                        Console.WriteLine("calcing: {0} {1} {2} = {3}", rootVal, op, invertVal, newRoot);
                    }
                    else
                    {
                        //WE DONT HIT THIS CASE EVER, YAY
                        //div
                        op = "/";
                        newRoot = DoOp(op, invertVal, rootVal);
                        Console.WriteLine("calcing: {0} {1} {2} = {3}", invertVal, op, rootVal, newRoot);
                    }
                }
                else if (rootToHumn[i].op == "-")
                {
                    if (rootToHumn[i].inputMonkEs[0] == rootToHumn[i + 1])
                    {
                        op = "+";
                        newRoot = DoOp(op, rootVal, invertVal);
                        Console.WriteLine("calcing: {0} {1} {2} = {3}", rootVal, op, invertVal, newRoot);
                    }
                    else
                    {
                        op = "-";
                        newRoot = DoOp(op, invertVal, rootVal);
                        Console.WriteLine("calcing: {0} {1} {2} = {3}", invertVal, op, rootVal, newRoot);
                    }
                }
                else if(rootToHumn[i].op == "*")
                {
                    op = "/";
                    newRoot = DoOp(op, rootVal, invertVal);
                    Console.WriteLine("calcing: {0} {1} {2} = {3}", rootVal, op, invertVal, newRoot);
                }
                else if (rootToHumn[i].op == "+")
                {
                    op = "-";
                    newRoot = DoOp(op, rootVal, invertVal);
                    Console.WriteLine("calcing: {0} {1} {2} = {3}", rootVal, op, invertVal, newRoot);
                }



                rootVal = newRoot;
            }

            Console.WriteLine("humn val for equality: " + rootVal);

            root.Reset();
            humn.val = rootVal;

            Console.WriteLine("root value: " + root.DoOp());
            Console.WriteLine(root.GetInputString());
           //Console.WriteLine(humn.GetChainToRootStr());
        }




    }


}
