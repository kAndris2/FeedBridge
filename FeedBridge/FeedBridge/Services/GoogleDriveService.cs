using System.Text;
using Microsoft.Extensions.Options;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using FeedBridge.Models.Configuration;

namespace FeedBridge.Services
{
    public class GoogleDriveService
    {
        private readonly GoogleDriveSettings _driveSettings;
        private readonly DriveService _driveService;

        public GoogleDriveService(IOptions<GoogleCredentials> credentials, IOptions<GoogleDriveSettings> driveSettings)
        {
            _driveSettings = driveSettings.Value;

            var clientSecrets = new ClientSecrets
            {
                ClientId = credentials.Value.ClientId,
                ClientSecret = credentials.Value.ClientSecret
            };

            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                [
                    DriveService.Scope.Drive
                ],
                "user",
                CancellationToken.None,
                new FileDataStore("GoogleAuth", true)
            ).Result;

            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });
        }

        public async Task UploadJsonAsync(string content)
        {
            var fileName = _driveSettings.JsonFileName + ".json";
            var request = _driveService.Files.List();

            request.Q =
                $"name = '{EscapeQueryValue(_driveSettings.TargetFolder)}' " +
                "and mimeType = 'application/vnd.google-apps.folder' " +
                "and trashed = false";

            request.Fields = "files(id, name)";
            request.PageSize = 1;

            var folderResult = await request.ExecuteAsync();
            var folder = folderResult.Files.FirstOrDefault() ?? throw new DirectoryNotFoundException($"Google Drive folder not found: {_driveSettings.TargetFolder}");
            request = _driveService.Files.List();

            request.Q =
                $"name = '{EscapeQueryValue(fileName)}' " +
                $"and '{folder.Id}' in parents " +
                "and trashed = false";

            request.Fields = "files(id, name)";
            request.PageSize = 1;

            var fileResult = await request.ExecuteAsync();
            var existingFile = fileResult.Files.FirstOrDefault();

            await using var stream = new MemoryStream(
                Encoding.UTF8.GetBytes(content));

            if (existingFile != null)
            {
                var updateRequest = _driveService.Files.Update(
                    new Google.Apis.Drive.v3.Data.File(),
                    existingFile.Id,
                    stream,
                    "application/json");

                updateRequest.Fields = "id";

                await updateRequest.UploadAsync();
            }

            var createRequest = _driveService.Files.Create(
                new Google.Apis.Drive.v3.Data.File
                {
                    Name = fileName,
                    Parents = [folder.Id]
                },
                stream,
                "application/json");

            createRequest.Fields = "id";

            await createRequest.UploadAsync();
        }

        private static string EscapeQueryValue(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("'", "\\'");
        }
    }
}