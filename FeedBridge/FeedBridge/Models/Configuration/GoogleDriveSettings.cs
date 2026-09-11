namespace FeedBridge.Models.Configuration
{
    public class GoogleDriveSettings
    {
        public string TargetFolder { get; set; }

        private string _jsonFileName;
        public string JsonFileName 
        {
            get => _jsonFileName;
            set => _jsonFileName = Path.GetFileNameWithoutExtension(value) + ".json";
        }
    }
}