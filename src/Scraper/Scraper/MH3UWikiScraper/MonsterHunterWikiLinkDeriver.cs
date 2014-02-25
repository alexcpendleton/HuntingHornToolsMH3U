using System;

namespace MH3UWikiScraper
{
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
            get { return Constants.WikiLinkKey; }
        }
    }
}