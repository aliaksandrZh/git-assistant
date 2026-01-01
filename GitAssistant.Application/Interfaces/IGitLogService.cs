using GitAssistant.Application.Models;

namespace GitAssistant.Application.Interface;

public interface IGitLogService
{
    Task<IReadOnlyList<GitCommit>> GetCommitsAsync(
        string repoPath,
        string? search,
        CancellationToken cancellationToken);
}
