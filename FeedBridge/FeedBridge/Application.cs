using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Options;
using FeedBridge.Models;
using FeedBridge.Models.Configuration;
using FeedBridge.Services;

namespace FeedBridge
{
    public class Application(GoogleDriveService driveService, CatalogItemFactory factory, IOptions<NcoreSettings> ncoreSettings)
    {
        private readonly NcoreSettings _ncoreSettings = ncoreSettings.Value;
        private readonly GoogleDriveService _driveService = driveService;
        private readonly CatalogItemFactory _factory = factory;

        public async Task Start()
        {
            var relevantItems = CollectRelevantItemsFromFeed();
            var catalogItems = await _factory.CreateCatalogItems(relevantItems);

            var jsonContent = JsonSerializer.Serialize(catalogItems, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            await _driveService.UploadJsonAsync(jsonContent);
        }

        private IEnumerable<Item> CollectRelevantItemsFromFeed()
        {
            var xmlReader = new XmlReader();
            var doc = xmlReader.FromFile<NcoreRssDocument>(_ncoreSettings.FeedLocation);

            return doc.Channel.Items.Where(item => _ncoreSettings.CategoryFilter.Contains(item.Category));
        }
    }
}