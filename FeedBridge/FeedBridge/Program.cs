using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FeedBridge;
using FeedBridge.Services;
using FeedBridge.Models.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<GoogleCredentials>(
    builder.Configuration.GetSection("GoogleCredentials"));
builder.Services.Configure<GoogleDriveSettings>(
    builder.Configuration.GetSection("GoogleDriveSettings"));
builder.Services.Configure<NcoreSettings>(
    builder.Configuration.GetSection("NcoreSettings"));
builder.Services.Configure<TmdbSettings>(
    builder.Configuration.GetSection("TmdbSettings"));

builder.Services.AddScoped<TmdbService>();
builder.Services.AddScoped<RssItemPropExtractor>();
builder.Services.AddScoped<CatalogItemFactory>();
builder.Services.AddScoped<GoogleDriveService>();
builder.Services.AddScoped<Application>();

using var host = builder.Build();
using var scope = host.Services.CreateScope();

await scope.ServiceProvider
    .GetRequiredService<Application>()
    .Start();