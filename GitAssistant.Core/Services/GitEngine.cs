using GitAssistant.Core.Interface;
using GitAssistant.Core.Models;

namespace GitAssistant.Core.Services;
public sealed class GitEngine(IGitLogService logService, IGitCommandRunner runner) : IGitEngine
{
    public async Task<IReadOnlyList<GitCommit>> GetCommitsAsync(GitLog options, CancellationToken cancellationToken)
    {
        return await logService.GetCommitsAsync(options, cancellationToken);
    }

    public async Task<IReadOnlyList<GitCommit>> SearchCommitsAsync(GitLog options, CancellationToken cancellationToken)
    {
        return await logService.SearchAsync(options, cancellationToken);
    }

    public async Task<GitRepositoryStatus> ValidateAsync(
        string path,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(path))
            return GitRepositoryStatus.DirectoryNotFound;

        var result = await runner.RunAsync(
        "rev-parse --is-inside-work-tree",
        path,
        cancellationToken);

        if (result.ExitCode == 0)
            return GitRepositoryStatus.Valid;

        if (result.StdErr.Contains("not a git repository",
            StringComparison.OrdinalIgnoreCase))
            return GitRepositoryStatus.NotAGitRepository;

        if (result.StdErr.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return GitRepositoryStatus.GitNotInstalled;

        return GitRepositoryStatus.UnknownError;
    }
}
