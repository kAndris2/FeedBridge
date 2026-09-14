using System.Text.Json.Serialization;
using FeedBridge.Enums;
using FeedBridge.Models;

namespace FeedBridge.Interfaces
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(MovieCatalogItem), "movie")]
    [JsonDerivedType(typeof(SeriesCatalogItem), "series")]
    [JsonDerivedType(typeof(MusicCatalogItem), "music")]
    [JsonDerivedType(typeof(LanguageCatalogItem), "language")]
    [JsonDerivedType(typeof(CatalogItem), "catalog")]
    public interface ICatalogItem
    {
        public string Title { get; set; }
        public Category? Category { get; set; }
        public DateTime PublishedDate { get; set; }
    }
}