using FeedBridge.Enums;

namespace FeedBridge.Models
{
    public class MusicCatalogItem(string originalTitle, Category? category, DateTime publishedDate) : LanguageCatalogItem(originalTitle, category, publishedDate)
    {
        public required string? ReleaseYear { get; init; }
    }
}