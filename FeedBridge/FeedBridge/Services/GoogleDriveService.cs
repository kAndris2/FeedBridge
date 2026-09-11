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
            var folderId = await FindStorageFolderId();
            var storageFileId = await FindStorageFileId(fileName, folderId);

            await using var stream = new MemoryStream(
                Encoding.UTF8.GetBytes(content));

            if (storageFileId != null)
            {
                await UpdateStorageFile(storageFileId, stream);
            }
            else
            {
                await CreateStorageFile(fileName, folderId, stream);
            }
        }

        private async Task CreateStorageFile(string fileName, string parentFolderId, MemoryStream stream)
        {
            var createRequest = _driveService.Files.Create(
                new Google.Apis.Drive.v3.Data.File
                {
                    Name = fileName,
                    Parents = [parentFolderId]
                },
                stream,
                "application/json");

            createRequest.Fields = "id";

            await createRequest.UploadAsync();
        }

        private async Task UpdateStorageFile(string storageFileId, MemoryStream stream)
        {
            var updateRequest = _driveService.Files.Update(
                new Google.Apis.Drive.v3.Data.File(),
                storageFileId,
                stream,
                "application/json");

            updateRequest.Fields = "id";

            await updateRequest.UploadAsync();
        }

        private async Task<string?> FindStorageFileId(string fileName, string folderId)
        {
            var request = _driveService.Files.List();

            request.Q =
                $"name = '{EscapeQueryValue(fileName)}' " +
                $"and '{folderId}' in parents " +
                "and trashed = false";

            request.Fields = "files(id, name)";
            request.PageSize = 1;

            var fileResult = await request.ExecuteAsync();
            
            return fileResult.Files.FirstOrDefault()?.Id;
        }

        private async Task<string> FindStorageFolderId()
        {
            var request = _driveService.Files.List();

            request.Q =
                $"name = '{EscapeQueryValue(_driveSettings.TargetFolder)}' " +
                "and mimeType = 'application/vnd.google-apps.folder' " +
                "and trashed = false";

            request.Fields = "files(id, name)";
            request.PageSize = 1;

            var folderResult = await request.ExecuteAsync();
            var folder = folderResult.Files.FirstOrDefault() 
                ?? throw new DirectoryNotFoundException($"Google Drive folder not found: {_driveSettings.TargetFolder}");

            return folder.Id;
        }

        private static string EscapeQueryValue(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("'", "\\'");
        }
    }
}