using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FeedBridge.Models;
using FeedBridge.Models.Configuration;
using FeedBridge.Services;

namespace FeedBridge
{
    public class Application(RssClient rssClient, GoogleDriveService driveService, CatalogItemFactory factory, IOptions<NcoreSettings> ncoreSettings, ILogger<Application> logger)
    {
        private readonly ILogger<Application> _logger = logger;
        private readonly RssClient _rssClient = rssClient;
        private readonly NcoreSettings _ncoreSettings = ncoreSettings.Value;
        private readonly GoogleDriveService _driveService = driveService;
        private readonly CatalogItemFactory _factory = factory;

        public async Task Start()
        {
            var relevantItems = await CollectRelevantItemsFromFeed();

            if (!relevantItems.Any())
            {
                _logger.LogInformation("No relevant items found in the RSS feed!");
                return;
            }

            _logger.LogInformation($"Found {relevantItems.Count()} relevant items in the RSS feed.");

            var catalogItems = await _factory.CreateCatalogItems(relevantItems);

            if (!catalogItems.Any()) return;

            var jsonContent = JsonSerializer.Serialize(catalogItems, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            await _driveService.UploadJsonAsync(jsonContent);
            _logger.LogInformation("Catalog items successfully uploaded on to Google Drive storage!");
        }

        private async Task<IEnumerable<Item>> CollectRelevantItemsFromFeed()
        {
            var content = await _rssClient.GetFeedAsync(_ncoreSettings.RssUrl);
            var doc = new XmlReader()
                .FromString<NcoreRssDocument>(content);

            return doc.Channel.Items
                .Where(item => _ncoreSettings.CategoryFilter.Contains(item.Category))
                .Where(item => DateTimeOffset.TryParse(item.PublishedDate, out var publishedDate) && publishedDate.Date == DateTimeOffset.Now.Date)
                .Where(item => !Regex.IsMatch(item.Title, @"\b(?:S\d{2}E\d{2}|E\d{2})\b", RegexOptions.IgnoreCase));
        }
    }
}