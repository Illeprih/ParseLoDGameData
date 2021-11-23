using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static LodmodsDM.BothEndian;

namespace LodmodsDM
{
    public class DirectoryTable
    {
        public byte NumEntries { get; private set; }
        public List<DirectoryTableEntry> DirectoryEntries { get; set; }

        public class DirectoryTableEntry
        {
            public byte DirectoryLength { get; private set; }
            public byte XARecordLength { get; private set; }
            public UInt32 ExtentLocation { get; private set; } // Then big-endian copy
            public UInt32 DataLength { get; } //Then big-endian copy
            public DirectoryDatetime RecordingDatetime { get; }
            public byte FileFlags { get; } // may need to implement BitFlag class at somepoint
            public byte InterleavedUnitSize { get; } // 0 means not interleaved, but shouldn't it be?
            public byte InterleaveGapSize { get; } // 0 means not interleaved
            public UInt16 VolumeSequenceNumber { get; } // Then big-endian copy
            public byte FileIdentifierLength { get; }
            public byte[] FileIdentifier { get; }
            public byte[] SystemUse { get; }

            public DirectoryTableEntry(BinaryReader reader)
            {
                long offset = reader.BaseStream.Position;
                DirectoryLength = reader.ReadByte();
                XARecordLength = reader.ReadByte();
                ExtentLocation = ReadUInt32Both(reader);
                RecordingDatetime = new DirectoryDatetime(reader.ReadBytes(7));
                FileFlags = reader.ReadByte();
                InterleavedUnitSize = reader.ReadByte();
                InterleaveGapSize = reader.ReadByte();
                VolumeSequenceNumber = ReadUInt16Both(reader);
                FileIdentifierLength = reader.ReadByte();
                FileIdentifier = reader.ReadBytes(FileIdentifierLength);
                if (FileIdentifierLength % 2 == 0)
                {
                    reader.ReadByte(); // padding
                }
                short bytesRemaining = (short)(offset + DirectoryLength - reader.BaseStream.Position);
                if (bytesRemaining > 0)
                {
                    SystemUse = reader.ReadBytes(bytesRemaining);
                }
                else
                {
                    SystemUse = new byte[0];
                }
            }
        }

        public class DirectoryDatetime
        {
            public byte YearsSince1900 { get; set; }
            public byte Month { get; set; }
            public byte Day { get; set; }
            public byte Hour { get; set; }
            public byte Minute { get; set; }
            public byte Second { get; set; }
            public sbyte GMT { get; set; }

            public DirectoryDatetime() { }

            public DirectoryDatetime(byte[] datetime)
            {
                YearsSince1900 = datetime[0];
                Month = datetime[1];
                Day = datetime[2];
                Hour = datetime[3];
                Minute = datetime[4];
                Second = datetime[5];
                GMT = Convert.ToSByte(datetime[6]);
            }
        }

        public DirectoryTable()
        {
            NumEntries = 0;
            DirectoryEntries = new List<DirectoryTableEntry>();
        }

        public void AddDirectoryTableEntry(BinaryReader reader)
        {
            DirectoryEntries.Add(new DirectoryTableEntry(reader));
            NumEntries++;
        }
    }
}