namespace FeedBridge.Models
{
    public class StorageInfo(string folderId, string? fileId)
    {
        public string FolderId { get; init; } = folderId;
        public string? FileId { get; init; } = fileId;
    }
}