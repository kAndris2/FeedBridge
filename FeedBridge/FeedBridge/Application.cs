using FeedBridge.Models;
using FeedBridge.Services;

namespace FeedBridge
{
    public class Application(GoogleDriveService driveService)
    {
        private readonly GoogleDriveService _driveService = driveService;

        public void Start()
        {
            var xmlReader = new XmlReader();
            var doc = xmlReader.FromFile<NcoreRssDocument>("");
        }
    }
}