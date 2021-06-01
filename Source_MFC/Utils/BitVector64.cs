using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Source_MFC.Utils
{
    [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 1, CharSet = CharSet.Ansi)]
    public struct Any64
    {
        [FieldOffset(0)]        // 'FieldOffset' offsets by byte.
        public Int64 INT64;

        [FieldOffset(0)]        // 'FieldOffset' offsets by byte.
        public UInt64 UINT64;

        [FieldOffset(0)]        // 'FieldOffset' offsets by byte.
        public float FLOAT_0;
        [FieldOffset(4)]        // float or Single is 4 bytes. So, it is FieldOffset(4).
        public float FLOAT_1;

        [FieldOffset(0)]
        public uint UINT32_0;
        [FieldOffset(4)]       //uint is 4 bytes. So, it is FieldOffset(4).
        public uint UINT32_1;

        [FieldOffset(0)]
        public ushort UINT16_0;
        [FieldOffset(2)]       //ushort is 2 bytes. So, it is FieldOffset(2).
        public ushort UINT16_1;
        [FieldOffset(4)]
        public ushort UINT16_2;
        [FieldOffset(6)]
        public ushort UINT16_3;

        [FieldOffset(0)]
        public byte UINT8_0;
        [FieldOffset(1)]       //byte is 1 byte. So, it is FieldOffset(1). increase one by one.
        public byte UINT8_1;
        [FieldOffset(2)]
        public byte UINT8_2;
        [FieldOffset(3)]
        public byte UINT8_3;
        [FieldOffset(4)]
        public byte UINT8_4;
        [FieldOffset(5)]
        public byte UINT8_5;
        [FieldOffset(6)]
        public byte UINT8_6;
        [FieldOffset(7)]
        public byte UINT8_7;

        [FieldOffset(0)]
        public int INT32_0;
        [FieldOffset(4)]       //int is 4 bytes. So, it is FieldOffset(4).
        public int INT32_1;

        [FieldOffset(0)]
        public short INT16_0;
        [FieldOffset(2)]      //short is 2 bytes. So, it is FieldOffset(2).
        public short INT16_1;
        [FieldOffset(4)]
        public short INT16_2;
        [FieldOffset(6)]
        public short INT16_3;

        [FieldOffset(0)]
        public sbyte INT8_0;
        [FieldOffset(1)]      //sbyte is 1 byte. So, it is FieldOffset(1). increase one by one.
        public sbyte INT8_1;
        [FieldOffset(2)]
        public sbyte INT8_2;
        [FieldOffset(3)]
        public sbyte INT8_3;
        [FieldOffset(4)]
        public sbyte INT8_4;
        [FieldOffset(5)]
        public sbyte INT8_5;
        [FieldOffset(6)]
        public sbyte INT8_6;
        [FieldOffset(7)]
        public sbyte INT8_7;

        [FieldOffset(0)]
        private BitVector32 LowBits;
        [FieldOffset(4)]       //BitVector32 is 4 bytes, So, it is FieldOffset(4).
        private BitVector32 HighBits;

        [FieldOffset(0)]
        private BitVector64 Bits;

        public bool this[int index]
        {
            get { return Bits[(long)(1 << index)]; }
            set { Bits[(long)(1 << index)] = value; }
        }
    }

    public struct BitVector64
    {
        long data;

        #region Section
        ///
        /// Section
        ///
        public struct Section
        {
            private short mask;
            private short offset;

            internal Section(short mask, short offset)
            {
                this.mask = mask;
                this.offset = offset;
            }

            public short Mask
            {
                get { return mask; }
            }

            public short Offset
            {
                get { return offset; }
            }
#if NET_2_0
            public static bool operator == (Section v1, Section v2)
            {
                return v1.mask == v2.mask &&
                       v1.offset == v2.offset;
            }
 
            public static bool operator != (Section v1, Section v2)
            {
                return v1.mask != v2.mask &&
                       v1.offset != v2.offset;
            }
 
            public bool Equals (Section obj)
            {
                return this.mask == obj.mask &&
                       this.offset == obj.offset;
            }
#endif
            public override bool Equals(object o)
            {
                if (!(o is Section))
                    return false;

                Section section = (Section)o;
                return this.mask == section.mask &&
                       this.offset == section.offset;
            }

            public override int GetHashCode()
            {
                return (((Int16)mask).GetHashCode() << 16) +
                       ((Int16)offset).GetHashCode();
            }

            public override string ToString()
            {
                return "Section{0x" + Convert.ToString(mask, 16) +
                       ", 0x" + Convert.ToString(offset, 16) + "}";
            }

            public static string ToString(Section value)
            {
                StringBuilder b = new StringBuilder();
                b.Append("Section{0x");
                b.Append(Convert.ToString(value.Mask, 16));
                b.Append(", 0x");
                b.Append(Convert.ToString(value.Offset, 16));
                b.Append("}");

                return b.ToString();
            }
        }
        #endregion //Section

        #region Constructors
        public BitVector64(BitVector64 source)
        {
            this.data = source.data;
        }
        public BitVector64(BitVector32 source)
        {
            this.data = source.Data;
        }

        public BitVector64(long source)
        {
            this.data = source;
        }
        public BitVector64(int init)
        {
            this.data = init;
        }
        #endregion // Constructors

        #region Properties
        public long Data
        {
            get { return this.data; }
        }

        public long this[BitVector64.Section section]
        {
            get
            {
                return ((data >> section.Offset) & section.Mask);
            }

            set
            {
                if (value < 0)
                    throw new ArgumentException("Section can't hold negative values");
                if (value > section.Mask)
                    throw new ArgumentException("Value too large to fit in section");
                this.data &= ~(section.Mask << section.Offset);
                this.data |= (value << section.Offset);
            }
        }

        public bool this[long mask]
        {
            get
            {
#if NET_2_0
                return (this.data & mask) == mask;
#else
                long tmp = /*(uint)*/this.data;
                return (tmp & (long)mask) == (long)mask;
#endif
            }

            set
            {
                if (value)
                    this.data |= mask;
                else
                    this.data &= ~mask;
            }
        }
        #endregion //Properties

        // Methods     
        public static long CreateMask()
        {
            return CreateMask(0);   // 1;
        }

        public static long CreateMask(long prev)
        {
            if (prev == 0)
                return 1;
            if (prev == Int64.MinValue)
                throw new InvalidOperationException("all bits set");
            return prev << 1;
        }

        public static Section CreateSection(int maxValue)
        {
            return CreateSection(maxValue, new Section(0, 0));
        }

        public static Section CreateSection(int maxValue, BitVector64.Section previous)
        {
            if (maxValue < 1)
                throw new ArgumentException("maxValue");

            int bit = HighestSetBit(maxValue) + 1;
            int mask = (1 << bit) - 1;
            int offset = previous.Offset + NumberOfSetBits(previous.Mask);

            if (offset > 64)
            {
                throw new ArgumentException("Sections cannot exceed 64 bits in total");
            }

            return new Section((short)mask, (short)offset);
        }

        public override bool Equals(object o)
        {
            if (!(o is BitVector64))
                return false;

            return data == ((BitVector64)o).data;
        }

        public override int GetHashCode()
        {
            return data.GetHashCode();
        }

        public override string ToString()
        {
            return ToString(this);
        }

        public static string ToString(BitVector64 value)
        {
            StringBuilder sb = new StringBuilder(0x2d);
            sb.Append("BitVector64{");
            ulong data = (ulong)value.Data;
            for (int i = 0; i < 0x40; i++)
            {
                sb.Append(((data & 0x8000000000000000) == 0) ? '0' : '1');
                data = data << 1;
            }

            sb.Append("}");
            return sb.ToString();

            //StringBuilder b = new StringBuilder();
            //b.Append("BitVector64{");
            //ulong mask = (ulong)Convert.ToInt64(0x8000000000000000);
            //while (mask > 0)
            //{
            //    b.Append((((ulong)value.Data & mask) == 0) ? '0' : '1');
            //    mask >>= 1;
            //}
            //b.Append('}');
            //return b.ToString();
        }

        // Private utilities
        private static int NumberOfSetBits(int i)
        {
            int count = 0;
            for (int bit = 0; bit < 64; bit++)
            {
                int mask = 1 << bit;
                if ((i & mask) != 0)
                    count++;
            }
            return count;
        }

        private static int HighestSetBit(int i)
        {
            for (int bit = 63; bit >= 0; bit--)
            {
                int mask = 1 << bit;
                if ((mask & i) != 0)
                {
                    return bit;
                }
            }
            return -1;
        }

        /*
            INS.BaseLib.Any64 bitField64 = new INS.BaseLib.Any64();
            bitField64.INT64 = 255; 
            bitField64.UINT8_5 = 17;
            bitField64[5] = true;
            bool bValues = bitField64[63];        
            bitField64.INT64 = 15; 
            bitField64[0] = false; 1
            bitField64[1] = true; 2
            bitField64[2] = true; 4
            bitField64[3] = true; 8
         */
    }
}
