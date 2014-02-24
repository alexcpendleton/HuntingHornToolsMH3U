using System.Collections.Generic;
using System.Diagnostics;

namespace MH3UWikiScraper
{
    [DebuggerDisplay("{Horn.Name}, {Horn.NoteKey} (x{Songs.Count})")]
    public class HornSongPair
    {
        public HuntingHorn Horn { get; set; }
        public List<Song> Songs { get; set; } 
    }
}