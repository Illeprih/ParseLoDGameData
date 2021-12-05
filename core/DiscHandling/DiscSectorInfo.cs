using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static LodmodsDM.Globals;

namespace LodmodsDM
{
    public class SectorInfo
    {
        public byte[] SyncPattern { get; set; }
        public byte Minutes { get; set; }
        public byte Seconds { get; set; }
        public byte Sectors { get; set; }
        public byte Mode { get; set; }
        public byte FileNumber { get; set; }
        public byte ChannelNumber { get; set; }
        public SubmodeByte Submode { get; set; }
        public byte CodingInfo { get; set; }
        public byte[] EDC { get; set; } = new byte[4];
        public byte[] ECC { get; set; } = new byte[276];

        public SectorInfo() { }

        public SectorInfo(byte[] syncPattern, byte minutes, byte seconds, byte sectors, byte mode,
            byte fileNumber, byte channelNumber, SubmodeByte submode, byte codingInfo)
        {
            SyncPattern = syncPattern;
            Minutes = minutes;
            Seconds = seconds;
            Sectors = sectors;
            Mode = mode;
            FileNumber = fileNumber;
            ChannelNumber = channelNumber;
            Submode = submode;
            CodingInfo = codingInfo;
        }

        public class SubmodeByte
        {
            public byte EndOfFile { get; private set; }
            public byte RealTime { get; private set; }
            public byte Form2 { get; private set; }
            public byte Trigger { get; private set; }
            public byte Data { get; private set; }
            public byte Audio { get; private set; }
            public byte Video { get; private set; }
            public byte EndOfRecord { get; private set; }

            public SubmodeByte(byte flagByte)
            {
                ByteToSubmode(flagByte);
            }

            public static SubmodeByte GenerateSubmode(byte flagByte)
            {
                return new SubmodeByte(flagByte);
            }

            public void ByteToSubmode(byte flagByte)
            {
                EndOfFile = (byte)((flagByte & 128) >> 7);
                RealTime = (byte)((flagByte & 64) >> 6);
                Form2 = (byte)((flagByte & 32) >> 5);
                Trigger = (byte)((flagByte & 16) >> 4);
                Data = (byte)((flagByte & 8) >> 3);
                Audio = (byte)((flagByte & 4) >> 2);
                Video = (byte)((flagByte & 2) >> 1);
                EndOfRecord = (byte)(flagByte & 1);
            }

            public byte SubmodeToByte()
            {
                return (byte)(EndOfFile<<7 | RealTime<<6 | Form2<<5 | Trigger<<4 | Data<<3 | Audio<<2 | Video<<1 | EndOfRecord);
            }
        }

        public void ReadHeaderInfo(BinaryReader reader)
        {
            SyncPattern = reader.ReadBytes(0xc);
            Minutes = reader.ReadByte();
            Seconds = reader.ReadByte();
            Sectors = reader.ReadByte();
            Mode = reader.ReadByte();
            FileNumber = reader.ReadByte();
            ChannelNumber = reader.ReadByte();
            Submode = new SubmodeByte(reader.ReadByte());
            CodingInfo = reader.ReadByte();
            reader.ReadBytes(0x4);
        }

        public static byte BCDToByte(byte bcd)
        {
            byte tens = (byte)((bcd >> 4) * 10);
            byte ones = (byte)(bcd & 0b00001111);

            return (byte)(tens + ones);
        }

        public static byte ByteToBCD(byte hexByte)
        {
            byte upper = (byte)(hexByte / 10 << 4);
            byte lower = (byte)(hexByte % 10);

            return (byte)(upper + lower);
        }

        public void ReadErrorCorrection(BinaryReader reader, uint sectorReadSize, bool isForm2)
        {
            int headerCorrection = sectorReadSize == 0x800 ? 0x18 : 0x0;
            reader.BaseStream.Seek(
                reader.BaseStream.Position / 0x930 * 0x930 + headerCorrection + sectorReadSize, SeekOrigin.Begin);
            EDC = reader.ReadBytes(0x4);
            ECC = !isForm2 ? reader.ReadBytes(0x114) : null;
        }

        public byte[] CalculateEDC(byte[] data, int dataSize) 
        {
            byte[] edcData = new byte[dataSize + 8];
            byte[] subheader = new byte[8] { FileNumber, ChannelNumber, Submode.SubmodeToByte(), CodingInfo,
                                                FileNumber, ChannelNumber, Submode.SubmodeToByte(), CodingInfo };
            Buffer.BlockCopy(subheader, 0, edcData, 0, 0x8);
            Buffer.BlockCopy(data, 0, edcData, 0x8, dataSize);

            // Calculate CRC32
            long crc = 0;
            for (int i = 0; i < dataSize + 8; i++)
            {
                crc = (crc >> 8) ^ EDC_TABLE[(int)(crc & 0xFF) ^ edcData[i] & 0xff] & 0xffffffffL;
            }

            EDC = BitConverter.GetBytes((uint)(crc ^ 0x00000000));

            return EDC;
        }

        public byte[] ComputeECCBlock(byte[] src, uint major_count, uint minor_count,
                                     uint major_mult, uint minor_inc)
        {
            uint size = major_count * minor_count;
            uint major, minor;
            byte[] dest = new byte[2 * major_count];
            for (major = 0; major < major_count; major++)
            {
                uint index = (major >> 1) * major_mult + (major & 1);
                byte ecc_a = 0;
                byte ecc_b = 0;
                for (minor = 0; minor < minor_count; minor++)
                {
                    byte temp = src[index];
                    index += minor_inc;
                    if (index >= size) index -= size;
                    ecc_a ^= temp;
                    ecc_b ^= temp;
                    ecc_a = ECC_F_LUT[ecc_a];
                }
                ecc_a = ECC_B_LUT[ECC_F_LUT[ecc_a] ^ ecc_b];
                dest[major] = ecc_a;
                dest[major + major_count] = (byte)(ecc_a ^ ecc_b);
            }
            return dest;
        }

        public byte[] CalculateECC(byte[] data) 
        {
            byte[] parityPData = new byte[0x810];
            byte[] parityQData = new byte[0x8bc];
            // Header is zeroed out in Mode 2 ECC
            byte[] subheader = new byte[8] { FileNumber, ChannelNumber, Submode.SubmodeToByte(), CodingInfo,
                                             FileNumber, ChannelNumber, Submode.SubmodeToByte(), CodingInfo };
            Buffer.BlockCopy(subheader, 0, parityPData, 0x4, 0x8);
            Buffer.BlockCopy(data, 0, parityPData, 0xc, 0x800);
            Buffer.BlockCopy(EDC, 0, parityPData, 0x80c, 0x4);

            byte[] parityP = ComputeECCBlock(parityPData, 86, 24, 2, 86);

            Buffer.BlockCopy(parityPData, 0, parityQData, 0, 0x810);
            Buffer.BlockCopy(parityP, 0, parityQData, 0x810, 0xac);

            byte[] parityQ = ComputeECCBlock(parityQData, 52, 43, 86, 88);

            Buffer.BlockCopy(parityP, 0, ECC, 0, 0xac);
            Buffer.BlockCopy(parityQ, 0, ECC, 0xac, 0x68);

            return ECC;
        }
    }
}
