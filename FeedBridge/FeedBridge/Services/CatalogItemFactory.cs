using Microsoft.Extensions.Logging;
using FeedBridge.Enums;
using FeedBridge.Interfaces;
using FeedBridge.Models;

namespace FeedBridge.Services
{
    public class CatalogItemFactory(RssItemPropExtractor rssItemPropExtractor, TmdbService tmdbService, ILogger<CatalogItemFactory> logger)
    {
        private readonly ILogger<CatalogItemFactory> _logger = logger;
        private readonly RssItemPropExtractor _rssItemPropExtractor = rssItemPropExtractor;
        private readonly TmdbService _tmdbService = tmdbService;

        public async Task<IEnumerable<ICatalogItem>> CreateCatalogItems(IEnumerable<Item> items)
        {
            _logger.LogInformation("Catalog item creation is about to start...");

            var tasks = items.Select(CreateCatalogItem);
            var catalogItems = await Task.WhenAll(tasks);

            _logger.LogInformation($"Catalog items successfully created! [{tasks.Count()}/{catalogItems.Count()}]");

            return catalogItems
                .Where(x => x != null)
                .Cast<ICatalogItem>();
        }

        private async Task<ICatalogItem?> CreateCatalogItem(Item item)
        {
            var category = GuessCategory(item.Category);
            var title = _rssItemPropExtractor.ExtractTitle(item.Title);

            switch (category)
            {
                case Category.Movie:
                {
                    var extractedReleaseYear = _rssItemPropExtractor.ExtractReleaseYear(item.Title);
                    _ = int.TryParse(extractedReleaseYear, out int releaseYear);
                    var mediaInfo = await _tmdbService.SearchMovieAsync(title, releaseYear);

                    return new MovieCatalogItem(title, category, item.PublishedDate)
                    {
                        Quality = _rssItemPropExtractor.ExtractQuality(item.Title),
                        Language = _rssItemPropExtractor.GuessLanguage(item.Title),
                        ReleaseYear = releaseYear,
                        PosterUrl = mediaInfo?.PosterUrl,
                        Rate = mediaInfo?.Rate
                    };
                }
                case Category.Series:
                {
                    var mediaInfo = await _tmdbService.SearchTvShowAsync(title);

                    return new SeriesCatalogItem(title, category, item.PublishedDate)
                    {
                        Quality = _rssItemPropExtractor.ExtractQuality(item.Title),
                        Language = _rssItemPropExtractor.GuessLanguage(item.Title),
                        Season = _rssItemPropExtractor.ExtractSeasonEpisode(item.Title),
                        PosterUrl = mediaInfo?.PosterUrl,
                        Rate = mediaInfo?.Rate
                    };
                }
                case Category.Music:
                {
                    return new MusicCatalogItem(title, category, item.PublishedDate)
                    {
                        Language = _rssItemPropExtractor.GuessLanguage(item.Title),
                        ReleaseYear = _rssItemPropExtractor.ExtractReleaseYear(item.Title)
                    };
                }
                case Category.Book:
                {
                    return new LanguageCatalogItem(title, category, item.PublishedDate)
                    {
                        Language = _rssItemPropExtractor.GuessLanguage(item.Title)
                    };
                }
                case Category.Game or Category.Program:
                {
                    return new CatalogItem(title, category, item.PublishedDate);
                }
                default: return null;
            }
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