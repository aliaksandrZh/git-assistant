using GitAssistant.Application.Models;

namespace GitAssistant.Application.Interface;

public interface IGitLogService
{
    Task<IReadOnlyList<GitCommit>> GetCommitsAsync(GitLog options,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<GitCommit>> SearchAsync(GitLog options, CancellationToken cancellationToken);
}
