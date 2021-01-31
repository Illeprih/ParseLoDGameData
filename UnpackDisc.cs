using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ParseLoDGameData {
    public class UnpackDisc {
        static readonly byte[] syncPattern = new byte[] { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 };
        static readonly byte[] pvdHeader = new byte[] { 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x09, 0x00 };
        static readonly byte[] dataHeader = new byte[] { 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x08, 0x00 };
        static readonly byte[] endHeader = new byte[] { 0x00, 0x00, 0x89, 0x00, 0x00, 0x00, 0x89, 0x00 };
        static readonly byte[] directoryEnd = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x8D, 0x55, 0x58, 0x41, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public class Disc {
            string path;
            dynamic primaryVolumeDescriptor;

            public string Path { get { return path; } }
            public dynamic PrimaryVolumeDescriptor { get { return primaryVolumeDescriptor; } }


            public Disc(string filePath, string unpackLocation) {
                path = filePath;
                BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open));

                if (!reader.ReadBytes(12).SequenceEqual(syncPattern)) {
                    throw new ArgumentException("Synchronization pattern not found. Incorrect file type.");
                }
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                /*
                using (BinaryWriter writer = new BinaryWriter(File.Open(unpackLocation + "SystemData.bin", FileMode.Create))) {
                    writer.Write(reader.ReadBytes(0x9300));
                }
                */
                reader.ReadBytes(0x9300); // we skip writing for now
                reader.ReadBytes(2 * 2352); // skip PVD for now
                //primaryVolumeDescriptor = new PrimaryVolumeDescriptorEntry(reader, unpackLocation);
                reader.ReadBytes(2352 * 4); // path tables (directories only)
  
            }
        }

        public class PrimaryVolumeDescriptorEntry {
            byte type;
            string name;
            byte version;
            string systemName;
            string volumeName;
            UInt32 volumeSize;
            UInt16 volumeSetSize;
            UInt16 volumeSequenceNumber;
            UInt16 logicalBlockSize;
            UInt32 pathTableSize;
            UInt32 pathLLocation;
            UInt32 optionalPathLLocation;
            UInt32 pathMLocation;
            UInt32 optionalPathMLocation;
            dynamic root; // Directory entry for the root directory
            string volumeSetName;
            string publisherName;
            string dataPreparerName;
            string applicationName;
            string copyrightName;
            string abstractName;
            string bibliographicName;
            dynamic volumeCreation;
            dynamic volumeModification;
            dynamic volumeExpiration;
            dynamic volumeEffective;
            byte fileStructureVersion;

            public PrimaryVolumeDescriptorEntry() {

            }

            public PrimaryVolumeDescriptorEntry(BinaryReader reader, string unpackLocation) {
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
                systemName = new string(reader.ReadChars(32)).Trim();
                volumeName = new string(reader.ReadChars(32)).Trim();
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
                root = new DirFileEntry(reader);
                volumeSetName = new string(reader.ReadChars(128)).Trim();
                publisherName = new string(reader.ReadChars(128)).Trim();
                dataPreparerName = new string(reader.ReadChars(128)).Trim();
                applicationName = new string(reader.ReadChars(128)).Trim();
                copyrightName = new string(reader.ReadChars(38)).Trim();
                abstractName = new string(reader.ReadChars(36)).Trim();
                bibliographicName = new string(reader.ReadChars(37)).Trim();
                reader.ReadBytes(17); //volumeCreation
                reader.ReadBytes(17); //volumeModification
                reader.ReadBytes(17); //volumeExpiration
                reader.ReadBytes(17); //volumeEffective
                fileStructureVersion = reader.ReadByte();
                reader.BaseStream.Seek(1, SeekOrigin.Current); // Unused
                using (BinaryWriter writer = new BinaryWriter(File.Open(unpackLocation + "PVDAppUsed.bin", FileMode.Create))) {
                    writer.Write(reader.ReadBytes(512));
                }
                using (BinaryWriter writer = new BinaryWriter(File.Open(unpackLocation + "PVDReserved.bin", FileMode.Create))) {
                    writer.Write(reader.ReadBytes(653));
                }
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
        }

        public class DirFileEntry {
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
            //List<dynamic> children = new List<dynamic>();

            public DirFileEntry() {

            }

            public DirFileEntry(BinaryReader reader) {
                long position = reader.BaseStream.Position;
                recordLength = reader.ReadByte();
                extendedAttributeRecordLength = reader.ReadByte();
                extentLocation = ReadUInt32LB(reader);
                dataLength = ReadUInt32LB(reader);
                reader.ReadBytes(7);
                flags = reader.ReadByte();
                interleavedSize = reader.ReadByte();
                interleaveGap = reader.ReadByte();
                volumeSequenceNumber = ReadUInt16LB(reader);
                nameLength = reader.ReadByte();
                name = new string(reader.ReadChars(nameLength));
            }
        }

        public static UInt32 ReadAddress(BinaryReader reader) { // Turns timestamp(min - sec - sec/75) into LBA
            UInt32 first = UInt32.Parse(reader.ReadByte().ToString("X")) * 4500;
            UInt32 second = (UInt32.Parse(reader.ReadByte().ToString("X")) - 2) * 75;
            UInt32 third = UInt32.Parse(reader.ReadByte().ToString("X"));
            return first + second + third;
        }

        public static byte[] WriteAddress(UInt32 val) { // Turns LBA into timestamp (min - sec - sec/75)
            byte[] result = new byte[3];
            result[0] = Convert.ToByte((val / 4500).ToString());
            result[1] = (byte)(Convert.ToByte(((val % 4500) / 75).ToString(), 16) + 2);
            result[2] = Convert.ToByte((val % 75).ToString(), 16);
            return result;
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
