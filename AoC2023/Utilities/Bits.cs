using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Utilities
{
    public class Bits<UnderlyingType> where UnderlyingType : struct
    {
        public UnderlyingType BitHolder;

        public Bits(UnderlyingType underlyingType)
        {
            this.BitHolder = underlyingType;
        }

    }
}
