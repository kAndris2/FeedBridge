using FeedBridge.Enums;

namespace FeedBridge.Models
{
    public abstract class VideoCatalogItem(string title, Category? category, string publishedDate) : LanguageCatalogItem(title, category, publishedDate)
    {
        public required string? Quality { get; set; }
        public required string? PosterUrl { get; init; }
        public required double? Rate { get; init; }
        public required string[] Genres { get; init; }
    }
}