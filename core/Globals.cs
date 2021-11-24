using System.Collections.Generic;

namespace LodmodsDM
{
    public static class Globals
    {
        public static readonly byte[] SYNC_PATTERN = new byte[]
            { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 };

        public static readonly Dictionary<string, string[]> REGION_CODES = new Dictionary<string, string[]>
        {
            { "USA", new string[] { "SCUS_944.91", "SCUS_945.84", "SCUS_945.85", "SCUS_945.86" } },
            { "JP1", new string[] { "SCPS_101.19", "SCPS_101.20", "SCPS_101.21", "SCPS_101.22" } },
            { "JP2", new string[] { "SCPS_454.61", "SCPS_454.62", "SCPS_454.63", "SCPS_454.64" } },
            { "UK", new string[] { "SCES_030.43", "SCES_130.43", "SCES_230.43", "SCES_330.43" } },
            { "FRA", new string[] { "SCES_030.44", "SCES_130.44", "SCES_230.44", "SCES_330.44" } },
            { "GER", new string[] { "SCES_030.45", "SCES_130.45", "SCES_230.45", "SCES_330.45" } },
            { "ITA", new string[] { "SCES_030.46", "SCES_130.46", "SCES_230.46", "SCES_330.46" } },
            { "SPA", new string[] { "SCES_030.47", "SCES_130.47", "SCES_230.47", "SCES_330.47" } },
        };

        public static readonly string[] UNIVERSAL_FILES = new string[]
        {
            "OHTA/MCX/DABAS.BIN", "OVL/BTTL.OV_", "OVL/S_BTLD.OV_", "OVL/S_EFFE.OV_",
            "OVL/S_INIT.OV_", "OVL/S_ITEM.OV_", "OVL/S_STRM.OV_", "OVL/SMAP.OV_", "OVL/TEMP.OV_", 
            "OVL/TTLE.OV_", "OVL/WMAP.OV_", "SECT/DRGN0.BIN", "SECT/DRGN1.BIN", "SIM/MES.MVB", 
            "SUBMAP/NEWROOT.RDT", "XA/LODXA00.XA", "XA/LODXA01.XA", "XA/LODXA02.XA"
        };

        public static readonly string[] COMPLETE_FILE_LIST = new string[]
        {
            "OHTA/MCX/DABAS.BIN", "OVL/BTTL.OV_", "OVL/S_BTLD.OV_", "OVL/S_EFFE.OV_",
            "OVL/S_INIT.OV_", "OVL/S_ITEM.OV_", "OVL/S_STRM.OV_", "OVL/SMAP.OV_", "OVL/TEMP.OV_",
            "OVL/TTLE.OV_", "OVL/WMAP.OV_", "SECT/DRGN0.BIN", "SECT/DRGN1.BIN", "SIM/MES.MVB",
            "SUBMAP/NEWROOT.RDT", "XA/LODXA00.XA", "XA/LODXA01.XA", "XA/LODXA02.XA",
            "SCUS_944.91", "SCUS_945.84", "SCUS_945.85", "SCUS_945.86",
            "SCPS_101.19", "SCPS_101.20", "SCPS_101.21", "SCPS_101.22",
            "SCPS_454.61", "SCPS_454.62", "SCPS_454.63", "SCPS_454.64",
            "SCES_030.43", "SCES_130.43", "SCES_230.43", "SCES_330.43",
            "SCES_030.44", "SCES_130.44", "SCES_230.44", "SCES_330.44",
            "SCES_030.45", "SCES_130.45", "SCES_230.45", "SCES_330.45",
            "SCES_030.46", "SCES_130.46", "SCES_230.46", "SCES_330.46",
            "SCES_030.47", "SCES_130.47", "SCES_230.47", "SCES_330.47",
            "SECT/DRGN21.BIN", "STR/DEMO2.IKI", "STR/DEMOH.IKI", "STR/OPENH.IKI", 
            "STR/WAR1H.IKI", "SECT/DRGN22.BIN", "STR/GOAST.IKI", "STR/ROZEH.IKI", 
            "STR/TVRH.IKI", "SECT/DRGN23.BIN", "STR/BLACKH.IKI", "STR/DEIASH.IKI",
            "STR/DENIN.IKI", "STR/DENIN2.IKI", "STR/DRAGON1.IKI", "STR/DRAGON2.IKI",
            "STR/TREEH.IKI", "STR/WAR2H.IKI", "SECT/DRGN24.BIN", "STR/ENDING1H.IKI",
            "STR/ENDING2H.IKI", "STR/MOONH.IKI", "XA/LODXA03.XA", "SYSTEM.CNF",
            "DA/MIX.DA"
        };
    }
}
