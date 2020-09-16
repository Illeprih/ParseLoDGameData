using System;
using System.Collections.Generic;
using System.Linq;

namespace ParseLoDGameData {
    class GameData {
        static int item_desc_ptr_addr = 0x1C298;
        static int item_desc_addr = 0x18DFC;
        static int item_name_ptr_addr = 0x1DFB4;
        static int item_name_addr = 0x1C698;
        static int equip_table_addr = 0x16879;

        public static dynamic[] RipItems(byte[] S_ITEM) {
            ulong[] item_desc_pointers = RipItemNameDescPTR(S_ITEM, item_desc_ptr_addr);
            string[] item_desc = RipItemNameDesc(S_ITEM, item_desc_pointers, item_desc_addr, item_desc_ptr_addr);
            ulong[] item_name_pointers = RipItemNameDescPTR(S_ITEM, item_name_ptr_addr);
            string[] item_name = RipItemNameDesc(S_ITEM, item_name_pointers, item_name_addr, item_name_ptr_addr);
            byte[][] equip_stats = RipEquipTable(S_ITEM, equip_table_addr);
            dynamic[] item_list = new dynamic[256];
            for (int i = 0; i < 192; i++) {
                item_list[i] = new Equipment(item_name[i], item_desc[i], equip_stats[i]);
            }
            return item_list;
        }

        static ulong[] RipItemNameDescPTR(byte[] S_ITEM, int start) {
            byte[] data = S_ITEM.Skip(start).Take(0x400).ToArray();
            ulong[] pointers = new ulong[256];
            for (int i = 0; i < 256; i++) {
                pointers[i] = BitConverter.ToUInt32(data, i * 4) - 0x80000000;
            }
            return pointers;
        }

        static string[] RipItemNameDesc(byte[] S_ITEM, ulong[] pointers, int start, int ptr_start) {
            int len = ptr_start - start;
            byte[] data = S_ITEM.Skip(start).Take(len).ToArray();
            ushort[] pairs = new ushort[len / 2];
            for (int i = 0; i < len / 2; i++) {
                pairs[i] = BitConverter.ToUInt16(data, i * 2);
            }
            string[] names = new string[256];
            int start_pos = (int)pointers[0];
            for (int i = 0; i < 256; i++) {
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
        public string name = " ";
        public string description = " ";
        public string encodedName = "00 00 FF A0";
        public string encodedDescription = "00 00 FF A0";
        public long descriptionPointer = 0;
        public long namePointer = 0;
        public byte icon = 0;
        public byte who_equips = 0;
        public byte type = 0;
        public byte attack_element = 0;
        public byte on_hit_status = 0;
        public byte status_chance = 0;
        public byte at = 0;
        public byte at2 = 0;
        public byte mat = 0;
        public byte df = 0;
        public byte mdf = 0;
        public byte spd = 0;
        public byte a_hit = 0;
        public byte m_hit = 0;
        public byte a_av = 0;
        public byte m_av = 0;
        public byte e_half = 0;
        public byte e_immune = 0;
        public byte stat_res = 0;
        public byte special1 = 0;
        public byte special2 = 0;
        public short special_amount = 0;
        public byte death_resist = 0;
        public short sell_price = 0;

        public Equipment(string set_name, string set_description, byte[] equip_stats) {
            name = set_name;
            description = set_description;
            type = equip_stats[0x0];
            who_equips = equip_stats[0x2];
            attack_element = equip_stats[0x3];
            e_half = equip_stats[0x5];
            e_immune = equip_stats[0x6];
            stat_res = equip_stats[0x7];
            at = equip_stats[0x9];
            special1 = equip_stats[0xA];
            special2 = equip_stats[0xB];
            special_amount = equip_stats[0xC];
            icon = equip_stats[0xD];
            spd = equip_stats[0xE];
            at2 = equip_stats[0xF];
            mat = equip_stats[0x10];
            df = equip_stats[0x11];
            mdf = equip_stats[0x12];
            a_hit = equip_stats[0x13];
            m_hit = equip_stats[0x14];
            a_av = equip_stats[0x15];
            m_av = equip_stats[0x16];
            status_chance = equip_stats[0x17];
            on_hit_status = equip_stats[0x1A];
            death_resist = equip_stats[0x1B];
        }
    }

}
