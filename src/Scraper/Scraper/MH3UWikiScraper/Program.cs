using System.CodeDom.Compiler;
using System.Dynamic;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using HtmlAgilityPack;
using Newtonsoft.Json;
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
#if false
            string inputPath = "Output.json";
            var data = JsonConvert.DeserializeObject<FullDataPack>(File.ReadAllText(inputPath));

            var hornSongs = new List<HornSongPair>();
            var allHorns = data.Horns.SelectMany(i => i.Value).ToList();
            var allSongs = data.SongPatterns.Values.ToList();

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
#endif
        }


        public static void ScrapeToOutput()
        {
            var document = new HtmlDocument();
            document.Load("MH3UNoteTables.html");
            var scraper = new SongTableScraper(document);
            scraper.Document = document;

            var scrapedSongs = scraper.Scrape();

            var weaponDocument = new HtmlDocument();
            weaponDocument.Load("WeaponTreeTable.html");

            var weaponScraper = new WeaponTreeScraper(weaponDocument);
            var scrapedHornMapping = weaponScraper.GetHornMapping();

            var dataPack = new FullDataPack();
            
            dataPack.SongPatterns = new Dictionary<string, SongPatternDatum>();
            dataPack.Colors = new List<string>();

            var colors = new HashSet<char>();
            var songGrouper = new SongGrouper();
            var songPatterns = dataPack.SongPatterns;
            foreach (var item in scrapedSongs.Songs)
            {
                string pattern = item.Key;
                if (!songPatterns.ContainsKey(pattern))
                {
                    songPatterns[pattern] = new SongPatternDatum();
                }
                var datum = songPatterns[pattern];

                foreach (var c in pattern.ToCharArray())
                {
                    colors.Add(c);
                }

                datum.Songs = item.Value;
                datum.Group = songGrouper.DetermineGroup(pattern);
                datum.Horns = scrapedHornMapping.ContainsKey(pattern) ? scrapedHornMapping[pattern] : new List<HuntingHorn>();

                // TODO!
                datum.ButtonMappings = new List<NoteButtonMapping>();
            }

            dataPack.Colors = colors.Select(i => i.ToString()).ToList();
            dataPack.Colors.Sort();

            dataPack.AvailableNoteCombinations = dataPack.SongPatterns.Keys.ToList();

            string outputPath = "Scraped.json";
            File.WriteAllText(outputPath, JsonConvert.SerializeObject(dataPack, Formatting.Indented));
            
        }

        public class SongGrouper
        {
            /// <summary>
            /// Determines what group this song is in
            /// </summary>
            /// <param name="songPattern"></param>
            /// <returns></returns>
            public string DetermineGroup(string notes)
            {
                // Orange group can have purple as well, but orange takes priority
                if (notes.Contains(Constants.Group_Orange))
                {
                    return Constants.Group_Orange;
                }
                // If it has purple but not orange, then it goes in purple group
                if (notes.Contains(Constants.Group_Purple))
                {
                    return Constants.Group_Purple;
                }
                // Otherwise it's white
                return Constants.Group_White;
            }
        }
    }
}
