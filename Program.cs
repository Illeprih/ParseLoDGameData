using System;

namespace ParseLoDGameData {
    class Program {
        static void Main(string[] args) {
            byte[] S_ITEM = BPE.Decompress("G:/Projekty/Disc 1 Extracted/OVL/S_ITEM.OV_");
            dynamic[] item_list = GameData.RipItems(S_ITEM);
            for (int i = 0; i < 192; i++) {
                Console.WriteLine($"Item: {item_list[i].name} \nDescription: {item_list[i].description}\nType:{item_list[i].type}\n");
            }
        }
    }
}
