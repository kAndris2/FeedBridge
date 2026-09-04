using FeedBridge.Services;

namespace FeedBridge
{
    public class Application(GoogleDriveService driveService)
    {
        private readonly GoogleDriveService _driveService = driveService;

        public void Start()
        {

        }
    }
}