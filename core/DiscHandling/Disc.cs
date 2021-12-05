using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using static LodmodsDM.Globals;

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

        public MainGameFile ExtractDiscFile(string filename, bool extractToDrive = false)
        {
            // TODO: need to handle extracting XA data as well
            string[] fileParts = filename.Split("/");

            DirectoryTableEntry fileEntry = MatchPVDEntry(PVD.Root, fileParts);
            string name = fileEntry.FileIdentifier.Split(";")[0].ToUpper();
            bool usesSectorPadding = name.Contains("DRGN");
            string discDirectory = fileParts.Length > 1 ? Path.Combine(fileParts[..^1]) : "";

            MainGameFile file = new MainGameFile(name, discDirectory, ExtractedFileDirectory,
                                                 usesSectorPadding, fileEntry.DataLength);
            file.SetIsForm2FromDisc();
            if (file.IsForm2 && extractToDrive)
            {
                uint outputDataLength = file.DataLength / 0x800 * 0x930;
                
                file.Data.Write(RIFF);
                file.Data.Write(BitConverter.GetBytes(outputDataLength + 0x24));
                file.Data.Write(CDXAFMT);
                file.Data.Write(new byte[4] { 0x10, 0x00, 0x00, 0x00 });
                file.Data.Write(RIFF_SYSTEM_USE);
                file.Data.Write(DATA);
                file.Data.Write(BitConverter.GetBytes(outputDataLength));
            }

            using BinaryReader reader = new BinaryReader(File.OpenRead(FilePath));
            {
                reader.BaseStream.Seek(fileEntry.ExtentLocation * 0x930, SeekOrigin.Begin);

                int totalBytesLeft = (int)fileEntry.DataLength;
                SectorInfo currentSector;
                bool sectorIsForm2;
                int sectorReadSize;
                int bytesToRead;
                while (totalBytesLeft > 0)
                {
                    if (totalBytesLeft < 0x800)
                        Console.WriteLine("");
                    currentSector = new SectorInfo();
                    file.DataSectorInfo.Add(currentSector);
                    currentSector.ReadHeaderInfo(reader);
                    sectorIsForm2 = currentSector.Submode.Audio == 1 || currentSector.Submode.Video == 1;
                    sectorReadSize = file.IsForm2 ? (sectorIsForm2 ? 0x92c : 0x818) : 0x800;

                    if (file.IsForm2) reader.BaseStream.Seek(-0x18, SeekOrigin.Current); // Always read everything for XA/IKI

                    bytesToRead = !file.IsForm2 ? 
                        (totalBytesLeft > sectorReadSize ? sectorReadSize : totalBytesLeft) : 
                        sectorReadSize;
                    byte[] data = reader.ReadBytes(bytesToRead);
                    file.Data.Write(data);

                    // Should work because Form 2 files will always be sector-aligned
                    if (!file.IsForm2 && totalBytesLeft < sectorReadSize) 
                        reader.ReadBytes(sectorReadSize - totalBytesLeft);
                    
                    currentSector.ReadErrorCorrection(reader, (uint)sectorReadSize, sectorIsForm2);
                    if (file.IsForm2) file.Data.Write(currentSector.EDC);
                    if (file.IsForm2 && !sectorIsForm2) file.Data.Write(currentSector.ECC);

                    // Size is given in multiples of 0x800 for XA/IKI files as well, even though they are longer.
                    // Have to decrement by 0x800 regardless so it will work.
                    totalBytesLeft -= 0x800;
                }

                file.Data.Seek(0, SeekOrigin.Begin);
            }

            if (extractToDrive)
            {
                file.WriteFile();
                return null;
            } else return file;
        }

        public void InsertDiscFile(string filename, bool fileOnDrive)
        {
            string[] fileParts = filename.Split("/");

            DirectoryTableEntry fileEntry = MatchPVDEntry(PVD.Root, fileParts);
            string name = fileEntry.FileIdentifier.Split(";")[0];
            string parentDirectory = fileParts.Length > 1 ? Path.Combine(fileParts[..^1]) : "";

            MainGameFile file = ExtractDiscFile(filename);
            // TODO: Something needs to handle all the stuff ReadFile used to handle
            // TODO: need to do something with fileOnDrive so that it can work with both file and stream
            file.ReadFile("D:/Game ROMs/The Legend of Dragoon/game_files/USA/Disc 1/SECT/DRGN21.BIN");

            uint sectorDataSize = (uint)(file.IsForm2 ? 0x930 : 0x800);
            // TODO: need to report changes in file size for directory update
            // Can be done with if (DataLength != PVD whatever length)
            int fileOffset = (int)(fileEntry.ExtentLocation * 0x930);

            // TODO: Need to update all sectors that exist after file inserted
            // if (!updatedMSS.All(i => i == 0))
            using BinaryReader brw = new BinaryReader(File.Open(FilePath, FileMode.Open, FileAccess.ReadWrite));
            byte[] dataToShift;
            if (file.DataLength > fileEntry.DataLength)
            {
                brw.BaseStream.Seek(fileOffset + fileEntry.DataLength / 0x800 * 0x930, SeekOrigin.Begin);  // TODO: I think this needs adjusting based on Form 1 vs Form 2
                dataToShift = brw.ReadBytes((int)(brw.BaseStream.Length - brw.BaseStream.Position));
            } else dataToShift = new byte[0];
            brw.BaseStream.Seek(fileOffset, SeekOrigin.Begin);

            int sectorIndex = 0;
            int dataSize;
            foreach (SectorInfo info in file.DataSectorInfo)
            {
                dataSize = info.Submode.Form2 == 1 ? 0x914 : 0x800;

                byte[] subheader = { info.FileNumber, info.ChannelNumber, info.Submode.SubmodeToByte(), info.CodingInfo,
                                        info.FileNumber, info.ChannelNumber, info.Submode.SubmodeToByte(), info.CodingInfo };
                brw.ReadBytes(0x10);
                brw.BaseStream.Write(subheader, 0, 0x8);

                byte[] data = new byte[dataSize];
                if (dataSize == 0x914) file.Data.Seek(0x18, SeekOrigin.Current);
                file.Data.Read(data, 0, dataSize);
                if (dataSize == 0x914) file.Data.Seek(0x4, SeekOrigin.Current);
                brw.BaseStream.Write(data);

                info.CalculateEDC(data, dataSize);
                brw.BaseStream.Write(info.EDC);
                if (dataSize == 0x800)
                {
                    info.CalculateECC(data);
                    brw.BaseStream.Write(info.ECC);
                }
                sectorIndex++;
            }

            brw.BaseStream.Seek(fileOffset + file.DataLength / 0x800 * 0x930, SeekOrigin.Begin);
            brw.BaseStream.Write(dataToShift);

            brw.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
        }

    public static void Main()
    {
        Stopwatch sw = new Stopwatch();
        Backup.BackupFile("D:/LodModding/Utils/lod_hack_tools/LOD1-4.iso", true);
        Disc disc = new Disc("D:/LodModding/Utils/lod_hack_tools/LOD1-4.iso", "D:/Game ROMs/The Legend of Dragoon/game_files/USA/Disc 1");
        disc.ExtractDiscFile("OVL/S_ITEM.OV_", true);

        /*sw.Start();
        disc.InsertDiscFile("SECT/DRGN21.BIN", true);
        Console.WriteLine("Done");
        sw.Stop();
        Console.WriteLine(sw.Elapsed.TotalSeconds.ToString());
        Console.ReadLine();*/
    }
    }
}
