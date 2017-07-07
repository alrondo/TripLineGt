using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripLine.Toolbox.Extensions
{
    public static class BitFieldHelpers
    {
        public static byte SetBits(this byte field, int offset, int mask, byte bitsValue)
        {
            field &= (byte)~(mask << offset);
            field |= (byte)((bitsValue & mask) << offset);
            return field;
        }

        public static ushort SetBits(this ushort field, int offset, int mask, ushort bitsValue)
        {
            field &= (ushort)~(mask << offset);
            field |= (ushort)((bitsValue & mask) << offset);
            return field;
        }

    }
}
