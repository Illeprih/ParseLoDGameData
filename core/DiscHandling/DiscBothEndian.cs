using System;
using System.Collections.Generic;
using System.Linq;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace LodmodsDM
{
    class BothEndian
    {
        public static UInt16 ReadUInt16Both(BinaryReader reader)
        {
            UInt16 little = reader.ReadUInt16();
            UInt32 big = BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray());
            if (little == big)
            {
                return little;
            }
            else
            {
                throw new ArgumentException($"Little and Big Endian do not match: {little}, {big}");
            }
        }

        public static UInt32 ReadUInt32Both(BinaryReader reader)
        {
            UInt32 little = reader.ReadUInt32();
            UInt32 big = BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray());
            if (little == big)
            {
                return little;
            }
            else
            {
                throw new ArgumentException($"Little and Big Endian do not match: {little}, {big}");
            }
        }

        public static byte[] WriteUInt16Both(UInt16 value)
        {
            byte[] little = BitConverter.GetBytes(value);
            byte[] both = little.Concat(BitConverter.GetBytes(value).Reverse()).ToArray();
            return both;
        }


        public static byte[] WriteUint32Both(UInt32 value)
        {
            byte[] little = BitConverter.GetBytes(value);
            byte[] both = little.Concat(BitConverter.GetBytes(value).Reverse()).ToArray();
            return both;
        }
    }
}
