using System.Xml.Serialization;

namespace FeedBridge.Models
{
    [XmlRoot(ElementName = "item")]
    public class Item
    {
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }

        [XmlElement(ElementName = "category")]
        public string Category { get; set; }
    }

    [XmlRoot(ElementName = "channel")]
    public class Channel
    {
        [XmlElement(ElementName = "item")]
        public List<Item> Item { get; set; }
    }

    [XmlRoot(ElementName = "rss")]
    public class NcoreRssDocument
    {
        [XmlElement(ElementName = "channel")]
        public Channel Channel { get; set; }
    }
}