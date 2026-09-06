using FeedBridge.Enums;

namespace FeedBridge.Interfaces
{
    public interface ICatalogItem
    {
        public string Title { get; set; }
        public Category? Category { get; set; }
        public DateTime PublishedDate { get; set; }
    }
}