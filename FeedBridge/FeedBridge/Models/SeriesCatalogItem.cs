using FeedBridge.Enums;

namespace FeedBridge.Models
{
    public class SeriesCatalogItem(string title, Category? category, string publishedDate) : VideoCatalogItem(title, category, publishedDate)
    {
        public required string? Season { get; set; }
    }
}