namespace FeedBridge.Models.Configuration
{
    public class NcoreSettings
    {
        public string Url { get; set; }
        public string PassKey { get; set; }
        public string RssUrl
        {
            get
            {
                return Url + "/rss.php?key=" + PassKey;
            }
        }
        public string FeedLocation { get; set; }
        public string[] CategoryFilter { get; set; }
    }
}