using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FeedBridge.Interfaces;
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

            var catalogItems = await _factory.CreateCatalogItems(relevantItems, _ncoreSettings.MinimumMovieReleaseYearFilter);

            if (!catalogItems.Any()) return;

            _logger.LogInformation($"Fetching stored catalog items...");

            var storedCatalogItems = await CollectRelevantCatalogItemsFromStorage();

            _logger.LogInformation($"Found {storedCatalogItems.Count} relevant catalog items in the storage.");

            if (storedCatalogItems.Count != 0)
            {
                var mergedCatalogItemList = catalogItems.Concat(storedCatalogItems);
                catalogItems = [.. _factory.MergeCatalogItems(mergedCatalogItemList)];

                _logger.LogInformation($"Merging of processed and stored catalog items have beend finished. - Catalog items count: {catalogItems.Count}");
            }

            var jsonContent = JsonSerializer.Serialize(catalogItems, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            await _driveService.UploadJsonAsync(jsonContent);
            _logger.LogInformation("Catalog items successfully uploaded on to Google Drive storage!");
        }

        private async Task<List<ICatalogItem>> CollectRelevantCatalogItemsFromStorage()
        {
            var catalogItems = await _driveService.ReadJsonAsync<ICatalogItem>();

            return [.. catalogItems.Where(item => item.PublishedDate.Date == DateTimeOffset.Now.Date)];
        }

        private async Task<IEnumerable<Item>> CollectRelevantItemsFromFeed()
        {
            _logger.LogInformation("Collecting relevant items from RSS feed...");

            var xmlReader = new XmlReader();
            NcoreRssDocument? doc = null;

            if (!string.IsNullOrEmpty(_ncoreSettings.RssUrl))
            {
                var content = await _rssClient.GetFeedAsync(_ncoreSettings.RssUrl);
                doc = XmlReader.FromString<NcoreRssDocument>(content);
            }
            else
            {
                if (string.IsNullOrEmpty(_ncoreSettings.RssFilePath))
                    throw new InvalidDataException($"{nameof(_ncoreSettings.RssFilePath)} can not be empty!");

                doc = XmlReader.FromFile<NcoreRssDocument>(_ncoreSettings.RssFilePath);
            }

            return doc.Channel.Items
                .Where(item => _ncoreSettings.CategoryFilter.Contains(item.Category))
                .Where(item => DateTimeOffset.TryParse(item.PublishedDate, out var publishedDate) && publishedDate.Date == DateTimeOffset.Now.Date)
                .Where(item => !Regex.IsMatch(item.Title, @"\b(?:S\d{2}E\d{2}|E\d{2})\b", RegexOptions.IgnoreCase))
                .Where(item => !_ncoreSettings.TitleKeyBlackList.Any(pair => item.Title.Contains(pair.Key, StringComparison.OrdinalIgnoreCase) && pair.Value.Any(value => item.Title.Contains(value, StringComparison.OrdinalIgnoreCase))));
        }
    }
}