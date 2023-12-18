using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Utilities
{ 
    public static class EnumerableExt
    {
        public static IEnumerable<Int64> Range64(Int64 start, Int64 count)
        {
            Int64 max = start + count - 1;
            if (count < 0 || max > Int64.MaxValue || max < start)
                throw new ArgumentOutOfRangeException();

            for (Int64 current = 0; current < count; ++current)
                yield return start + current;
        }
    }
}
