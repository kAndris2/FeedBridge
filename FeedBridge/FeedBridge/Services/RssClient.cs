namespace FeedBridge.Services
{
    public class RssClient
    {
        private readonly HttpClient _httpClient;

        public RssClient(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/152.0.0.0 Safari/537.36 " +
                "Edg/152.0.0.0");

            _httpClient.DefaultRequestHeaders.Accept.ParseAdd(
                "text/html,application/xhtml+xml,application/xml;q=0.9," +
                "image/avif,image/webp,image/apng,*/*;q=0.8");

            _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd(
                "hu-HU,hu;q=0.9,en-US;q=0.8,en;q=0.7");
        }

        public async Task<string> GetFeedAsync(string url)
        {
            return await _httpClient.GetStringAsync(url);
        }
    }
}