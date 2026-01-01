using GitAssistant.Core.Models;

namespace GitAssistant.Core.Interface;

public interface IGitLogService
{
    Task<IReadOnlyList<GitCommit>> GetCommitsAsync(GitLog options,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<GitCommit>> SearchAsync(GitLog options, CancellationToken cancellationToken);
}
