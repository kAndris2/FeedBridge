using Microsoft.Extensions.Logging;
using TMDbLib.Objects.General;
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

        private List<Genre> _movieGenres;
        private List<Genre> _tvGenres;

        public async Task<List<ICatalogItem>> CreateCatalogItems(List<Item> items, int minimumMovieReleaseYearFilter)
        {
            _logger.LogInformation("Catalog item creation is about to start...");

            _movieGenres = await _tmdbService.GetMovieGenresAsync();
            _tvGenres = await _tmdbService.GetTvGenresAsync();

            var catalogItems = new List<ICatalogItem>();

            foreach (var item in items)
            {
                var catalogItem = await CreateCatalogItem(item, minimumMovieReleaseYearFilter);

                if (catalogItem == null) continue;

                if (catalogItem is SeriesCatalogItem series && string.IsNullOrEmpty(series.Season)) continue;

                catalogItems.Add(catalogItem);
            }

            if (catalogItems.Count == 0)
            {
                _logger.LogInformation("No relevant catalog items were created!");
                return [];
            }

            _logger.LogInformation($"Catalog items successfully created! [{items.Count}/{catalogItems.Count}]");

            if (items.Count != catalogItems.Count)
                _logger.LogInformation($"Some items were not matched with the catalog item's criteria! Affected items count: {items.Count - catalogItems.Count}");

            var mergedCatalogItems = MergeCatalogItems(catalogItems).ToList();

            _logger.LogInformation($"Merging of duplicated catalog items have been finsihed. - Merged items: {catalogItems.Count - mergedCatalogItems.Count} | Catalog items count: {mergedCatalogItems.Count}");

            return mergedCatalogItems;
        }

        public IEnumerable<ICatalogItem> MergeCatalogItems(IEnumerable<ICatalogItem> items)
        {
            foreach (var group in items.GroupBy(x => new { x.Category, x.OriginalTitle }))
            {
                var first = group.First();

                if (first is VideoCatalogItem video)
                {
                    video.Quality = string.Join(";",
                        group
                            .OfType<VideoCatalogItem>()
                            .SelectMany(x => (x.Quality ?? "SD")
                                .Split(';', StringSplitOptions.RemoveEmptyEntries))
                            .Select(x => x.Trim())
                            .Distinct(StringComparer.OrdinalIgnoreCase));

                    if (first is SeriesCatalogItem series)
                    {
                        series.Season = string.Join(";",
                            group
                                .OfType<SeriesCatalogItem>()
                                .Where(x => !string.IsNullOrWhiteSpace(x.Season))
                                .SelectMany(x => x.Season!
                                    .Split(';', StringSplitOptions.RemoveEmptyEntries))
                                .Select(x => x.Trim())
                                .Distinct(StringComparer.OrdinalIgnoreCase));
                    }
                }

                yield return first;
            }
        }

        private async Task<ICatalogItem?> CreateCatalogItem(Item item, int minimumMovieReleaseYearFilter)
        {
            var category = GuessCategory(item.Category);
            var originalTitle = _rssItemPropExtractor.ExtractTitle(item.Title);
            var publishedDate = DateTime.Parse(item.PublishedDate);

            switch (category)
            {
                case Category.Movie:
                {
                    var extractedReleaseYear = _rssItemPropExtractor.ExtractReleaseYear(item.Title);
                    _ = int.TryParse(extractedReleaseYear, out int releaseYear);

                    if (releaseYear != 0 && releaseYear < minimumMovieReleaseYearFilter) return null;

                    var mediaInfo = await _tmdbService.SearchMultiAsync(originalTitle, category, releaseYear);

                    return new MovieCatalogItem(originalTitle, category, publishedDate)
                    {
                        Quality = _rssItemPropExtractor.ExtractQuality(item.Title),
                        Language = _rssItemPropExtractor.GuessLanguage(item.Title),
                        ReleaseYear = releaseYear,
                        PosterUrl = mediaInfo?.PosterUrl,
                        Rate = mediaInfo?.Rate,
                        TitleInfo = mediaInfo?.TitleInfo,
                        Genres = mediaInfo != null ? _movieGenres
                            .Where(g => mediaInfo.GenreIds.Contains(g.Id))
                            .Select(g => g?.Name)
                            .ToArray() : []
                    };
                }
                case Category.Series:
                {
                    var mediaInfo = await _tmdbService.SearchMultiAsync(originalTitle, category);

                    return new SeriesCatalogItem(originalTitle, category, publishedDate)
                    {
                        Quality = _rssItemPropExtractor.ExtractQuality(item.Title),
                        Language = _rssItemPropExtractor.GuessLanguage(item.Title),
                        Season = _rssItemPropExtractor.ExtractSeasonEpisode(item.Title),
                        PosterUrl = mediaInfo?.PosterUrl,
                        Rate = mediaInfo?.Rate,
                        TitleInfo = mediaInfo?.TitleInfo,
                        Genres = mediaInfo != null ? _tvGenres
                            .Where(g => mediaInfo.GenreIds.Contains(g.Id))
                            .Select(g => g?.Name)
                            .ToArray() : []
                    };
                }
                case Category.Music:
                {
                    return new MusicCatalogItem(originalTitle, category, publishedDate)
                    {
                        Language = _rssItemPropExtractor.GuessLanguage(item.Title),
                        ReleaseYear = _rssItemPropExtractor.ExtractReleaseYear(item.Title)
                    };
                }
                case Category.Book:
                {
                    return new LanguageCatalogItem(originalTitle, category, publishedDate)
                    {
                        Language = _rssItemPropExtractor.GuessLanguage(item.Title)
                    };
                }
                case Category.Game or Category.Program:
                {
                    return new CatalogItem(originalTitle, category, publishedDate);
                }
                default: return null;
            }
        }

        private static Category? GuessCategory(string category)
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