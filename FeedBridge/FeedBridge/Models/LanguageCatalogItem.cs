using FeedBridge.Enums;

namespace FeedBridge.Models
{
    public class LanguageCatalogItem(string title, Category? category, DateTime publishedDate) : CatalogItem(title, category, publishedDate)
    {
        public required string? Language { get; init; }
    }
}