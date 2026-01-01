using GitAssistant.Settings;
using GitAssistant.Settings.Models;
using System.IO;
using Xunit;

namespace GitAssistant.Tests.Settings;

public class SettingsLoaderTests : IDisposable
{
    public void Dispose()
    {
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
    }

    private string CreateTempJsonFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void LoadBaseSettings_ShouldLoadCorrectly()
    {
        var settings = SettingsLoader.LoadSettings<TestGitRepositorySettings>();
        Assert.Equal(GetExpectedResultBasedOnEnv(), settings.Path);
    }

    [Fact]
    public void LoadOverrideSettings_ShouldOverrideBase()
    {
        var settings = SettingsLoader.LoadSettings<TestGitRepositorySettings>(
            extraFiles: new[] { "appsettings.Test.json" });

        Assert.Equal("Test", settings.Path);
    }

    private string GetExpectedResultBasedOnEnv()
    {
        var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        return env ?? "Base";
    }

    // [Fact]
    // public void LoadOverrideSettings_ShouldOverrideBase()
    // {
    //     var baseJson = @"{ ""GitRepositorySettings"": { ""Path"": ""BaseRepo"" } }";
    //     var overrideJson = @"{ ""GitRepositorySettings"": { ""Path"": ""OverrideRepo"" } }";

    //     var baseFile = CreateTempJsonFile(baseJson);
    //     var overrideFile = CreateTempJsonFile(overrideJson);

    //     var settings = SettingsLoader.LoadSettings<GitRepositorySettings>(
    //         extraFiles: new[] { baseFile, overrideFile });

    //     Assert.Equal("OverrideRepo", settings.Path);

    //     File.Delete(baseFile);
    //     File.Delete(overrideFile);
    // }

    // [Fact]
    // public void Load_ShouldMergeEnvironmentJson()
    // {
    //     var baseJson = @"{ ""GitRepositorySettings"": { ""Path"": ""BaseRepo"" } }";
    //     var baseFile = CreateTempJsonFile(baseJson);

    //     var envJson = @"{ ""GitRepositorySettings"": { ""Path"": ""DevRepo"" } }";
    //     var envFile = Path.Combine(Path.GetTempPath(), "appsettings.Development.json");
    //     File.WriteAllText(envFile, envJson);

    //     Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");

    //     var settings = SettingsLoader.LoadSettings<GitRepositorySettings>(
    //         extraFiles: new[] { baseFile });

    //     Assert.Equal("DevRepo", settings.Path);

    //     File.Delete(baseFile);
    //     File.Delete(envFile);
    //     Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
    // }

    // [Fact]
    // public void Get_ShouldLazyLoadSettings()
    // {
    //     var json = @"{ ""GitRepositorySettings"": { ""Path"": ""LazyRepo"" } }";
    //     var tempFile = CreateTempJsonFile(json);

    //     var settings = SettingsLoader.Get<GitRepositorySettings>(sectionName: "GitRepositorySettings");

    //     // Will load default empty value (no JSON)
    //     Assert.Equal(string.Empty, settings.Path);

    //     // Load with extra JSON
    //     settings = SettingsLoader.LoadSettings<GitRepositorySettings>(
    //         extraFiles: new[] { tempFile });

    //     Assert.Equal("LazyRepo", settings.Path);

    //     File.Delete(tempFile);
    // }
}
