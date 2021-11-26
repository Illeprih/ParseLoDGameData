using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LodmodsDM
{
    public class GameFile
    {
        public string Filename { get; set; }
        public string FileType { get; set; }
        public uint DataLength { get; set; }
        public bool UsesSectorPadding { get; }
        public string ParentDirectory { get; }
        public string ParentFile { get; }
        public List<SectorInfo> DataSectorInfo { get; } = new List<SectorInfo>();
        public MemoryStream Data { get; set; } = new MemoryStream();

        public GameFile(string filename, uint dataLength, bool usesSectorPadding,
            string parentDirectory, string parentFile)
        {
            Filename = filename;
            DataLength = dataLength;
            UsesSectorPadding = usesSectorPadding;
            ParentDirectory = parentDirectory;
            ParentFile = parentFile;
        }
    }
}
