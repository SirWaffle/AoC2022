using System.Collections.Concurrent;
using System.ComponentModel.Design;

namespace ConsoleApp1.Solutions
{
    internal class Day25 : AbstractPuzzle
    {
        override public void Part1()
        {
            Both(false);
        }
   
        override public void Part2()
        {
            Both(true);
        }



        Dictionary<int, string> DecToSnafu = new();

        void Both(bool part2)
        {
            //hack a lookup table cus lazy, good for testing
            //these are correct
            DecToSnafu.Add(0, "0");
            DecToSnafu.Add(1, "1");
            DecToSnafu.Add(2, "2");
            DecToSnafu.Add(3, "1=");
            DecToSnafu.Add(4, "1-");
            DecToSnafu.Add(5, "10");
            DecToSnafu.Add(6, "11");
            DecToSnafu.Add(7, "12");
            DecToSnafu.Add(8, "2=");
            DecToSnafu.Add(9, "2-");
            DecToSnafu.Add(10, "20");
            DecToSnafu.Add(11, "21");
            DecToSnafu.Add(12, "22");
            DecToSnafu.Add(13, "1==");
            DecToSnafu.Add(14, "1=-");
            DecToSnafu.Add(15, "1=0");
            DecToSnafu.Add(16, "1=1");
            DecToSnafu.Add(17, "1=2");
            DecToSnafu.Add(18, "1-="); 
            DecToSnafu.Add(19, "1--"); 
            DecToSnafu.Add(20, "1-0");     // 0 1 2 3 4

            //2=00   == 200
            // powers of 5
            // 5  25  125   625   3125   15625   78125

            //2== is 38

            //do tests...
            DecimalToSnafu(11);
            for(int testNum = 0; testNum < 400; ++testNum)
            {
                string testSnaf = DecimalToSnafu(testNum);
                if(testSnaf != "FAILED_TO_CONVERT")
                    Console.WriteLine("   and bak to snafu: " + SnafuToDecimal(testSnaf));
                else
                    Console.WriteLine("   and bak to snafu: " + testSnaf);

            }

            var lines = File.ReadAllText(InputFile!).Split("\r\n").Select(x => x.Trim()).ToList();
            Int64 sum = 0;
            foreach (var line in lines)
            {
                if (true)
                {
                    Int64 snafToDec = SnafuToDecimal(line);
                    string backToSnaf = DecimalToSnafu(snafToDec);
                    Int64 backToDec =  SnafuToDecimal(backToSnaf);

                    Console.WriteLine("Snafu: " + line + " to decimal " + snafToDec + " to snafu " + backToSnaf + " and back to dec " + backToDec);
                    if(snafToDec != backToDec)
                    {
                        Console.WriteLine("mismatch");
                    }
                    if(line != backToSnaf)
                    {
                        Console.WriteLine("mismatch");
                    }
                    sum += snafToDec;
                }
                
            }
            Console.WriteLine("Snafu sum: " + sum); //34279402189875

            string snafu = DecimalToSnafu(sum);

            Console.WriteLine("dec to snafu: " + snafu + " converted back to decimal " + SnafuToDecimal(snafu));
            Console.WriteLine("delta between ints: " + (sum - SnafuToDecimal(snafu)));

        }

        Int64 SnafuToDecimal(string snafu)
        {
            //valid chars: 2,1,0,-,=
            Int64 val = 0;

            //125  25  5  1
            Int64 baseMultiple = 1;
            for(int i = snafu.Length - 1; i >= 0; --i)
            {
                if (snafu[i] == '\0')
                    continue;

                Int64 num = snafu[i] switch
                {
                    '0' => 0,
                    '1' => baseMultiple,
                    '2' => baseMultiple * 2,
                    '=' => baseMultiple * -2,
                    '-' => baseMultiple * -1,
                    _ => throw new Exception("invalid digit in snafu")
                };

                val += num;

                //next spot
                baseMultiple *= 5;
            }

            return val;
        }

        string DecimalToSnafu(Int64 dec)
        {
            string snafu = String.Empty;

            //lets..find the largest power of 5 larger than our number..
            Int64 baseMultiple = 1;
            int count = 0;
            Int64 sumOfSquares = 1;
            while (baseMultiple < dec)
            {               
                baseMultiple *= 5;
                sumOfSquares += baseMultiple;
                count++;
            }

            //f it, exhaustive search...
            string result = string.Empty;
            char[] snafuChars = new char[30];
            if (DoSearch(ref snafuChars, 0, baseMultiple, sumOfSquares, dec, dec, ref result))
            {
                //remove null spots
                result = result.Where(x => x != '\0').Select(x => x.ToString()).Aggregate((a, b) => a + b);
                //trim leading zeros
                while (result[0] == '0' && result.Length > 1)
                    result = result.Substring(1);
                return result;
            }

            Console.WriteLine(" FAILED TO CONVERT " + dec + "! Failed snafu is " + result);
            return result;
        }

