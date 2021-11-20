using System;
using System.Collections.Generic;
using System.Text;

namespace LodmodsDM
{
    public class DirectoryTable
    {
        byte _numEntries;
        public DirectoryTableEntry[] DirectoryEntries { get; private set; }

        public class DirectoryTableEntry
        {
            byte _directoryLength;
            byte _xaRecordLength;
            readonly UInt32 _extentLocation; // Then big-endian copy
            readonly UInt32 _dataLength = 0x800; //Then big-endian copy
            readonly DirectoryDatetime _recordingDatetime;
            readonly byte _fileFlags; // may need to implement BitFlag class at somepoint
            readonly byte _interleavedUnitSize; // 0 means not interleaved, but shouldn't it be?
            readonly byte _interleaveGapSize; // 0 means not interleaved
            readonly UInt16 _volumeSequenceNumber; // Then big-endian copy
            readonly byte _fileIdentifierLength;
            readonly byte[] _fileIdentifier;
            readonly byte[] _systemUse; // Not sure what 0x8D 55 58 41 means
        }

        public class DirectoryDatetime
        {
            public byte YearsSince1900 { get; set; }
            public byte Month { get; set; }
            public byte Day { get; set; }
            public byte Hour { get; set; }
            public byte Minute { get; set; }
            public byte Second { get; set; }
            public sbyte GMT { get; set; }

            public DirectoryDatetime(byte[] datetime)
            {
                YearsSince1900 = datetime[0];
                Month = datetime[1];
                Day = datetime[2];
                Hour = datetime[3];
                Minute = datetime[4];
                Second = datetime[5];
                GMT = Convert.ToSByte(datetime[6]);
            }
        }
    }
}
