using System.Text.Json.Serialization;
using FeedBridge.Enums;
using FeedBridge.Models;

namespace FeedBridge.Interfaces
{
    [JsonDerivedType(typeof(MovieCatalogItem))]
    [JsonDerivedType(typeof(SeriesCatalogItem))]
    [JsonDerivedType(typeof(MusicCatalogItem))]
    [JsonDerivedType(typeof(LanguageCatalogItem))]
    [JsonDerivedType(typeof(CatalogItem))]
    public interface ICatalogItem
    {
        public string Title { get; set; }
        public Category? Category { get; set; }
        public DateTime PublishedDate { get; set; }
    }
}