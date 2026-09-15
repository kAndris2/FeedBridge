using FeedBridge.Enums;

namespace FeedBridge.Models
{
    public class LanguageCatalogItem(string originalTitle, Category? category, DateTime publishedDate) : CatalogItem(originalTitle, category, publishedDate)
    {
        public required string? Language { get; init; }
    }
}