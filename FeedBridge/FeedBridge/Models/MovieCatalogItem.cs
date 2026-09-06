using FeedBridge.Enums;

namespace FeedBridge.Models
{
    public class MovieCatalogItem(string title, Category? category, string publishedDate) : VideoCatalogItem(title, category, publishedDate)
    {
        public required string? ReleaseYear { get; init; }
    }
}