using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace LodmodsDM
{
    public class CRC32
    {
        private uint[] _checksumTable;

        public CRC32()
        {
            this._checksumTable = new uint[256];
            Init();
        }

        /**
         * Initialize the _checksumTable aplying the polynomial by CD-XA.
         */
        private void Init()
        {
             uint polynomial = 0x8001801b;

            for (uint indexByte = 0; indexByte <= 0xFF; indexByte++)
            {
                _checksumTable[indexByte] = (uint)(Reflect(indexByte, 8) << 24);

                for (int i = 0; i <= 7; i++)
                {
                    if ((_checksumTable[indexByte] & 0x80000000L) == 0) _checksumTable[indexByte] = (_checksumTable[indexByte] << 1) ^ 0;
                    else _checksumTable[indexByte] = (_checksumTable[indexByte] << 1) ^ polynomial;
                }
                _checksumTable[indexByte] = (uint)(Reflect(_checksumTable[indexByte], 32));
            }
        }

        /**
         * Reflection is a requirement for the official CRC-32 standard. Note that you can create CRC without it,
         * but it won't conform to the standard.
         *
         * @param valToReflect
         *           value to which to apply the reflection
         * @param width
         *           bit width of value to reflect
         * @return the calculated value
         */
        private int Reflect(uint valToReflect, int width)
        {
            int returnVal = 0;
            // Swap bit 0 for bit 7, bit 1 For bit 6, etc....
            for (int i = 1; i < (width + 1); i++)
            {
                if ((valToReflect & 1) != 0)
                {
                    returnVal |= (1 << (width - i));
                }
                valToReflect >>= 1;
            }
            return returnVal;
        }

        /**
         * PartialCRC caculates the CRC32 by looping through each byte in sData
         *
         * @param data
         *           array of bytes to calculate the CRC
         * @return the new caculated CRC
         */
        public byte[] CalculateCRC(byte[] data)
        {
            if (data.Length != 0x808) throw new ArgumentException("Data must be 0x808 bytes long to calculate EDC.");

            long crc = 0;
            for (int i = 0; i < 0x808; i++)
            {
                crc = (crc >> 8) ^ _checksumTable[(int)(crc & 0xFF) ^ data[i] & 0xff] & 0xffffffffL;
            }

            return BitConverter.GetBytes((uint)(crc ^ 0x00000000));
        }

        public static void Main()
        {
            using BinaryReader reader = new BinaryReader(File.Open("D:/Game ROMs/The Legend of Dragoon/LOD1-4.iso", FileMode.Open));
            reader.BaseStream.Seek(0xCA30, SeekOrigin.Begin);
            byte[] arrayOfBytes = reader.ReadBytes(0x808);
            var crc32 = new CRC32();
            byte[] crcResult = crc32.CalculateCRC(arrayOfBytes);
            Console.WriteLine(string.Join("", crcResult));
        }
    }
}
