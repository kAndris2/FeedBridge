using FeedBridge.Enums;

namespace FeedBridge.Models
{
    public class LanguageCatalogItem(string title, Category? category, string publishedDate) : CatalogItem(title, category, publishedDate)
    {
        public required string? Language { get; init; }
    }
}