        bool DoSearch(ref char[] snafuString, int charInd, Int64 baseMultiplier, Int64 sumOfCurrentAndReamingSquares, Int64 remainingNumber, Int64 roriginalNumber, ref string result)
        {
            if (remainingNumber > (baseMultiplier * 2) + (sumOfCurrentAndReamingSquares * 2))
            {
                //failed somewher ebefore here, cant get up to the number
                return false;
            }
            if (remainingNumber < -1 * ((baseMultiplier * 2) + (sumOfCurrentAndReamingSquares * 2)))
            {
                return false;
            }

            if(baseMultiplier == 1 && 
                (remainingNumber > 2 || remainingNumber < -2) )
            {
                //fail
                return false;
            }
            else if(baseMultiplier == 1)
            {
                snafuString[charInd] = DecValToSnafuDigit(remainingNumber);
                //pad remaining with nulls
                for (int i = charInd + 1; i < snafuString.Length; ++i)
                    snafuString[i] = '\0';
                string snafuStr = new string(snafuString);
                //Console.WriteLine("Completed snafu guess for " + roriginalNumber + ". Snafu guess: " + snafuStr + " back to decimal " + SnafuToDecimal(snafuStr));
                
                result = snafuStr;

                Int64 guess = SnafuToDecimal(snafuStr);
                if (guess == roriginalNumber)
                {
                    return true;
                }
                return true; // this is the end, but we messed up somehow
                return false;
            }

            if( remainingNumber >= baseMultiplier)
            {
                //at least a 2, might require some minuses later somewhere
                snafuString[charInd] = '2';
                Int64 newRemaining = remainingNumber - (baseMultiplier * 2);
                if (DoSearch(ref snafuString, charInd + 1, baseMultiplier / 5, sumOfCurrentAndReamingSquares - baseMultiplier, newRemaining, roriginalNumber, ref result))
                    return true;
            }

            //if (remainingNumber > baseMultiplier - (sumOfCurrentAndReamingSquares * 2))
            {
                //could be a 1, or 0
                snafuString[charInd] = '1';
                Int64 newRemaining = remainingNumber - (baseMultiplier);
                if (DoSearch(ref snafuString, charInd + 1, baseMultiplier / 5, sumOfCurrentAndReamingSquares - baseMultiplier, newRemaining, roriginalNumber, ref result))
                    return true;
            }
           // else
            {
                //could also be any combination of 0, -, =
                snafuString[charInd] = '0';
                Int64 newRemaining = remainingNumber;
                if (DoSearch(ref snafuString, charInd + 1, baseMultiplier / 5, sumOfCurrentAndReamingSquares - baseMultiplier, remainingNumber, roriginalNumber, ref result))
                    return true;

                snafuString[charInd] = '=';
                newRemaining = remainingNumber - (baseMultiplier * -2);
                if (DoSearch(ref snafuString, charInd + 1, baseMultiplier / 5, sumOfCurrentAndReamingSquares - baseMultiplier, newRemaining, roriginalNumber, ref result))
                    return true;

                snafuString[charInd] = '-';
                newRemaining = remainingNumber - (baseMultiplier * -1);
                if (DoSearch(ref snafuString, charInd + 1, baseMultiplier / 5, sumOfCurrentAndReamingSquares - baseMultiplier, newRemaining, roriginalNumber, ref result))
                    return true;
            }
            return false;
        }

        char DecValToSnafuDigit(Int64 dec)
        {
            char snafChar = dec switch
            {
                0 => '0',
                1 => '1',
                2 => '2',
                -2 => '=',
                -1 => '-',
                _ => throw new Exception("invalid digit! 0 through 4 expected")
            };

            return snafChar;
        }


        //this is incorect, worth a try though
        string DecimalToSnafuStandardBaseConversion(Int64 dec)
        {
            string snafu = String.Empty;

            //then lets work backwards, dividing and modding... and appending who knows what i guess
            Int64 workingNum = dec;
            while (workingNum > 0)
            {
                Int64 mod = workingNum % 5;
                workingNum = workingNum / 5;

                char snafChar = mod switch
                {
                    0 => '0',
                    1 => '1',
                    2 => '2',
                    3 => '=',
                    4 => '-',
                    _ => throw new Exception("invalid digit in snafu")
                };

                snafu += snafChar;

            }

            snafu = snafu.Reverse().Select(x => x.ToString()).Aggregate((a, b) => a + b);
            return snafu;
        }

    }
}
