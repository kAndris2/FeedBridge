using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using FeedBridge.Models;
using FeedBridge.Models.Configuration;

namespace FeedBridge.Services
{
    public class GoogleDriveService
    {
        private readonly ILogger<GoogleDriveService> _logger;
        private readonly GoogleDriveSettings _driveSettings;
        private readonly DriveService _driveService;
        private readonly StorageInfo _storageInfo;

        public GoogleDriveService(IOptions<GoogleCredentials> credentials, IOptions<GoogleDriveSettings> driveSettings, ILogger<GoogleDriveService> logger)
        {
            _driveSettings = driveSettings.Value;
            _logger = logger;
            _driveService = CreateDriveService(credentials.Value).Result;
            _storageInfo = FetchStorageInfo().Result;
        }

        public async Task<T[]> ReadJsonAsync<T>()
        {
            try
            {
                await using var stream = new MemoryStream();

                var request = _driveService.Files.Get(_storageInfo.FileId);
                await request.DownloadAsync(stream);

                stream.Position = 0;

                return await JsonSerializer.DeserializeAsync<T[]>(stream)
                    ?? throw new JsonException($"Failed to deserialize storage file '{_storageInfo.FileId}'.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return [];
            }
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

        private async Task<DriveService> CreateDriveService(GoogleCredentials credentials)
        {
            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = await Authorize(credentials)
            });
        }

        private async Task<UserCredential> Authorize(GoogleCredentials credentials)
        {
            var clientSecrets = new ClientSecrets
            {
                ClientId = credentials.ClientId,
                ClientSecret = credentials.ClientSecret
            };

            try
            {
                var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets,
                    [
                        DriveService.Scope.Drive
                    ],
                    "user",
                    CancellationToken.None,
                    new FileDataStore("GoogleAuth", true)
                );

                _logger.LogInformation("Successfully authorized to Google!");

                return credential;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unexpected error occurred during Google authorization! - Ex.: {ex.Message}");
                throw;
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

            _logger.LogInformation($"Storage file created! - Filename: {_driveSettings.JsonFileName} | Folder: {_driveSettings.TargetFolder}");
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

            _logger.LogInformation($"Storage file updated! - Filename: {_driveSettings.JsonFileName} | Folder: {_driveSettings.TargetFolder}");
        }

        private async Task<StorageInfo> FetchStorageInfo()
        {
            var folderId = await FindStorageFolderId();
            var storageFileId = await FindStorageFileId(folderId);

            _logger.LogInformation("Storage information collected!");

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