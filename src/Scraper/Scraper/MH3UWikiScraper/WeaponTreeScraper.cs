using System.Diagnostics;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScrapySharp.Extensions;

namespace MH3UWikiScraper
{
    public class WeaponTreeScraper
    {
        public WeaponTreeScraper(HtmlDocument document)
        {
            Document = document;
            Utils = new HornScrapingUtilities();
        }
        public HornScrapingUtilities Utils { get; set; }

        public HtmlDocument Document { get; set; }

        virtual public Dictionary<string, List<HuntingHorn>> GetHornMapping()
        {
            var results = new Dictionary<string, List<HuntingHorn>>();
            var treeTables = Document.DocumentNode.CssSelect("table");
            var linkDeriver = new MonsterHunterWikiLinkDeriver();
            foreach (var table in treeTables)
            {
                var rows = table.CssSelect("tr").ToList();
                for (int i = 0; i < rows.Count; i++)
                {
                    // skip the first two rows
                    if (i > 1)
                    {

                        var item = new HuntingHorn();

                        var currentRow = rows[i];
                        var cells = currentRow.CssSelect("td").ToList();


                        if (cells.Count < 4)
                        {
                            continue;
                        }

                        var nameCell = cells[0];

                        var anchor = nameCell.CssSelect("a");
                        if (anchor.Count() > 0)
                        {
                            var anchorNode = anchor.FirstOrDefault();
                            item.Name = Utils.ParseText(anchorNode);
                            string page = anchorNode.GetAttributeValue("href");
                            string result = linkDeriver.FullyQualifyPage(page);
                            item.Links[Constants.WikiLinkKey] = result;
                        }
                        else
                        {
                            var lastChild = nameCell.LastChild;

                            var bold = nameCell.SelectSingleNode("b");
                            if (bold == null)
                            {
                                item.Name = Utils.ParseText(lastChild);
                            }
                            else
                            {
                                var toParse = bold;
                                if (bold.ChildNodes.Count() > 0)
                                {
                                    // Some bold tags have images and junk in them
                                    toParse = bold.ChildNodes.Last();
                                }
                                item.Name = Utils.ParseText(toParse);
                            }
                        }

                        item.Rarity = Utils.ParseText(cells[1]);
                        item.Attack = Utils.ParseText(cells[2]);
                        item.Notes = Utils.ParseColors(cells[3]);
                        item.NoteKey = Utils.BuildNoteKey(item.Notes, false);
                        
                        if (String.IsNullOrWhiteSpace(item.NoteKey))
                        {
                            continue;
                        }
                        if (!results.ContainsKey(item.NoteKey))
                        {
                            results[item.NoteKey] = new List<HuntingHorn>();
                        }
                        results[item.NoteKey].Add(item);
                    }
                }
            }
            return results;
        }
    }

    [DebuggerDisplay("{Name} ({Notes})")]
    public class HuntingHorn
    {
        public HuntingHorn()
        {
            Links = new Dictionary<string, string>();
            Notes = new List<string>();
        }
        public string Name { get; set; }
        public string Rarity { get; set; }
        public string Attack { get; set; }
        public IEnumerable<string> Notes { get; set; }
        public string NoteKey { get; set; }

        public Dictionary<string, string> Links { get; set; }
    }

    public class Constants
    {
        public const string KiranicoLinkKey = "Kiranico";
        public const string WikiLinkKey = "MHWiki";
        public const string GoogleLinkKey = "Google";

        public const string Group_Orange = "o";
        public const string Group_Purple = "p";
        public const string Group_White = "w";
    }

}
