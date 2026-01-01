using GitAssistant.Application.Interface;
using GitAssistant.Application.Services;
using GitAssistant.Settings;
using GitAssistant.Settings.Models;
using Microsoft.Extensions.DependencyInjection;


var settings = SettingsLoader.LoadSettings<GitRepositorySettings>();


Console.WriteLine($"repositorySettings: {settings.Path}");
// string repoPath = repositorySettings.Path;
// Console.WriteLine($"Using repo: {repoPath}");

var services = new ServiceCollection();

services.AddSingleton<IGitCommandRunner, GitCommandRunner>();
services.AddSingleton<IGitLogService, GitLogService>();

var provider = services.BuildServiceProvider();


// var repoPath = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
// var search = args.Length > 1 ? args[1] : null;

Console.WriteLine("Console is started");

// var repoPath = "$HOME/Programming/git/external";
// var gitLog = provider.GetRequiredService<IGitLogService>();

// var commits = await gitLog.GetCommitsAsync(
//     repoPath,
//     null,
//     CancellationToken.None);

// foreach (var c in commits)
// {
//     Console.WriteLine($"{c.Hash[..7]} {c.Author} {c.Date:d} {c.Message}");
// }
