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

        public async Task<TmdbMediaInfo?> SearchMultiAsync(string title, int year = 0)
        {
            try
            {
                var searchResult = await _client.SearchMultiAsync(title, year: year); 
                
                if (searchResult == null)
                    throw new InvalidDataException("The search result is null!");

                if (searchResult.Results == null || searchResult.Results.Count == 0)
                    throw new InvalidOperationException("The search result contains no items!");

                return await CreateTmdbMediaInfo(searchResult
                    .Results
                    .Where(r => r is SearchMovieTvBase)
                    .OfType<SearchMovieTvBase>()
                    .ToList());
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"No media info found for this media '{title} - {year}' ! - Ex.: {ex.Message}");
                return null;
            }
        }

        private async Task<TmdbMediaInfo> CreateTmdbMediaInfo(List<SearchMovieTvBase> searchResult)
        {
            try
            {
                var media = searchResult.FirstOrDefault(mItem => mItem != null)
                    ?? throw new InvalidDataException("The selected search result is null!");

                var titleInfo = await GetTmdbMediaTitleInfo(media.Id, media.MediaType);

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

        private async Task<TmdbMediaTitleInfo?> GetTmdbMediaTitleInfo(int mediaId, MediaType mediaType)
        {
            try
            {
                TmdbMediaTitleInfo? titleInfo = null;

                if (mediaType == MediaType.Movie)
                {
                    titleInfo = await GetMovieTitleInfo(mediaId);
                }
                else if (mediaType == MediaType.Tv)
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