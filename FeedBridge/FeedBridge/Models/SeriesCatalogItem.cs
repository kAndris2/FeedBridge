using FeedBridge.Enums;

namespace FeedBridge.Models
{
    public class SeriesCatalogItem(string originalTitle, Category? category, DateTime publishedDate) : VideoCatalogItem(originalTitle, category, publishedDate)
    {
        public required string? Season { get; set; }
    }
}