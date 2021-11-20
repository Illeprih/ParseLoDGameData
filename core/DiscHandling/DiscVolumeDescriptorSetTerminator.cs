using System;
using System.Collections.Generic;
using System.Text;

namespace LodmodsDM
{
    // Does nothing, but including for information completeness
    class VolumeDescriptorSetTerminator
    {
        readonly byte _type = 0xff; // 0xff indictates Volume Descriptor Set Terminator
        readonly string _identifier = "CD001";
        readonly byte _version = 0x01; // Volume Descriptor Version
    }
}
