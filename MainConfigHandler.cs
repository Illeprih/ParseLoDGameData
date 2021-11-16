using System;
using System.Collections.Generic;

namespace LodmodsDM
{
    public class ModdingDirConfig
    {
        private string _extractedFilesDir;
        public string ExtractedFilesDir
        {
            get { return _extractedFilesDir; }
            set { _extractedFilesDir = value is null ? "game_files" : value; }
        }
        public string GameTextDir { get; set; }
        public string PatchesDir { get; set; }
        public string ModsDir { get; set; }

        // Form should accept empty values in fields, since, for instance,
        // players won't need a patches directory

        // For modders
        public ModdingDirConfig(string extractedFilesDir, string gameTextDir,
            string patchesDir)
        {
            // This folder will always be necessary.
            ExtractedFilesDir = extractedFilesDir;
            GameTextDir = gameTextDir;
            PatchesDir = patchesDir;
        }

        // For players (unsure whether gameTextDir will be needed; probably not, should be patched)
        public ModdingDirConfig(string extractedFilesDir, /*string gameTextDir,*/ string modsDir)
        {
            // This folder will always be necessary.
            ExtractedFilesDir = extractedFilesDir;
            //GameTextDir = gameTextDir;
            ModsDir = modsDir;
        }
    }

    public class DiscFileConfig
    {
        public string DiscFileName { get; set; }

        public Dictionary<string, bool> DiscFileDict { get; private set; } = new Dictionary<string, bool>();

        public DiscFileConfig(string region, string discName, Dictionary<string, string[]> regionCodes)
        {
            switch (discName)
            {
                case "Disc 1":
                    DiscFileDict = new Dictionary<string, bool>()
                    {
                        { "SECT/DRGN21.BIN", false },
                        { "STR/DEMO2.IKI", false },
                        { "STR/DEMOH.IKI", false },
                        { "STR/OPENH.IKI", false },
                        { "STR/WAR1H.IKI", false },
                        { regionCodes[region][0], false },
                        { "SYSTEM.CNF", false }
                    };
                    this.AddLODXA03(region, discName);
                    this.AddMixDA(region, discName);
                    break;
                case "Disc 2":
                    DiscFileDict = new Dictionary<string, bool>()
                    {
                        { "SECT/DRGN22.BIN", false },
                        { "STR/GOAST.IKI", false },
                        { "STR/ROZEH.IKI", false },
                        { "STR/TVRH.IKI", false },
                        { regionCodes[region][1] , false },
                        { "SYSTEM.CNF", false },
                    };
                    this.AddLODXA03(region, discName);
                    this.AddMixDA(region, discName);
                    break;
                case "Disc 3":
                    DiscFileDict = new Dictionary<string, bool>()
                    {
                        { "SECT/DRGN23.BIN", false },
                        { "STR/BLACKH.IKI", false },
                        { "STR/DEIASH.IKI", false },
                        { "STR/DENIN.IKI", false },
                        { "STR/DENIN2.IKI", false },
                        { "STR/DRAGON1.IKI", false },
                        { "STR/DRAGON2.IKI", false },
                        { "STR/TREEH.IKI", false },
                        { "STR/WAR2H.IKI", false },
                        { regionCodes[region][2], false },
                        { "SYSTEM.CNF", false },
                    };
                    this.AddLODXA03(region, discName);
                    this.AddMixDA(region, discName);
                    break;
                case "Disc 4":
                    DiscFileDict = new Dictionary<string, bool>()
                    {
                        { "SECT/DRGN24.BIN", false },
                        { "STR/ENDING1H.IKI", false },
                        { "STR/ENDING2H.IKI", false },
                        { "STR/MOONH.IKI", false },
                        { "XA/LODXA03.XA", false },
                        { "SYSTEM.CNF", false }
                    };
                    break;
                default: // All Discs
                    DiscFileDict = new Dictionary<string, bool>()
                    {
                        { "OHTA/MCX/DABAS.BIN", false },
                        { "OVL/BTTL.OV_", false },
                        { "OVL/S_BTLD.OV_", false },
                        { "OVL/S_EFFE.OV_", false },
                        { "OVL/S_INIT.OV_", false },
                        { "OVL/S_ITEM.OV_", false },
                        { "OVL/S_STRM.OV_", false },
                        { "OVL/SMAP.OV_", false },
                        { "OVL/TEMP.OV_", false },
                        { "OVL/TTLE.OV_", false },
                        { "OVL/WMAP.OV_", false },
                        { "SECT/DRGN0.BIN", false },
                        { "SECT/DRGN1.BIN", false },
                        { "SIM/MES.MVB", false },
                        { "SUBMAP/NEWROOT.RDT", false },
                        { "XA/LODXA00.XA", false },
                        { "XA/LODXA01.XA", false },
                        { "XA/LODXA02.XA", false },
                    };
                    break; 
            }

        }

