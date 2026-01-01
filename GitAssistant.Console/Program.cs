using GitAssistant.Core;
using GitAssistant.Core.Interface;
using GitAssistant.Core.Models;
using GitAssistant.Core.Services;
using GitAssistant.Settings;
using GitAssistant.Settings.Models;
using Microsoft.Extensions.DependencyInjection;

var settings = SettingsLoader.LoadSettings<GitRepositorySettings>();
var services = new ServiceCollection();
services.AddApplicationServices();

var provider = services.BuildServiceProvider();
var git = provider.GetRequiredService<IGitEngine>();
var query = "31414|26871|30908|30903";
// var commits = await git.GetCommitsAsync(new GitLog
// {
//     Path = settings.Path,
//     Limit = 100
// },
// CancellationToken.None);



var commits = await git.SearchCommitsAsync(new GitLog
{
    Path = settings.Path,
    Query = query,
    Limit = 100
},
CancellationToken.None);

foreach (var c in commits)
{
    Console.WriteLine($"{c.Hash[..7]} {c.Author} {c.Date:d} {c.Message}");
}