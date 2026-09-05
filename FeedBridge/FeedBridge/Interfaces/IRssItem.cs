using TMDbLib.Objects.General;

namespace FeedBridge.Interfaces
{
    public interface IRssItem
    {
        public string Title { get; set; }
        public string? Quality { get; set; }
        public string Category { get; set; }
    }
}