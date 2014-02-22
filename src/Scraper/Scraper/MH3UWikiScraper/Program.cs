using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Web;
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

#if false
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

            var songs = scraper.Scrape();

            var weaponDocument = new HtmlDocument();
            weaponDocument.Load("WeaponTreeTable.html");

            var weaponScraper = new WeaponTreeScraper(weaponDocument);
            var mapping = weaponScraper.GetHornMapping();

            var dataPack = new FullDataPack();
            dataPack.Songs = songs.Songs;
            dataPack.Horns = mapping;
            dataPack.Colors = new List<string>();

            var colors = new HashSet<char>();
            foreach (var item in dataPack.Songs)
            {
                foreach (var c in item.Key.ToCharArray())
                {
                    colors.Add(c);
                }
            }
            dataPack.Colors = colors.Select(i => i.ToString()).ToList();
            dataPack.Colors.Sort();

            dataPack.AvailableNoteCombinations = dataPack.Songs.Keys.ToList();

            string outputPath = "Scraped.json";
            File.WriteAllText(outputPath, JsonConvert.SerializeObject(dataPack, Formatting.Indented));
            
        }
    }

    public class LinkFiller
    {
        public LinkFiller()
        {
            Derivers = new List<IHuntingHornLinkDeriver>
            {
                new KiranicoHornLinkDeriver(),
                new MonsterHunterWikiLinkDeriver(),
            };
        }
        public bool ShouldOverwriteExisting { get; set; }
        public List<IHuntingHornLinkDeriver> Derivers { get; set; }

        public void Fill(IEnumerable<HuntingHorn> horns)
        {
            foreach (var huntingHorn in horns)
            {
                if (huntingHorn.Links == null)
                {
                    huntingHorn.Links = new Dictionary<string, string>();
                }
                
                foreach (var linkDeriver in Derivers)
                {
                    if (ShouldOverwriteExisting || !huntingHorn.Links.ContainsKey(linkDeriver.Key))
                    {
                        huntingHorn.Links[linkDeriver.Key] = linkDeriver.DeriveLink(huntingHorn);
                    }
                }
            }
        }
    }


    public interface IHuntingHornLinkDeriver
    {
        string Key { get; }
        string DeriveLink(HuntingHorn horn);
    }

    public class MonsterHunterWikiLinkDeriver : IHuntingHornLinkDeriver
    {
        public MonsterHunterWikiLinkDeriver()
        {
            Domain = "http://monsterhunter.wikia.com";
            PageFormatString = "/wiki/{0}";
        }
        public string Domain { get; set; }
        public string PageFormatString { get; set; }

        public string FullyQualifyPage(string page)
        {
            var builder = new UriBuilder(Domain);
            builder.Path = page;
            builder.Port = -1;
            return builder.ToString();
        }

        public string DeriveLink(HuntingHorn horn)
        {
            string massagedName = horn.Name.Replace(" ", "_");
            string page = String.Format(PageFormatString, massagedName);
            return FullyQualifyPage(page);
        }

        public string Key
        {
            get { return Constants.KiranicoLinkKey; }
        }
    }

    public class KiranicoHornLinkDeriver : IHuntingHornLinkDeriver
    {
        public string BaseUriFormatString = "http://kiranico.com/weapon/huntinghorn/{0}";

        public string DeriveLink(HuntingHorn horn)
        {
            string massagedName = horn.Name.ToLower().Replace(" ", "-");
            return String.Format(BaseUriFormatString, massagedName);
        }

        public string Key
        {
            get { return Constants.KiranicoLinkKey; }
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
        public List<string> AvailableNoteCombinations { get; set; }
        public Dictionary<string, List<Song>> Songs { get; set; }
        public Dictionary<string, List<HuntingHorn>> Horns { get; set; }
        public List<string> Colors { get; set; } 

    }
}
