using FeedBridge.Enums;

namespace FeedBridge.Models
{
    public abstract class VideoCatalogItem(string originalTitle, Category? category, DateTime publishedDate) : LanguageCatalogItem(originalTitle, category, publishedDate)
    {
        public required string? Quality { get; set; }
        public required string? PosterUrl { get; init; }
        public required double? Rate { get; init; }
        public required string[] Genres { get; init; }
        public required TmdbMediaTitleInfo TitleInfo { get; init; }
    }
}