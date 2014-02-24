namespace MH3UWikiScraper
{
    public interface IHuntingHornLinkDeriver
    {
        string Key { get; }
        string DeriveLink(HuntingHorn horn);
    }
}