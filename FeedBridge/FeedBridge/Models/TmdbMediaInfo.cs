using System.Text.Json.Serialization;

namespace FeedBridge.Models
{
    public class TmdbMediaInfo(string? posterUrl, double rate, IList<int>? genreIds, TmdbMediaTitleInfo? titleInfo)
    {
        public string? PosterUrl { get; private set; } = posterUrl;
        public double Rate { get; private set; } = rate;
        public TmdbMediaTitleInfo? TitleInfo { get; private set; } = titleInfo;
        public IList<int> GenreIds { get; private set; } = genreIds ?? [];

        public TmdbMediaInfo(double rate, IList<int>? genreIds, TmdbMediaTitleInfo? titleInfo)
            : this(null, rate, genreIds, titleInfo)
        {
            Rate = rate;
            GenreIds = genreIds;
            TitleInfo = titleInfo;
        }
    }

    public class TmdbMediaTitleInfo
    {
        public string? PrimaryTitle { get; private set; }
        public string? SecondaryTitle { get; private set; }

        public TmdbMediaTitleInfo(string originalTitle, string? primaryTitle, string? secondaryTitle)
        {
            PrimaryTitle = primaryTitle;
            SecondaryTitle = secondaryTitle;

            if (string.IsNullOrEmpty(PrimaryTitle))
            {
                PrimaryTitle = originalTitle;
            }

            if (string.Equals(PrimaryTitle, SecondaryTitle, StringComparison.OrdinalIgnoreCase))
            {
                SecondaryTitle = null;
            }
        }

        [JsonConstructor]
        public TmdbMediaTitleInfo()
        {
        }
    }
}