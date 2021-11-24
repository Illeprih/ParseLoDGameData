using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static LodmodsDM.BothEndian;

namespace LodmodsDM
{
    public class PrimaryVolumeDescriptor
    {
        public SectorInfo PVDSectorInfo { get; } = new SectorInfo();
        public byte Type { get; } = 0x01;
        public string Identifier { get; } = "CD001";
        public byte Version { get; } = 0x01;
        // Unused byte
        public string SystemIdentifier { get; } // Don't forget to pad when writing
        public string VolumeIdentifier { get; } // Don't forget to pad when writing
        // 8 unused bytes
        public UInt32 VolumeSpaceSize { get; set; } // Then big-endian copy
        // 32 unused bytes
        public UInt16 VolumeSetSize { get; } // Then big-endian copy
        public UInt16 VolumeSequenceNum { get; } // Then big-endian copy
        public UInt16 LogicalBlockSize { get; } // Then big-endian copy
        public UInt32 PathTableSize { get; } // Then big-endian copy
        public UInt32 PathLLocation { get; } // L for LSB
        public UInt32 OptionalPathLLocation { get; }
        public UInt32 PathMLocation { get; } // M for MSB
        public UInt32 OptionalPathMLocation { get; }
        public DirectoryTable Root { get; private set; }
        public string VolumeSetIdentifier { get; } // 128 bytes, don't forget to pad
        public string PublisherIdentifier { get; } // 128 bytes, don't forget to pad
        public string DataPreparerIdentifier { get; } // 128 bytes, don't forget to pad
        public string AppIdentifier { get; } // 128 bytes, don't forget to pad
        public string CopyrightFileIdentifier { get; } // 37 bytes, don't forget to pad
        public string AbstractFileIdentifier { get; } // 37 bytes, don't forget to pad
        public string BibliographicFileIdentifier { get; } // 37 bytes, don't forget to pad
        public PVDDatetime VolumeCreationDate { get; } // 17 bytes
        public PVDDatetime VolumeModificationDate { get; set; } // 17 bytes
        public PVDDatetime VolumeExpirationDate { get; } // 17 bytes
        public PVDDatetime VolumeEffectiveDate { get; } // 17 bytes
        public byte FileStructureVersion { get; } = 0x01;
        // 1 unused byte

        // 512 Application Used bytes, and at 0x8D bytes in:
        public string XAIdentifier { get; } = "CD-XA001";
        public byte[] XAFlags { get; } = new byte[] { 0, 0 };
        public byte[] StartupDirectory { get; } = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }; // 8 bytes, d-character string else all 0
        // 8 Application Used reserved bytes

        // 653 reserved bytes to end PVD

        public PrimaryVolumeDescriptor(BinaryReader reader)
        {
            // Get sector info
            PVDSectorInfo.SyncPattern = reader.ReadBytes(0xc);
            PVDSectorInfo.Minutes = reader.ReadByte();
            PVDSectorInfo.Seconds = reader.ReadByte();
            PVDSectorInfo.Sector = reader.ReadByte();
            PVDSectorInfo.Mode = reader.ReadByte();
            PVDSectorInfo.FileNumber = reader.ReadByte();
            PVDSectorInfo.ChannelNumber = reader.ReadByte();
            PVDSectorInfo.Submode = reader.ReadByte();
            PVDSectorInfo.CodingInfo = reader.ReadByte();

            reader.ReadBytes(0xc);

            // Get PVD info
            SystemIdentifier = Encoding.ASCII.GetString(reader.ReadBytes(0x20)).Trim();
            VolumeIdentifier = Encoding.ASCII.GetString(reader.ReadBytes(0x20)).Trim();
            reader.ReadBytes(0x8);
            VolumeSpaceSize = ReadUInt32Both(reader);
            reader.ReadBytes(0x20);
            VolumeSetSize = ReadUInt16Both(reader);
            VolumeSequenceNum = ReadUInt16Both(reader);
            LogicalBlockSize = ReadUInt16Both(reader);
            PathTableSize = ReadUInt32Both(reader);
            PathLLocation = BitConverter.ToUInt32(reader.ReadBytes(0x4));
            OptionalPathLLocation = BitConverter.ToUInt32(reader.ReadBytes(0x4));
            PathMLocation = BitConverter.ToUInt32(reader.ReadBytes(0x4).Reverse().ToArray());
            OptionalPathMLocation = BitConverter.ToUInt32(reader.ReadBytes(0x4).Reverse().ToArray());
            Root = new DirectoryTable(reader, false);
            VolumeSetIdentifier = Encoding.ASCII.GetString(reader.ReadBytes(0x80)).Trim();
            PublisherIdentifier = Encoding.ASCII.GetString(reader.ReadBytes(0x80)).Trim();
            DataPreparerIdentifier = Encoding.ASCII.GetString(reader.ReadBytes(0x80)).Trim();
            AppIdentifier = Encoding.ASCII.GetString(reader.ReadBytes(0x80)).Trim();
            CopyrightFileIdentifier = Encoding.ASCII.GetString(reader.ReadBytes(0x25)).Trim();
            AbstractFileIdentifier = Encoding.ASCII.GetString(reader.ReadBytes(0x25)).Trim();
            BibliographicFileIdentifier = Encoding.ASCII.GetString(reader.ReadBytes(0x25)).Trim();
            VolumeCreationDate = new PVDDatetime(reader.ReadBytes(0x11));
            VolumeModificationDate = new PVDDatetime(reader.ReadBytes(0x11));
            VolumeExpirationDate = new PVDDatetime(reader.ReadBytes(0x11));
            VolumeEffectiveDate = new PVDDatetime(reader.ReadBytes(0x11));
            reader.ReadBytes(0x48f);
            PVDSectorInfo.EDC = reader.ReadBytes(0x4);
            PVDSectorInfo.ECC = reader.ReadBytes(0x114);
        }

        public class PVDDatetime
        {
            public string Year { get; set; }
            public string Month { get; set; }
            public string Day { get; set; }
            public string Hour { get; set; }
            public string Minute { get; set; }
            public string Second { get; set; }
            public string Centisecond { get; set; }
            public sbyte GMT { get; set; }

            public PVDDatetime(byte[] datetime)
            {
                string datetimeString = Encoding.ASCII.GetString(datetime[..^1]);

                Year = datetimeString[..4];
                Month = datetimeString[4..6];
                Day = datetimeString[6..8];
                Hour = datetimeString[8..10];
                Minute = datetimeString[10..12];
                Second = datetimeString[12..14];
                Centisecond = datetimeString[14..16];
                GMT = Convert.ToSByte(datetime[16]);
            }
        }
    }
}
