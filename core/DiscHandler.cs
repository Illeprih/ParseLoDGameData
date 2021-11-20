using System;
using System.Collections.Generic;
using System.Text;

namespace LodmodsDM
{
    class DiscHandler
    {
        public class Disc
        {
            internal class Sector
            {
                static readonly byte[] _syncPattern = new byte[] 
                    { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 };
                readonly byte _min;
                readonly byte _sec;
                readonly byte _sctr;
                static readonly byte _mode = 0x02;
                readonly byte[] _header; // 4 bytes
                readonly byte _fileNum;
                readonly byte _channelNum;
                readonly byte _submode;
                readonly byte _codingInfo;
                readonly byte[] _subheader; // 8 bytes
                byte[] _data;
                readonly byte[] _edc; // 4 bytes
                readonly byte[] _ecc; // 276 bytes
            }

            // Volume Descriptor Set in sector after can be skipped
            internal class PrimaryVolumeDescriptor
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
                // 34 byte root directory entry. Differs for each disc, not sure how it translates
                readonly string _volumeSetIdentifier; // 128 bytes
                readonly string _publisherIdentifier; // 128 bytes
                readonly string _dataPreparerIdentifier; // 128 bytes
                readonly string _appIdentifier; // 128 bytes
                readonly string _copyrightFileIdentifier; // 37 bytes
                readonly string _abstractFileIdentifier; // 37 bytes
                readonly string _bibliographicFileIdentifier; // 37 bytes
                readonly string _volumeCreationDate; // 17 bytes
                string _volumeModificationDate; // 17 bytes
                readonly string _volumeEffectiveDate; // 17 bytes
                readonly byte _fileStructureVersion = 0x01;
                // 1 unused byte
                // 512 Application Used bytes (just says CD-XA001 141 bytes in)
                // 653 reserved bytes
            }

            internal class DateEntry
            {
                public string Year { get; set; }
                public string Month { get; set; }
                public string Day { get; set; }
                public string Hour { get; set; }
                public string Minute { get; set; }
                public string Second { get; set; }
                public string Centisecond { get; set; }

                internal DateEntry(byte[] datetime)
                {
                    Year = Encoding.ASCII.GetString(datetime[0..4]);
                    Month = Encoding.ASCII.GetString(datetime[4..6]);
                    Day = Encoding.ASCII.GetString(datetime[6..8]);
                    Hour = Encoding.ASCII.GetString(datetime[8..10]);
                    Minute = Encoding.ASCII.GetString(datetime[10..12]);
                    Second = Encoding.ASCII.GetString(datetime[12..14]);
                    Centisecond = Encoding.ASCII.GetString(datetime[14..16]);
                }
            }
        }
    }
}
