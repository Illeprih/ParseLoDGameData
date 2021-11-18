using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LodmodsDM {

    public class UnpackPS1Data {

        static readonly byte[] syncPattern = new byte[] { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 };

        public static void UnpackData(dynamic file, byte[] data) {
            BinaryReader reader = new BinaryReader(new MemoryStream(data));
            List<byte> result = new List<byte>();
            for (int i = 0; i < Math.Ceiling((double)file.DataLength / 2048); i++) {
                if (!reader.ReadBytes(12).SequenceEqual(syncPattern)) {
                    throw new ArgumentException($"Synchronization pattern not found. Incorrect file type. {file.Name} Segment: {i}");
                }
                reader.BaseStream.Seek(4, SeekOrigin.Current); // Address and Mode
                reader.BaseStream.Seek(1, SeekOrigin.Current); // File Number
                reader.BaseStream.Seek(1, SeekOrigin.Current); // Channel Number
                var flags = new SubheaderFlags(reader.ReadByte());
                reader.BaseStream.Seek(1, SeekOrigin.Current); // Coding info
                reader.BaseStream.Seek(4, SeekOrigin.Current); // skip copy of subheader
                int form = flags.Form2 ? 1 : 0;
                result.AddRange(reader.ReadBytes(2048 + form * 0x114));
                reader.BaseStream.Seek(4, SeekOrigin.Current); // Error detection
                reader.BaseStream.Seek(276 - form * 276, SeekOrigin.Current); // Error correction
            }
            
            if (file.Name.Contains(".OV_")) {
                file.Data = BPE.Decompress(result.ToArray());
            } else {
                file.Data = result.ToArray();
            }
            reader.Close();
        }

        public class SubheaderFlags {
            bool endOfRecord;
            bool video;
            bool audio;
            bool data;
            bool trigger;
            bool form2;
            bool realTime;
            bool endOfFile;

            public bool EndOfRecord { get { return endOfRecord; } }
            public bool Video { get { return video; } }
            public bool Audio { get { return audio; } }
            public bool Data { get { return data; } }
            public bool Trigger { get { return trigger; } }
            public bool Form2 { get { return form2; } }
            public bool RealTime { get { return realTime; } }
            public bool EndOfFile { get { return endOfFile; } }

            public SubheaderFlags(byte flagByte) {
                endOfRecord = Convert.ToBoolean(flagByte & 128);
                video = Convert.ToBoolean(flagByte & 64);
                audio = Convert.ToBoolean(flagByte & 32);
                data = Convert.ToBoolean(flagByte & 16);
                trigger = Convert.ToBoolean(flagByte & 8);
                form2 = Convert.ToBoolean(flagByte & 4);
                realTime = Convert.ToBoolean(flagByte & 2);
                endOfFile = Convert.ToBoolean(flagByte & 1);
            }
        }


    }
    
    
}
