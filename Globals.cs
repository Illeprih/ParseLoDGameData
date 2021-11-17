using System.Collections.Generic;

namespace LodmodsDM
{
    public static class Globals
    {
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
    }
}
