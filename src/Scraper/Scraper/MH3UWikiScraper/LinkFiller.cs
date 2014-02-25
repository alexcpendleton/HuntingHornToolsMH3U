using System.Collections.Generic;

namespace MH3UWikiScraper
{
    public class LinkFiller
    {
        public LinkFiller()
        {
            ShouldOverwriteExisting = false;
            Derivers = new List<IHuntingHornLinkDeriver>
            {
                new KiranicoHornLinkDeriver(),
                //new MonsterHunterWikiLinkDeriver(),
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
}