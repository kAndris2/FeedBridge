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

        public async Task<IEnumerable<ICatalogItem>> CreateCatalogItems(IEnumerable<Item> items)
        {
            _logger.LogInformation("Catalog item creation is about to start...");

            _movieGenres = await _tmdbService.GetMovieGenresAsync();
            _tvGenres = await _tmdbService.GetTvGenresAsync();

            var tasks = items.Select(CreateCatalogItem);
            var catalogItems = (await Task.WhenAll(tasks))
                .Where(c => c != null)
                .Where(c => c is not VideoCatalogItem video || !string.IsNullOrEmpty(video.PosterUrl))
                .Where(c => c is not SeriesCatalogItem series || !string.IsNullOrEmpty(series.Season))
                .Cast<ICatalogItem>();

            if (!catalogItems.Any())
            {
                _logger.LogInformation("No relevant catalog items were created!");
                return [];
            }

            _logger.LogInformation($"Catalog items successfully created! [{tasks.Count()}/{catalogItems.Count()}]");

            if (tasks.Count() != catalogItems.Count())
                _logger.LogInformation($"Some items were not matched with the catalog item's criteria! Affacted items count: {tasks.Count() - catalogItems.Count()}");

            return MergeCatalogItems(catalogItems);
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
                        Rate = mediaInfo?.Rate,
                        Genres = mediaInfo != null ? _movieGenres
                            .Where(g => mediaInfo.GenreIds.Contains(g.Id))
                            .Select(g => g?.Name)
                            .ToArray() : []
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
                        Rate = mediaInfo?.Rate,
                        Genres = mediaInfo != null ? _tvGenres
                            .Where(g => mediaInfo.GenreIds.Contains(g.Id))
                            .Select(g => g?.Name)
                            .ToArray() : []
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

        private static IEnumerable<ICatalogItem> MergeCatalogItems(IEnumerable<ICatalogItem> items)
        {
            foreach (var group in items.GroupBy(x => new { x.Category, x.Title }))
            {
                var first = group.First();

                if (first is VideoCatalogItem video)
                {
                    video.Quality = string.Join(";",
                        group
                            .OfType<VideoCatalogItem>()
                            .Select(x => x.Quality ?? "SD")
                            .Distinct());

                    if (first is SeriesCatalogItem series)
                    {
                        series.Season = string.Join(";",
                            group
                                .OfType<SeriesCatalogItem>()
                                .Select(x => x.Season)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Distinct());
                    }
                }

                yield return first;
            }
        }
    }
}