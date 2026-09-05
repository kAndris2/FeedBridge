using System.Text.RegularExpressions;

namespace FeedBridge.Models
{
    public class Series : RssItem
    {
        public string? Season { get; set; }

        public Series(string category, string itemTitle) : base(category, itemTitle)
        {
            Season = ExtractSeasonEpisode(itemTitle);
        }

        private string? ExtractSeasonEpisode(string input)
        {
            var match = Regex.Match(input, @"\b(?<season>S\d{2})(?<episode>E\d{2})?\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            return match.Success
                ? match.Value
                : null;
        }
    }
}