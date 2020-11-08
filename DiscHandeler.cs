using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ParseLoDGameData {
    class DiscHandeler {
        static readonly byte[] syncPattern = new byte[] { 0x0, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x0 };


        public class Disc {
            string path;
            byte[] systemSegment;
            PrimaryVolumeDescriptorEntry primaryVolumeDescriptor;
            PathTableEntry pathTable;
            List<DirectoryEntry> root;


            public Disc(string path) {
                this.path = path;
                BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open));
                if (!reader.ReadBytes(12).SequenceEqual(syncPattern)) {
                    throw new ArgumentException("Synchronization pattern not found. Incorrect file type.");
                }
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                systemSegment = reader.ReadBytes(0x9300);
                primaryVolumeDescriptor = new PrimaryVolumeDescriptorEntry(reader);
                pathTable = new PathTableEntry(reader);
                while (reader.ReadByte() != 0) {
                    reader.BaseStream.Seek(-1, SeekOrigin.Current);
                    root.Add(new DirectoryEntry(reader));
                }
                reader.BaseStream.Seek(2352 - reader.BaseStream.Position % 2352, SeekOrigin.Current); // seek end of current segment
                foreach (var children in root) {
                    if (children.Flags == 2) { // Folder

                    } else { // File
                        children.GetData(reader);
                    }
                }

            }
        }

        public class PrimaryVolumeDescriptorEntry {

            public PrimaryVolumeDescriptorEntry() {

            }

            public PrimaryVolumeDescriptorEntry(BinaryReader reader) {

            }
        }

        public class PathTableEntry {

            public PathTableEntry() {

            }

            public PathTableEntry(BinaryReader reader) {

            }
        }

        public class DirectoryEntry {
            byte recordLength = 30;
            byte extendedAttributeRecordLength = 0;
            UInt32 extentLocation = 22;
            UInt32 dataLength = 0;
            DateEntry date = new DateEntry();
            byte flags = 2;
            byte interleavedSize = 0;
            byte interleaveGap = 0;
            UInt16 volumeSequenceNumber = 1;
            byte nameLength = 1;
            string name = "A";
            byte[] systemUse = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x0D, 0x55, 0x58, 0x41, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] data = new byte[0];
            byte[] decompressedData = new byte[0];
            List<dynamic> children = new List<dynamic>();

            public byte RecordLength { get { return recordLength; } set { recordLength = value; } }
            public byte ExtenderAttributeRecordLenght { get { return extendedAttributeRecordLength; } set { extendedAttributeRecordLength = value; } }
            public UInt32 ExtentLocation { get { return extentLocation; } set { extentLocation = value; } }
            public UInt32 DataLength { get { return dataLength; } set { dataLength = value; } }
            public DateEntry Date { get { return date; } set { date = value; } }
            public byte Flags { get { return flags; } set { flags = value; } }
            public byte InterleavedSize { get { return interleavedSize; } set { interleavedSize = value; } }
            public byte InterleaveGap { get { return interleaveGap; } set { interleaveGap = value; } }
            public UInt16 VolumeSequenceNumbter { get { return volumeSequenceNumber; } set { volumeSequenceNumber = value; } }
            public byte NameLength { get { return nameLength; } set { nameLength = value; } }
            public string Name { get { return name; } set { name = value; } }
            public byte[] SystemUse { get { return systemUse; } set { systemUse = value; } }
            public byte[] Data { get { return data; } set { data = value; } }
            public byte[] DecompressedData { get { return decompressedData; } set { decompressedData = value; } }
            public List<dynamic> Children { get{ return children; } set { children = value; } }


            public DirectoryEntry() {

            }

            public DirectoryEntry(BinaryReader reader) {
                long position = reader.BaseStream.Position;
                recordLength = reader.ReadByte();
                extendedAttributeRecordLength = reader.ReadByte();
                extentLocation = ReadUInt32LB(reader);
                dataLength = ReadUInt32LB(reader);
                date = new DateEntry(reader);
                flags = reader.ReadByte();
                interleavedSize = reader.ReadByte();
                interleaveGap = reader.ReadByte();
                volumeSequenceNumber = ReadUInt16LB(reader);
                nameLength = reader.ReadByte();
                name = new string(reader.ReadChars(nameLength));
                if (nameLength % 2 == 0) {
                    reader.ReadByte(); // padding
                }
                int left2Read = (int)(position + recordLength - reader.BaseStream.Position);
                if (left2Read > 0) {
                    systemUse = reader.ReadBytes(left2Read);
                }


            }

            public class DateEntry {
                UInt16 year = 1900;
                byte month = 1;
                byte day = 1;
                byte hour = 0;
                byte minute = 0;
                byte second = 0;
                byte gmt = 0;

                public UInt16 Year { get { return year; } }
                public byte Month { get { return month; } }
                public byte Day { get { return day; } }
                public byte Hour { get { return hour; } }
                public byte Minute { get { return minute; } }
                public byte Second { get { return second; } }
                public byte GMT { get { return gmt; } }

                public DateEntry() {

                }

                public DateEntry(int year, int month, int day, int hour, int minute, int second, int gmt) {
                    this.year = (UInt16)(year);
                    this.month = (byte) month;
                    this.day = (byte)day;
                    this.hour = (byte)hour;
                    this.minute = (byte)minute;
                    this.second = (byte)second;
                    this.gmt = (byte)gmt;
                }
                public DateEntry(BinaryReader reader) {
                    year = (UInt16)(1900 + reader.ReadByte());
                    month = reader.ReadByte();
                    day = reader.ReadByte();
                    hour = reader.ReadByte();
                    minute = reader.ReadByte();
                    second = reader.ReadByte();
                    gmt = reader.ReadByte();
                }
            }

            public void GetData(BinaryReader reader) {
                reader.BaseStream.Seek(extentLocation * 2352, SeekOrigin.Begin); // get first data segment, rest is writen sequentially
                data = new byte[dataLength];

                for (int i = 0; i < Math.Ceiling((double)dataLength / 2048); i++) { // get number of segments
                    if (!reader.ReadBytes(12).SequenceEqual(syncPattern)) {
                        throw new ArgumentException($"Synchronization pattern not found. Incorrect file type. {name}");
                    }
                    reader.ReadBytes(12);
                    for (int j = 0; j < 2048; j++) { // read up to 2048 bytes of each segment
                        if (j + i * 2048 > dataLength - 1) {
                            break;
                        } else {
                            data[j + i * 2048] = reader.ReadByte();
                        }
                    }
                    reader.ReadBytes(4); // error detection
                    reader.ReadBytes(276); // error correction
                }
                if (name.Contains(".OV_")) {
                    decompressedData = BPE.Decompress(data);
                }
            }


        }

        public static UInt32 ReadUInt32LB(BinaryReader reader) {
            UInt32 little = reader.ReadUInt32();
            UInt32 big = BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray());
            if (little == big) {
                return little;
            } else {
                throw new ArgumentException($"Little and Big Endian do not match {little}, {big}");
            }
        }

        public static UInt16 ReadUInt16LB(BinaryReader reader) {
            UInt16 little = reader.ReadUInt16();
            UInt16 big = BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray());
            if (little == big) {
                return little;
            } else {
                throw new ArgumentException($"Little and Big Endian do not match {little}, {big}");
            }
        }

        public static byte[] WriteUInt32LB(uint value) {
            var bytes = BitConverter.GetBytes(value);
            var result = bytes.Concat(bytes.Reverse()).ToArray();
            return result;
        }

        public static byte[] WriteUInt16LB(UInt16 value) {
            var bytes = BitConverter.GetBytes(value);
            var result = bytes.Concat(bytes.Reverse()).ToArray();
            return result;
        }
    }
}