        // I'm not really sure what this particular constructor would ever be used for
        public DiscFileConfig(string discFileName)
        {
            DiscFileName = discFileName;
        }

        public bool GetFileValue(string key)
        {
            bool value;
            DiscFileDict.TryGetValue(key, out value);
            return value;
        }

        public void SetFileValue(string key, bool value)
        {
            DiscFileDict[key] = value;
        }

        private void AddLODXA03(string region, string discName)
        {
            string[] jpCodes = new string[] { "JP1", "JP2" };
            if (!Array.Exists(jpCodes, r => r == region))
            {
                this.SetFileValue("XA/LODXA03.XA", false);
            }
        }
        private void AddMixDA(string region, string discName)
        {
            if (Array.Exists(new string[] { "JP1", "JP2" }, r => r == region) 
                && Array.Exists(new string[] { "Disc 1", "Disc 2", "Disc 3" }, d => d == discName))
            {
                this.SetFileValue("DA/MIX.DA", false);
            }
        }
    }

    public class GameRegionConfig
    {
        private Dictionary<string, string[]> _regionCodes = new Dictionary<string, string[]>
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
        //public Dictionary<string, string[]> RegionCodes => _regionCodes;
        //public string Region { get; private set; }
        public string DiscDir { get; set; }
        public List<string> AssetListFiles { get; }
        public Dictionary<string, DiscFileConfig> GameDiscs { get; private set; }

        public GameRegionConfig(string region)
        {
            //Region = region;
            GameDiscs = new Dictionary<string, DiscFileConfig>();
            string[] discNameList = new string[] { "All Discs", "Disc 1", "Disc 2", "Disc 3", "Disc 4" };

            for (int i = 0; i < 5; i++)
            {
                string discName = discNameList[i];
                GameDiscs.Add(discName, new DiscFileConfig(region, discName, _regionCodes));
            }
        }

        public void AddAssetListFile(string assetListFile) { AssetListFiles.Add(assetListFile); }

        public void RemoveAssetListFile(string assetListFile) { AssetListFiles.Remove(assetListFile);}
    }

    // Config form should have option for either Player or Modder config creation
    // Player config would exclude certain folders, be in main directory
    // Modder config would create a new subfolder with mod title and its own config file
    class MainConfig
    {
        public string ConfigName { get; set; }
        public string ConfigType { get; set; }
        public ModdingDirConfig ModdingDirs { get; private set; }
        public Dictionary<string, GameRegionConfig> Regions { get; }

        public void AddRegion(string region)
        {
            GameRegionConfig regionConfig = new GameRegionConfig(region);
            Regions.Add(region, regionConfig);
        }

        public void RemoveRegion(string region) { Regions.Remove(region); }

        public MainConfig(string configName, string configType, List<string> regions)
        {
            ConfigName = $"{configName}.config";
            ConfigType = configType;
            
            if (configType == "Modder")
            {
                ModdingDirs = new ModdingDirConfig("game_files", "script_dumps", "patches");
            } 
            else if (configType == "Player")
            {
                ModdingDirs = new ModdingDirConfig("game_files", /*"script_dumps",*/ "mods");
            }
            else
            {
                throw new ArgumentException("Config type must be 'Modding' or 'Player'");
            }

            foreach (string region in regions)
            {
                this.AddRegion(region);
            }
        }
    }
}
