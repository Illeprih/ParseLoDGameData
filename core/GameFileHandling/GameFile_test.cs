using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static LodmodsDM.Globals;

namespace LodmodsDM
{
    public class BaseGameFile
    {
        public string Filename { get; }
        public string DiscDirectory { get; }
        public string ExtractedFileDirectory { get; }
        public string Filetype { get; set; }
        public uint DataLength { get; set; }
        public MemoryStream Data { get; set; } = new MemoryStream();
        
        public BaseGameFile() { }

        public BaseGameFile(string filename, string parentDirectory)
        {
            Filename = filename;
            parentDirectory = parentDirectory.Replace(Path.DirectorySeparatorChar, '/'); // Normalize path
            string[] dirParts = Path.GetDirectoryName(parentDirectory).Split('/');
            int discDirectoryIndex = Array.FindLastIndex(dirParts,
                s => s.Contains("disc", StringComparison.OrdinalIgnoreCase)) + 1;
            ExtractedFileDirectory = string.Join('/', dirParts[..discDirectoryIndex]);
            DiscDirectory = string.Join('/', dirParts[discDirectoryIndex..]);
            DataLength = (uint)new FileInfo(filename).Length;
        }

        public BaseGameFile(string filename, string discDirectory, string extractedFileDirectory,
                            uint dataLength)
        {
            Filename = filename;
            ExtractedFileDirectory = extractedFileDirectory;
            DiscDirectory = discDirectory;
            DataLength = dataLength;
        }

        public void ReadFile(string filename)
        {
            if (!File.Exists(filename)) throw new FileNotFoundException($"{filename} does not exist.");

            using BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open));

            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            reader.BaseStream.CopyTo(Data);

            Data.Seek(0, SeekOrigin.Begin);
        }

        public void WriteFile()
        {
            string fullExtractedPath = Path.Combine(ExtractedFileDirectory, DiscDirectory);
            string fullExtractedFilename = Path.Combine(fullExtractedPath, Filename);
            if (!Directory.Exists(fullExtractedPath)) Directory.CreateDirectory(fullExtractedPath);

            try
            {
                if (File.Exists(fullExtractedFilename)) File.Delete(fullExtractedFilename);
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException(
                    $"Access denied. Could not overwrite {fullExtractedFilename}. Make sure file is closed.");
            }

            using BinaryWriter writer = new BinaryWriter(File.Create(fullExtractedFilename));
            Data.Seek(0, SeekOrigin.Begin); // For safety
            writer.Write(Data.ToArray());
        }
    }

    public class MainGameFile : BaseGameFile
    {
        public bool IsForm2 { get; set; }
        public bool UsesSectorPadding { get; }
        public List<SectorInfo> DataSectorInfo { get; private set; }

        public MainGameFile() { }

        public MainGameFile(string filename, string parentDirectory, bool usesSectorPadding) : base(filename, parentDirectory)
        {
            UsesSectorPadding = usesSectorPadding;
        }

        public MainGameFile(string filename, string discDirectory, string extractedFileDirectory,
                            bool usesSectorPadding, uint dataLength) :
            base(filename, discDirectory, extractedFileDirectory, dataLength)
        {
            UsesSectorPadding = usesSectorPadding;
            DataSectorInfo = new List<SectorInfo>();
        }

        public void SetIsForm2FromFilesystem()
        {
            long currentOffset = Data.Position;
            Data.Seek(0, SeekOrigin.Begin);
            byte[] isRIFF = new byte[4];
            Data.Read(isRIFF);
            IsForm2 = isRIFF.SequenceEqual(RIFF);

            Data.Seek(currentOffset, SeekOrigin.Begin); // Reset stream to previous offset
        }

        public void SetIsForm2FromDisc()
        {
            IsForm2 = Filename.Contains(".XA", StringComparison.OrdinalIgnoreCase) ||
                Filename.Contains(".IKI", StringComparison.OrdinalIgnoreCase);
        }

        public static void Main()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead("D:/Lodmodding/lod1b/xa/lodxa00.xa"));
            byte[] x = reader.ReadBytes(4);
            Console.WriteLine(x);
        }
    }
}
