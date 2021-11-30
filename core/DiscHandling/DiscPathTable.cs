using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static LodmodsDM.DirectoryTableEntry;

namespace LodmodsDM
{
    public class PathTable
    {
        public UInt32 ExtentLocation { get; }
        public bool TypeIsL { get; }
        public bool IsOptionalTable { get; }
        public SectorInfo PathTableSectorInfo { get; } = new SectorInfo();
        public List<PathTableEntry> PathEntries { get; private set; }

        public PathTable(BinaryReader reader, UInt32 extentLocation, bool typeIsL, bool isOptional)
        {
            long offset = reader.BaseStream.Position;
            reader.BaseStream.Seek(extentLocation * 0x930, SeekOrigin.Begin);
            PathTableSectorInfo.ReadHeaderInfo(reader);
            ExtentLocation = extentLocation;
            TypeIsL = typeIsL;
            IsOptionalTable = isOptional;
            PathEntries = new List<PathTableEntry>();

            do
            {
                PathEntries.Add(new PathTableEntry(reader, TypeIsL));
            } while (reader.PeekChar() != 0x00);

            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
        }

        public class PathTableEntry
        {
            public byte DirectoryIdentifierLength { get; }
            public byte XaRecordLength { get; }
            public UInt32 ExtentLocation { get; private set; }
            public UInt16 ParentDirNum { get; }
            public string DirectoryIdentifier { get; }
            // public List<DirectoryTableEntry> Children { get; } // Program will not use Path Tables, just need to be updated

            public PathTableEntry(BinaryReader reader, bool type)
            {
                DirectoryIdentifierLength = reader.ReadByte();
                XaRecordLength = reader.ReadByte();
                byte[] extentLocation = reader.ReadBytes(4);
                ExtentLocation = type ? BitConverter.ToUInt32(extentLocation) 
                                      : BitConverter.ToUInt32(extentLocation.Reverse().ToArray());
                byte[] parentDirNum = reader.ReadBytes(2);
                ParentDirNum = type ? BitConverter.ToUInt16(parentDirNum)
                                    : BitConverter.ToUInt16(parentDirNum.Reverse().ToArray());
                DirectoryIdentifier = Encoding.ASCII.GetString(reader.ReadBytes(DirectoryIdentifierLength));
                if (DirectoryIdentifierLength % 2 == 1) reader.ReadByte();
            }
        }
    }
}
