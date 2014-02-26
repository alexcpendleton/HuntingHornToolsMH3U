using System.Diagnostics;
using System.Web;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScrapySharp.Extensions;
using System.IO;

namespace MH3UWikiScraper
{
    public class SongTableScraper
    {
        public SongTableScraper(HtmlDocument doc)
        {
            Document = doc;
            Utils = new HornScrapingUtilities();
        }
        virtual public HtmlDocument Document { get; set; }
        virtual public HornScrapingUtilities Utils { get; set; }

        virtual public SongTableScrapeResults Scrape()
        {
            var results = new SongTableScrapeResults();

            var tables = Document.DocumentNode.CssSelect("table.wikitable");
            foreach (var table in tables)
            {
                int i = 0;
                var rows = table.CssSelect("tr");
                string currentGroupAlphabetized = "";
                // A few song patterns on the wiki song table page are mislabeled ("wrong" order)
                // So we patch that up with these (Key = song table version, Value = more correct version)
                var specialCases = new Dictionary<string, string>
                {
                    {"ryw", "yrw"},
                    {"byp", "ybp" },
                };
                foreach (var tr in rows)
                {
                    i++;
                    if (i == 1) continue;

                    var cells = tr.CssSelect("td").ToList();
                    var firstCell = cells.FirstOrDefault();
                    if (firstCell != null)
                    {
                        int cellCorrection = 0;
                        bool isRelevantRow = true;
                        var bgColor = firstCell.GetAttributeValue("bgcolor");
                        // Skip header rows, which we identify by them having a background color of 996600
                        if (bgColor != null && bgColor == "#996600")
                        {
                            isRelevantRow = false;
                        }
                        if (firstCell.InnerText.Contains("Any"))
                        {
                            isRelevantRow = false;
                        }
                        if (cells.Count == 5)
                        {
                            cellCorrection = -1;
                        }
                        if (cells.Count < 5)
                        {
                            isRelevantRow = false;
                        }
                        if (isRelevantRow)
                        {
                            if (cells.Count == 6)
                            {
                                // First cell will be the group
                                var colors = Utils.ParseColors(firstCell);
                                string noteKey = Utils.BuildNoteKey(colors, false);
                                if (specialCases.ContainsKey(noteKey))
                                {
                                    noteKey = specialCases[noteKey];
                                }
                                currentGroupAlphabetized = noteKey;
                            }
                            var item = new Song();
                            var songCell = cells[1 + cellCorrection];
                            // Song/colors
                            item.Notes = Utils.ParseColors(songCell);
                            item.NoteKey = Utils.BuildNoteKey(item.Notes);

                            // Effect 1
                            var e1Cell = cells[2 + cellCorrection];
                            item.Effect1 = Utils.ParseText(e1Cell);

                            // Effect 2
                            var e2Cell = cells[3 + cellCorrection];
                            item.Effect2 = Utils.ParseText(e2Cell);

                            // Duration
                            var durationCell = cells[4 + cellCorrection];
                            item.Duration1 = Utils.ParseText(durationCell);

                            if (cells.Count > 5 + cellCorrection)
                            {

                                // Extension
                                var extensionCell = cells[5 + cellCorrection];
                                item.Extension = Utils.ParseText(extensionCell);
                            }
                            else
                            {
                                // Some rows don't have an extension cell...
                                item.Extension = "-";
                            }

                            if (!results.Songs.ContainsKey(currentGroupAlphabetized))
                            {
                                results.Songs[currentGroupAlphabetized] = new List<Song>();
                            }
                            results.Songs[currentGroupAlphabetized].Add(item);
                        }
                    }
                }
            }

            return results;
        }

    }

    public class HornScrapingUtilities
    {
        virtual public IEnumerable<string> ParseColors(HtmlNode songCell)
        {
            var colors = new List<string>();
            // The song cell has three anchors in it
            var anchors = songCell.CssSelect("a").ToList();
            foreach (var anchor in anchors)
            {
                string href = anchor.GetAttributeValue("href");
                var uri = new Uri(href);
                string localPath = Path.GetFileName(uri.LocalPath);
                // Looks like: "Note.white.png", We need "w"
                string colorLetter = localPath.Substring(5, 1);
                colors.Add(colorLetter);
            }
            //colors.Sort();
            return colors;
        }

        virtual public string BuildNoteKey(IEnumerable<string> notes, bool alphabetize=false)
        {
            var x = notes;
            if (alphabetize)
            {
                var y = x.ToList();
                y.Sort();
                x = y;
            }
            return String.Join("", x);
        }

        virtual public string ParseText(HtmlNode extensionCell, bool htmlEncode = true)
        {
            string results = extensionCell.InnerText.Trim();
            if (htmlEncode && !String.IsNullOrWhiteSpace(results))
            {
                results = HttpUtility.HtmlDecode(results);
            }
            return results;
        }
        
    }

    public class SongTableScrapeResults
    {
        public SongTableScrapeResults()
        {
            Songs = new Dictionary<string, List<Song>>();
        }
        // Get every song and put it into a list
        public Dictionary<string, List<Song>> Songs { get; set; }

    }
    [DebuggerDisplay("{NoteKey} ({Effect1})")]
    public class Song
    {
        public string NoteKey { get; set; }
        public IEnumerable<string> Notes { get; set; }
        public string Effect1 { get; set; }
        public string Effect2 { get; set; }
        public string Duration1 { get; set; }
        public string Extension { get; set; }

        //public string Group { get; set; }

        //public Dictionary<string, NoteButtonMapping> NoteButtonMappings { get; set; }
    }

    public class NoteButtonMapping
    {
        public string Note { get; set; }
        public string Button { get; set; }
    }
}
