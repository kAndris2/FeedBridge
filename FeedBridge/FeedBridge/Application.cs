using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using FeedBridge.Models;
using FeedBridge.Models.Configuration;
using FeedBridge.Services;

namespace FeedBridge
{
    public class Application(GoogleDriveService driveService, CatalogItemFactory factory, IOptions<NcoreSettings> ncoreSettings, ILogger<Application> logger)
    {
        private readonly ILogger<Application> _logger = logger;
        private readonly NcoreSettings _ncoreSettings = ncoreSettings.Value;
        private readonly GoogleDriveService _driveService = driveService;
        private readonly CatalogItemFactory _factory = factory;

        public async Task Start()
        {
            var relevantItems = CollectRelevantItemsFromFeed();

            _logger.LogInformation($"Found {relevantItems.Count()} relevant items in the RSS feed.");

            var catalogItems = await _factory.CreateCatalogItems(relevantItems);
            var jsonContent = JsonSerializer.Serialize(catalogItems, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            await _driveService.UploadJsonAsync(jsonContent);
            _logger.LogInformation("Catalog items successfully uploaded on to Google Drive storage!");
        }

        private IEnumerable<Item> CollectRelevantItemsFromFeed()
        {
            var xmlReader = new XmlReader();
            var doc = xmlReader.FromFile<NcoreRssDocument>(_ncoreSettings.FeedLocation);

            return doc.Channel.Items
                .Where(item => _ncoreSettings.CategoryFilter.Contains(item.Category))
                .Where(item => DateTimeOffset.TryParse(item.PublishedDate, out var publishedDate) && publishedDate.Date == DateTimeOffset.Now.Date)
                .Where(item => !item.Title.Contains("E0"));
        }
    }
}