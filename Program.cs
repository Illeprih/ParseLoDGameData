using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Collections;
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
        static dynamic DRGN0 = new System.Dynamic.ExpandoObject();
        static dynamic DRGN1 = new System.Dynamic.ExpandoObject();
        static dynamic DRGN21 = new System.Dynamic.ExpandoObject();
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
            //string fileName = "D:/Program Files (x86)/ePSXe/Hry/The Legend of Dragoon/The Legend of Dragoon - Disc 1.bin";
            string fileName = "G:/Projekty/LoD Versions/JP/(PSX) The Legend Of Dragoon (CD1) (SCPS-10119).bin";
            var unpackPath = Directory.GetCurrentDirectory() + "/Unpacked/";
            DiscHandeler2.UnpackDisc(fileName, unpackPath);


            //string fileName = "G:/Projekty/LoD Versions/JP/(PSX) The Legend Of Dragoon (CD1) (SCPS-10119).bin";
            /*
            var disc1 = new DiscHandeler.Disc(fileName);
            foreach(var children in disc1.Root) {
                Console.WriteLine(children.Name);
                foreach (var sub in children.Children) {
                    Console.WriteLine("\t\t" + sub.Name);
                    foreach( var subsub in sub.Children) {
                        Console.WriteLine("\t\t\t\t" + subsub.Name);
                    }
                }
            }
            SetupFiles(disc1);
            */

            /*
            dynamic[] itemList = GameData.RipItems(S_ITEM.DecompressedData);
            dynamic[] monsterList = GameData.RipMonsters(S_BTLD.DecompressedData);
  
            for (int i = 0; i < 192; i++) {
                Console.WriteLine($"{itemList[i].Name} \t {itemList[i].Description}");
            }
            */
        }

        static void SetupFiles(dynamic disc) {
            List<dynamic> root = disc.Root;
           
            try {
                MIX = root.Find(dir => dir.Name == "DA").Children[0];
            } catch (RuntimeBinderException) {
                Console.WriteLine("Non-JP");
            }

            DABAS = root.Find(dir => dir.Name == "OHTA").Children[0].Children[0];

            List<dynamic> OVs = root.Find(dir => dir.Name == "OVL").Children;
            BTTL = OVs.Find(file => file.Name == "BTTL.OV_;1");
            S_BTLD = OVs.Find(file => file.Name == "S_BTLD.OV_;1");
            S_EFFE = OVs.Find(file => file.Name == "S_EFFE.OV_;1");
            S_INIT = OVs.Find(file => file.Name == "S_INIT.OV_;1");
            S_ITEM = OVs.Find(file => file.Name == "S_ITEM.OV_;1");
            S_STRM = OVs.Find(file => file.Name == "S_STRM.OV_;1");
            SMAP = OVs.Find(file => file.Name == "SMAP.OV_;1");
            TEMP = OVs.Find(file => file.Name == "TEMP.OV_;1");
            TTLE = OVs.Find(file => file.Name == "TTLE.OV_;1");
            WMAP = OVs.Find(file => file.Name == "WMAP.OV_;1");

            List<dynamic> SECT = root.Find(dir => dir.Name == "SECT").Children;
            DRGN0 = SECT.Find(file => file.Name == "DRGN0.BIN;1");
            DRGN1 = SECT.Find(file => file.Name == "DRGN1.BIN;1");
            DRGN21 = SECT.Find(file => file.Name == "DRGN21.BIN;1");

            MES = root.Find(dir => dir.Name == "SIM").Children;

            List<dynamic> STR = root.Find(dir => dir.Name == "STR").Children;
            DEMO2 = STR.Find(file => file.Name == "DEMO2.IKI;1");
            DEMOH = STR.Find(file => file.Name == "DEMOH.IKI;1");
            OPENH = STR.Find(file => file.Name == "OPENH.IKI;1");
            WAR1H = STR.Find(file => file.Name == "WAR1H.IKI;1");

            NEWROOT = root.Find(dir => dir.Name == "SUBMAP").Children;

            List<dynamic> XA = root.Find(dir => dir.Name == "XA").Children;
            LODXA00 = XA.Find(file => file.Name == "LODXA00.XA;1");
            LODXA01 = XA.Find(file => file.Name == "LODXA01.XA;1");
            LODXA02 = XA.Find(file => file.Name == "LODXA02.XA;1");
            try {
                LODXA03 = XA.Find(file => file.Name == "LODXA03.XA;1");
            } catch (RuntimeBinderException) {
                Console.WriteLine("JP version");
            }
        }
    }
}
