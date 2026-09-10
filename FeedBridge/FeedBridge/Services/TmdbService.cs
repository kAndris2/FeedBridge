using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
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
        private readonly TMDbClient _client;
        private readonly Task _configInitTask;

        public TmdbService(IOptions<TmdbSettings> settings, ILogger<TmdbService> logger)
        {
            _client = new TMDbClient(settings.Value.ApiKey);
            _configInitTask = _client.GetConfigAsync();
            _logger = logger;
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

                if (media.PosterPath == null)
                    return new TmdbMediaInfo(null, media.VoteAverage, media.GenreIds ?? []);

                await _configInitTask;
                var posterUrl = _client.GetImageUrl("w500", media.PosterPath);

                return new TmdbMediaInfo(posterUrl.AbsoluteUri, media.VoteAverage, media.GenreIds ?? []);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}