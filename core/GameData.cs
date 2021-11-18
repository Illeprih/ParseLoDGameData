using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace LodmodsDM {
    class GameData {
        static int item_desc_ptr_addr = 0x1C298;
        static int item_desc_addr = 0x18DFC;
        static int item_name_ptr_addr = 0x1DFB4;
        static int item_name_addr = 0x1C698;
        static int equip_table_addr = 0x16879;
        static int map_encounters_addr = 0x2FE3C;
        static int map_encounter_slots_addr = 0x30E3C;
        static int monster_names_ptr_addr = 0x168F0;
        static int monster_names_addr = 0x14460;
        static int monster_stats_addr = 0x10320;

        public static dynamic[] RipItems(byte[] S_ITEM) {
            ulong[] item_desc_pointers = RipNameDescPTR(S_ITEM, item_desc_ptr_addr, 256);
            string[] item_desc = RipNameDesc(S_ITEM, item_desc_pointers, item_desc_addr, item_desc_ptr_addr, 256);
            ulong[] item_name_pointers = RipNameDescPTR(S_ITEM, item_name_ptr_addr, 256);
            string[] item_name = RipNameDesc(S_ITEM, item_name_pointers, item_name_addr, item_name_ptr_addr, 256);
            byte[][] equip_stats = RipEquipTable(S_ITEM, equip_table_addr);
            dynamic[] item_list = new dynamic[256];
            for (int i = 0; i < 192; i++) {
                item_list[i] = new Equipment(item_name[i], item_desc[i], equip_stats[i]);
            }
            return item_list;
        }

        static ulong[] RipNameDescPTR(byte[] file, int start, int amount) {
            byte[] data = file.Skip(start).Take(amount * 4).ToArray();
            ulong[] pointers = new ulong[amount];
            for (int i = 0; i < amount; i++) {
                pointers[i] = BitConverter.ToUInt32(data, i * 4) - 0x80000000;
            }
            return pointers;
        }

        static string[] RipNameDesc(byte[] file, ulong[] pointers, int start, int ptr_start, int amount) {
            int len = ptr_start - start;
            byte[] data = file.Skip(start).Take(len).ToArray();
            ushort[] pairs = new ushort[len / 2];
            for (int i = 0; i < len / 2; i++) {
                pairs[i] = BitConverter.ToUInt16(data, i * 2);
            }
            string[] names = new string[amount];
            int start_pos = (int)pointers[0];
            for (int i = 0; i < amount; i++) {
                string temp = "";
                foreach (ushort letter in pairs.Skip(((int)pointers[i] - start_pos) / 2).ToArray()) {
                    if (letter == 41215) {
                        names[i] = temp;
                        break;
                    } else {
                        temp += DecodeText(letter);
                    }
                }
            }
            return names;
        }

        static byte[][] RipEquipTable(byte[] S_ITEM, int addr) {
            byte[] data = S_ITEM.Skip(addr).Take(192 * 0x1C).ToArray();
            byte[][] split = new byte[192][];
            for (int i = 0; i < 192; i++) {
                split[i] = data.Skip(i * 0x1C).Take(0x1C).ToArray();
            }
            return split;
        }
        
        static short[][] RipMapEncounters(byte[] SMAP, int addr) {
            byte[] data = SMAP.Skip(addr).Take(4 * 900).ToArray();
            short[][] split = new short[900][];
            for (int i = 0; i < 900; i++) {
                short[] temp = new short[3];
                temp[0] = BitConverter.ToInt16(data, i * 4);
                temp[1] = data[i * 4 + 1];
                temp[2] = data[i * 4 + 2];
                split[i] = temp;
            }
            return split;
        }
        
        static short[][] RipMapEncounterSlots(byte[] SMAP, int addr) {
            byte[] data = SMAP.Skip(addr).Take(8 * 300).ToArray();
            short[][] split = new short[300][];
            for (int i = 0; i <300; i++) {
                short[] temp = new short[4];
                temp[0] = BitConverter.ToInt16(data, i * 8);
                temp[1] = BitConverter.ToInt16(data, i * 8 + 2);
                temp[2] = BitConverter.ToInt16(data, i * 8 + 4);
                temp[3] = BitConverter.ToInt16(data, i * 8 + 6);
                split[i] = temp;
            }
            return split;
        }
        
        public static dynamic RipMonsters(byte[] S_BTLD) {
            dynamic[] monsters = new dynamic[0x200];
            ulong[] monster_name_pointers = RipNameDescPTR(S_BTLD, monster_names_ptr_addr, 0x200);
            string[] monster_names = RipNameDesc(S_BTLD, monster_name_pointers, monster_names_addr, monster_names_ptr_addr, 0x200);
            ushort[][] monster_stats = RipMonsterStats(S_BTLD, monster_stats_addr);
            for (int i = 0; i < 0x200; i++) {
                monsters[i] = new Monster(monster_names[i], monster_stats[i]);
            }
            
            return monsters;
        }

        static ushort[][] RipMonsterStats(byte[] S_BTLD, int addr) {
            byte[] data = S_BTLD.Skip(addr).Take(0x200 * 28).ToArray();
            ushort[][] split = new ushort[0x200][];
            for (int i = 0; i < 0x200; i++) {
                ushort[] temp = new ushort[23];
                temp[0] = BitConverter.ToUInt16(data, i * 28);
                temp[1] = (ushort)BitConverter.ToInt16(data, i * 28 + 4);
                temp[2] = (ushort)BitConverter.ToInt16(data, i * 28 + 6);
                temp[3] = data[i * 28 + 8];
                temp[4] = data[i * 28 + 9];
                temp[5] = data[i * 28 + 10];
                temp[6] = data[i * 28 + 11];
                temp[7] = data[i * 28 + 12];
                temp[8] = data[i * 28 + 13];
                temp[9] = data[i * 28 + 14];
                temp[10] = data[i * 28 + 15];
                temp[11] = data[i * 28 + 16];
                temp[12] = data[i * 28 + 17];
                temp[13] = data[i * 28 + 18];
                temp[14] = data[i * 28 + 19];
                temp[15] = data[i * 28 + 20];
                temp[16] = data[i * 28 + 21];
                temp[17] = data[i * 28 + 22];
                temp[18] = data[i * 28 + 23];
                temp[19] = data[i * 28 + 24];
                temp[20] = data[i * 28 + 25];
                temp[21] = data[i * 28 + 26];
                temp[22] = data[i * 28 + 27];
                split[i] = temp;
            }
            return split;
        }

        public static string DecodeText(ushort letter) {
            IDictionary<ushort, string> textDict = new Dictionary<ushort, string>() {
                { 0x0000, " " },
                { 0x0001, "," },
                { 0x0002, "." },
                { 0x0003, "·" },
                { 0x0004, ":" },
                { 0x0005, "?" },
                { 0x0006, "!" },
                { 0x0007, "_" },
                { 0x0008, "/" },
                { 0x0009, "\'" },
                { 0x000A, "\"" },
                { 0x000B, "(" },
                { 0x000C, ")" },
                { 0x000D, "-" },
                { 0x000E, "`" },
                { 0x000F, "%" },
                { 0x0010, "%" },
                { 0x0011, "*" },
                { 0x0012, "@" },
                { 0x0013, "+" },
                { 0x0014, "~" },
                { 0x0015, "0" },
                { 0x0016, "1" },
                { 0x0017, "2" },
                { 0x0018, "3" },
                { 0x0019, "4" },
                { 0x001A, "5" },
                { 0x001B, "9" },
                { 0x001C, "7" },
                { 0x001D, "8" },
                { 0x001E, "9" },
                { 0x001F, "A" },
                { 0x0020, "B" },
                { 0x0021, "C" },
                { 0x0022, "D" },
                { 0x0023, "E" },
                { 0x0024, "F" },
                { 0x0025, "G" },
                { 0x0026, "H" },
                { 0x0027, "I" },
                { 0x0028, "J" },
                { 0x0029, "K" },
                { 0x002A, "L" },
                { 0x002B, "M" },
                { 0x002C, "N" },
                { 0x002D, "O" },
                { 0x002E, "P" },
                { 0x002F, "Q" },
                { 0x0030, "R" },
                { 0x0031, "S" },
                { 0x0032, "T" },
                { 0x0033, "U" },
                { 0x0034, "V" },
                { 0x0035, "W" },
                { 0x0036, "X" },
                { 0x0037, "Y" },
                { 0x0038, "Z" },
                { 0x0039, "a" },
                { 0x003A, "b" },
                { 0x003B, "c" },
                { 0x003C, "d" },
                { 0x003D, "e" },
                { 0x003E, "f" },
                { 0x003F, "g" },
                { 0x0040, "h" },
                { 0x0041, "i" },
                { 0x0042, "j" },
                { 0x0043, "k" },
                { 0x0044, "l" },
                { 0x0045, "m" },
                { 0x0046, "n" },
                { 0x0047, "o" },
                { 0x0048, "p" },
                { 0x0049, "q" },
                { 0x004A, "r" },
                { 0x004B, "s" },
                { 0x004C, "t" },
                { 0x004D, "u" },
                { 0x004E, "v" },
                { 0x004F, "w" },
                { 0x0050, "x" },
                { 0x0051, "y" },
                { 0x0052, "z" },
                { 0x0053, "[" },
                { 0x0054, "]" },
                { 0xA1FF, "<LINE>" },
            };
            string output = "";
            if (textDict.TryGetValue(letter, out string value)) {
                output = value;
            }
            return output;
        }


    }

    public class Equipment {
        string _name = " ";
        string _description = " ";
        string _encodedName = "00 00 FF A0";
        string _encodedDescription = "00 00 FF A0";
        long _descriptionPointer = 0;
        long _namePointer = 0;
        byte _icon = 0;
        byte _who_equips = 0;
        byte _type = 0;
        byte _attack_element = 0;
        byte _on_hit_status = 0;
        byte _status_chance = 0;
        byte _at = 0;
        byte _at2 = 0;
        byte _mat = 0;
        byte _df = 0;
        byte _mdf = 0;
        byte _spd = 0;
        byte _a_hit = 0;
        byte _m_hit = 0;
        byte _a_av = 0;
        byte _m_av = 0;
        byte _e_half = 0;
        byte _e_immune = 0;
        byte _stat_res = 0;
        byte _special1 = 0;
        byte _special2 = 0;
        short _special_amount = 0;
        byte _death_resist = 0;
        short _sell_price = 0;

        public string Name { get { return _name; } set { _name = value; } }
        public string Description { get { return _description; } set { _description = value; } }
        public string EncodedName { get { return _encodedName; } set { _encodedName = value; } }
        public string EncodedDescription { get { return _encodedDescription; } set { _encodedDescription = value; } }

        public Equipment(string name, string description, byte[] equip_stats) {
            _name = name;
            _description = description;
            _type = equip_stats[0x0];
            _who_equips = equip_stats[0x2];
            _attack_element = equip_stats[0x3];
            _e_half = equip_stats[0x5];
            _e_immune = equip_stats[0x6];
            _stat_res = equip_stats[0x7];
            _at = equip_stats[0x9];
            _special1 = equip_stats[0xA];
            _special2 = equip_stats[0xB];
            _special_amount = equip_stats[0xC];
            _icon = equip_stats[0xD];
            _spd = equip_stats[0xE];
            _at2 = equip_stats[0xF];
            _mat = equip_stats[0x10];
            _df = equip_stats[0x11];
            _mdf = equip_stats[0x12];
            _a_hit = equip_stats[0x13];
            _m_hit = equip_stats[0x14];
            _a_av = equip_stats[0x15];
            _m_av = equip_stats[0x16];
            _status_chance = equip_stats[0x17];
            _on_hit_status = equip_stats[0x1A];
            _death_resist = equip_stats[0x1B];
        }
    }

    public class Monster {
        string _name = " ";
        ushort _hp = 1;
        ushort _at = 1;
        ushort _mat = 1;
        byte _spd = 1;
        byte _df = 1;
        byte _mdf = 1;
        byte _a_av = 0;
        byte _m_av = 0;
        byte _death_resist = 0;
        byte _uu2 = 0;
        byte _element = 0;
        byte _null_element = 0;
        byte _status_resist = 0;
        byte _uu3 = 0;
        byte _uu4 = 0;
        byte _uu5 = 0;
        byte _counter = 0;
        byte _uu7 = 0;
        byte _uu8 = 0;
        byte _uu9 = 0;
        byte _uu10 = 0;
        byte _uu11 = 0;
        byte _uu12 = 0;


        public string Name { get { return _name; } set { _name = value; } }
        public ushort HP { get { return _hp; } set { _hp = value; } }
        public ushort AT { get { return _at; } set { _at = value; } }
        public ushort MAT { get { return _mat; } set { _mat = value; } }
        public byte SPD { get { return _spd; } set { _spd = value; } }
        public byte DF { get { return _df; } set { _df = value; } }
        public byte MDF { get { return _mdf; } set { _mdf = value; } }
        public byte A_AV { get { return _a_av; } set { _a_av = value; } }
        public byte M_AV { get { return _m_av; } set { _m_av = value; } }
        public byte Death_Resist { get { return _death_resist; } set { _death_resist = value; } }
        public byte UU2 { get { return _uu2; } set { _uu2 = value; } }
        public byte Element { get { return _element; } set { _element = value; } }
        public byte Null_Element { get { return _null_element; } set { _null_element = value; } }
        public byte Status_Resist { get { return _status_resist; } set { _status_resist = value; } }
        public byte UU3 { get { return _uu3; } set { _uu3 = value; } }
        public byte UU4 { get { return _uu4; } set { _uu4 = value; } }
        public byte UU5 { get { return _uu5; } set { _uu5 = value; } }
        public byte Counter { get { return _counter; } set { _counter = value; } }
        public byte UU7 { get { return _uu7; } set { _uu7 = value; } }
        public byte UU8 { get { return _uu8; } set { _uu8 = value; } }
        public byte UU9 { get { return _uu9; } set { _uu9 = value; } }
        public byte UU10 { get { return _uu10; } set { _uu10 = value; } }
        public byte UU11 { get { return _uu11; } set { _uu11 = value; } }
        public byte UU12 { get { return _uu12; } set { _uu12 = value; } }


        public Monster(string name, ushort[] monsterStats) {
            _name = name;
            _hp = monsterStats[0];
            _at = monsterStats[1];
            _mat = monsterStats[2];
            _spd = (byte)monsterStats[3];
            _df = (byte)monsterStats[4];
            _mdf = (byte)monsterStats[5];
            _a_av = (byte)monsterStats[6];
            _m_av = (byte)monsterStats[7];
            _death_resist = (byte)monsterStats[8];
            _uu2 = (byte)monsterStats[9];
            _element = (byte)monsterStats[10];
            _null_element = (byte)monsterStats[11];
            _status_resist = (byte)monsterStats[12];
            _uu3 = (byte)monsterStats[13];
            _uu4 = (byte)monsterStats[14];
            _uu5 = (byte)monsterStats[15];
            _counter = (byte)monsterStats[16];
            _uu7 = (byte)monsterStats[17];
            _uu8 = (byte)monsterStats[18];
            _uu9 = (byte)monsterStats[19];
            _uu10 = (byte)monsterStats[20];
            _uu11 = (byte)monsterStats[21];
            _uu12 = (byte)monsterStats[22];

        }

    }

}
