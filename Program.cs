using System;

namespace ParseLoDGameData {
    class Program {
        static void Main(string[] args) {
            byte[] S_ITEM = BPE.Decompress("G:/Projekty/Disc 1 Extracted/OVL/S_ITEM.OV_");
            byte[] S_BTLD = BPE.Decompress("G:/Projekty/Disc 1 Extracted/OVL/S_BTLD.OV_");
            dynamic[] itemList = GameData.RipItems(S_ITEM);
            dynamic[] monsterList = GameData.RipMonsters(S_BTLD);
            foreach (dynamic monster in monsterList) {
                Console.WriteLine($"{monster.Name} \t\t {monster.UU2} {monster.UU3} {monster.UU4} {monster.UU5} {monster.UU7} {monster.UU8} {monster.UU9} {monster.UU10} {monster.UU11} {monster.UU12}");
            }
        }
    }
}
