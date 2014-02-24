using System;

namespace MH3UWikiScraper
{
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
}