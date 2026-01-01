using Microsoft.Extensions.Configuration;

namespace GitAssistant.Settings;
// Program.cs or Startup.cs
// builder.Services.Configure<TestRepoSettings>(builder.Configuration.GetSection("TestRepoSettings"));

public static class SettingsLoader
{
    private static IConfigurationRoot? _config;

    private static IConfigurationRoot Config
    {
        get
        {
            if (_config == null)
            {
                _config = Load();
            }
            return _config;
        }
    }

    public static T Get<T>(string? sectionName = null) where T : class, new()
    {
        sectionName ??= typeof(T).Name;
        return Config.GetSection(sectionName).Get<T>() ?? new T();
    }

    /// <summary>
    /// Loads configuration with base JSON, environment-specific JSON, optional extra files, and environment variables.
    /// </summary>
    /// <param name="basePath">Optional base path. Defaults to assembly location.</param>
    /// <param name="extraFiles">Optional array of extra JSON files to load (e.g., project-specific overrides).</param>
    public static IConfigurationRoot Load(string? basePath = null, string[]? extraFiles = null)
    {
        basePath ??= AppContext.BaseDirectory;

        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty;

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

        if (extraFiles != null)
        {
            foreach (var file in extraFiles)
            {
                builder.AddJsonFile(file, optional: true, reloadOnChange: true);
            }
        }

        // builder.AddEnvironmentVariables();

        return builder.Build();
    }

    /// <summary>
    /// Load a typed section from config, defaults to new T() if missing.
    /// </summary>
    public static T LoadSettings<T>(string? basePath = null, string[]? extraFiles = null) where T : class, new()
    {
        var config = Load(basePath, extraFiles);
        var sectionName = typeof(T).Name; // convention: section name = class name
        return config.GetSection(sectionName).Get<T>() ?? new T();
    }
}
