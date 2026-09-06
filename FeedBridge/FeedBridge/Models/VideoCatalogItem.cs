using FeedBridge.Enums;

namespace FeedBridge.Models
{
    public abstract class VideoCatalogItem(string title, Category? category, string publishedDate) : LanguageCatalogItem(title, category, publishedDate)
    {
        public required string? Quality { get; init; }
    }
}