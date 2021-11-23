using System;
using System.Collections.Generic;
using System.Text;

namespace LodmodsDM
{
    public class SectorInfo
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
        byte[] _edc; // 4 bytes
        readonly byte[] _ecc; // 276 bytes
    }
}
