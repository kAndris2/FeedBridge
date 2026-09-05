using FeedBridge.Interfaces;
using System.Text.RegularExpressions;

namespace FeedBridge.Models
{
    public class RssItem : IRssItem
    {
        public string Title { get; set; }
        public string? Quality { get; set; }
        public string Category { get; set; }

        protected RssItem(string category, string itemTitle)
        {
            Category = category;
            Title = ExtractTitle(itemTitle);
            Quality = ExtractQuality(itemTitle);
        }

        protected string ExtractTitle(string input)
        {
            var match = Regex.Match(input, @"^(?<title>.+?)(?=[.\s](?:(?:19|20)\d{2}|S\d{2}(?:E\d{2})?|\d{3,4}p)\b)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (!match.Success)
            {
                return input;
            }

            return match.Groups["title"].Value
                .Replace('.', ' ')
                .Trim();
        }

        protected string? ExtractQuality(string input)
        {
            var match = Regex.Match(input, @"\b(?<quality>\d{3,4}p)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            return match.Success
                ? match.Groups["quality"].Value
                : null;
        }
    }
}