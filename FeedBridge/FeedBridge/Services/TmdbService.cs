using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;
using FeedBridge.Models;
using FeedBridge.Models.Configuration;
using FeedBridge.Enums;

namespace FeedBridge.Services
{
    public class TmdbService
    {
        private readonly ILogger<TmdbService> _logger;
        private readonly TmdbSettings _settings;
        private readonly TMDbClient _client;
        private readonly Task _configInitTask;
        private readonly Dictionary<string, TmdbMediaInfo> _mediaInfoCache = [];

        public TmdbService(IOptions<TmdbSettings> settings, ILogger<TmdbService> logger)
        {
            _client = new TMDbClient(settings.Value.ApiKey);
            _configInitTask = _client.GetConfigAsync();
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task<List<Genre>> GetMovieGenresAsync()
        {
            try
            {
                return (await _client.GetMovieGenresAsync()) ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unsuccessful movie genre retrieval! - Ex.: {ex.Message}");
                return [];
            }
        }

        public async Task<List<Genre>> GetTvGenresAsync()
        {
            try
            {
                return (await _client.GetTvGenresAsync()) ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unsuccessful tv genre retrieval! - Ex.: {ex.Message}");
                return [];
            }
        }

        public async Task<TmdbMediaInfo?> SearchMultiAsync(string title, Category? category, int year = 0)
        {
            if (_mediaInfoCache.TryGetValue(title, out TmdbMediaInfo? value)) 
                return value;

            try
            {
                SearchContainer<SearchBase>? searchResult = null;
                SearchMovieTvBase? media = null;
                int currentPage = 1;

                while (media == null && currentPage <= (searchResult?.TotalPages ?? 1))
                {
                    _logger.LogInformation($"Searching media... - Title: {title} | Year: {year} | Category: {category} | Page: {currentPage}");

                    searchResult = await _client.SearchMultiAsync(title, currentPage, year: year) 
                        ?? throw new InvalidDataException("The search result is null!");

                    if (searchResult.Results == null || searchResult.Results.Count == 0)
                        throw new InvalidOperationException("The search result contains no items!");

                    media = FindRelevantMedia([.. searchResult
                        .Results
                        .OfType<SearchMovieTvBase>()], category, year);

                    currentPage++;
                }

                if (media == null)
                    throw new InvalidDataException($"No relevant media found in the whole collection! Checked pages: {currentPage}/{searchResult?.TotalPages}");

                var mediaInfo = await CreateTmdbMediaInfo(media, category, year, title);
                _mediaInfoCache.Add(title, mediaInfo);

                return mediaInfo;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"No media info found for this media '{title} - {year}'! - Ex.: {ex.Message}");
                return null;
            }
        }

        private async Task<TmdbMediaInfo> CreateTmdbMediaInfo(SearchMovieTvBase media, Category? category, int year, string originalTitle)
        {
            try
            {
                var titleInfo = await GetTmdbMediaTitleInfo(media.Id, media.MediaType, originalTitle);

                if (media.PosterPath == null)
                    return new TmdbMediaInfo(media.VoteAverage, media.GenreIds, titleInfo);

                await _configInitTask;
                var posterUrl = _client.GetImageUrl("w500", media.PosterPath);

                return new TmdbMediaInfo(posterUrl.AbsoluteUri, media.VoteAverage, media.GenreIds, titleInfo);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<TmdbMediaTitleInfo?> GetTmdbMediaTitleInfo(int mediaId, MediaType mediaType, string originalTitle)
        {
            try
            {
                TmdbMediaTitleInfo? titleInfo = null;

                if (mediaType == MediaType.Movie)
                {
                    titleInfo = await GetMovieTitleInfo(mediaId, originalTitle);
                }
                else if (mediaType == MediaType.Tv)
                {
                    titleInfo = await GetTvShowTitleInfo(mediaId, originalTitle);
                }

                return titleInfo;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"An error occured while fetching translations for media - {mediaId}! - Ex.: {ex.Message}");
                return null;
            }
        }

        private async Task<TmdbMediaTitleInfo?> GetMovieTitleInfo(int mediaId, string originalTitle)
        {
            try
            {
                var translations = (await _client.GetMovieTranslationsAsync(mediaId))?.Translations;
                return CreateTmdbMediaTitleInfo(translations, originalTitle);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"No translations found for movie! - Ex.: {ex.Message}");
            }
        }

        private async Task<TmdbMediaTitleInfo?> GetTvShowTitleInfo(int mediaId, string originalTitle)
        {
            try
            {
                var translations = (await _client.GetTvShowTranslationsAsync(mediaId))?.Translations;
                return CreateTmdbMediaTitleInfo(translations, originalTitle);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"No translations found for tv show! -  Ex.: {ex.Message}");
            }
        }

        private TmdbMediaTitleInfo CreateTmdbMediaTitleInfo(List<Translation>? translations, string originalTitle)
        {
            if (translations == null || translations.Count == 0) return null;

            var primaryTitle = translations
                .FirstOrDefault(t => t.Iso_639_1 == _settings.PrimaryLanguage)
                ?.Data?.Name;
            var secondaryTitle = translations
                .FirstOrDefault(t => t.Iso_639_1 == _settings.SecondaryLanguage)
                ?.Data?.Name;

            return new TmdbMediaTitleInfo(originalTitle, primaryTitle, secondaryTitle);
        }

        private static SearchMovieTvBase? FindRelevantMedia(List<SearchMovieTvBase> searchResult, Category? category, int year)
        {
            var mediaType = category switch
            {
                Category.Movie => MediaType.Movie,
                Category.Series => MediaType.Tv,
                _ => throw new InvalidDataException($"Invalid category at media selection! - Category: {category}")
            };

            var relevantSearchResults = searchResult
                .Where(r => r != null)
                .Where(r => r.MediaType == mediaType);

            if (relevantSearchResults == null || !relevantSearchResults.Any())
                throw new InvalidDataException($"No relevant search result found by category '{category}'!");
            
            if (mediaType == MediaType.Movie)
            {
                var mediaByReleaseYear = relevantSearchResults
                    .Cast<SearchMovie>()
                    .FirstOrDefault(r => r.ReleaseDate.Value.Year == year);

                return mediaByReleaseYear;
            }

            return relevantSearchResults.First();
        }
    }
}