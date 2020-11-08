using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ParseLoDGameData {
    class Program {
        static dynamic MIX = new System.Dynamic.ExpandoObject();
        static dynamic DABAS = new System.Dynamic.ExpandoObject();
        static dynamic BTTL = new System.Dynamic.ExpandoObject();
        static dynamic S_BTLD = new System.Dynamic.ExpandoObject();
        static dynamic S_EFFE = new System.Dynamic.ExpandoObject();
        static dynamic S_INIT = new System.Dynamic.ExpandoObject();
        static dynamic S_ITEM = new System.Dynamic.ExpandoObject();
        static dynamic S_STRM = new System.Dynamic.ExpandoObject();
        static dynamic SMAP = new System.Dynamic.ExpandoObject();
        static dynamic TEMP = new System.Dynamic.ExpandoObject();
        static dynamic TTLE = new System.Dynamic.ExpandoObject();
        static dynamic WMAP = new System.Dynamic.ExpandoObject();
        static dynamic DRAGN0 = new System.Dynamic.ExpandoObject();
        static dynamic DRAGN1 = new System.Dynamic.ExpandoObject();
        static dynamic DRAGN21 = new System.Dynamic.ExpandoObject();
        static dynamic MES = new System.Dynamic.ExpandoObject();
        static dynamic DEMO2 = new System.Dynamic.ExpandoObject();
        static dynamic DEMOH = new System.Dynamic.ExpandoObject();
        static dynamic OPENH = new System.Dynamic.ExpandoObject();
        static dynamic WAR1H = new System.Dynamic.ExpandoObject();
        static dynamic NEWROOT = new System.Dynamic.ExpandoObject();
        static dynamic LODXA00 = new System.Dynamic.ExpandoObject();
        static dynamic LODXA01 = new System.Dynamic.ExpandoObject();
        static dynamic LODXA02 = new System.Dynamic.ExpandoObject();
        static dynamic LODXA03 = new System.Dynamic.ExpandoObject();



        static void Main(string[] args) {
            string fileName = "D:/Program Files (x86)/ePSXe/Hry/The Legend of Dragoon/The Legend of Dragoon - Disc 1.bin";
            //string fileName = "G:/Projekty/LoD Versions/JP/(PSX) The Legend Of Dragoon (CD1) (SCPS-10119).bin";
            
            var root = DiscRead.GetFiles(fileName);
            /*
            SetupFiles(root);
            
            dynamic[] itemList = GameData.RipItems(S_ITEM.decompressedData);
            dynamic[] monsterList = GameData.RipMonsters(S_BTLD.decompressedData);
  
            for (int i = 0; i < 192; i++) {
                Console.WriteLine($"{itemList[i].Name} \t {itemList[i].Description}");
            }
            */

            DiscRead.RecalculateLBA(root, 0x17);
            foreach (byte b in DiscRead.CreateDirectory(root, 0x16, 0x16)) {
                Console.Write(b.ToString("X2") + " ");
            }
            
            
        }

        static void SetupFiles(List<dynamic>[] root) {
            var subroot = root[0];
            try {
                MIX = root[0].Find(directory => directory.fileIdentifier == "DA").subDirectory[1][0];
            } catch (RuntimeBinderException) {
                Console.WriteLine("Non-JP");
            }

            DABAS = root[0].Find(directory => directory.fileIdentifier == "OHTA").subDirectory[0][0].subDirectory[1][0];

            List<dynamic> OVs = root[0].Find(directory => directory.fileIdentifier == "OVL").subDirectory[1];
            BTTL = OVs.Find(file => file.fileIdentifier == "BTTL.OV_;1");
            BTTL.decompressedData = BPE.Decompress(BTTL.data);
            S_BTLD = OVs.Find(file => file.fileIdentifier == "S_BTLD.OV_;1");
            S_BTLD.decompressedData = BPE.Decompress(S_BTLD.data);
            S_EFFE = OVs.Find(file => file.fileIdentifier == "S_EFFE.OV_;1");
            S_EFFE.decompressedData = BPE.Decompress(S_EFFE.data);
            S_INIT = OVs.Find(file => file.fileIdentifier == "S_INIT.OV_;1");
            S_INIT.decompressedData = BPE.Decompress(S_INIT.data);
            S_ITEM = OVs.Find(file => file.fileIdentifier == "S_ITEM.OV_;1");
            S_ITEM.decompressedData = BPE.Decompress(S_ITEM.data);
            S_STRM = OVs.Find(file => file.fileIdentifier == "S_STRM.OV_;1");
            S_STRM.decompressedData = BPE.Decompress(S_STRM.data);
            SMAP = OVs.Find(file => file.fileIdentifier == "SMAP.OV_;1");
            SMAP.decompressedData = BPE.Decompress(SMAP.data);
            TEMP = OVs.Find(file => file.fileIdentifier == "TEMP.OV_;1");
            TEMP.decompressedData = BPE.Decompress(TEMP.data);
            TTLE = OVs.Find(file => file.fileIdentifier == "TTLE.OV_;1");
            TTLE.decompressedData = BPE.Decompress(TTLE.data);
            WMAP = OVs.Find(file => file.fileIdentifier == "WMAP.OV_;1");
            WMAP.decompressedData = BPE.Decompress(WMAP.data);

            List<dynamic> SECT = root[0].Find(directory => directory.fileIdentifier == "SECT").subDirectory[1];
            DRAGN0 = SECT.Find(file => file.fileIdentifier == "DRGN0.BIN;1");
            DRAGN1 = SECT.Find(file => file.fileIdentifier == "DRGN1.BIN;1");
            DRAGN21 = SECT.Find(file => file.fileIdentifier == "DRGN21.BIN;1");

            MES = root[0].Find(directory => directory.fileIdentifier == "SIM").subDirectory[1][0];

            List<dynamic> STR = root[0].Find(directory => directory.fileIdentifier == "STR").subDirectory[1];
            DEMO2 = STR.Find(file => file.fileIdentifier == "DEMO2.IKI;1");
            DEMOH = STR.Find(file => file.fileIdentifier == "DEMOH.IKI;1");
            OPENH = STR.Find(file => file.fileIdentifier == "OPENH.IKI;1");
            WAR1H = STR.Find(file => file.fileIdentifier == "WAR1H.IKI;1");

            NEWROOT = root[0].Find(directory => directory.fileIdentifier == "SUBMAP").subDirectory[1][0];

            List<dynamic> XA = root[0].Find(directory => directory.fileIdentifier == "XA").subDirectory[1];
            LODXA00 = STR.Find(file => file.fileIdentifier == "LODXA00.XA;1");
            LODXA01 = STR.Find(file => file.fileIdentifier == "LODXA01.XA;1");
            LODXA02 = STR.Find(file => file.fileIdentifier == "LODXA02.XA;1");
            try {
                LODXA03 = STR.Find(file => file.fileIdentifier == "LODXA03.XA;1");
            } catch (RuntimeBinderException) {
                Console.WriteLine("JP version");
            }
            
        }
    }
}
