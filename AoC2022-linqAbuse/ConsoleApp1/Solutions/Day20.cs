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
using static ConsoleApp1.Solutions.Day14;
using static ConsoleApp1.Utils;

namespace ConsoleApp1.Solutions
{
    internal class Day20 : AbstractPuzzle
    {

        override public void Part1()
        {
            Both();
        }



        override public void Part2()
        {
            Both();
        }

        public class NodeInfo
        {
            public Int64 originalValue;
            public Int64 originalP2Value;
            public Int64 originalIndex;
            public Int64 moveAmount;
            public Int64 value;

            public NodeInfo(Int64 oi, Int64 ma, Int64 ov)
            {
                originalIndex = oi;
                moveAmount = ma;
                value = ov;
                originalValue = ov;
            }

            public override string ToString()
            {
                return string.Format("[oi:{0},ov:{3},v:{1},p2v:{2}]", originalIndex, value, originalP2Value, originalValue);
            }
        }

        void Both()
        {
            var lines = File.ReadAllText(InputFile!).Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
            var nums = lines.Select((x, i) => new NodeInfo(i, Int64.Parse(x.Trim()), Int64.Parse(x.Trim() ))).ToList();

            //for part two, we make the numbers huge. pretty sure we can modulo here to reduce how much we have to scan around and shit
            var mixed = new LinkedList<NodeInfo>(nums);

            //TODO: need to make sure we move based on the original order of the nodes...annoying
            //use the originalIndex to find them in order and move them
            //needed for passes past 1.

            for (int mixCount = 0; mixCount < 10; ++mixCount)
            {
                LinkedListNode<NodeInfo> curNode = mixed.First!;

                for (var node = mixed.First; ; node = node.Next)
                {
                    node.ValueRef.originalP2Value = node.ValueRef.originalValue * 811589153;
                    node.ValueRef.value = node.ValueRef.originalValue * 811589153;
                    if (node.ValueRef.value / 811589153 != node.ValueRef.originalValue)
                    {
                        throw new Exception("overflow");
                    }
                    node.ValueRef.value = (node.ValueRef.value % (nums.Count - 1));// * Math.Sign(node.ValueRef.value);
                    //if(Math.Sign(node.ValueRef.originalValue) < 0)
                    //{
                    //    node.ValueRef.value = (nums.Count - 1) + node.ValueRef.value;
                    //}
                    node.ValueRef.moveAmount = node.ValueRef.value;
                    if (node.Next == null)
                        break;
                }

                Console.WriteLine("Starting mix: " + mixCount);
                int numMoved = 0;
                for (; ; )
                {
                    curNode = null;
                    for (var node = mixed.First; numMoved < mixed.Count; )
                    {
                        if (node.ValueRef.originalIndex == numMoved)
                        {
                            if(node.ValueRef.moveAmount == 0)
                            {
                                node = mixed.First;
                                numMoved++;
                                continue;
                            }
                            curNode = node;
                            break;
                        }

                        node = node.Next;
                    }

                    if (curNode == null)
                        break;

                    LinkedListNode<NodeInfo> toMoveNode = curNode;
                    LinkedListNode<NodeInfo> moveTo = toMoveNode!;
                    int moveDir = Math.Sign(toMoveNode.ValueRef.moveAmount);

                    // Console.Write("Moving " + toMoveNode.Value.ToString());
                    while (toMoveNode.ValueRef.moveAmount != 0)
                    {
                        toMoveNode.ValueRef.moveAmount -= moveDir;

                        if (moveDir > 0)
                        {
                            moveTo = moveTo!.Next == null ? mixed.First! : moveTo!.Next!;
                        }
                        else
                        {
                            moveTo = moveTo!.Previous == null ? mixed.Last! : moveTo!.Previous!;
                        }

                        if (moveTo == toMoveNode)
                        {
                            throw new Exception("shouldnt loop over ourselves");
                        }
                    }

                    //Console.Write(" to " + moveTo.Value.ToString());
                    //it seems like we dont ever move to the front, instead should be added to back isntead of inserted at first...
                    if(toMoveNode != moveTo)
                        mixed.Remove(toMoveNode);

                    if (moveDir < 0 && moveTo == mixed.First)
                        mixed.AddAfter(mixed.Last, toMoveNode.Value);
                    else if (moveDir > 0)
                        mixed.AddAfter(moveTo, toMoveNode.Value);
                    else
                        mixed.AddBefore(moveTo, toMoveNode.Value);

                    numMoved++;

                    /*
                    Console.WriteLine("\nmove " + numMoved);
                    for (var node = mixed.First; ; node = node.Next)
                    {
                        Console.Write(node.Value + " - ");
                        if (node.Next == null)
                            break;
                    }
                    Console.WriteLine("\n");
                    */
                }

                /*
                for (var node = mixed.First; ; node = node.Next)
                {
                    Console.Write(node.Value + " - ");
                    if (node.Next == null)
                        break;
                }
                */
                Console.WriteLine("");
            }

            //score it...
            Int64 score = 0;
            Int64 scored = 0;
            Int64 scoreSum = 0;
            bool foundZero = false;
            for (var node = mixed.First;; node = node!.Next == null ? mixed.First! : node!.Next!)
            {
                if (foundZero == false && node.ValueRef.value == 0)
                    foundZero = true;

                else if (foundZero)
                {
                    score++;

                    if (score == 1000 || score == 2000 || score == 3000)
                    {
                        scoreSum += node.ValueRef.originalP2Value;
                        Console.WriteLine("Score node: " + node.Value);
                        scored++;

                        if (scored >= 3)
                            break;
                    }
                }
            }

            Console.WriteLine("Score: " + scoreSum);
        }

    }


}
