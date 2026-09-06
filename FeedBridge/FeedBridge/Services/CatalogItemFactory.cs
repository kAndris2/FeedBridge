using FeedBridge.Enums;
using FeedBridge.Interfaces;
using FeedBridge.Models;

namespace FeedBridge.Services
{
    public class CatalogItemFactory(RssItemPropExtractor rssItemPropExtractor)
    {
        private readonly RssItemPropExtractor _rssItemPropExtractor = rssItemPropExtractor;

        public IEnumerable<ICatalogItem> CreateCatalogItems(IEnumerable<Item> items)
        {
            return items.Select(item => CreateCatalogItem(item))
                .Where(item => item != null)
                .Select(item => item!);
        }

        private ICatalogItem? CreateCatalogItem(Item item)
        {
            var category = GuessCategory(item.Category);
            var title = _rssItemPropExtractor.ExtractTitle(item.Title);

            return (category) switch
            {
                Category.Movie => new MovieCatalogItem(title, category, item.PublishedDate)
                {
                    Quality = _rssItemPropExtractor.ExtractQuality(item.Title),
                    Language = _rssItemPropExtractor.GuessLanguage(item.Title),
                    ReleaseYear = _rssItemPropExtractor.ExtractReleaseYear(item.Title)
                },
                Category.Series => new SeriesCatalogItem(title, category, item.PublishedDate)
                {
                    Quality = _rssItemPropExtractor.ExtractQuality(item.Title),
                    Language = _rssItemPropExtractor.GuessLanguage(item.Title),
                    Season = _rssItemPropExtractor.ExtractSeasonEpisode(item.Title),
                },
                Category.Music => new MusicCatalogItem(title, category, item.PublishedDate)
                {
                    Language = _rssItemPropExtractor.GuessLanguage(item.Title),
                    ReleaseYear = _rssItemPropExtractor.ExtractReleaseYear(item.Title)
                },
                Category.Book => new LanguageCatalogItem(title, category, item.PublishedDate)
                {
                    Language = _rssItemPropExtractor.GuessLanguage(item.Title)
                },
                Category.Game or Category.Program => new CatalogItem(title, category, item.PublishedDate),
                _ => throw new InvalidDataException($"Unknown item category! ({item.Category})")
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
    }
}