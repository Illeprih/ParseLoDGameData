using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ParseLoDGameData {
    class DiscHandeler {
        static readonly byte[] syncPattern = new byte[] { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 };
        static readonly byte[] pvdHeader = new byte[] { 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x09, 0x00};
        static readonly byte[] dataHeader = new byte[] { 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x08, 0x00 };
        static readonly byte[] endHeader = new byte[] { 0x00, 0x00, 0x89, 0x00, 0x00, 0x00, 0x89, 0x00 };


        public class Disc {
            string path;
            byte[] systemSegment;
            dynamic primaryVolumeDescriptor;
            dynamic pathTable;
            List<dynamic> root = new List<dynamic>();

            public string Path { get { return path; } }
            public byte[] SystemSegment { get { return systemSegment; } }
            public dynamic PrimaryVolumeDescriptor { get { return primaryVolumeDescriptor; } }
            public dynamic PathTable { get { return pathTable; } }
            public List<dynamic> Root { get { return root; } }


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
                if (!reader.ReadBytes(12).SequenceEqual(syncPattern)) {
                    throw new ArgumentException("Synchronization pattern not found. Incorrect file type.");
                }
                reader.BaseStream.Seek(12, SeekOrigin.Current);
                root = Contents(reader);
            }

            List<dynamic> Contents(BinaryReader reader) {
                List<dynamic> result = new List<dynamic>();
                while (reader.ReadByte() != 0) {
                    reader.BaseStream.Seek(-1, SeekOrigin.Current);
                    var temp = new DirectoryEntry(reader);
                    if (temp.Name != Convert.ToChar(0).ToString() && temp.Name != Convert.ToChar(1).ToString()) {
                        result.Add(temp);
                    }
                }
                reader.BaseStream.Seek(2352 - reader.BaseStream.Position % 2352, SeekOrigin.Current); // seek end of current segment
                foreach (var children in result) {
                    if (children.Flags == 2) { // Folder
                        reader.BaseStream.Seek(children.ExtentLocation * 2352, SeekOrigin.Begin); // get location of the folder
                        if (!reader.ReadBytes(12).SequenceEqual(syncPattern)) {
                            throw new ArgumentException("Synchronization pattern not found. Incorrect file type.");
                        }
                        reader.BaseStream.Seek(12, SeekOrigin.Current); // Skip address, type and header.
                        children.Children = Contents(reader);

                    } else { // File
                        if (children.Name == "MIX.DA;1") { // Currently gives header error. Different format??
                            continue;
                        }
                        children.GetData(reader);
                    }
                }
                return result;
            }
        }

        public class PrimaryVolumeDescriptorEntry {
            byte type = 1;
            string name = "CD001";
            byte version = 1;
            string systemName = "PLAYSTATION                     ";
            string volumeName = "SCUS944491                      ";
            UInt32 volumeSize = 206187;
            UInt16 volumeSetSize = 1;
            UInt16 volumeSequenceNumber = 1;
            UInt16 logicalBlockSize = 2048;
            UInt32 pathTableSize = 106;
            UInt32 pathLLocation = 18;
            UInt32 optionalPathLLocation = 19;
            UInt32 pathMLocation = 20;
            UInt32 optionalPathMLocation = 21;
            dynamic root = new DirectoryEntry(); // Directory entry for the root directory
            string volumeSetName = "DISC1                                                                                                                           ";
            string publisherName = "SONY COMPUTER ENTERTAINMENT INC                                                                                                 ";
            string dataPreparerName = "SONY COMPUTER ENTERTAINMENT INC                                                                                                 ";
            string applicationName = "PLAYSTATION                                                                                                                     ";
            string copyrightName = "SCEI                                  ";
            string abstractName = "                                    ";
            string bibliographicName = "                                     ";
            dynamic volumeCreation = new DateEntry(1999, 10, 28, 0, 0, 0, 0, 0);
            dynamic volumeModification = new DateEntry(0, 0, 0, 0, 0, 0, 0, 0);
            dynamic volumeExpiration = new DateEntry(0, 0, 0, 0, 0, 0, 0, 0);
            dynamic volumeEffective = new DateEntry(0, 0, 0, 0, 0, 0, 0, 0);
            byte fileStructureVersion = 1;
            byte[] applicationUsed = new byte[512];
            byte[] reserved = new byte[653];

            public byte Type { get { return type; } set { type = value; } }
            public string Name { get { return name; } set { name = value; } }
            public byte Version { get { return version; } set { version = value; } }
            public string SystemName { get { return systemName; } set { systemName = value; } }
            public string VolumeName { get { return volumeName; } set { volumeName = value; } }
            public UInt32 VolumeSize { get { return volumeSize; } set { volumeSize = value; } }
            public UInt16 VolumeSetSize { get { return volumeSetSize; } set { volumeSetSize = value; } }
            public UInt16 VolumeSequenceNumber { get { return volumeSequenceNumber; } set { volumeSequenceNumber = value; } }
            public UInt16 LogicalBlockSize { get { return logicalBlockSize; } set { logicalBlockSize = value; } }
            public UInt32 PathTableSize { get { return pathTableSize; } set { pathTableSize = value; } }
            public UInt32 PathLLocation { get { return pathLLocation; } set { pathLLocation = value; } }
            public UInt32 OptionalPathLLocation { get { return optionalPathLLocation; } set { optionalPathLLocation = value; } }
            public UInt32 PathMLocation { get { return pathMLocation; } set { pathMLocation = value; } }
            public UInt32 OptionalPathMLocatuin { get { return optionalPathMLocation; } set { optionalPathMLocation = value; } }
            public DirectoryEntry Root { get { return root; } set { root = value; } }
            public string VolumeSetName { get { return volumeSetName; } set { volumeSetName = value; } }
            public string PublisherName { get { return publisherName; } set { publisherName = value; } }
            public string DataPreparerName { get { return dataPreparerName; } set { dataPreparerName = value; } }
            public string ApplicationName { get { return applicationName; } set { applicationName = value; } }
            public string CopyrightName { get { return copyrightName; } set { copyrightName = value; } }
            public string AbstractName { get { return abstractName; } set { abstractName = value; } }
            public string BibliographicName { get { return bibliographicName; } set { bibliographicName = value; } }
            public DateEntry VolumeCreation { get { return volumeCreation; } set { volumeCreation = value; } }
            public DateEntry VolumeModification { get { return volumeModification; } set { volumeModification = value; } }
            public DateEntry VolumeExpiration { get { return volumeExpiration; } set { volumeExpiration = value; } }
            public DateEntry VolumeEffective { get { return volumeEffective; } set { volumeEffective = value; } }
            public byte FileStructureVersion { get { return FileStructureVersion; } set { fileStructureVersion = value; } }
            public byte[] ApplicationUsed { get { return applicationUsed; } }
            public byte[] Reserverd { get { return reserved; } }



            public PrimaryVolumeDescriptorEntry() {

            }

            public PrimaryVolumeDescriptorEntry(BinaryReader reader) {
                if (!reader.ReadBytes(12).SequenceEqual(syncPattern)) {
                    throw new ArgumentException("Synchronization pattern not found. Incorrect file type.");
                }
                reader.BaseStream.Seek(4, SeekOrigin.Current); //Address + mode
                if (!reader.ReadBytes(8).SequenceEqual(pvdHeader)) {
                    throw new ArgumentException("Primary Volume descriptor header not found. Incorrect file type.");
                }
                type = reader.ReadByte();
                name = new string(reader.ReadChars(5));
                version = reader.ReadByte();
                reader.BaseStream.Seek(1, SeekOrigin.Current); // Unused
                systemName = new string(reader.ReadChars(32));
                volumeName = new string(reader.ReadChars(32));
                reader.BaseStream.Seek(8, SeekOrigin.Current); // Unused
                volumeSize = ReadUInt32LB(reader);
                reader.BaseStream.Seek(32, SeekOrigin.Current); // Unused
                volumeSetSize = ReadUInt16LB(reader);
                volumeSequenceNumber = ReadUInt16LB(reader);
                logicalBlockSize = ReadUInt16LB(reader);
                pathTableSize = ReadUInt32LB(reader);
                pathLLocation = reader.ReadUInt32();
                optionalPathLLocation = reader.ReadUInt32();
                pathMLocation = BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray());
                optionalPathMLocation = BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray());
                root = new DirectoryEntry(reader);
                volumeSetName = new string(reader.ReadChars(128));
                publisherName = new string(reader.ReadChars(128));
                dataPreparerName = new string(reader.ReadChars(128));
                applicationName = new string(reader.ReadChars(128));
                copyrightName = new string(reader.ReadChars(38));
                abstractName = new string(reader.ReadChars(36));
                bibliographicName = new string(reader.ReadChars(37));
                volumeCreation = new PrimaryVolumeDescriptorEntry.DateEntry(reader);
                volumeModification = new PrimaryVolumeDescriptorEntry.DateEntry(reader);
                volumeExpiration = new PrimaryVolumeDescriptorEntry.DateEntry(reader);
                volumeEffective = new PrimaryVolumeDescriptorEntry.DateEntry(reader);
                fileStructureVersion = reader.ReadByte();
                reader.BaseStream.Seek(1, SeekOrigin.Current); // Unused
                applicationUsed = reader.ReadBytes(512);
                reserved = reader.ReadBytes(653);
                reader.BaseStream.Seek(280, SeekOrigin.Current); // Error detection/correction
                if (!reader.ReadBytes(12).SequenceEqual(syncPattern)) {
                    throw new ArgumentException("Synchronization pattern not found. Incorrect file type.");
                }
                reader.BaseStream.Seek(4, SeekOrigin.Current); //Address + Mode
                if (!reader.ReadBytes(8).SequenceEqual(endHeader)) {
                    throw new ArgumentException("Primary Volume Descriptor end header not found. Incorrect file type.");
                }
                if (reader.ReadByte() != 0xFF) {
                    throw new ArgumentException("Incorect Primary Volume Descriptor set terminator.");
                }
                if (new string(reader.ReadChars(5)) != name) {
                    throw new ArgumentException("Primary Volume Descriptor set terminator identifier doesn't match");
                }
                reader.BaseStream.Seek(0x912, SeekOrigin.Current); //Move to end of the block

            }

            public class DateEntry {
                public UInt16 year = 1999;
                public byte month = 10;
                public byte day = 28;
                public byte hour = 0;
                public byte minute = 0;
                public byte second = 0;
                public byte milisecond = 0;
                public byte gmt = 0;

                public DateEntry() {

                }
                public DateEntry(int year, int month, int day, int hour, int minute, int second, int milisecond, int gmt) {
                    this.year = (UInt16)year;
                    this.month = (byte)month;
                    this.day = (byte)day;
                    this.hour = (byte)hour;
                    this.minute = (byte)minute;
                    this.second = (byte)second;
                    this.milisecond = (byte)milisecond;
                    this.gmt = (byte)gmt;
                }

                public DateEntry(BinaryReader reader) {
                    year = Convert.ToUInt16(new string(reader.ReadChars(4)));
                    month = Convert.ToByte(new string(reader.ReadChars(2)));
                    day = Convert.ToByte(new string(reader.ReadChars(2)));
                    hour = Convert.ToByte(new string(reader.ReadChars(2)));
                    minute = Convert.ToByte(new string(reader.ReadChars(2)));
                    second = Convert.ToByte(new string(reader.ReadChars(2)));
                    milisecond = Convert.ToByte(new string(reader.ReadChars(2)));
                    gmt = reader.ReadByte();
                }
            }
        }

        public class PathTableEntry {

            public PathTableEntry() {

            }

            public PathTableEntry(BinaryReader reader) {
                reader.BaseStream.Seek(4 * 2352, SeekOrigin.Current);
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
                date = new DirectoryEntry.DateEntry(reader);
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
