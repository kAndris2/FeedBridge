using FeedBridge;
using FeedBridge.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<GoogleDriveService>();
builder.Services.AddScoped<Application>();

using var host = builder.Build();

host.Services
    .GetRequiredService<Application>()
    .Start();