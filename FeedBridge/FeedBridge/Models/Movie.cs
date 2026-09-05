using System.Text.RegularExpressions;

namespace FeedBridge.Models
{
    public class Movie : RssItem
    {
        public int? ReleaseYear { get; set; }

        public Movie(string category, string itemTitle) : base(category, itemTitle)
        {
            ReleaseYear = ExtractReleaseYear(itemTitle);
        }

        private int? ExtractReleaseYear(string input)
        {
            var match = Regex.Match(input, @"\b(?<year>(19|20)\d{2})\b", RegexOptions.CultureInvariant);

            return match.Success
                ? int.Parse(match.Groups["year"].Value)
                : null;
        }
    }
}