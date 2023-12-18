using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AoC2023.Utilities;

namespace AoC2023.Solutions
{
    internal class Day7 : AbstractPuzzle<Day7>
    {
        public override void Init()
        {
            DoPart1 = false;
            DoPart2 = true;
        }

        //5
        //4
        //full
        //3
        //2 pair
        //pair
        //high

        //1 left = 5 of a kind
        //2 left = full house, 4 of a kind (remove unique from hand ) == 4 or 1, then 4oak
        //3 left = 2 pair, 3 of a kind  
        //4 left = 1 pair
        //5 left = nothing

        //(hand array, bet, unique array, raw card score))


        override public void Part1Impl()
        {
            //(hand array, bet, unique array, raw card score)
            var score = File.ReadAllText(InputFilePart1).Split('\n', StringSplitOptions.None).Select(line => line.Split(' ', StringSplitOptions.TrimEntries).Chunk(2).Select(l => (l[0], Int64.Parse(l[1]), l[0].Distinct().ToArray(), l[0].Select((c, i) => new List<char>() { '2', '3', '4', '5', '6', '7', '8', '9', 'T','J','Q','K','A'}.IndexOf(c) * Math.Pow(16, 6 - i)).Sum() ) )).SelectMany(l => l).OrderBy(x => x.Item3.Length).ThenBy(x => 5 - x.Item3.Select(u => x.Item1.Count(c => c == u)).Max() ).ThenByDescending(x => x.Item4).Reverse().Select( (x, i) => x.Item2 * (long)(i + 1)).Sum();

           
            Console.WriteLine("Answer p1: " + score);

            //249748283
        }


        struct HandInfo
        {
            public double handValue;
            public Int64 bet;
            public string hand;
            public int numUniqueCards;
        }

        override public void Part2Impl()
        {         
            List<char> cards = new() { 'J', '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'Q', 'K', 'A' };
            List<HandInfo> handInfos = new List<HandInfo>();

            foreach(string line in File.ReadAllText(InputFilePart1).Split('\n', StringSplitOptions.None))
            {
                HandInfo handInfo = new HandInfo();
                handInfo.hand = line.Split(' ', StringSplitOptions.TrimEntries)[0].Trim();
                handInfo.bet = Int64.Parse(line.Split(' ', StringSplitOptions.TrimEntries)[1].Trim());
                handInfo.handValue = 0;

                Dictionary<char, int> cardCountMap = new(){ { 'J', 0 } };
                int max = 0;
                for(int index = 0; index < handInfo.hand.Length; index++)
                {
                    char card = handInfo.hand[index];
                    handInfo.handValue += (double)cards.IndexOf(card) * Math.Pow(16, 6 - index);

                    if (!cardCountMap.ContainsKey(card))
                        cardCountMap.Add(card, 0);

                    cardCountMap[card]++;
                 
                    if (card != 'J' && cardCountMap[card] > max ) 
                        max = cardCountMap[card];
                }

                handInfo.numUniqueCards = Math.Max(cardCountMap.Keys.Count - 1, 1);
                handInfo.handValue += (double)(6 - handInfo.numUniqueCards) * Math.Pow(16, 8);
                handInfo.handValue += (double)(max + cardCountMap['J']) * Math.Pow(16, 7);

                handInfos.Add(handInfo);
            }

            handInfos = handInfos.OrderByDescending(hand => hand.handValue).ToList();

            Int64 totalScore = 0;
            for(int i = 0; i < handInfos.Count; i++) 
                totalScore += handInfos[i].bet * (handInfos.Count - i);

            Console.WriteLine("Answer p2: " + totalScore); //248029057
        }


        public void Part2Impl_o()
        {

            //(item1 = hand char array, item2 = int64 bet, item3 = unique cards char array, item4: double base16 hash of hand)
            var score = 
                File.ReadAllText(InputFilePart1)
                .Split('\n', StringSplitOptions.None)
                .Select(line => 
                    line.Split(' ', StringSplitOptions.TrimEntries)
                    .Chunk(2)
                    .Select(handAndBetArray => 
                           (
                            handAndBetArray[0], 
                            Int64.Parse(handAndBetArray[1]),
                            handAndBetArray[0]
                                .Distinct()
                                .Where(c => c != 'J').ToArray(),
                            handAndBetArray[0].Select((c, ind) => new List<char>() { 'J', '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'Q', 'K', 'A' }
                                                                                 .IndexOf(c) * Math.Pow(16, 6 - ind))
                                .Sum()
                            )
                    )
                )
                .SelectMany(l => l)
                .OrderBy(handEntry => Math.Max(handEntry.Item3.Length,1))
                .ThenByDescending(handEntry => handEntry.Item3
                                            .Select(uniquCardChar => handEntry.Item1.Count(handCardChar => handCardChar == uniquCardChar || handCardChar == 'J'))
                                            .Append(5 - handEntry.Item3.Length)
                                            .Max()
                 )
                .ThenByDescending(handEntry => handEntry.Item4)
                .Reverse()
                .Select((handEntry, ind) => handEntry.Item2 * (long)(ind + 1))
                .Sum();

            Console.WriteLine("Answer p2: " + score);  //248029057

        }
    }
}
