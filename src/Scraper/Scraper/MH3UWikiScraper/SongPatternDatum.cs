using System.Collections.Generic;

namespace MH3UWikiScraper
{
    public class SongPatternDatum
    {
        public string Group { get; set; }
        public List<NoteButtonMapping> ButtonMappings { get; set; }
        public List<Song> Songs { get; set; }
        public List<HuntingHorn> Horns { get; set; } 
    }
}