using System;
using System.Collections.Generic;
using System.Text;

namespace LodmodsDM
{
    public class PathTable
    {
        readonly string _pathTableType; // L or M
        readonly bool _isOptionalTable;
        byte _numEntries;
        public PathTableEntry[] PathEntries { get; private set; }

        public class PathTableEntry
        {
            readonly byte _directoryIdentifierLength;
            readonly byte _xaRecordLength;
            readonly UInt32 _extentLocation;
            readonly UInt16 _parentDirNum;
            readonly string _directoryIdentifier;
        }
    }
}
