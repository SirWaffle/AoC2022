using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Utilities
{
    internal static class Various
    {
        static long LCM(Int64[] numbers)
        {
            return numbers.Aggregate(LCM);
        }
        static long LCM(Int64 a, Int64 b)
        {
            return Math.Abs(a * b) / GCD(a, b);
        }
        static Int64 GCD(Int64 a, Int64 b)
        {
            return b == 0 ? a : GCD(b, a % b);
        }


        public static void TestListCycles()
        {
            List<int> test = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var cycles = Various.ListCycles(test);
            var cyclist = cycles.Select(x => x.ToList()).ToList();
        }

        public static IOrderedEnumerable<IEnumerable<T>> ListCycles<T>(IEnumerable<T> list, int minCycleLength = 2)
        {
            //MOSTLY UNTESTED
            //creates a list of possible cycles in a list, ordered by length
            return Enumerable.Range(0, (list.Count() - 1) - minCycleLength).Select(i =>
                Enumerable.Range(i + 1, ((list.Count() - 1) - minCycleLength))
                    .Select(j => list.Skip(i).TakeWhile((val, li) => j + li < list.Count() && EqualityComparer<T>.Default.Equals(val, list.ElementAt(j + li))))
                .Where(cyc => cyc.Count() >= minCycleLength)
                .OrderByDescending(cyc => cyc.Count())
                .FirstOrDefault(new List<T>())
            )
            .Where(cyc => cyc.Count() >= minCycleLength)
            .OrderByDescending(cyc => cyc.Count());
        }
    }
}
