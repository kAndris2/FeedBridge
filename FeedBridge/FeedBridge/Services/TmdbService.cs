using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;
using FeedBridge.Models;
using FeedBridge.Models.Configuration;

namespace FeedBridge.Services
{
    public class TmdbService
    {
        private readonly ILogger<TmdbService> _logger;
        private readonly TmdbSettings _settings;
        private readonly TMDbClient _client;
        private readonly Task _configInitTask;

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

        public async Task<TmdbMediaInfo?> SearchMovieAsync(string title, int year = 0)
        {
            try
            {
                var searchResult = await _client.SearchMovieAsync(title, year: year);
                return await CreateTmdbMediaInfo(searchResult);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"No media info found for this movie '{title} - {year}' ! - Ex.: {ex.Message}");
                return null;
            }
        }

        public async Task<TmdbMediaInfo?> SearchTvShowAsync(string title)
        {
            try
            {
                var searchResult = await _client.SearchTvShowAsync(title);
                return await CreateTmdbMediaInfo(searchResult);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"No media info found for this tv show '{title}'! - Ex.: {ex.Message}");
                return null;
            }
        }

        private async Task<TmdbMediaInfo> CreateTmdbMediaInfo<T>(SearchContainer<T>? searchResult)
            where T : SearchMovieTvBase
        {
            try
            {
                if (searchResult == null)
                    throw new InvalidDataException("The search result is null!");

                if (searchResult.Results == null || searchResult.Results.Count == 0)
                    throw new InvalidOperationException("The search result contains no items!");

                var media = searchResult.Results
                    .FirstOrDefault(mItem => mItem != null)
                    ?? throw new InvalidDataException("The selected search result is null!");

                var titleInfo = await GetTmdbMediaTitleInfo<T>(media.Id);

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

        private async Task<TmdbMediaTitleInfo?> GetTmdbMediaTitleInfo<T>(int mediaId)
            where T : SearchMovieTvBase
        {
            try
            {
                TmdbMediaTitleInfo? titleInfo = null;

                if (typeof(T) == typeof(SearchMovie))
                {
                    titleInfo = await GetMovieTitleInfo(mediaId);
                }
                else if (typeof(T) == typeof(SearchTv))
                {
                    titleInfo = await GetTvShowTitleInfo(mediaId);
                }

                return titleInfo;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"An error occured while fetching translations for media - {mediaId}! - Ex.: {ex.Message}");
                return null;
            }
        }

        private async Task<TmdbMediaTitleInfo?> GetMovieTitleInfo(int mediaId)
        {
            try
            {
                var translations = (await _client.GetMovieTranslationsAsync(mediaId))?.Translations;
                return CreateTmdbMediaTitleInfo(translations);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"No translations found for movie! - Ex.: {ex.Message}");
            }
        }

        private async Task<TmdbMediaTitleInfo?> GetTvShowTitleInfo(int mediaId)
        {
            try
            {
                var translations = (await _client.GetTvShowTranslationsAsync(mediaId))?.Translations;
                return CreateTmdbMediaTitleInfo(translations);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"No translations found for tv show! -  Ex.: {ex.Message}");
            }
        }

        private TmdbMediaTitleInfo? CreateTmdbMediaTitleInfo(List<Translation>? translations)
        {
            if (translations == null || translations.Count == 0) return null;

            var primaryTitle = translations
                .FirstOrDefault(t => t.Iso_639_1 == _settings.PrimaryLanguage)
                ?.Data?.Name;
            var secondaryTitle = translations
                .FirstOrDefault(t => t.Iso_639_1 == _settings.SecondaryLanguage)
                ?.Data?.Name;

            return (string.IsNullOrEmpty(primaryTitle) && string.IsNullOrEmpty(secondaryTitle)) ? null : new TmdbMediaTitleInfo(primaryTitle, secondaryTitle);
        }
    }
}