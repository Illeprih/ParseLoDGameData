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
        public byte Sector { get; set; }
        public byte Mode { get; set; }
        public byte FileNumber { get; set; }
        public byte ChannelNumber { get; set; }
        public SubmodeByte Submode { get; set; }
        public byte CodingInfo { get; set; }
        public byte[] EDC { get; set; } = new byte[4];
        public byte[] ECC { get; set; } = new byte[276];

        public SectorInfo() { }

        public void CalculateEDC(byte[] data) { }

        public void CalculateECC(byte[] data) { }

        public class SubmodeByte
        {
            public bool EndOfFile { get; }
            public bool RealTime { get; }
            public bool Form2 { get; }
            public bool Trigger { get; }
            public bool Data { get; }
            public bool Audio { get; }
            public bool Video { get; }
            public bool EndOfRecord { get; }

            public SubmodeByte(byte flagByte)
            {
                EndOfFile = Convert.ToBoolean(flagByte & 128);
                RealTime = Convert.ToBoolean(flagByte & 64);
                Form2 = Convert.ToBoolean(flagByte & 32);
                Trigger = Convert.ToBoolean(flagByte & 16);
                Data = Convert.ToBoolean(flagByte & 8);
                Audio = Convert.ToBoolean(flagByte & 4);
                Video = Convert.ToBoolean(flagByte & 2);
                EndOfRecord = Convert.ToBoolean(flagByte & 1);
            }
        }

        public void GetHeaderInfo(BinaryReader reader)
        {
            SyncPattern = reader.ReadBytes(0xc);
            Minutes = reader.ReadByte();
            Seconds = reader.ReadByte();
            Sector = reader.ReadByte();
            Mode = reader.ReadByte();
            FileNumber = reader.ReadByte();
            ChannelNumber = reader.ReadByte();
            Submode = new SubmodeByte(reader.ReadByte());
            CodingInfo = reader.ReadByte();
            reader.ReadBytes(0x4);
        }

        public void GetErrorCorrection(BinaryReader reader)
        {
            reader.BaseStream.Seek(0x800 - (reader.BaseStream.Position % 0x930 - 0x18), SeekOrigin.Current);
            EDC = reader.ReadBytes(0x4);
            ECC = reader.ReadBytes(0x114);
        }
    }
}
