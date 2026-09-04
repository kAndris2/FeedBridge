using FeedBridge.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<GoogleDriveService>();

using var host = builder.Build();

var googleDriveService =
    host.Services.GetRequiredService<GoogleDriveService>();