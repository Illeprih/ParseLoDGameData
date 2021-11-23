using System;
using System.Collections.Generic;
using System.Text;

namespace LodmodsDM
{
    public class SectorInfo
    {
        public byte[] SyncPattern { get; set; }
        public byte Minutes { get; set; }
        public byte Seconds { get; set; }
        public byte Sector { get; set; }
        public byte Mode { get; set; }
        public byte FileNumber { get; set; }
        public byte ChannelNumber { get; set; }
        public byte Submode { get; set; }
        public byte CodingInfo { get; set; }
        public byte[] EDC { get; set; } = new byte[4];
        public byte[] ECC { get; set; } = new byte[276];

        public SectorInfo() { }

        public void CalculateEDC(byte[] data) { }

        public void CalculateECC(byte[] data) { }
    }
}
