namespace FeedBridge.Models
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
    }
}