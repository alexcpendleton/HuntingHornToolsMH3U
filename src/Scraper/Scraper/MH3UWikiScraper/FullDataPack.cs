using System.Collections.Generic;

namespace MH3UWikiScraper
{
    public class FullDataPack
    {
        public List<string> AvailableNoteCombinations { get; set; }
        public Dictionary<string, SongPatternDatum> SongPatterns { get; set; }
        public List<string> Colors { get; set; }
    }
}