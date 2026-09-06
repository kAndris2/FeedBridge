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
        private readonly TMDbClient _client;
        private readonly Task _configInitTask;

        public TmdbService(IOptions<TmdbSettings> settings)
        {
            _client = new TMDbClient(settings.Value.ApiKey);
            _configInitTask = _client.GetConfigAsync();
        }

        public async Task<TmdbMediaInfo?> SearchMovieAsync(string title, int year = 0)
        {
            var searchResult = await _client.SearchMovieAsync(title);

            return await CreateTmdbMediaInfo(searchResult);
        }

        public async Task<TmdbMediaInfo?> SearchTvShowAsync(string title)
        {
            var searchResult = await _client.SearchTvShowAsync(title);

            return await CreateTmdbMediaInfo(searchResult);
        }

        private async Task<TmdbMediaInfo> CreateTmdbMediaInfo<T>(SearchContainer<T> searchResult)
            where T : SearchMovieTvBase
        {
            if (searchResult?.Results?.Count == 0) return null;

            var media = searchResult?.Results?.FirstOrDefault();

            await _configInitTask;
            var posterUrl = _client.GetImageUrl("w500", media.PosterPath);

            return new TmdbMediaInfo(posterUrl.AbsoluteUri, media.VoteAverage);
        }
    }
}