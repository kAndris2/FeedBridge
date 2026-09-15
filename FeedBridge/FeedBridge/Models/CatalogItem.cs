using FeedBridge.Enums;
using FeedBridge.Interfaces;

namespace FeedBridge.Models
{
    public class CatalogItem(string originalTitle, Category? category, DateTime publishedDate) : ICatalogItem
    {
        public string OriginalTitle { get; set; } = originalTitle;
        public Category? Category { get; set; } = category;
        public DateTime PublishedDate { get; set; } = publishedDate;
    }
}