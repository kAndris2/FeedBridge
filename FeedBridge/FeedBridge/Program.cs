using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using FeedBridge;
using FeedBridge.Interfaces;
using FeedBridge.Models.Configuration;
using FeedBridge.Services;
using FeedBridge.Services.Validators;

try
{
    var configValidators = GetConfigValidators();
    var builder = Host.CreateApplicationBuilder(args);
    builder.Logging.ClearProviders();
    builder.Logging.AddNLog();

    Configure<GoogleCredentials>(builder, configValidators);
    Configure<GoogleDriveSettings>(builder, configValidators);
    Configure<NcoreSettings>(builder, configValidators);
    Configure<TmdbSettings>(builder, configValidators);

    builder.Services.AddScoped<TmdbService>();
    builder.Services.AddScoped<RssItemPropExtractor>();
    builder.Services.AddScoped<CatalogItemFactory>();
    builder.Services.AddScoped<GoogleDriveService>();
    builder.Services.AddScoped<Application>();

    builder.Services.AddHttpClient<RssClient>();

    using var host = builder.Build();
    using var scope = host.Services.CreateScope();

    await scope.ServiceProvider
        .GetRequiredService<Application>()
        .Start();
}
catch (Exception ex)
{
    LogManager.GetCurrentClassLogger()
        .Error($"Unhandled exception! Ex.: {ex.Message}");
}
finally
{
    LogManager.Shutdown();
}

static void Configure<T>(HostApplicationBuilder builder, List<IConfigValidator> configValidators)
    where T : class, new()
{
    builder.Services.Configure<T>(
        GetConfigSection<T>(builder.Configuration, typeof(T).Name, configValidators)
    );
}

static IConfigurationSection GetConfigSection<T>(ConfigurationManager configurationManager, string sectionKey, List<IConfigValidator> configValidators)
{
    var section = configurationManager.GetSection(sectionKey);

    var config = section.Get<T>()
        ?? throw new ArgumentException($"{sectionKey} cannot be null!");

    var configValidator = configValidators.FirstOrDefault(validator => validator.CanValidate(config))
        ?? throw new ArgumentException($"{sectionKey} validator could not be found!");

    configValidator.Validate(config);

    return section;
}

static List<IConfigValidator> GetConfigValidators()
{
    return
    [
        new GoogleCredentialsValidator(),
        new GoogleDriveSettingsValidator(),
        new NcoreSettingsValidator(),
        new TmdbSettingsValidator()
    ];
}