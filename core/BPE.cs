using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LodmodsDM {
    class BPE {
        public static byte[] Decompress(byte[] file) {
            BinaryReader compressedFile = new BinaryReader(new MemoryStream(file));
            //BinaryReader compressedFile = new BinaryReader(File.Open(path, FileMode.Open));
            List<byte> decompressedBytes = new List<byte>();
            var size = compressedFile.ReadBytes(8);
            if (Encoding.Default.GetString(size).Contains("BPE")) {
                //compressedFile.BaseStream.Seek(0, SeekOrigin.Begin);
            } else {
                Console.WriteLine("Not a BPE file");
                return decompressedBytes.ToArray();
            }

            int block = -1;

            int decompressedFileOffset = 0;
            List<int> blocksizeList = new List<int>();

            while (true) {
                int decompressedBlockSize = compressedFile.ReadInt32();
                if (decompressedBlockSize == 0) {
                    break;
        
                } else if (decompressedBlockSize > 0x800) {
                    Console.WriteLine("Invalid block size. Skipping file.");
                }

                Dictionary<byte, byte> leftch = new Dictionary<byte, byte>();
                Dictionary<byte, byte?> rightch = new Dictionary<byte, byte?>();

                for (int i = 0; i < 256; i++) {
                    leftch.Add((byte)i, (byte)i);
                    rightch.Add((byte)i, null);
                }

                int key = 0x0;
                while (key < 0x100) {
                    byte bytePairToRead = compressedFile.ReadByte();
                    if (bytePairToRead >= 0x80) {
                        key += bytePairToRead - 0x7F;
                        bytePairToRead = 0;
                    }

                    if (key < 0x100) {
                        for (int i = 0; i < bytePairToRead + 1; i++) {
                            byte compressedByte = compressedFile.ReadByte();
                            leftch[(byte)key] = compressedByte;
        

                    if (compressedByte != key) {
                                compressedByte = compressedFile.ReadByte();
                                rightch[(byte)key] = compressedByte;
                            }
                            key++;
                        }
                    }
                }

                List<byte> unresolvedBytes = new List<byte>();
                while (decompressedBlockSize > 0) {
                    byte compressedByte = compressedFile.ReadByte();
                    unresolvedBytes.Add(compressedByte);

                    while (unresolvedBytes.Count > 0) {
                        compressedByte = unresolvedBytes[0];
                        unresolvedBytes.RemoveAt(0);

                        if (compressedByte == leftch[compressedByte]) {
                            // if
                            decompressedBytes.Add(compressedByte);
                            decompressedBlockSize -= 1;
                        } else {
                            unresolvedBytes.Insert(0, (byte)rightch[compressedByte]);
                            unresolvedBytes.Insert(0, leftch[compressedByte]);
                        }
                    }
                }

                if (compressedFile.BaseStream.Position % 4 != 0) {
                    compressedFile.BaseStream.Seek(compressedFile.BaseStream.Position + (4 - compressedFile.BaseStream.Position % 4), SeekOrigin.Begin);
                }
            }
            return decompressedBytes.ToArray();
        }
    }
}
