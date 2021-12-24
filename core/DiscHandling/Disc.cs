using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using static LodmodsDM.Globals;
using static LodmodsDM.SectorInfo;

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
            }

            return returnEntry;
        }

        private MainGameFile ExtractDiscFile(string filename, bool extractToDrive = false)
        {
            string[] fileParts = filename.Split("/");

            DirectoryTableEntry fileEntry = MatchPVDEntry(PVD.Root, fileParts);
            if (fileEntry is null)
            {
                Console.WriteLine($"File \"{filename}\" does not exist on disc.");
                return null;
            }

            string name = fileEntry.FileIdentifier.Split(";")[0].ToUpper();
            bool usesSectorPadding = name.Contains("DRGN");
            string discDirectory = fileParts.Length > 1 ? Path.Combine(fileParts[..^1]) : "";

            MainGameFile file = new MainGameFile(name, discDirectory, ExtractedFileDirectory,
                                                 usesSectorPadding, fileEntry.DataLength);

            using BinaryReader reader = new BinaryReader(File.OpenRead(FilePath));
            {
                reader.BaseStream.Seek(fileEntry.ExtentLocation * 0x930, SeekOrigin.Begin);

                file.SetIsForm2(reader);
                if (file.IsForm2 && extractToDrive)
                { // Write metadata to Data stream if writing XA/RIFF file to drive
                    uint outputDataLength = file.DataLength / 0x800 * 0x930;

                    file.Data.Write(RIFF);
                    file.Data.Write(BitConverter.GetBytes(outputDataLength + 0x24));
                    file.Data.Write(CDXAFMT);
                    file.Data.Write(new byte[4] { 0x10, 0x00, 0x00, 0x00 });
                    file.Data.Write(fileEntry.SystemUse);
                    file.Data.Write(DATA);
                    file.Data.Write(BitConverter.GetBytes(outputDataLength));
                }

                int totalBytesLeft = (int)fileEntry.DataLength;
                SectorInfo currentSector;
                bool sectorIsForm2;
                int sectorReadSize;
                int bytesToRead;
                while (totalBytesLeft > 0)
                {
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

        public Dictionary<string, MainGameFile> ExtractDiscFiles(string[] filenames, bool extractToDisc)
        { // For real-time use, can use this to extract list, then operate on whichever ones, and 
            // insert whichever ones are desired back in whenever
            Dictionary<string, MainGameFile> discFileDict = new Dictionary<string, MainGameFile>();
            foreach (string filename in filenames)
            {
                MainGameFile file = ExtractDiscFile(filename, extractToDisc);
                if (file != null) discFileDict.Add(filename, file);
            }

            if (discFileDict.Count > 0) return discFileDict;
            else return null;
        }

        // TODO: Was I going to actually go this direction or not?
        public void InsertForm1() { }

        public void InsertForm2() { }

        private void InsertDiscFile(DirectoryTableEntry fileEntry, MainGameFile file)
        {
            // The SectorInfo list should not be updated until this moment
            uint sectorDataSize = (uint)(file.IsForm2 ? 0x930 : 0x800);
            uint riffOffset = (uint)(file.IsForm2 ? 0x2c : 0);
            uint newSectorCount = (uint)Math.Ceiling((file.DataLength - riffOffset) / (float)sectorDataSize);
            int sectorDiff = (int)newSectorCount - file.DataSectorInfo.Count;
            
            SectorInfo lastSector;
            byte minutes = 0;
            byte seconds = 0;
            byte sectors = 0;
            if (sectorDiff < 0)
            {
                file.DataSectorInfo.RemoveRange((int)newSectorCount, Math.Abs(sectorDiff));
                lastSector = file.DataSectorInfo[^1];
                byte eofEOR = (byte)(lastSector.Submode.RealTime == 1 ? 0x80 : 0x81);
                byte newSubmodeByte = (byte)(lastSector.Submode.SubmodeToByte() ^ eofEOR);
                lastSector.Submode.ByteToSubmode(newSubmodeByte);
            } else if (sectorDiff > 0)
            {
                if (file.IsForm2)
                {
                    file.Data.Seek(file.DataLength, SeekOrigin.Begin);
                    SectorInfo newSectorInfo;
                    for (int i = sectorDiff; i > 0; i--)
                    {
                        newSectorInfo = new SectorInfo();
                        newSectorInfo.ReadHeaderInfo(file.Data);
                        file.Data.Seek(0x918, SeekOrigin.Current);
                    }
                }
                else
                {
                    lastSector = file.DataSectorInfo[^1];
                    lastSector.Submode.ByteToSubmode((byte)(lastSector.Submode.SubmodeToByte() & 0x7e));
                    minutes = BCDToByte(lastSector.Minutes);
                    seconds = BCDToByte(lastSector.Seconds);
                    sectors = BCDToByte(lastSector.Sectors);

                    for (int i = sectorDiff; i > 0; i--)
                    {
                        minutes = seconds == 59 && sectors == 73 ? (byte)(minutes + 1) : minutes;
                        seconds = sectors == 73 ? (seconds == 59 ? (byte)0 : (byte)(seconds + 1)) : seconds;
                        sectors = sectors == 73 ? (byte)0 : (byte)(sectors + 1);
                        byte bcdSectors = ByteToBCD(sectors);
                        byte bcdSeconds = ByteToBCD(seconds);
                        byte bcdMinutes = ByteToBCD(minutes);
                        SubmodeByte submode = i == 1 ? SubmodeByte.GenerateSubmode(0x89)
                            : SubmodeByte.GenerateSubmode(0x8); // These are the only submode values in form 1 sectors
                        SectorInfo newSector = new SectorInfo(SYNC_PATTERN, bcdMinutes, bcdSeconds, bcdSectors,
                            lastSector.Mode, lastSector.FileNumber, lastSector.ChannelNumber, submode, lastSector.CodingInfo);
                        file.DataSectorInfo.Add(newSector);
                    }
                }
            }
            file.Data.Seek(0, SeekOrigin.Begin);

            sbyte fileSizeChanged = (sbyte)(file.DataLength != fileEntry.DataLength ?
                (file.DataLength < fileEntry.DataLength ? -1 : 1) : 0);

            // TODO: This should go in outer method
            int fileOffset = (int)(fileEntry.ExtentLocation * 0x930);

            using BinaryReader brw = new BinaryReader(File.Open(FilePath, FileMode.Open, FileAccess.ReadWrite));
            byte[] dataToShift;
            if (sectorDiff > 0)
            {
                brw.BaseStream.Seek(fileOffset + fileEntry.DataLength / 0x800 * 0x930, SeekOrigin.Begin);
                dataToShift = brw.ReadBytes((int)(brw.BaseStream.Length - brw.BaseStream.Position));
            } else dataToShift = new byte[0];
            brw.BaseStream.Seek(fileOffset, SeekOrigin.Begin);

            int dataSize;
            foreach (SectorInfo info in file.DataSectorInfo)
            {
                dataSize = info.Submode.Form2 == 1 ? 0x914 : 0x800;

                byte[] subheader = { info.FileNumber, info.ChannelNumber, info.Submode.SubmodeToByte(), info.CodingInfo,
                                     info.FileNumber, info.ChannelNumber, info.Submode.SubmodeToByte(), info.CodingInfo };
                brw.ReadBytes(0x10);
                brw.BaseStream.Write(subheader, 0, 0x8);

                // It may be possible to shortcut Form 2 writes by just writing the whole thing.
                // It depends on whether modifying XA files will involve updating the sector info,
                // Which it probably should. Will leave for now, though.
                byte[] data = new byte[dataSize];
                if (file.IsForm2) file.Data.Seek(0x18, SeekOrigin.Current); // Skip header
                file.Data.Read(data, 0, dataSize);
                if (file.IsForm2) // Skip EDC/ECC
                {
                    if (dataSize == 0x914) file.Data.Seek(0x4, SeekOrigin.Current);
                    else file.Data.Seek(0x118, SeekOrigin.Current);
                }

                brw.BaseStream.Write(data);

                if (dataSize == 0x800)
                { // Calculate EDC/ECC only for form 1 sectors, LoD 0s out form 2 EDC
                    info.CalculateEDC(data, dataSize);
                    brw.BaseStream.Write(info.EDC);
                    info.CalculateECC(data);
                    brw.BaseStream.Write(info.ECC);
                } else brw.ReadBytes(4);
            }

            //TODO: update size in PVD, as well as offsets of subsequent files if necessary

            // Can use this equation universally because form 2 file size still given in increments of 0x800
            brw.BaseStream.Seek(fileOffset + file.DataLength / 0x800 * 0x930, SeekOrigin.Begin);
            brw.BaseStream.Write(dataToShift);
        }

        public void InsertDiscFiles(string[] filenames, Dictionary<string, MainGameFile> gameFileDict=null)
        { // For real-time use, can use this to extract list, then operate on whichever ones, and 
            // insert whichever ones are desired back in whenever

            Dictionary<string, object[]> entryFileDict = new Dictionary<string, object[]>();
            List<uint[]> expansionCutList = new List<uint[]>();
            List<byte[]> expansionSegmentList = new List<byte[]>();
            // TODO: Should try to sort filename list first. Might help reduce number of loops.
            // Need to sample out files in dict that are in filenames if there is a dict. otherwise,  make a dict
            // via extraction (so that bit of code needs to move here)
            foreach (string filename in filenames)
            {
                string[] fileParts = filename.Split("/");
                DirectoryTableEntry fileEntry = MatchPVDEntry(PVD.Root, fileParts);

                // Read file from filesystem if file object not passed as argument.
                // If file object passed, it should only be because a file is being extracted from disc,
                // operated on, and inserted back without ever being written to the filesystem.
                MainGameFile gameFile;
                if (gameFileDict is null)
                {
                    string fullExtractedFilename = Path.Combine(ExtractedFileDirectory, filename);
                    gameFile = ExtractDiscFile(filename);
                    if (gameFile is null) continue;

                    gameFile.ReadFile(fullExtractedFilename);
                    gameFile.SetIsForm2();
                    if (gameFile.IsForm2)
                    { // Remove RIFF header
                        using MemoryStream ms = new MemoryStream();
                        gameFile.Data.Seek(0x2c, SeekOrigin.Begin);
                        gameFile.Data.CopyTo(ms);
                        gameFile.Data.Seek(0, SeekOrigin.Begin);
                        gameFile.Data.SetLength(ms.Length);
                        ms.WriteTo(gameFile.Data);
                    }
                } else gameFile = gameFileDict[filename];

                entryFileDict[filename] = new object[] { fileEntry, gameFile };
            }

            var sortedEntryFileDict = from entry in entryFileDict orderby ((DirectoryTableEntry)entry.Value[0]).ExtentLocation ascending select entry;


            foreach (var file in sortedEntryFileDict)
            {
                DirectoryTableEntry fileEntry = ((DirectoryTableEntry)file.Value[0]);
                if (expansionCutList.Count > 0) expansionCutList[^1][1] = fileEntry.ExtentLocation * 0x930;
                expansionCutList.Add(new uint[2] { fileEntry.ExtentLocation * 0x930 + (fileEntry.DataLength / 0x800 * 0x930), 0 });
                // TODO: This should be where the logic for segmenting the disc file is
            }
            // TODO: This should be where the logic for updating the MSS info is, and maybe the PVD info (maybe)
            
        }

        public static void Main()
        {
            Stopwatch sw = new Stopwatch();
            Backup.BackupFile("D:/LodModding/Utils/lod_hack_tools/LOD1-4.iso", true);
            Disc disc = new Disc("D:/LodModding/Utils/lod_hack_tools/LOD1-4.iso", "D:/Game ROMs/The Legend of Dragoon/game_files/USA/Disc 1");
            Dictionary<string, MainGameFile> fileDict = disc.ExtractDiscFiles(new string[] { "OVL/S_ITEM.OV_", "SECT/DRGN1.BIN", "OHTA/MCX/DABAS.BIN", "SCUS_944.91"}, false);

            sw.Start();
            disc.InsertDiscFiles(new string[] { "OVL/S_ITEM.OV_", "SECT/DRGN1.BIN", "OHTA/MCX/DABAS.BIN" }, fileDict);
            Console.WriteLine("Done");
            sw.Stop();
            Console.WriteLine(sw.Elapsed.TotalSeconds.ToString());
        }
    }
}
