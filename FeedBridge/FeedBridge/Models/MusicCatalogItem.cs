using FeedBridge.Enums;

namespace FeedBridge.Models
{
    public class MusicCatalogItem(string title, Category? category, string publishedDate) : LanguageCatalogItem(title, category, publishedDate)
    {
        public required int? ReleaseYear { get; init; }
    }
}