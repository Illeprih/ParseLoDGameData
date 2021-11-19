using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LodmodsDM {
    class DiscHandler2_old {
        static readonly byte[] syncPattern = new byte[] { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 };
        static readonly byte[] pvdHeader = new byte[] { 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x09, 0x00 };
        static readonly byte[] endHeader = new byte[] { 0x00, 0x00, 0x89, 0x00, 0x00, 0x00, 0x89, 0x00 };

        static int LBA = 0x16;
        static dynamic brokenFile = new System.Dynamic.ExpandoObject();


        public static void UnpackDisc(string path, string unpackLocation) {
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open))) {
                if (!reader.ReadBytes(12).SequenceEqual(syncPattern)) {
                    throw new ArgumentException("Synchronization pattern not found. Incorrect file type.");
                }
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                using (BinaryWriter writer = new BinaryWriter(File.Open(unpackLocation + "SystemData.bin", FileMode.Create))) {
                    writer.Write(reader.ReadBytes(0x9300));
                }

                if (!reader.ReadBytes(12).SequenceEqual(syncPattern)) {
                    throw new ArgumentException("Synchronization pattern not found. Incorrect file type.");
                }
                reader.BaseStream.Seek(4, SeekOrigin.Current); //Address + mode
                if (!reader.ReadBytes(8).SequenceEqual(pvdHeader)) {
                    throw new ArgumentException("Primary Volume descriptor header not found. Incorrect file type.");
                }
                reader.BaseStream.Seek(80, SeekOrigin.Current);
                int discSize = reader.ReadInt32();
                reader.BaseStream.Seek(-108, SeekOrigin.Current);

                using (BinaryWriter writer = new BinaryWriter(File.Open(unpackLocation + "PrimaryVolumeDescriptor.bin", FileMode.Create))) {
                    writer.Write(reader.ReadBytes(0x1260));
                }

                reader.ReadBytes(4 * 2352); // Path tables

                reader.ReadBytes(24); // skip headers etc.

                var files = ScanFiles(reader);

                CorrectLBA(files, discSize);
                brokenFile.ExtentLocation = (uint)(LBA + 1);
                brokenFile.DataLength = (uint)(150 * 2048); // doesn't seem like the right solution

                /*
                var t1 = new Task(() => {
                    ExtractFiles(reader, files, unpackLocation);
                });
                t1.Start();
                */
                ExtractFiles(reader, files, unpackLocation);
                
            }
        }

        static void ExtractFiles(BinaryReader reader, List<dynamic> files, string unpackPath) {
            foreach (dynamic entry in files) {
                if (entry.Flags == 2) { // folder
                    System.IO.Directory.CreateDirectory(unpackPath + entry.Name);
                    ExtractFiles(reader, entry.Children, unpackPath + entry.Name + "/");

                } else {
                    reader.BaseStream.Seek(entry.ExtentLocation * 2352, SeekOrigin.Begin);
                    UnpackPS1Data.UnpackData(entry, reader.ReadBytes((int)(Math.Ceiling((double)entry.DataLength / 2048) * 2352)));
                    using (BinaryWriter writer = new BinaryWriter(File.Open(unpackPath + entry.Name.Remove(entry.Name.Length - 2), FileMode.Create))) {
                        writer.Write(entry.Data);
                    }
                }
            }
        }

        static void CorrectLBA(List<dynamic> files, int discSize) {
            
            foreach (dynamic entry in files.OrderBy(o => o.ExtentLocation).ToList()) {
                
                if(LBA + 1 != entry.ExtentLocation) {
                    brokenFile = entry;
                } else {
                    LBA = (int)(entry.ExtentLocation + Math.Ceiling((double)entry.DataLength / 2048) - 1);
                }
                if (entry.Flags == 2) {
                    CorrectLBA(entry.Children, discSize);
                }
            }
            
            
        }

        static List<dynamic> ScanFiles(BinaryReader reader) {
            List<dynamic> result = new List<dynamic>();
            while (reader.ReadByte() != 0) {
                reader.BaseStream.Seek(-1, SeekOrigin.Current);
                var temp = new Directory(reader);
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
                    children.Children = ScanFiles(reader);
                }

            }
            return result;
        }

        public class Directory {
            byte recordLength;
            byte extendedAttributeRecordLength;
            UInt32 extentLocation;
            UInt32 dataLength;
            //DateEntry date = new DateEntry();
            byte flags;
            byte interleavedSize;
            byte interleaveGap;
            UInt16 volumeSequenceNumber;
            byte nameLength;
            string name;
            byte[] systemUse = new byte[14];
            List<dynamic> children = new List<dynamic>();
            byte[] data;
            List<byte[]> headerList = new List<byte[]>();

            public byte RecordLength { get { return recordLength; } }
            public byte ExtendedAttributeRecordLength { get { return extendedAttributeRecordLength; } }
            public UInt32 ExtentLocation { get { return extentLocation; } set { extentLocation = value; } }
            public UInt32 DataLength { get { return dataLength; } set { dataLength = value; } }
            public byte Flags { get { return flags; } }
            public byte InterleavedSize { get { return interleavedSize; } }
            public byte InterleavedGap { get { return interleaveGap; } }
            public byte NameLength { get { return nameLength; } }
            public string Name { get { return name; } }
            public byte[] SystemUse { get { return systemUse; } }
            public List<dynamic> Children { get { return children; } set { children = value; } }
            public byte[] Data { get { return data; } set { data = value; } }
            public List<byte[]> HeaderList { get { return headerList; } set { headerList = value; } }


            public Directory(BinaryReader reader) {
                long position = reader.BaseStream.Position;
                recordLength = reader.ReadByte();
                extendedAttributeRecordLength = reader.ReadByte();
                extentLocation = ReadUInt32LB(reader);
                dataLength = ReadUInt32LB(reader);
                reader.ReadBytes(7); // Skip date for now
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
                } else {
                    systemUse = new byte[0];
                }
            }
        }

        static UInt32 ReadAddress(BinaryReader reader) { // Turns timestamp(min - sec - sec/75) into LBA
            UInt32 first = UInt32.Parse(reader.ReadByte().ToString("X")) * 4500;
            UInt32 second = (UInt32.Parse(reader.ReadByte().ToString("X")) - 2) * 75;
            UInt32 third = UInt32.Parse(reader.ReadByte().ToString("X"));
            return first + second + third;
        }

        static byte[] WriteAddress(UInt32 val) { // Turns LBA into timestamp (min - sec - sec/75)
            byte[] result = new byte[3];
            result[0] = Convert.ToByte((val / 4500).ToString());
            result[1] = (byte)(Convert.ToByte(((val % 4500) / 75).ToString(), 16) + 2);
            result[2] = Convert.ToByte((val % 75).ToString(), 16);
            return result;
        }

        static UInt32 ReadUInt32LB(BinaryReader reader) {
            UInt32 little = reader.ReadUInt32();
            UInt32 big = BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray());
            if (little == big) {
                return little;
            } else {
                throw new ArgumentException($"Little and Big Endian do not match {little}, {big}");
            }
        }

        static UInt16 ReadUInt16LB(BinaryReader reader) {
            UInt16 little = reader.ReadUInt16();
            UInt16 big = BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray());
            if (little == big) {
                return little;
            } else {
                throw new ArgumentException($"Little and Big Endian do not match {little}, {big}");
            }
        }

        static byte[] WriteUInt32LB(uint value) {
            var bytes = BitConverter.GetBytes(value);
            var result = bytes.Concat(bytes.Reverse()).ToArray();
            return result;
        }

        static byte[] WriteUInt16LB(UInt16 value) {
            var bytes = BitConverter.GetBytes(value);
            var result = bytes.Concat(bytes.Reverse()).ToArray();
            return result;
        }

    }
}
