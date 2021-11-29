using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            public byte EndOfFile { get; set; }
            public byte RealTime { get; private set; }
            public byte Form2 { get; private set; }
            public byte Trigger { get; private set; }
            public byte Data { get; private set; }
            public byte Audio { get; private set; }
            public byte Video { get; private set; }
            public byte EndOfRecord { get; set; }

            public SubmodeByte(byte flagByte)
            {
                ConvertByteToSubmode(flagByte);
            }

            public void ConvertByteToSubmode(byte flagByte)
            {
                EndOfFile = (byte)((flagByte & 128)>>7);
                RealTime = (byte)((flagByte & 64)>>6);
                Form2 = (byte)((flagByte & 32)>>5);
                Trigger = (byte)((flagByte & 16)>>4);
                Data = (byte)((flagByte & 8)>>3);
                Audio = (byte)((flagByte & 4)>>2);
                Video = (byte)((flagByte & 2)>>1);
                EndOfRecord = (byte)(flagByte & 1);
            }

            public static SubmodeByte ByteToSubmode(byte flagByte)
            {
                return new SubmodeByte(flagByte);
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

        public void ReadErrorCorrection(BinaryReader reader)
        {
            reader.BaseStream.Seek(0x800 - (reader.BaseStream.Position % 0x930 - 0x18), SeekOrigin.Current);
            EDC = reader.ReadBytes(0x4);
            ECC = reader.ReadBytes(0x114);
        }

        public byte[] CalculateEDC(byte[] data, int dataSize) 
        {
            byte[] edcData = new byte[dataSize + 8];
            byte[] subheader = new byte[8] { FileNumber, ChannelNumber, Submode.SubmodeToByte(), CodingInfo,
                                             FileNumber, ChannelNumber, Submode.SubmodeToByte(), CodingInfo };
            Buffer.BlockCopy(subheader, 0, edcData, 0, 0x8);
            Buffer.BlockCopy(data, 0, edcData, 0x8, dataSize);

            EDC = new CRC32().CalculateCRC(edcData);

            return EDC;
        }

        public byte[] CalculateECC(byte[] data) 
        {
            byte[] parityPData = new byte[0x810];
            byte[] parityQData = new byte[0x8bc];
            byte[] header = new byte[4] { Minutes, Seconds, Sectors, Mode };
            byte[] subheader = new byte[8] { FileNumber, ChannelNumber, Submode.SubmodeToByte(), CodingInfo,
                                             FileNumber, ChannelNumber, Submode.SubmodeToByte(), CodingInfo };
            Buffer.BlockCopy(header, 0, parityPData, 0, 0x4);
            Buffer.BlockCopy(subheader, 0, parityPData, 0x4, 0x8);
            Buffer.BlockCopy(data, 0, parityPData, 0xc, 0x800);
            Buffer.BlockCopy(EDC, 0, parityPData, 0x80c, 0x4);

            // Do stuff
            byte[] parityP = new byte[0xac];

            Buffer.BlockCopy(parityPData, 0, parityQData, 0, 0x810);
            Buffer.BlockCopy(parityP, 0, parityQData, 0x810, 0xac);

            // Do stuff
            byte[] parityQ = new byte[0x68];
            //Buffer.BlockCopy(parityP, 0, ECC, 0, 0xac);
            //Buffer.BlockCopy(parityQ, 0, ECC, 0xac, 0x68);

            return ECC;
        }
    }
}
