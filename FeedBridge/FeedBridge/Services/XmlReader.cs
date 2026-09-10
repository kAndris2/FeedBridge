using System.Xml.Serialization;

namespace FeedBridge.Services
{
    public class XmlReader
    {
        public T FromString<T>(string content)
        {
            var xmlStart = content.IndexOf("<rss", StringComparison.Ordinal);

            if (xmlStart == -1)
                throw new InvalidOperationException("RSS root element not found.");

            content = content[xmlStart..]
                .Replace("&", "&amp;");

            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(content);

            return (T)serializer.Deserialize(reader)!;
        }
    }
}