namespace FeedBridge.Models
{
    public class TmdbMediaInfo(string posterUrl, double rate)
    {
        public string PosterUrl { get; set; } = posterUrl;
        public double Rate { get; set; } = rate;
    }
}