using FeedBridge.Enums;
using FeedBridge.Interfaces;
using FeedBridge.Models;
using System.Text.RegularExpressions;

namespace FeedBridge.Services
{
    public class CatalogItemFactory
    {
        public IEnumerable<ICatalogItem> CreateCatalogItems(IEnumerable<Item> items)
        {
            return items.Select(item => CreateCatalogItem(item))
                .Where(item => item != null)
                .Select(item => item!);
        }

        private ICatalogItem? CreateCatalogItem(Item item)
        {
            var category = GuessCategory(item.Category);
            var title = ExtractTitle(item.Title);

            return (category) switch
            {
                Category.Movie => new MovieCatalogItem(title, category, item.PublishedDate)
                {
                    Quality = ExtractQuality(item.Title),
                    Language = GuessLanguage(item.Title),
                    ReleaseYear = ExtractReleaseYear(item.Title)
                },
                Category.Series => new SeriesCatalogItem(title, category, item.PublishedDate)
                {
                    Quality = ExtractQuality(item.Title),
                    Language = GuessLanguage(item.Title),
                    Season = ExtractSeasonEpisode(item.Title),
                },
                Category.Music => new MusicCatalogItem(title, category, item.PublishedDate)
                {
                    Language = GuessLanguage(item.Title),
                    ReleaseYear = ExtractReleaseYear(item.Title)
                },
                Category.Book => new LanguageCatalogItem(title, category, item.PublishedDate)
                {
                    Language = GuessLanguage(item.Title)
                },
                Category.Game or Category.Program => new CatalogItem(title, category, item.PublishedDate),
                _ => null
            };
        }

        private Category? GuessCategory(string category)
        {
            var categoryKeywordPairs = new Dictionary<string[], Category>()
            {
                { ["film"], Category.Movie },
                { ["sorozat"], Category.Series },
                { ["mp3"], Category.Music },
                { ["játék", "konzol"], Category.Game },
                { ["book"], Category.Book },
                { ["program", "mobil"], Category.Program },
            };

            foreach (var (keywords, categoryType) in categoryKeywordPairs)
            {
                if (keywords.Any(keyword => category.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                {
                    return categoryType;
                }
            }

            return null;
        }

        private string ExtractTitle(string input)
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

        private string? ExtractQuality(string input)
        {
            var match = Regex.Match(input, @"\b(?<quality>\d{3,4}p)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            return match.Success
                ? match.Groups["quality"].Value
                : null;
        }

        private int? ExtractReleaseYear(string input)
        {
            var match = Regex.Match(input, @"\b(?<year>(19|20)\d{2})\b", RegexOptions.CultureInvariant);

            return match.Success
                ? int.Parse(match.Groups["year"].Value)
                : null;
        }

        private string? ExtractSeasonEpisode(string input)
        {
            var match = Regex.Match(input, @"\b(?<season>S\d{2})(?<episode>E\d{2})?\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            return match.Success
                ? match.Value
                : null;
        }

        private string? GuessLanguage(string title)
        {
            if (title.Contains("(HUN"))
                return "HUN";

            if (title.Contains("(ENG"))
                return "ENG";

            return null;
        }
    }
}