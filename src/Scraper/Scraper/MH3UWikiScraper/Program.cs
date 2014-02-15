using System.Diagnostics;
using System.IO;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MH3UWikiScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            bool scrape = true;
            if (scrape)
            {
                ScrapeToOutput();
            }
            else
            {
                LoadScrapedOutput();
            }
        }

        public static void LoadScrapedOutput()
        {
            string inputPath = "Output.json";
            var data = JsonConvert.DeserializeObject<FullDataPack>(File.ReadAllText(inputPath));

            var hornSongs = new List<HornSongPair>();
            var allHorns = data.Horns.SelectMany(i => i.Value).ToList();
            var allSongs = data.Songs.Values.ToList();

            
            foreach (var huntingHorn in allHorns)
            {
                var pair = new HornSongPair();
                pair.Horn = huntingHorn;
                pair.Songs = new List<Song>();

                foreach (var song in allSongs)
                {
                    // If this song has all of the notes that the horn has
                    var intersection = song.Notes.Intersect(huntingHorn.Notes).ToList();
                    int songNoteCount = song.Notes.Distinct().Count();
                    int hornNoteCount = huntingHorn.Notes.Distinct().Count();

                    if (intersection.Count >= songNoteCount)
                    {
                        pair.Songs.Add(song);
                    }
                }
                hornSongs.Add(pair);
            }
        }


        public static void ScrapeToOutput()
        {
            var document = new HtmlDocument();
            document.Load("MH3UNoteTables.html");
            var scraper = new SongTableScraper(document);
            scraper.Document = document;

            var songs = scraper.Scrape();

            var weaponDocument = new HtmlDocument();
            weaponDocument.Load("WeaponTreeTable.html");

            var weaponScraper = new WeaponTreeScraper(weaponDocument);
            var mapping = weaponScraper.GetHornMapping();

            var dataPack = new FullDataPack();
            dataPack.Songs = songs.Songs;
            dataPack.Horns = mapping;
            dataPack.Colors = new List<string>(songs.Songs.SelectMany(i => i.Value.Notes).Distinct());
            dataPack.Colors.Sort();

            string outputPath = "Output.json";
            File.WriteAllText(outputPath, JsonConvert.SerializeObject(dataPack, Formatting.Indented));
            
        }
    }
    
    [DebuggerDisplay("{Horn.Name}, {Horn.NoteKey} (x{Songs.Count})")]
    public class HornSongPair
    {
        public HuntingHorn Horn { get; set; }
        public List<Song> Songs { get; set; } 
    }

    public class FullDataPack
    {
        public Dictionary<string, Song> Songs { get; set; }
        public Dictionary<string, Song> Songs2 { get; set; }
        public Dictionary<string, List<HuntingHorn>> Horns { get; set; }
        public List<string> Colors { get; set; } 

    }
}
