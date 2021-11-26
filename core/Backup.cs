using System;
using System.IO;

namespace LodmodsDM
{
    public class Backup
    {
        public static void BackupFile(string inputFile, bool restoreFromBackup=false, bool hideOutput=false)
        {
            string inputBackup = string.Join(".", new string[] { inputFile, "orig" });

            if (!File.Exists(inputBackup))
            {
                if (!hideOutput) Console.WriteLine($"Backing up {inputFile}.");
                File.Copy(inputFile, inputBackup);
            } else if (restoreFromBackup)
            {
                if (!hideOutput) Console.WriteLine($"Restoring {inputFile} from backup}.");
                File.Copy(inputBackup, inputFile, true);
            }
        }
    }
}
