using System;
using System.Collections.Generic;
using System.Text;

namespace LodmodsDM
{
    public class PrimaryVolumeDescriptor
    {
        readonly byte _type = 0x01;
        readonly string _identifier = "CD001";
        // Unused byte
        readonly string _systemIdentifier = "PLAYSTATION";
        readonly string _VolumeIdentifier;
        // 8 unused bytes
        UInt32 _volumeSpaceSize; // Then big-endian copy
        // 32 unused bytes
        readonly UInt16 _volumeSetSize = 0x01; // Then big-endian copy
        readonly UInt16 _volumeSequenceNum = 0x01; // Then big-endian copy
        readonly UInt16 _logicalBlockSize = 0x800; // Then big-endian copy
        UInt32 _pathTableSize; // Then big-endian copy
        readonly UInt32 _pathLLocation = 0x12; // L for LSB
        readonly UInt32 _optionalPathLLocation = 0x13;
        readonly UInt32 _pathMLocation = 0x14; // M for MSB
        readonly UInt32 _optionalPathMLocation;
        readonly DirectoryTable _root;
        readonly string _volumeSetIdentifier; // 128 bytes
        readonly string _publisherIdentifier; // 128 bytes
        readonly string _dataPreparerIdentifier; // 128 bytes
        readonly string _appIdentifier; // 128 bytes
        readonly string _copyrightFileIdentifier; // 37 bytes
        readonly string _abstractFileIdentifier; // 37 bytes
        readonly string _bibliographicFileIdentifier; // 37 bytes
        readonly PVDDatetime _volumeCreationDate; // 17 bytes
        PVDDatetime _volumeModificationDate; // 17 bytes
        readonly PVDDatetime _volumeEffectiveDate; // 17 bytes
        readonly byte _fileStructureVersion = 0x01;
        // 1 unused byte
        // 512 Application Used bytes, and at 0x8D bytes in:
        readonly string _xaIdentifier = "CD-XA001";
        readonly byte[] _xaFlags = new byte[] { 0, 0 };
        readonly dynamic[] _startupDirectory; // 8 bytes, d-character string else all 0
                                              // 8 reserved bytes

        // 653 reserved bytes

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
