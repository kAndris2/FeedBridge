using FeedBridge.Enums;
using FeedBridge.Interfaces;

namespace FeedBridge.Models
{
    public class CatalogItem(string title, Category? category, string publishedDate) : ICatalogItem
    {
        public string Title { get; set; } = title;
        public Category? Category { get; set; } = category;
        public DateTime PublishedDate { get; set; } = DateTime.Parse(publishedDate);
    }
}