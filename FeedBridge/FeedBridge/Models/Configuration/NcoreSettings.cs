namespace FeedBridge.Models.Configuration
{
    public class NcoreSettings
    {
        public string RssFilePath { get; set; }
        public string RssUrl { get; set; }
        public string[] CategoryFilter { get; set; }
        public IDictionary<string, string[]> TitleKeyBlackList { get; set; }
        public int MinimumMovieReleaseYearFilter { get; set; }
    }
}