using GitAssistant.Application;
using GitAssistant.Application.Interface;
using GitAssistant.Application.Models;
using GitAssistant.Application.Services;
using GitAssistant.Settings;
using GitAssistant.Settings.Models;
using Microsoft.Extensions.DependencyInjection;

var settings = SettingsLoader.LoadSettings<GitRepositorySettings>();
var services = new ServiceCollection();
services.AddApplicationServices();

var provider = services.BuildServiceProvider();
var git = provider.GetRequiredService<IGitEngine>();

var commits = await git.GetCommitsAsync(new GitLog
{
    Path = settings.Path,
},
CancellationToken.None);

foreach (var c in commits)
{
    Console.WriteLine($"{c.Hash[..7]} {c.Author} {c.Date:d} {c.Message}");
}
