using GitAssistant.Application.Interface;
using GitAssistant.Application.Models;

namespace GitAssistant.Application.Services;
public class GitLogService(IGitCommandRunner runner) : IGitLogService
{
    private const string GitHash = "%H";
    private const string GitAuthor = "%an";
    private const string GitDate = "%ad";
    private const string GitSubject = "%s";
    private static string LogFormat => $"{GitHash}|{GitAuthor}|{GitDate}|{GitSubject}";

    public async Task<IReadOnlyList<GitCommit>> GetCommitsAsync(GitLog options,
        CancellationToken cancellationToken)
    {
        var args = $"log --date=iso --pretty=format:\"{LogFormat}\"";

        if (!string.IsNullOrWhiteSpace(options.Branch))
        {
            args += $" {options.Branch}";
        }

        if (!string.IsNullOrWhiteSpace(options.Query))
            args += $" --grep=\"{options.Query}\"";

        var result = await runner.RunAsync(args, options.Path, cancellationToken);

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
