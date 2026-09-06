using System.Text.Json;
using Microsoft.Extensions.Options;
using FeedBridge.Models;
using FeedBridge.Services;
using FeedBridge.Models.Configuration;

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
                WriteIndented = true
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