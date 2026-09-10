namespace FeedBridge.Models
{
    public class TmdbMediaInfo(string? posterUrl, double rate, IList<int> genreIds)
    {
        public string? PosterUrl { get; set; } = posterUrl;
        public double Rate { get; set; } = rate;
        public IList<int> GenreIds { get; set; } = genreIds;
    }
}