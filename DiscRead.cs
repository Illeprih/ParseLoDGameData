using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ParseLoDGameData {
    class DiscRead {
        public class PrimaryVolumeDescriptor {
            public byte[] syncPattern = new byte[] { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00};
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
            // Directory entry for the root directory
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



            public PrimaryVolumeDescriptor(BinaryReader data) {
                syncPattern = data.ReadBytes(12);
                byte[] tempAddress = new byte[4];
                for (int i = 0; i < 3; i++) {
                    tempAddress[i] = data.ReadByte();
                }
                address = BitConverter.ToUInt32(tempAddress);
                mode = data.ReadByte();
                subheader = data.ReadBytes(8);
                typeCode = data.ReadByte();
                standardIdentifier = data.ReadChars(5).ToString();
                version = data.ReadByte();
                data.ReadByte(); // Unused
                systemIdentifier = data.ReadChars(32).ToString();
                volumeIdentifier = data.ReadChars(32).ToString();
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
                data.ReadBytes(34); // Directory entry for the root directory
                volumeSetIdentifier = data.ReadChars(128).ToString();
                publisherIdentifier = data.ReadChars(128).ToString();
                dataPreparerIdentifier = data.ReadChars(128).ToString();
                applicationIdentifier = data.ReadChars(128).ToString();
                copyrightFileIdentifier = data.ReadChars(38).ToString();
                abstractFileIdentifier = data.ReadChars(36).ToString();
                bibliographicFileIdentifier = data.ReadChars(37).ToString();
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
                volumeSetTerminatorType = data.ReadByte();
                volumeSetTerminatorIdentifier = data.ReadChars(5).ToString();
                volumeSetTerminatorVersion = data.ReadByte();
            }

            
        }

        public class Directory {
            public byte recordLength = 33;
            public byte extendedAttributeRecordLength = 0;
            public UInt32 extentLocation = 0;
            public UInt32 dataLength = 0;
            public Directory.Date date = new Directory.Date( 0, 1, 1, 0, 0, 0, 0 );
            public byte fileFlags = 0;
            public byte interleavedSize = 0;
            public byte interleaveGap = 0;
            public UInt16 volumeSequenceNumber = 1;
            public byte identifierLength = 1;
            public string fileIdentifier = "a;1";
            public byte[] systemUse = new byte[0];

            public Directory(BinaryReader data) {
                long position = data.BaseStream.Position;
                recordLength = data.ReadByte();
                extendedAttributeRecordLength = data.ReadByte();
                extentLocation = ReadUInt32LB(data);
                dataLength = ReadUInt32LB(data);
                date = new Directory.Date(data);
                fileFlags = data.ReadByte();
                interleavedSize = data.ReadByte();
                interleaveGap = data.ReadByte();
                volumeSequenceNumber = ReadUInt16LB(data);
                identifierLength = data.ReadByte();
                fileIdentifier = data.ReadChars(identifierLength).ToString();
                if (identifierLength % 2 == 0) {
                    data.ReadByte(); // padding
                }
                int left2Read = (int) (position + recordLength - data.BaseStream.Position);
                if (left2Read > 0) {
                    systemUse = data.ReadBytes(left2Read);
                }


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
                year = Convert.ToUInt16(segment.ReadChars(4).ToString());
                month = Convert.ToByte(segment.ReadChars(2).ToString());
                day = Convert.ToByte(segment.ReadChars(2).ToString());
                hour = Convert.ToByte(segment.ReadChars(2).ToString());
                minute = Convert.ToByte(segment.ReadChars(2).ToString());
                second = Convert.ToByte(segment.ReadChars(2).ToString());
                milisecond = Convert.ToByte(segment.ReadChars(2).ToString());
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
    }
}
