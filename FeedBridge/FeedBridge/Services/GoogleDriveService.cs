using System.Text;
using Microsoft.Extensions.Options;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using FeedBridge.Models.Configuration;
using FeedBridge.Models;

namespace FeedBridge.Services
{
    public class GoogleDriveService
    {
        private readonly GoogleDriveSettings _driveSettings;
        private readonly DriveService _driveService;
        private readonly StorageInfo _storageInfo;

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

            _storageInfo = FetchStorageInfo().Result;
        }

        public async Task UploadJsonAsync(string content)
        {
            await using var stream = new MemoryStream(
                Encoding.UTF8.GetBytes(content));

            if (_storageInfo.FileId != null)
            {
                await UpdateStorageFile(stream);
            }
            else
            {
                await CreateStorageFile(stream);
            }
        }

        private async Task CreateStorageFile(MemoryStream stream)
        {
            var createRequest = _driveService.Files.Create(
                new Google.Apis.Drive.v3.Data.File
                {
                    Name = _driveSettings.JsonFileName,
                    Parents = [_storageInfo.FolderId]
                },
                stream,
                "application/json");

            createRequest.Fields = "id";

            await createRequest.UploadAsync();
        }

        private async Task UpdateStorageFile(MemoryStream stream)
        {
            var updateRequest = _driveService.Files.Update(
                new Google.Apis.Drive.v3.Data.File(),
                _storageInfo.FileId,
                stream,
                "application/json");

            updateRequest.Fields = "id";

            await updateRequest.UploadAsync();
        }

        private async Task<StorageInfo> FetchStorageInfo()
        {
            var folderId = await FindStorageFolderId();
            var storageFileId = await FindStorageFileId(folderId);

            return new StorageInfo(folderId, storageFileId);
        }

        private async Task<string?> FindStorageFileId(string folderId)
        {
            var request = _driveService.Files.List();

            request.Q =
                $"name = '{EscapeQueryValue(_driveSettings.JsonFileName)}' " +
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