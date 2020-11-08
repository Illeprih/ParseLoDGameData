using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Text;

namespace ParseLoDGameData {
    class DiscRead {
        public static byte[] syncPattern = new byte[] { 0x0, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x0 };
        public static byte[] PVDsubHeader = new byte[] { 0x0, 0x0, 0x9, 0x0, 0x0, 0x0, 0x9, 0x0 };
        public static byte[] startSubHeader = new byte[] { 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x8, 0x0 };
        public static byte[] endSubHeader = new byte[] { 0x0, 0x0, 0x89, 0x0, 0x0, 0x0, 0x89, 0x0 };
        public static PrimaryVolumeDescriptor PVD = new PrimaryVolumeDescriptor();
        public static byte[] systemSegment = new byte[0xD350];

        public static List<dynamic>[] GetFiles(string fileName) {
            BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open));
            PVD = LocatePrimaryVolumeDescriptor(reader);
            
            var root = DirectoryContents(reader, PVD.rootDirectory.extentLocation); // gets contents of root directory
            return root;
        }

        public static List<dynamic>[] DirectoryContents(BinaryReader data, uint location) {
            data.BaseStream.Seek(location * 2352, SeekOrigin.Begin); //seek start of the segment
            if (!data.ReadBytes(12).SequenceEqual(syncPattern)) {
                throw new ArgumentException("Synchronization pattern not found. Incorrect file type.");
            }
            data.ReadBytes(12);
            List<dynamic> directories = new List<dynamic>();
            List<dynamic> files = new List<dynamic>();
            while (data.ReadByte() != 0) {
                data.BaseStream.Seek(-1, SeekOrigin.Current);
                var temp = new DirectoryEntry(data);
                if (!(temp.fileIdentifier == Convert.ToChar(0).ToString() || temp.fileIdentifier == Convert.ToChar(1).ToString())) {
                    if (temp.fileIdentifier.EndsWith(";1")) { // files
                        files.Add(temp);
                    } else {
                        directories.Add(temp);
                    }
                }
            }
            data.BaseStream.Seek(2352 - data.BaseStream.Position % 2352, SeekOrigin.Current); // seek end of current segment
            var result = new List<dynamic>[] { directories, files };
            foreach(var file in files) {
                if (file.fileIdentifier != "MIX.DA;1") {
                    file.GetData(data, file.fileIdentifier); // start reading data of each file
                }
            }
            foreach(var directory in directories) {
                directory.SetSubDirectory(data); // recursion for each subfolder
            }
            return result;
        }

        public static PrimaryVolumeDescriptor LocatePrimaryVolumeDescriptor(BinaryReader data) {
            systemSegment = data.ReadBytes(0xD350);
            data.BaseStream.Seek(0, SeekOrigin.Begin);
            if (!data.ReadBytes(12).SequenceEqual(syncPattern)) {
                throw new ArgumentException("Synchronization pattern not found. Incorrect file type.");
            }
            data.ReadBytes(4);
            byte[] subheader = data.ReadBytes(8);
            while (!subheader.SequenceEqual(PVDsubHeader)) {
                data.BaseStream.Seek(0x918, SeekOrigin.Current);
                if (!data.ReadBytes(12).SequenceEqual(syncPattern)) {
                    throw new ArgumentException("Synchronization pattern not found. Incorrect file type.");
                }
                data.ReadBytes(4);
                subheader = data.ReadBytes(8);
            }
            data.BaseStream.Seek(-12, SeekOrigin.Current);
            return new PrimaryVolumeDescriptor(data);
        }

        public class PrimaryVolumeDescriptor {
            public UInt32 address = 0;
            public byte mode = 2;
            public byte[] subheader = new byte[] { 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x09, 0x00 };
            public byte typeCode = 1;
            public string standardIdentifier = "CD001";
            public byte version = 1;
            public string systemIdentifier = "PLAYSTATION                     ";
            public string volumeIdentifier = "SCUS944491                      ";
            public UInt32 volumeSpaceSize = 206187;
            public UInt16 volumeSetSize = 1;
            public UInt16 volumeSequenceNumber = 1;
            public UInt16 logicalBlockSize = 2048;
            public UInt32 pathTableSize = 106;
            public UInt32 LPathTableLocation = 18;
            public UInt32 optionalLPathTableLocation = 19;
            public UInt32 MPathTableLocation = 20;
            public UInt32 optionalMPathTableLocation = 21;
            public DirectoryEntry rootDirectory = new DirectoryEntry(); // Directory entry for the root directory
            public string volumeSetIdentifier = "DISC1                                                                                                                           ";
            public string publisherIdentifier = "SONY COMPUTER ENTERTAINMENT INC                                                                                                 ";
            public string dataPreparerIdentifier = "SONY COMPUTER ENTERTAINMENT INC                                                                                                 ";
            public string applicationIdentifier = "PLAYSTATION                                                                                                                     ";
            public string copyrightFileIdentifier = "SCEI                                  ";
            public string abstractFileIdentifier = "                                    ";
            public string bibliographicFileIdentifier = "                                     ";
            public Date volumeCreation = new Date(1999, 10, 28, 0, 0, 0, 0, 0);
            public Date volumeModification = new Date(0, 0, 0, 0, 0, 0, 0, 0);
            public Date volumeExpiration = new Date(0, 0, 0, 0, 0, 0, 0, 0);
            public Date volumeEffective = new Date(0, 0, 0, 0, 0, 0, 0, 0);
            public byte fileStructureVersion = 1;
            public byte[] applicationUsed = new byte[512];
            public byte[] reserved = new byte[653];
            public byte[] errorDetection = new byte[4];
            public byte[] errorCorrection = new byte[276];
            public byte volumeSetTerminatorType = 255;
            public string volumeSetTerminatorIdentifier = "CD001";
            public byte volumeSetTerminatorVersion = 1;


            public PrimaryVolumeDescriptor() {

            }

            public PrimaryVolumeDescriptor(BinaryReader data) {
                byte[] tempAddress = new byte[4];
                for (int i = 0; i < 3; i++) {
                    tempAddress[i] = data.ReadByte();
                }
                address = BitConverter.ToUInt32(tempAddress);
                mode = data.ReadByte();
                subheader = data.ReadBytes(8);
                typeCode = data.ReadByte();
                standardIdentifier = new string(data.ReadChars(5));
                version = data.ReadByte();
                data.ReadByte(); // Unused
                systemIdentifier = new string(data.ReadChars(32));
                volumeIdentifier = new string(data.ReadChars(32));
                data.ReadBytes(8); // Unused
                volumeSpaceSize = ReadUInt32LB(data);
                data.ReadBytes(32); // Unused
                volumeSetSize = ReadUInt16LB(data);
                volumeSequenceNumber = ReadUInt16LB(data);
                logicalBlockSize = ReadUInt16LB(data);
                pathTableSize = ReadUInt32LB(data);
                LPathTableLocation = data.ReadUInt32();
                optionalLPathTableLocation = data.ReadUInt32();
                MPathTableLocation = BitConverter.ToUInt32(data.ReadBytes(4).Reverse().ToArray());
                optionalMPathTableLocation = BitConverter.ToUInt32(data.ReadBytes(4).Reverse().ToArray());
                rootDirectory = new DirectoryEntry(data);
                volumeSetIdentifier = new string(data.ReadChars(128));
                publisherIdentifier = new string(data.ReadChars(128));
                dataPreparerIdentifier = new string(data.ReadChars(128));
                applicationIdentifier = new string(data.ReadChars(128));
                copyrightFileIdentifier = new string(data.ReadChars(38));
                abstractFileIdentifier = new string(data.ReadChars(36));
                bibliographicFileIdentifier = new string(data.ReadChars(37));
                volumeCreation = new Date(data);
                volumeModification = new Date(data);
                volumeExpiration = new Date(data);
                volumeEffective = new Date(data);
                fileStructureVersion = data.ReadByte();
                data.ReadByte(); // Unused
                applicationUsed = data.ReadBytes(512);
                reserved = data.ReadBytes(653);
                errorDetection = data.ReadBytes(4);
                errorCorrection = data.ReadBytes(276);
                data.ReadBytes(24); // syncPatter segment again
                volumeSetTerminatorType = data.ReadByte();
                volumeSetTerminatorIdentifier = new string(data.ReadChars(5));
                volumeSetTerminatorVersion = data.ReadByte();
                data.BaseStream.Seek(0x911, SeekOrigin.Current);
            }

            
        }

        public class DirectoryEntry {
            public byte recordLength = 33;
            public byte extendedAttributeRecordLength = 0;
            public UInt32 extentLocation = 0;
            public UInt32 dataLength = 0;
            public DirectoryEntry.Date date = new DirectoryEntry.Date( 0, 1, 1, 0, 0, 0, 0 );
            public byte fileFlags = 0;
            public byte interleavedSize = 0;
            public byte interleaveGap = 0;
            public UInt16 volumeSequenceNumber = 1;
            public byte identifierLength = 1;
            public string fileIdentifier = "A";
            public byte[] systemUse = new byte[0];
            public byte[] data = new byte[0];
            public byte[] decompressedData = new byte[0];
            public List<dynamic>[] subDirectory = new List<dynamic>[2];


            public DirectoryEntry() {

            }

            public DirectoryEntry(BinaryReader data) {
                long position = data.BaseStream.Position;
                recordLength = data.ReadByte();
                extendedAttributeRecordLength = data.ReadByte();
                extentLocation = ReadUInt32LB(data);
                dataLength = ReadUInt32LB(data);
                date = new DirectoryEntry.Date(data);
                fileFlags = data.ReadByte();
                interleavedSize = data.ReadByte();
                interleaveGap = data.ReadByte();
                volumeSequenceNumber = ReadUInt16LB(data);
                identifierLength = data.ReadByte();
                fileIdentifier = new string(data.ReadChars(identifierLength));
                if (identifierLength % 2 == 0) {
                    data.ReadByte(); // padding
                }
                int left2Read = (int) (position + recordLength - data.BaseStream.Position);
                if (left2Read > 0) {
                    systemUse = data.ReadBytes(left2Read);
                }


            }

            public void GetData(BinaryReader reader, string fileID) {
                reader.BaseStream.Seek(extentLocation * 2352, SeekOrigin.Begin); // get first data segment, rest is writen sequentially
                data = new byte[dataLength];
               
                for (int i = 0; i < Math.Ceiling((double)dataLength / 2048); i++) { // get number of segments
                    if (!reader.ReadBytes(12).SequenceEqual(syncPattern)) {
                        throw new ArgumentException($"Synchronization pattern not found. Incorrect file type. {fileID}");
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
            }

            public void SetSubDirectory(BinaryReader reader) {
                subDirectory = DirectoryContents(reader, extentLocation);
            }

            public byte[] CreateDate() {
                var result = new byte[7];
                return result;
            }


            public class Date {
                public UInt16 year = 0;
                public byte month = 0;
                public byte day = 0;
                public byte hour = 0;
                public byte minute = 0;
                public byte second = 0;
                public byte GMT = 0;

                public Date(int _year, int _month, int _day, int _hour, int _minute, int _second, int _GMT) {
                    year = (UInt16) (1900 + _year);
                    month = (byte) _month;
                    day = (byte)_day;
                    hour = (byte)_hour;
                    minute = (byte)_minute;
                    second = (byte)_second;
                    GMT = (byte)_GMT;
                }
                public Date(BinaryReader segment) {
                    year = (UInt16) (1900 + segment.ReadByte());
                    month = segment.ReadByte();
                    day = segment.ReadByte();
                    hour = segment.ReadByte();
                    minute = segment.ReadByte();
                    second = segment.ReadByte();
                    GMT = segment.ReadByte();
                }
            }
        }

        public class Date {
            public UInt16 year = 0;
            public byte month = 0;
            public byte day = 0;
            public byte hour = 0;
            public byte minute = 0;
            public byte second = 0;
            public byte milisecond = 0;
            public byte GMT = 0;

            public Date(int _year, int _month, int _day, int _hour, int _minute, int _second, int _milisecond, int _GMT) {
                year = (UInt16) _year;
                month = (byte) _month;
                day = (byte)_day;
                hour = (byte)_hour;
                minute = (byte)_minute;
                second = (byte)_second;
                milisecond = (byte)_milisecond;
                GMT = (byte)_GMT;
            }

            public Date(BinaryReader segment) {
                year = Convert.ToUInt16(new string(segment.ReadChars(4)));
                month = Convert.ToByte(new string(segment.ReadChars(2)));
                day = Convert.ToByte(new string(segment.ReadChars(2)));
                hour = Convert.ToByte(new string(segment.ReadChars(2)));
                minute = Convert.ToByte(new string(segment.ReadChars(2)));
                second = Convert.ToByte(new string(segment.ReadChars(2)));
                milisecond = Convert.ToByte(new string(segment.ReadChars(2)));
                GMT = segment.ReadByte();
            }
        }

        public static UInt32 ReadUInt32LB(BinaryReader segment) {
            UInt32 little = segment.ReadUInt32();
            UInt32 big = BitConverter.ToUInt32(segment.ReadBytes(4).Reverse().ToArray());
            if (little == big) {
                return little;
            } else {
                throw new ArgumentException($"Little and Big Endian do not match {little}, {big}");
            }
        }

        public static UInt16 ReadUInt16LB(BinaryReader segment) {
            UInt16 little = segment.ReadUInt16();
            UInt16 big = BitConverter.ToUInt16(segment.ReadBytes(2).Reverse().ToArray());
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

        public static byte[] WriteUInt16LB (UInt16 value) {
            var bytes = BitConverter.GetBytes(value);
            var result = bytes.Concat(bytes.Reverse()).ToArray();
            return result;
        }

        public static uint RecalculateLBA(List<dynamic>[] directory, uint i) {
            i++;
            foreach (var file in directory[1]) {
                file.extentLocation = i;
                i += (uint)Math.Ceiling((double)file.dataLength / 2048);
            }

            foreach (dynamic dir in directory[0]) {
                dir.extentLocation = i;
                List<dynamic>[] subdir = dir.subDirectory;
                i = RecalculateLBA(subdir, i);
            }
            
            return i;
        }

        public static void Create(BinaryWriter writer, List<dynamic>[] root, uint currentIndex, uint parentIndex) {
            writer.Write(CreateDirectory(root, currentIndex, parentIndex));
            foreach (dynamic file in root[1]) {
                writer.Write(CreateData(file));
            }
            foreach( dynamic directory in root[0]) {
                Create(writer, directory.subDirectory, directory.extentLocation, parentIndex); //parent index needs fix
            }
        }

        public static byte[] CreateData(dynamic file) {
            int blocks = (int)Math.Ceiling((double)file.dataLength / 2048);
            byte[] result = new byte[blocks * 2352];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(result));
            BinaryReader reader = new BinaryReader(new MemoryStream(file.data));
            for (int i = 0; i < blocks; i++) {
                writer.Write(syncPattern);
                writer.Seek(3, SeekOrigin.Current); //address to be filled in
                writer.Write((byte)2); //mode
                if (i == blocks - 1) {
                    writer.Write(endSubHeader);
                } else {
                    writer.Write(startSubHeader);
                }
                writer.Write(reader.ReadBytes(2048));
            }
            return result;
        }


        public static byte[] CreateDirectory(dynamic directory, uint currentIndex, uint parentIndex) {
            byte[] result = new byte[2352];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(result));
            writer.Write(syncPattern);
            writer.Seek(3, SeekOrigin.Current); //address to be filled in
            writer.Write((byte)2); //mode
            writer.Write(endSubHeader);
            writer.Write(CreateDirectoryEntry(Convert.ToChar(0).ToString(), currentIndex));
            writer.Write(CreateDirectoryEntry(Convert.ToChar(1).ToString(), parentIndex));
            foreach (dynamic dir in directory[0]) {
                writer.Write(CreateDirectoryEntry(dir.fileIdentifier, dir.extentLocation));
            }
            foreach(dynamic file in directory[1]) {
                writer.Write(CreateDirectoryEntry(file.fileIdentifier, file.extentLocation));
            }

            return result;
        }

        public static byte[] CreateDirectoryEntry(string name, uint index) {
            byte len = (byte)(Math.Round((double)(47 + Encoding.ASCII.GetBytes(name).Length) / 2, MidpointRounding.AwayFromZero) * 2);
            var result = new byte[len];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(result));
            writer.Write(len);
            writer.Seek(1, SeekOrigin.Current); //skip Extended Attribute Record Length
            writer.Write(WriteUInt32LB(index));
            writer.Write(WriteUInt32LB(2048));
            writer.Seek(7, SeekOrigin.Current); //skip date for now
            if (name.Contains(";1")) {
                writer.Write((byte)0);
            } else {
                writer.Write((byte)2);
            }
            writer.Seek(2, SeekOrigin.Current); //skip interleave values
            writer.Write(WriteUInt16LB(1));
            writer.Write((byte)name.Length);
            writer.Write(Encoding.ASCII.GetBytes(name));
            if (name.Length % 2 == 0) {
                writer.Write((byte)0); // padding
            }
            writer.Seek(4, SeekOrigin.Current);
            if (name.Contains(";1")) {
                writer.Write((byte)0x0D);
            } else {
                writer.Write((byte)0x8D);
            }
            writer.Write(4282453);

            return result;
        }
    }
}
