using System.Text.RegularExpressions;

namespace FeedBridge.Services
{
    public class RssItemPropExtractor
    {
        public string ExtractTitle(string input)
        {
            var match = Regex.Match(
                input,
                @"^(?<title>.+?)(?=[.\s](?:(?:19|20)\d{2}|S\d{2}(?:E\d{2})?|\d{3,4}p|BDRip|BRRip|WEBRip|WEB-DL|BluRay|HDTV|DVDRip|HDRip)\b)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (!match.Success)
            {
                return input;
            }

            return match.Groups["title"].Value
                .Replace('.', ' ')
                .Trim();
        }

        public string? ExtractQuality(string input)
        {
            var match = Regex.Match(input, @"\b(?<quality>\d{3,4}p)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            return match.Success
                ? match.Groups["quality"].Value
                : null;
        }

        public int? ExtractReleaseYear(string input)
        {
            var match = Regex.Match(input, @"\b(?<year>(19|20)\d{2})\b", RegexOptions.CultureInvariant);

            return match.Success
                ? int.Parse(match.Groups["year"].Value)
                : null;
        }

        public string? ExtractSeasonEpisode(string input)
        {
            var match = Regex.Match(input, @"\b(?<season>S\d{2})(?<episode>E\d{2})?\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            return match.Success
                ? match.Value
                : null;
        }

        public string? GuessLanguage(string title)
        {
            if (title.Contains("(HUN"))
                return "HUN";

            if (title.Contains("(ENG"))
                return "ENG";

            return null;
        }
    }
}