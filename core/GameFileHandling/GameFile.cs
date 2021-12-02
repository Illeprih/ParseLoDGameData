using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LodmodsDM
{
    public class GameFile
    {
        public string Filename { get; set; }
        public bool IsForm2 { get; set; }
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

        public static byte BCDToByte(byte bcd)
        {
            byte tens = (byte)((bcd >> 4) * 10);
            byte ones = (byte)(bcd & 0b00001111);

            return (byte)(tens + ones);
        }

        public static byte ByteToBCD(byte hexByte)
        {
            byte upper = (byte)(hexByte / 10 << 4);
            byte lower = (byte)(hexByte % 10);

            return (byte)(upper + lower);
        }

        public byte[] ReadMainGameFile(string filepath)
        {
            if (!File.Exists(filepath)) throw new FileNotFoundException($"{filepath} does not exist.");

            uint fileSize = (uint)new FileInfo(filepath).Length;

            uint sectorDataSize;
            if (fileSize % 0x800 == 0)
            {
                IsForm2 = false;
                sectorDataSize = 0x800;
            }
            else if ((fileSize - 0x2c) % 0x930 == 0)
            {
                IsForm2 = true;
                sectorDataSize = 0x930;
            }
            else throw new InvalidDataException("Main game file size must be divisible by 2048 for data files and 2352 for XA/IKI files.");
            
            uint newSectorCount = fileSize / sectorDataSize;

            using BinaryReader reader = new BinaryReader(File.Open(filepath, FileMode.Open));
            int sectorDiff = (int)newSectorCount - DataSectorInfo.Count;
            SectorInfo lastSector;
            byte minutes = 0;
            byte seconds = 0;
            byte sectors = 0;
            if (sectorDiff < 0)
            {
                DataSectorInfo.RemoveRange((int)newSectorCount, sectorDiff);
                lastSector = DataSectorInfo[^1];
                byte eofEOR = (byte)(lastSector.Submode.RealTime == 1 ? 0x80 : 0x81);
                byte newSubmodeByte = (byte)(lastSector.Submode.SubmodeToByte() ^ eofEOR);
                lastSector.Submode.ByteToSubmode(newSubmodeByte);
            }
            else if (sectorDiff > 0)
            {
                if (IsForm2)
                {
                    reader.BaseStream.Seek(DataLength, SeekOrigin.Begin);
                    for (int i = sectorDiff; i > 0; i--)
                    {
                        SectorInfo newSectorInfo = new SectorInfo();
                        newSectorInfo.ReadHeaderInfo(reader);
                        reader.BaseStream.Seek(0x918, SeekOrigin.Current);
                    }
                }
                else
                {
                    lastSector = DataSectorInfo[^1];
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
                        SectorInfo.SubmodeByte submode = i == 1 ? SectorInfo.SubmodeByte.GenerateSubmode(0x89)
                            : SectorInfo.SubmodeByte.GenerateSubmode(0x8); // These are the only submode values in form 1 sectors
                        SectorInfo newSector = new SectorInfo(Globals.SYNC_PATTERN, bcdMinutes, bcdSeconds, bcdSectors,
                            lastSector.Mode, lastSector.FileNumber, lastSector.ChannelNumber, submode, lastSector.CodingInfo);
                        DataSectorInfo.Add(newSector);
                    }
                }
            }

            Data.SetLength(fileSize);
            DataLength = fileSize;

            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            Data.Seek(0, SeekOrigin.Begin);
            reader.BaseStream.CopyTo(Data);

            Data.Seek(0, SeekOrigin.Begin);

            return new byte[3] { minutes, seconds, sectors };
        }

        public void WriteGameFile(string filename)
        {
            if (File.Exists(filename)) File.Delete(filename);
            using BinaryWriter writer = new BinaryWriter(File.Create(filename));
            Data.Seek(0, SeekOrigin.Begin);
            writer.Write(Data.ToArray());
        }
    }
}
