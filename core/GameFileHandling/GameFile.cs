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

        public void ReadMainGameFile(string filepath)
        {
            if (!File.Exists(filepath)) throw new FileNotFoundException($"{filepath} does not exist.");

            uint fileSize = (uint)new FileInfo(filepath).Length;
            if (fileSize % 0x800 == 0) IsForm2 = false;
            else if ((fileSize - 0x2c) % 0x930 == 0) IsForm2 = true;
            else throw new InvalidDataException("Main game file size must be divisible by 2048 for data files and 2352 for XA/IKI files.");
            uint sectorDataSize = (uint)(IsForm2 ? 0x930 : 0x800);
            uint newSectorCount = fileSize / sectorDataSize;

            int sectorDiff = (int)newSectorCount - DataSectorInfo.Count;
            if (sectorDiff < 0)
            {
                DataSectorInfo.RemoveRange((int)newSectorCount, sectorDiff);
                if (DataSectorInfo[^1].Submode.RealTime == 1) DataSectorInfo[^1].Submode.EndOfFile = 1;
                else {
                    DataSectorInfo[^1].Submode.EndOfFile = 1;
                    DataSectorInfo[^1].Submode.EndOfRecord = 1;
                }
                Data.SetLength(fileSize);
                DataLength = fileSize;
            }

            using BinaryReader reader = new BinaryReader(File.Open(filepath, FileMode.Open));
            {
                Data.Seek(0, SeekOrigin.Begin);
                reader.BaseStream.CopyTo(Data);
                Data.Seek(0, SeekOrigin.Begin);
                /*Data.Seek(0, SeekOrigin.Begin);

                int remainingFileLength = (int)reader.BaseStream.Length;
                int sectorIndex;
                int dataSize;
                bool stopEarly = false;
                foreach (SectorInfo info in DataSectorInfo)
                {
                    sectorIndex = DataSectorInfo.IndexOf(info);
                    if (info.Submode.Data == 1) dataSize = 0x800;
                    else if (info.Submode.Audio == 1) dataSize = 0x930;
                    else throw new System.Data.DataException($"{filepath} sector {sectorIndex} is not data or audio.");

                    byte[] newSectorData = reader.ReadBytes(dataSize);
                    if (0 < newSectorData.Length && newSectorData.Length < dataSize) 
                        throw new System.Data.DataException($"Should always be getting sector aligned data for main game file.");

                    if (remainingFileLength == dataSize && sectorIndex < DataSectorInfo.Count)
                    {
                        info.Submode.ConvertByteToSubmode(0x89); // Only for data sectors atm, still no idea how XA decides what EOF is
                        int sectorUnderflow = DataSectorInfo.Count - sectorIndex - 1;
                        DataSectorInfo.RemoveRange(sectorIndex + 1, sectorUnderflow);
                        Data.SetLength(Data.Length - (sectorUnderflow * dataSize));
                        DataLength = (uint)Data.Length;
                        stopEarly = true;
                    }

                    byte[] oldData = new byte[dataSize];
                    Data.Read(oldData, 0, dataSize);
                    if (!newSectorData.SequenceEqual(oldData))
                    {
                        Data.Seek(-dataSize, SeekOrigin.Current);
                        Data.Write(newSectorData);

                        if (dataSize == 0x800)
                        {
                            info.CalculateEDC(newSectorData);
                            info.CalculateECC(newSectorData);
                        }
                    }

                    remainingFileLength -= dataSize;
                    if (stopEarly) break;
                }

                if (remainingFileLength > 0)
                {
                    dataSize = 0x800; // Can only make this work for data files for now
                    int overflowSectorCount = (int)Math.Ceiling((double)remainingFileLength / 0x800);

                    SectorInfo lastSector = DataSectorInfo[^1];
                    byte sectors = BCDToByte(lastSector.Sectors);
                    byte seconds = BCDToByte(lastSector.Seconds);
                    byte minutes = BCDToByte(lastSector.Minutes);
                    lastSector.Submode.ConvertByteToSubmode(0x8);

                    byte[] currentSector = new byte[dataSize];
                    for (int i = overflowSectorCount; i > 0; i--)
                    {
                        currentSector = reader.ReadBytes(dataSize);

                        sectors = sectors == 74 ? (byte)0 : (byte)(sectors + 1);
                        seconds = sectors == 0 ? (seconds == 60 ? (byte)0 : (byte)(seconds + 1)) : seconds;
                        minutes = seconds == 0 ? (byte)(minutes + 1) : minutes;
                        byte bcdSectors = ByteToBCD(sectors);
                        byte bcdSeconds = ByteToBCD(seconds);
                        byte bcdMinutes = ByteToBCD(minutes);
                        SectorInfo.SubmodeByte submode = i == 1 ? SectorInfo.SubmodeByte.ByteToSubmode(0x89)
                            : SectorInfo.SubmodeByte.ByteToSubmode(0x8);
                        SectorInfo newSector = new SectorInfo(Globals.SYNC_PATTERN, bcdMinutes, bcdSeconds, bcdSectors, 
                            lastSector.Mode, lastSector.FileNumber, lastSector.ChannelNumber, submode, lastSector.CodingInfo);
                        DataSectorInfo.Add(newSector);
                        Data.Write(currentSector);
                        newSector.CalculateEDC(currentSector);
                        newSector.CalculateECC(currentSector);
                    }
                    DataLength = (uint)Data.Length;
                }*/
            }
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
