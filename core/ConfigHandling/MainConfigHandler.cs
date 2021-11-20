using System;
using System.Collections.Generic;
using System.IO;
using static LodmodsDM.Globals;
using YamlDotNet.Serialization;

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

        // Form should accept empty values in fields, since for instance,
        // players won't need a patches directory

        public ModdingDirConfig() { }

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
        [YamlIgnore]
        public string Region { get; set; }
        [YamlIgnore]
        public string DiscName { get; set; }
        public string DiscFileName { get; set; }

        public Dictionary<string, bool> DiscFileDict { get; private set; } = new Dictionary<string, bool>();

        public DiscFileConfig() { }

        public DiscFileConfig(string region, string discName)
        {
            Region = region;
            DiscName = discName;

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
                        { REGION_CODES[region][0], false },
                        { "SYSTEM.CNF", false }
                    };
                    this.SetFileValue("XA/LODXA03.XA", false);
                    this.SetFileValue("DA/MIX.DA", false);
                    break;
                case "Disc 2":
                    DiscFileDict = new Dictionary<string, bool>()
                    {
                        { "SECT/DRGN22.BIN", false },
                        { "STR/GOAST.IKI", false },
                        { "STR/ROZEH.IKI", false },
                        { "STR/TVRH.IKI", false },
                        { REGION_CODES[region][1] , false },
                        { "SYSTEM.CNF", false },
                    };
                    this.SetFileValue("XA/LODXA03.XA", false);
                    this.SetFileValue("DA/MIX.DA", false);
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
                        { REGION_CODES[region][2], false },
                        { "SYSTEM.CNF", false },
                    };
                    this.SetFileValue("XA/LODXA03.XA", false);
                    this.SetFileValue("DA/MIX.DA", false);
                    break;
                case "Disc 4":
                    DiscFileDict = new Dictionary<string, bool>()
                    {
                        { "SECT/DRGN24.BIN", false },
                        { "STR/ENDING1H.IKI", false },
                        { "STR/ENDING2H.IKI", false },
                        { "STR/MOONH.IKI", false },
                        { "XA/LODXA03.XA", false },
                        { REGION_CODES[region][3], false },
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

        // If I need to combine them I guess. Not sure I'll need these, but here they are
        public DiscFileConfig(string region, string discName, string discFileName) : 
            this(region, discName)
        {
            DiscFileName = discFileName;
        }

        public bool GetFileValue(string key)
        {
            DiscFileDict.TryGetValue(key, out bool value);
            return value;
        }

        public void SetFileValue(string key, bool value)
        {
            key = key.ToUpper();
            string[] jpCodes = new string[] { "JP1", "JP2" };

            if (!Array.Exists(COMPLETE_FILE_LIST, k => k == key)) 
            {
                throw new ArgumentException($"{key} is not a valid file name.");
            } else if (key == "XA/LODXA03.XA")
            {
                if (!Array.Exists(jpCodes, r => r == Region)) 
                { 
                    DiscFileDict[key] = value; 
                }
            } else if (key == "DA/MIX.DA")
            {
                if (Array.Exists(new string[] { "JP1", "JP2" }, r => r == Region)
                    && Array.Exists(new string[] { "Disc 1", "Disc 2", "Disc 3" }, d => d == DiscName))
                {
                    DiscFileDict[key] = value;
                }
            } else { DiscFileDict[key] = value; }
        }
    }

    public class GameRegionConfig
    {
        public string DiscDir { get; set; }
        public List<string> AssetListFiles { get; private set; }
        public Dictionary<string, DiscFileConfig> GameDiscs { get; private set; }

        public GameRegionConfig() { }

        public GameRegionConfig(string region)
        {
            GameDiscs = new Dictionary<string, DiscFileConfig>();
            string[] discNameList = new string[] { "All Discs", "Disc 1", "Disc 2", "Disc 3", "Disc 4" };

            for (int i = 0; i < 5; i++)
            {
                string discName = discNameList[i];
                GameDiscs.Add(discName, new DiscFileConfig(region, discName));
            }
        }

        public void AddAssetListFile(string assetListFile) { AssetListFiles.Add(assetListFile); }

        public void RemoveAssetListFile(string assetListFile) { AssetListFiles.Remove(assetListFile);}
    }

    // Config form should have option for either Player or Modder config creation
    // Player config would exclude certain folders, be in main directory
    // Modder config would create a new subfolder with mod title and its own config file
    public class MainConfig
    {
        public string ConfigName { get; set; }
        public string ConfigType { get; set; }
        public ModdingDirConfig ModdingDirs { get; private set; }
        public Dictionary<string, GameRegionConfig> Regions { get; private set; }

        public MainConfig() { }

        public MainConfig(string configName, string configType, List<string> regions)
        {
            ConfigName = $"{configName}.yaml";
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

            Regions = new Dictionary<string, GameRegionConfig>();
            foreach (string region in regions)
            {
                this.AddRegion(region);
            }
        }

        public void AddRegion(string region)
        {
            GameRegionConfig regionConfig = new GameRegionConfig(region);
            Regions.Add(region, regionConfig);
        }

        public void RemoveRegion(string region) { Regions.Remove(region); }

        public static MainConfig ReadMainConfig(string configFileName)
        {
            var deserializer = new Deserializer();
            using StreamReader reader = new StreamReader(configFileName);
            string fileContent = reader.ReadToEnd();
            MainConfig mainConfig = deserializer.Deserialize<MainConfig>(fileContent);

            foreach (KeyValuePair<string, GameRegionConfig> region in mainConfig.Regions)
            {
                foreach (KeyValuePair<string, DiscFileConfig> disc in region.Value.GameDiscs)
                {
                    disc.Value.Region = region.Key;
                    disc.Value.DiscName = disc.Key;
                }
            }

            return mainConfig;
        }

        public void WriteMainConfig()
        {
            var serializer = new Serializer();
            string fileContent = serializer.Serialize(this);
            using StreamWriter writer = new StreamWriter(ConfigName);
            writer.Write(fileContent);
        }
    }
}
