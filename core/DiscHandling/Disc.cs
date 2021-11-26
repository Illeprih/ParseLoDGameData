using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LodmodsDM
{
    public class Disc
    {
        static readonly byte[] _pvdHeader = new byte[] { 0x0, 0x0, 0x9, 0x0 };
        public string FilePath { get; set; }
        public byte[] SystemData { get; }
        public PrimaryVolumeDescriptor PVD { get; set; }
        public VolumeDescriptorSetTerminator VDST { get; }
        public string ExtractedFileDirectory { get; set; }

        public Disc(string filepath, string outputDirectory)
        {
            if (File.Exists(filepath)) FilePath = filepath; else throw new FileNotFoundException($"{filepath} does not exist.");
            ExtractedFileDirectory = outputDirectory;
            if (!Directory.Exists(ExtractedFileDirectory)) Directory.CreateDirectory(ExtractedFileDirectory);

            using BinaryReader reader = new BinaryReader(File.OpenRead(FilePath));
            {
                if (!reader.ReadBytes(0xc).SequenceEqual(Globals.SYNC_PATTERN))
                {
                    throw new ArgumentException("Synchronization pattern not found. Incorrect file type.");
                }
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                SystemData = reader.ReadBytes(0x9300);

                reader.BaseStream.Seek(0x10, SeekOrigin.Current);
                byte[] subheader = reader.ReadBytes(0x4);
                if (!subheader.SequenceEqual(_pvdHeader)) 
                    throw new ArgumentException("Primary Volume descriptor header not found. Incorrect file type.");
                reader.BaseStream.Seek(-0x14, SeekOrigin.Current);
                PVD = new PrimaryVolumeDescriptor(reader);

                // In place of creating proper DescriptorTerminator
                reader.ReadBytes(0x930);
                VDST = new VolumeDescriptorSetTerminator();
            }
        }

        public static DirectoryTableEntry MatchPVDEntry(DirectoryTableEntry entry, string[] fileParts)
        {
            DirectoryTableEntry returnEntry = entry.Children.FirstOrDefault(
                dirEntry => dirEntry.FileIdentifier.Split(";")[0] == fileParts[0]);

            if (fileParts.Length > 1)
            {
                returnEntry = MatchPVDEntry(returnEntry, fileParts[1..]);
                return returnEntry;
            } else return returnEntry;
        }

        public GameFile ExtractDiscFile(string filename, bool extractToDrive)
        {
            string[] fileParts = filename.Split("/");
           
            DirectoryTableEntry fileEntry = MatchPVDEntry(PVD.Root, fileParts);
            string name = fileEntry.FileIdentifier.Split(";")[0];
            bool usesSectorPadding = name.Contains("OV_") || name.Contains("IKI") ? false : true;
            string parentDirectory = fileParts.Length > 1 ? Path.Combine(fileParts[..^1]) : "";

            GameFile file = new GameFile(name, fileEntry.DataLength, usesSectorPadding, parentDirectory, null);

            string fullExtractedPath = Path.Combine(ExtractedFileDirectory, parentDirectory);
            string fullExtractedFilename = Path.Combine(ExtractedFileDirectory, filename);
            if (!Directory.Exists(fullExtractedPath)) Directory.CreateDirectory(fullExtractedPath);

            using BinaryWriter writer = new BinaryWriter(File.Create(fullExtractedFilename));
            using BinaryReader reader = new BinaryReader(File.OpenRead(FilePath));
            {
                reader.BaseStream.Seek(fileEntry.ExtentLocation * 0x930, SeekOrigin.Begin);

                int totalBytesLeft = (int)fileEntry.DataLength;
                int bytesToRead;
                while (totalBytesLeft > 0)
                {
                    file.DataSectorInfo.Add(new SectorInfo());
                    file.DataSectorInfo[^1].ReadHeaderInfo(reader);

                    bytesToRead = totalBytesLeft > 0x800 ? 0x800 : totalBytesLeft;
                    byte[] data = reader.ReadBytes(bytesToRead);
                    if (extractToDrive) writer.Write(data); else file.Data.Write(data);
                    if (totalBytesLeft < 0x800) reader.ReadBytes(0x800 - (int)totalBytesLeft);
                    totalBytesLeft -= 0x800;
                    file.DataSectorInfo[^1].ReadErrorCorrection(reader);
                }
            }

            if (!extractToDrive) return file; else return null;
        }

        public static void Main()
        {
            Disc disc = new Disc("D:/Game ROMs/The Legend of Dragoon/LOD1-4.iso", "D:/Game ROMs/The Legend of Dragoon/game_files/USA/Disc 1");
            GameFile gameFile = disc.ExtractDiscFile("SECT/DRGN21.BIN", false);
            Console.WriteLine("Done");
        }
    }
}
