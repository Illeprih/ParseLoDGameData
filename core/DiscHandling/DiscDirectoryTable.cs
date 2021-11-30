using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text;
using static LodmodsDM.BothEndian;

namespace LodmodsDM
{
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
        public string FileIdentifier { get; }
        public byte[] SystemUse { get; }
        public string EntryType { get; private set; }
        public bool HasParent { get; set; }
        public List<SectorInfo> ChildDirSectorInfo { get; private set; }
        public List<DirectoryTableEntry> Children { get; }
        public byte NumEntries { get; private set; }

        public DirectoryTableEntry(BinaryReader reader, bool hasParent = true)
        {
            HasParent = hasParent;

            long offset = reader.BaseStream.Position;
            DirectoryLength = reader.ReadByte();
            XARecordLength = reader.ReadByte();
            ExtentLocation = ReadUInt32Both(reader);
            DataLength = ReadUInt32Both(reader);
            RecordingDatetime = new DirectoryDatetime(reader.ReadBytes(0x7));
            FileFlags = reader.ReadByte();
            InterleavedUnitSize = reader.ReadByte();
            InterleaveGapSize = reader.ReadByte();
            VolumeSequenceNumber = ReadUInt16Both(reader);
            FileIdentifierLength = reader.ReadByte();
            FileIdentifier = Encoding.ASCII.GetString(reader.ReadBytes(FileIdentifierLength));
            if (FileIdentifierLength > 1 && FileIdentifier[^2..] == ";1")
            {
                EntryType = "File";
            }
            else {
                EntryType = "Directory";
                if (HasParent)
                {
                    if (FileIdentifierLength > 1) Children = new List<DirectoryTableEntry>();
                } else Children = new List<DirectoryTableEntry>();

                if (Children != null) ChildDirSectorInfo = new List<SectorInfo>();
            }
            if (FileIdentifierLength % 2 == 0) reader.ReadByte(); // padding

            short bytesRemaining = (short)(offset + DirectoryLength - reader.BaseStream.Position);
            SystemUse = bytesRemaining > 0 ? reader.ReadBytes(bytesRemaining) : new byte[0];

            if (EntryType != "File" && Children != null)
            {
                ChildDirSectorInfo.Add(new SectorInfo());
                offset = reader.BaseStream.Position;
                reader.BaseStream.Seek(ExtentLocation * 0x930, SeekOrigin.Begin);
                ChildDirSectorInfo[^1].ReadHeaderInfo(reader);
            }

            do
            {
                if (EntryType != "File" && Children != null)
                {
                    AddDirectoryTableEntry(Children, reader);
                } else return;
            } while (reader.PeekChar() != 0x00);

            if (EntryType != "File" && Children != null) ChildDirSectorInfo[^1].ReadErrorCorrection(reader);
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
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

        public void AddDirectoryTableEntry(List<DirectoryTableEntry> entryList, BinaryReader reader)
        {
            entryList.Add(new DirectoryTableEntry(reader));
            NumEntries++;
        }
    }
}