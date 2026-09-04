using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace FeedBridge
{
    public class GoogleDriveService
    {
        private readonly DriveService _driveService;

        public GoogleDriveService()
        {
            using var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);

            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new[]
                {
                    DriveService.Scope.Drive
                },
                "user",
                CancellationToken.None,
                new FileDataStore("token.json", true)
            ).Result;

            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "My C# Google Drive App"
            });
        }

        public async Task<string> UploadOrUpdateJsonAsync(string[] folderPath, string fileName, string jsonContent)
        {
            string? parentFolderId = null;

            foreach (var folderName in folderPath)
            {
                parentFolderId = await GetOrCreateFolderAsync(
                    folderName,
                    parentFolderId);
            }

            string? existingFileId = await FindFileAsync(
                fileName,
                parentFolderId!);

            using var stream = new MemoryStream(
                Encoding.UTF8.GetBytes(jsonContent));

            if (existingFileId != null)
            {
                var metadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = fileName,
                    MimeType = "application/json"
                };

                await _driveService.Files
                    .Update(metadata, existingFileId, stream, "application/json")
                    .UploadAsync();

                return existingFileId;
            }
            else
            {
                var metadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = fileName,
                    MimeType = "application/json",
                    Parents = new List<string>
                    {
                        parentFolderId!
                    }
                };

                var request = _driveService.Files.Create(metadata, stream, "application/json");

                request.Fields = "id";

                await request.UploadAsync();

                return request.ResponseBody.Id;
            }
        }

        private async Task<string?> FindFileAsync(string fileName, string parentFolderId)
        {
            string query =
                $"name = '{EscapeQueryValue(fileName)}' " +
                $"and '{parentFolderId}' in parents " +
                $"and trashed = false";

            var request = _driveService.Files.List();

            request.Q = query;
            request.Fields = "files(id, name)";
            request.PageSize = 1;

            var result = await request.ExecuteAsync();

            return result.Files.FirstOrDefault()?.Id;
        }

        private async Task<string> GetOrCreateFolderAsync(string folderName, string? parentFolderId)
        {
            string query =
                $"name = '{EscapeQueryValue(folderName)}' " +
                $"and mimeType = 'application/vnd.google-apps.folder' " +
                $"and trashed = false";

            if (parentFolderId != null)
            {
                query += $" and '{parentFolderId}' in parents";
            }
            else
            {
                query += " and 'root' in parents";
            }

            var request = _driveService.Files.List();

            request.Q = query;
            request.Fields = "files(id, name)";
            request.PageSize = 1;

            var result = await request.ExecuteAsync();

            var existingFolder = result.Files.FirstOrDefault();

            if (existingFolder != null)
            {
                return existingFolder.Id;
            }

            var folderMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            if (parentFolderId != null)
            {
                folderMetadata.Parents = new List<string>
                {
                    parentFolderId
                };
            }

            var folder = await _driveService.Files
                .Create(folderMetadata)
                .ExecuteAsync();

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