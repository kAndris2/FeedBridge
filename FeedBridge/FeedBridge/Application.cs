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

        public void Start()
        {
            var relevantItems = CollectRelevantItemsFromFeed();
            var catalogItems = _factory.CreateCatalogItems(relevantItems);
        }

        private IEnumerable<Item> CollectRelevantItemsFromFeed()
        {
            var xmlReader = new XmlReader();
            var doc = xmlReader.FromFile<NcoreRssDocument>(_ncoreSettings.FeedLocation);

            return doc.Channel.Items.Where(item => _ncoreSettings.CategoryFilter.Contains(item.Category));
        }
    }
}