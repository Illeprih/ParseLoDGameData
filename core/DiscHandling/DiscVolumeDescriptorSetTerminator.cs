using System;
using System.Collections.Generic;
using System.Text;

namespace LodmodsDM
{
    // Does nothing, but including for information completeness
    public class VolumeDescriptorSetTerminator
    {
        public SectorInfo TerminatorSectorInfo { get; }
        public byte Type { get; } = 0xff; // 0xff indictates Volume Descriptor Set Terminator
        public string Identifier { get; } = "CD001";
        public byte Version { get; } = 0x01; // Volume Descriptor Version
    }
}
