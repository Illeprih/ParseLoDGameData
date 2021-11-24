using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text;
using static LodmodsDM.BothEndian;

namespace LodmodsDM
{
    public class DirectoryTable
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
        public List<SectorInfo> ChildrenSectorInfo { get; private set; }
        public List<DirectoryTable> Children { get; }
        public byte NumEntries { get; private set; }
        public byte[] Data { get; set; }

        public DirectoryTable(BinaryReader reader, bool hasParent=true)
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
                ChildrenSectorInfo = new List<SectorInfo>();
                Data = new byte[DataLength];
            }
            else { 
                EntryType = "Directory";
                if (HasParent) 
                { 
                    if (FileIdentifierLength > 1) Children = new List<DirectoryTable>();
                } else Children = new List<DirectoryTable>(); 

                if (Children != null) ChildrenSectorInfo = new List<SectorInfo>();
            }
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

            if (EntryType != "File" && Children != null)
            {
                ChildrenSectorInfo.Add(new SectorInfo());
                offset = reader.BaseStream.Position;
                reader.BaseStream.Seek(ExtentLocation * 0x930, SeekOrigin.Begin);
                ChildrenSectorInfo[^1].SyncPattern = reader.ReadBytes(0xc);
                ChildrenSectorInfo[^1].Minutes = reader.ReadByte();
                ChildrenSectorInfo[^1].Seconds = reader.ReadByte();
                ChildrenSectorInfo[^1].Sector = reader.ReadByte();
                ChildrenSectorInfo[^1].Mode = reader.ReadByte();
                ChildrenSectorInfo[^1].FileNumber = reader.ReadByte();
                ChildrenSectorInfo[^1].ChannelNumber = reader.ReadByte();
                ChildrenSectorInfo[^1].Submode = reader.ReadByte();
                ChildrenSectorInfo[^1].CodingInfo = reader.ReadByte();
                long dataStart = reader.BaseStream.Position + 0x4;
                reader.BaseStream.Seek(0x804, SeekOrigin.Current);
                ChildrenSectorInfo[^1].EDC = reader.ReadBytes(0x4);
                ChildrenSectorInfo[^1].ECC = reader.ReadBytes(0x114);
                reader.BaseStream.Seek(dataStart, SeekOrigin.Begin);
            }

            do
            {
                if (EntryType != "File" && Children != null) 
                { 
                    AddDirectoryTableEntry(Children, offset, reader); 
                } else return;
            } while (reader.PeekChar() != 0x00);

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

        public void AddDirectoryTableEntry(List<DirectoryTable> entryList, long entryOffset, BinaryReader reader)
        {
            entryList.Add(new DirectoryTable(reader));
            NumEntries++;
        }
    }
}