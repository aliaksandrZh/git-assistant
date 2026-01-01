using GitAssistant.Application.Interface;
using GitAssistant.Application.Models;

namespace GitAssistant.Application.Services;
public class GitLogService : IGitLogService
{
    private readonly IGitCommandRunner _runner;

    public GitLogService(IGitCommandRunner runner)
    {
        _runner = runner;
    }

    public async Task<IReadOnlyList<GitCommit>> GetCommitsAsync(
        string repoPath,
        string? search,
        CancellationToken cancellationToken)
    {
        var format = "%H|%an|%ad|%s";
        var args = $"log --date=iso --pretty=format:\"{format}\"";

        if (!string.IsNullOrWhiteSpace(search))
            args += $" --grep=\"{search}\"";

        var result = await _runner.RunAsync(args, repoPath, cancellationToken);

        if (result.ExitCode != 0)
            throw new InvalidOperationException(result.StdErr);

        return Parse(result.StdOut);
    }

    private static IReadOnlyList<GitCommit> Parse(string output)
    {
        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line =>
            {
                var parts = line.Split('|', 4);
                return new GitCommit(
                    parts[0],
                    parts[1],
                    DateTime.Parse(parts[2]),
                    parts[3]);
            })
            .ToList();
    }
}
