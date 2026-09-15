using FeedBridge.Enums;

namespace FeedBridge.Models
{
    public class MovieCatalogItem(string originalTitle, Category? category, DateTime publishedDate) : VideoCatalogItem(originalTitle, category, publishedDate)
    {
        public required int? ReleaseYear { get; init; }
    }
}