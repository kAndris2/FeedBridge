using Microsoft.Extensions.Options;
using FeedBridge.Models;
using FeedBridge.Services;
using FeedBridge.Models.Configuration;

namespace FeedBridge
{
    public class Application(GoogleDriveService driveService, IOptions<NcoreSettings> ncoreSettings)
    {
        private readonly NcoreSettings _ncoreSettings = ncoreSettings.Value;
        private readonly GoogleDriveService _driveService = driveService;

        public void Start()
        {
            var relevantItems = CollectRelevantItemsFromFeed();
        }

        private IEnumerable<Item> CollectRelevantItemsFromFeed()
        {
            var xmlReader = new XmlReader();
            var doc = xmlReader.FromFile<NcoreRssDocument>(_ncoreSettings.FeedLocation);

            return doc.Channel.Items.Where(item => _ncoreSettings.CategoryFilter.Contains(item.Category));
        }
    }
}