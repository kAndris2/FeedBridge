using Microsoft.Extensions.Options;
using TMDbLib.Client;
using FeedBridge.Models.Configuration;
using FeedBridge.Models;

namespace FeedBridge.Services
{
    public class TmdbService
    {
        private readonly TMDbClient _client;

        public TmdbService(IOptions<TmdbSettings> settings)
        {
            _client = new TMDbClient(settings.Value.ApiKey);
        }

        public async Task<TmdbMediaInfo?> SearchMovieAsync(string title, int year = 0)
        {
            var searchResult = await _client.SearchMovieAsync(title, year: year);

            if (searchResult?.Results?.Count == 0) return null;

            var movie = searchResult?.Results?.FirstOrDefault();
            await _client.GetConfigAsync();
            var posterUrl = _client.GetImageUrl("w500", movie.PosterPath);

            return new TmdbMediaInfo(posterUrl.AbsoluteUri, movie.VoteAverage);
        }
    }
}