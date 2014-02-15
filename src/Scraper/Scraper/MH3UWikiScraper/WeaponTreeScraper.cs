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
                            item.Name = Utils.ParseText(anchor.FirstOrDefault());
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
                                item.Name = Utils.ParseText(bold);
                            }
                        }

                        item.Rarity = Utils.ParseText(cells[1]);
                        item.Attack = Utils.ParseText(cells[2]);
                        item.Notes = Utils.ParseColors(cells[3]);
                        item.NoteKey = Utils.BuildNoteKey(item.Notes);
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
        public string Name { get; set; }
        public string Rarity { get; set; }
        public string Attack { get; set; }
        public IEnumerable<string> Notes { get; set; }
        public string NoteKey { get; set; }
    }
}
